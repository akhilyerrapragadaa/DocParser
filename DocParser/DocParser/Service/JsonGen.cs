using DocParser.Models;
using Newtonsoft.Json;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;


namespace DocParser.Service
{
    public class JsonGen
    {
        private XWPFDocument Document;
        public IndexGenerator hg = new IndexGenerator();
        public PdfToWord pdfTword = new PdfToWord();
        string saved = " ";
        public static Dictionary<String, List<Demand>> allDemands = new Dictionary<String, List<Demand>>();
        public List<Demand> headerDemands = new List<Demand>();
        public List<Demand> bodyDemands = new List<Demand>();


        //Creating a Json
        public void readFileData(Stream file, string extensionName)
        {
            if (extensionName.Equals(".pdf"))
            {
                Task<XWPFDocument> pdfThread = Task<XWPFDocument>.Factory.StartNew(() =>
                {
                    file.Position = 0;
                    XWPFDocument pdfTowordData = new XWPFDocument(pdfTword.getWordStream(file));
                    return pdfTowordData;
                });
                Document = pdfThread.Result;
            }
            else
            {
                file.Position = 0;
                Document = new XWPFDocument(file);
            }

            var headers = this.getTitle();

            foreach (Demand header in headers) { headerDemands.Add(header); }

            Task<List<String>> task = Task<List<String>>.Factory.StartNew(() =>
            {
                var test = this.getBody();
                return test;
            });
            List<String> body = task.Result;


            Debug.WriteLine(body);
            this.demandNumberGenerator(body);
            
        }

        public void demandNumberGenerator(List<String> body) { 

            Parallel.ForEach(body, new ParallelOptions { MaxDegreeOfParallelism = 1 }, eachPara =>
            {
                string firstCharOfString = " ";
                int counter = 0;

                if (eachPara.Length != 0)
                    firstCharOfString = this.getInit(Regex.Replace(eachPara, @"\s+", ""));


                if (saved.Equals(firstCharOfString) && eachPara.Length != 0)
                {
                    counter++;

                    if (counter >= 1)
                    {
                        int num = Convert.ToInt32(new string(hg.Current.Last(), 1)) - counter;
                        while (num != 0) { hg.DecreNext(); num--; }

                        if (Regex.IsMatch(hg.Current, @"\d\.\d\.\d"))
                            hg.UpLevels(1);
                    }


                    if (!Regex.IsMatch(hg.Current, @"\d\.\d\.\d"))
                         this.godown();
                  
                       parasToDemand(eachPara, firstCharOfString);
               
                }
                else
                {
                    counter = 0;
                    if (!hg.Equals("null") && eachPara.Length != 0)
                    {
                        while (hg.Current.Contains("."))
                        {
                            hg.UpLevels(1);
                        }
                       parasToDemand(eachPara, firstCharOfString);
             
                    }
                }
                saved = firstCharOfString;
            });
            this.addDataToDict(bodyDemands, headerDemands);
          
        }


            public void parasToDemand(String eachPara, String firstCharOfString) {
            
                if (eachPara.Length != 0)
                {
                    var allShouldDemands = this.getShouldDemands(eachPara);

                    foreach (Demand eachDemand in allShouldDemands)
                    {
                        if (!saved.Equals(firstCharOfString))
                        {

                            if (hg.Current.Contains("."))
                            {
                                hg.MoveNext();
                            }
                            else
                            {
                                hg.DownOneLevel();
                                hg.MoveNext();
                            }
                            eachDemand.setdemandNumber(hg.Current);
                            bodyDemands.Add(eachDemand);
                            Debug.WriteLine(eachDemand.getdemandNumber());
                            Debug.WriteLine(eachDemand.getDemand());
                        }
                        else
                        {

                            if (hg.Current.Contains("."))
                            {
                                hg.MoveNext();
                            }
                            else
                            {
                                hg.DownOneLevel();
                                hg.MoveNext();
                            }
                            eachDemand.setdemandNumber(hg.Current);
                            bodyDemands.Add(eachDemand);
                            Debug.WriteLine(eachDemand.getdemandNumber());
                            Debug.WriteLine(eachDemand.getDemand());
                        }
                    }

                }
                if (eachPara.Length != 0)
                {
                    var allShallDemands = this.getShallDemands(eachPara);


                    foreach (Demand eachDemand in allShallDemands)
                    {
                        if (!saved.Equals(firstCharOfString))
                        {

                            if (hg.Current.Contains("."))
                            {
                                hg.MoveNext();
                            }
                            else
                            {
                                hg.DownOneLevel();
                                hg.MoveNext();
                            }
                            eachDemand.setdemandNumber(hg.Current);
                            bodyDemands.Add(eachDemand);
                            Debug.WriteLine(eachDemand.getdemandNumber());
                            Debug.WriteLine(eachDemand.getDemand());
                        }
                        else
                        {

                            if (hg.Current.Contains("."))
                            {
                                hg.MoveNext();
                            }
                            else
                            {
                                hg.DownOneLevel();
                                hg.MoveNext();
                            }
                            eachDemand.setdemandNumber(hg.Current);
                            bodyDemands.Add(eachDemand);
                            Debug.WriteLine(eachDemand.getdemandNumber());
                            Debug.WriteLine(eachDemand.getDemand());
                        }
                    }

                }           
            }
    

