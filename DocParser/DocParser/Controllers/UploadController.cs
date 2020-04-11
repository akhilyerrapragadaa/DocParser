using DocParser.Models;
using DocParser.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [Obsolete]
        public ActionResult Upload(HttpPostedFileBase file, FormData smodel) 
        {
            string[] selectedLanguages = smodel.SelectedLanguages;
            languageSelector getLanguageDemands = new languageSelector();
            var shouldList = getLanguageDemands.shouldSearch(selectedLanguages);
            var shallList =  getLanguageDemands.shallSearch(selectedLanguages);          

            var model = Server.MapPath("~/DocsFold/");
            var compatibledocTypes = new List<string> { ".doc", ".docx", ".pdf" };
            string sFileExtension = Path.GetExtension(file.FileName).ToLower();
            bool contains = compatibledocTypes.Contains(sFileExtension, StringComparer.OrdinalIgnoreCase);
         
            if (file.ContentLength > 0 && contains)
            {
                //Generate Json
                BodyRetriever retrieve = new BodyRetriever();
                retrieve.newMethod(file.InputStream);

                JsonGen newGenerate = new JsonGen();  
                newGenerate.readFileData(file.InputStream, sFileExtension, shouldList, shallList);

                System.IO.File.WriteAllText(model + Path.GetFileNameWithoutExtension(file.FileName) + ".json", retrieve.getJson());
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