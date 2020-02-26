using DocParser.Service;
using java.io;
using NPOI.HSSF.UserModel;
using NPOI.XWPF.Usermodel;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DocParser.Controllers
{
    public class UploadController : Controller
    {
  
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Upload(HttpPostedFileBase file) 
        {
            var model = Server.MapPath("~/DocsFold/");
            var compatibledocTypes = new List<string> { ".doc", ".docx", ".pdf" };
            string sFileExtension = Path.GetExtension(file.FileName).ToLower();
            bool contains = compatibledocTypes.Contains(sFileExtension, StringComparer.OrdinalIgnoreCase);
         
            if (file.ContentLength > 0 && contains)
            {
                //Generate Json
                JsonGen newGenerate = new JsonGen();             
                newGenerate.readFileData(file.InputStream, sFileExtension);
                System.IO.File.WriteAllText(model + Path.GetFileNameWithoutExtension(file.FileName) + ".json", newGenerate.MyDictionaryToJson());
                ViewBag.Msg = "Uploaded Succesfully";
            }
            else
            {
                ViewBag.Msg = "Upload Failed";
            }
            return View("Index");
        }
    }
}