        public void addDataToDict(List<Demand> inputBodyDemands, List<Demand> inputHeaderDemands)
        {
            List<Demand> headerChunks = new List<Demand>();
            List<List<Demand>> bodyChunks = new List<List<Demand>>();
            List<List<Demand>> subBodyChunks = new List<List<Demand>>();


            for (int f = 1; f <= inputHeaderDemands.Count; f++)
            {
                foreach (Demand headerDemand in inputHeaderDemands)
                {
                    headerChunks.Add(headerDemand);
                }
                string appended = "Title";
                allDemands.Add(appended, headerChunks);
            }


            string noOfChapters = this.getInit(Regex.Replace(inputBodyDemands.Last().getdemandNumber(), @"\s+", ""));

            for (int m = 0; m < int.Parse(noOfChapters); m++)
            {
                bodyChunks.Add(new List<Demand>());
            }

            for (int m = 0; m < (inputBodyDemands.Count - int.Parse(noOfChapters)) + 1; m++)
            {
                subBodyChunks.Add(new List<Demand>());
            }

            int count = 0;
            for (int m = 0; m < bodyChunks.Count; m++)
            {
                string index = " ";

                foreach (Demand demand in inputBodyDemands)
                {
                    if ((m + 1).ToString() == this.getInit(demand.getdemandNumber()) && Regex.IsMatch(demand.getdemandNumber(), @"\d\.\d\.\d"))
                    {
                        if (!index.Equals(this.getMultiInit(demand.getdemandNumber())))
                        {
                            index = this.getMultiInit(demand.getdemandNumber());
                            count += 1;
                        }
                        subBodyChunks.ElementAt(count).Add(demand);
                        string subAppend = "Chapter " + index;
                        this.addToDictionary(subAppend, subBodyChunks.ElementAt(count));

                    }
                    else if ((m + 1).ToString() == this.getInit(demand.getdemandNumber()) && Regex.IsMatch(demand.getdemandNumber(), @"\d\.\d"))
                    {
                        bodyChunks.ElementAt(m).Add(demand);
                        string appended = "Chapter " + (m + 1).ToString();
                        this.addToDictionary(appended, bodyChunks.ElementAt(m));
                    }
                }

            }

        }
        public void addToDictionary(String key, List<Demand> chunks)
        {
            if (!allDemands.ContainsKey(key))
                allDemands.Add(key, chunks);
            else
            {
                allDemands[key] = chunks;
            }
        }

        public string MyDictionaryToJson()
        {
            string json = JsonConvert.SerializeObject(allDemands, Formatting.Indented);
            return json;
        }

        public string getMultiInit(String demandNumber)
        {
            string multiValues = " ";
            Regex.Replace(demandNumber, @"\s+", "");
            for (int i = 0; i < demandNumber.Length; i++)
            {
                char ch = demandNumber.ToCharArray().ElementAt(i);
                if (Regex.IsMatch(multiValues, @"\d+\.\d+"))
                {
                    break;
                }
                else
                {
                    multiValues += ch;
                }
            }
            return multiValues;
        }

        public string getInit(String input)
        {
            char ch = '-';
            for (int i = 0; i < input.Length; i++)
            {
                ch = input.ToCharArray().ElementAt(i);
                if (Regex.IsMatch(ch.ToString(), @"\d+"))
                {
                    break;
                }
            }
            return ch.ToString();

        }
        public void godown()
        {
            hg.DownOneLevel();

        }


        public List<Demand> getShouldDemands(String docData)
        {
            List<Demand> allShouldDemands = new List<Demand>();

            var checkShouldDemands = new Regex("[^.!?;]*(should|ska)[^.!?;]*");
            var filteredShouldDemands = checkShouldDemands.Matches(docData);

            var result = Enumerable.Range(0, filteredShouldDemands.Count).Select(index => filteredShouldDemands[index].Value).ToList();

            foreach (string should in result)
            {
                allShouldDemands.Add(new Demand(should));
            }

            return allShouldDemands;
        }


        public List<Demand> getShallDemands(String docData)
        {

            List<Demand> allShallDemands = new List<Demand>();

            var checkShallDemands = new Regex("[^.!?;]*(shall|sko)[^.!?;]*");
            var filteredShallDemands = checkShallDemands.Matches(docData);

            var result = Enumerable.Range(0, filteredShallDemands.Count).Select(index => filteredShallDemands[index].Value).ToList();

            foreach (string shall in result)
            {
                allShallDemands.Add(new Demand(shall));
            }

            return allShallDemands;
        }

        public List<Demand> getTitle()
        {
            var allHeaders = new List<Demand>();
            IList<XWPFHeader> headers = Document.HeaderList;
            foreach (XWPFHeader header in headers)
            {
                if (header.Text.Length != 0)
                    allHeaders.Add(new Demand(header.Text));
            }
            return allHeaders;
        }

        public List<String> getBody()
        {
            var allParas = new List<String>();
            var bos = Document.Paragraphs;
            var strHTMLContent = new StringBuilder();
            string[] spitOnChapter;
        
                foreach (XWPFParagraph para in bos)
                {
                    strHTMLContent.Append(para.Text);
                }
               
            spitOnChapter = strHTMLContent.ToString().Split(new string[] { "Chapter" }, StringSplitOptions.None);
         
            foreach (string chapter in spitOnChapter)
                    {
                        if (chapter.Length != 0)
                        {

                        Regex.Replace(chapter, @"\s+", "");
                        Debug.WriteLine(chapter + "******************************************************************************");
                            allParas.Add(chapter);
                        }
                    }           
            Debug.WriteLine(allParas.Count);
            return allParas;
        }
    }
}