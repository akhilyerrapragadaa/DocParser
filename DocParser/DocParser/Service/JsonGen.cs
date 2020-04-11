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
using System.Collections.Concurrent;


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
        private ConcurrentBag<string> newshouldList = new ConcurrentBag<string>();
        private ConcurrentBag<string> newshallList = new ConcurrentBag<string>();

        //Creating a Json

        public void readFileData(Stream file, string extensionName, ConcurrentBag<string> shouldList, ConcurrentBag<string> shallList)
        {
            newshouldList = shouldList;
            newshallList = shallList;

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

            Task<List<StringBuilder>> task = Task<List<StringBuilder>>.Factory.StartNew(() =>
            {
                BodyRetriever retrieve = new BodyRetriever();
                var test = retrieve.getBody(Document);

               return test;
            });
              List<StringBuilder> body = task.Result;

            Debug.WriteLine(body);
            this.demandNumberGenerator(body);

        }

        public void demandNumberGenerator(List<StringBuilder> body)
        {

            Parallel.ForEach(body, new ParallelOptions { MaxDegreeOfParallelism = 1 }, eachPara =>
            {
                string firstCharOfString = " ";
                int counter = 0;

                if (eachPara.Length != 0)
                    firstCharOfString = this.getInit(Regex.Replace(eachPara.ToString(), @"\s+", ""));


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

                    parasToDemand(eachPara.ToString(), firstCharOfString);

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
                        parasToDemand(eachPara.ToString(), firstCharOfString);

                    }
                }
                saved = firstCharOfString;
            });
            this.addDataToDict(bodyDemands, headerDemands);

        }


        public void parasToDemand(String eachPara, String firstCharOfString)
        {

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
            this.parasToDemandd(eachPara, firstCharOfString);
        }
            public void parasToDemandd(String eachPara, String firstCharOfString)
            {
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
            string json = JsonConvert.SerializeObject(allDemands, Newtonsoft.Json.Formatting.Indented);
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

            foreach (string shoulDemand in newshouldList)
            {
                string name = "[^.!?;]*()[^.!?;]*";
                string modified = name.Insert(9, shoulDemand);
                // var checkShouldDemands = new Regex("[^.!?;]*(should|ska)[^.!?;]*");
                var checkShouldDemands = new Regex(modified);
                var filteredShouldDemands = checkShouldDemands.Matches(docData);

                var result = Enumerable.Range(0, filteredShouldDemands.Count).Select(index => filteredShouldDemands[index].Value).ToList();

                foreach (string should in result)
                {
                    allShouldDemands.Add(new Demand(should));
                }
            }
            return allShouldDemands;
        }


        public List<Demand> getShallDemands(String docData)
        {

            List<Demand> allShallDemands = new List<Demand>();

            foreach (string shallDemand in newshallList)
            {
                string name = "[^.!?;]*()[^.!?;]*";
                string modified = name.Insert(9, shallDemand);
                var checkShouldDemands = new Regex(modified);
                // var checkShallDemands = new Regex("[^.!?;]*(shall|sko)[^.!?;]*");
                var filteredShallDemands = checkShouldDemands.Matches(docData);

                var result = Enumerable.Range(0, filteredShallDemands.Count).Select(index => filteredShallDemands[index].Value).ToList();

                foreach (string shall in result)
                {
                    allShallDemands.Add(new Demand(shall));
                }
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


        public StringBuilder FilePathHasInvalidChars(StringBuilder path)
        {

            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                path = path.Replace(c.ToString(), "");
                Debug.WriteLine(path.Length);
            }
            return path;
        }
    }
}
