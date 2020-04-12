using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.XWPF.UserModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using System.Xml.Linq;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml;
using Newtonsoft.Json;

namespace DocParser.Service
{
    public class BodyRetriever
    {
        public static WordprocessingDocument doc;
        public static string json;


        public void newMethod(Stream mainStream)
        {
          
            MemoryStream mem = new MemoryStream();
            mainStream.Position = 0;
            mainStream.CopyTo(mem);
            doc = WordprocessingDocument.Open(mem, true);
           Debug.WriteLine(doc);
        }

        public List<StringBuilder> getBody(XWPFDocument Document)
        {
            var allParas = new List<StringBuilder>();
            var bos = Document.Paragraphs;

            Debug.WriteLine(doc);

            MainDocumentPart mdp = doc.MainDocumentPart;
            XDocument xDoc = mdp.GetXDocument();

            var paragraphs = xDoc.Descendants(W.p);

            List<string> allListItems = new List<string>();

            foreach (var para in paragraphs)
            {
                string listItem = string.Empty;

                try
                {
                    string paraText = para.Descendants(W.t).Select(t => (string)t).StringConcatenate();
                    listItem = ListItemRetriever.RetrieveListItem(doc, para, null);

                    Debug.WriteLine(listItem.Length);
                    allListItems.Add(listItem);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Spaces");
                }

            }

            allParas = this.getHeadings(bos, allListItems);

            Debug.WriteLine(allParas.Count);
            return allParas;
        }

        public void Interop()
        {
            
            foreach (Microsoft.Office.Interop.Word.Paragraph paragraph in doc.ContentParts())
            {
                Microsoft.Office.Interop.Word.Style style =
                    paragraph.get_Style() as Microsoft.Office.Interop.Word.Style;

                string styleName = style.NameLocal;
                Debug.WriteLine(styleName);
                string text = paragraph.Range.Text;

                if (styleName == "Title")
                {
                    string title = text.ToString();
                    Debug.WriteLine(title);
                }
                else if (styleName == "Subtitle")
                {
                    string st = text.ToString() + "\n";
                    Debug.WriteLine(st);
                }
                else if (styleName == "Heading 1")
                {
                    string heading1 = text.ToString() + "\n";
                    Debug.WriteLine(heading1);
                }
            }
        }

        public List<StringBuilder> getHeadings(IList<XWPFParagraph> getAllIn, List<string> allListItems)
        {
      
            var allParas = new List<StringBuilder>();
            var bos = getAllIn;
            int keyvalue = 0;
            Dictionary<int, StringBuilder> allHeadingkeys = new Dictionary<int, StringBuilder>();

            List<String> headings = new List<String>() { "Rubrik1", "Rubrik2", "Rubrik3", "Rubrik4", "Rubrik5", "Rubrik6", "Rubrik7", "Rubrik8", "Rubrik9", "Heading1", "Heading2", "Heading3", "Heading4", "Heading5", "Heading6", "Heading7", "Heading8", "Heading9" };

            foreach (XWPFParagraph para in bos)
            {
                
                try
                {
                    if (headings.Contains(para.StyleID) && para.Text.Length != 0)
                    {
                        keyvalue++;
                        allHeadingkeys.Add(keyvalue, new StringBuilder(para.Text));
                    }

                    if (!headings.Contains(para.StyleID) && para.Text.Length != 0)
                    {
                        if (allHeadingkeys[keyvalue].Equals(String.Empty))
                        {
                            StringBuilder noHeadingData = allHeadingkeys[keyvalue];
                            noHeadingData.Append(para.Text);
                            allHeadingkeys[keyvalue] = noHeadingData;
                        }

                        else
                        {
                            StringBuilder getDataForKey = allHeadingkeys[keyvalue];
                            getDataForKey.Append(para.Text);
                        }
                    }
                }

                catch (Exception e)
                {
                    Debug.WriteLine("Incorrect paragraph Format");
                }

            }
            allParas = this.dataGetItems(allHeadingkeys, allListItems);

            return allParas;
        }



        public List<StringBuilder> dataGetItems(Dictionary<int, StringBuilder> allHeadingkeys, List<string> allListItems)
        {

            var allParas = new List<StringBuilder>();
            int numRed = 1;
            
                        if (allHeadingkeys.Keys.Count == allListItems.Count)
                        {
                               foreach (string eachListFormat in allListItems)
                               {
                                  String valueData = allHeadingkeys[numRed].ToString(); 
                                  valueData.Insert(0, eachListFormat);
                                  StringBuilder appender = new StringBuilder(valueData);
                                  allHeadingkeys[numRed] = appender;
                                  numRed++;
                               }
                            
                        }
                        else
                        {
                            Debug.WriteLine("Doc is Bad!");
                        }

                        foreach (KeyValuePair<int, StringBuilder> item in allHeadingkeys)
                        {
                            allParas.Add(item.Value);

                        }
           
            return allParas;
        } 

        public string getJson()
        {
          //  json = JsonConvert.SerializeObject(allHeadingkeys, Newtonsoft.Json.Formatting.Indented);
            return json;
        }

    }

    
}