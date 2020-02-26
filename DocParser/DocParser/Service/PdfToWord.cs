using System.IO;
using SautinSoft;

namespace DocParser.Service
{
    public class PdfToWord
    {
        public Stream getWordStream(Stream file)
        {
            PdfFocus f = new PdfFocus();
            f.OpenPdf(file);
            byte[] docx = null;

            if (f.PageCount > 0)
            {
                docx = f.ToWord();
            }
            Stream stream = new MemoryStream(docx);
            return stream;

        }
    }
}