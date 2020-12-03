using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WEB_CLIENTE.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult TrabajarConArchivos(HttpPostedFileBase Archivo1, HttpPostedFileBase Archivo2)
        {
            if (Archivo1.ContentLength > 0)
            {
                var fileName = System.IO.Path.GetFileName(Archivo1.FileName);
                var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/ArchivosAlmacenados"), fileName);
                Archivo1.SaveAs(path);
            }
            return View();
        }

        [HttpPost]
        public void Subir(HttpPostedFileBase[] file)
        {
            if (file == null) return;

            foreach (var f in file)
            {
                if (f != null)
                {
                    string archivo = (DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + f.FileName).ToLower();
                    f.SaveAs(Server.MapPath("~/Uploads/" + archivo));
                }
            }
        }

        // GET: Home  
        public ActionResult UploadFiles()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadFiles(HttpPostedFileBase[] files)
        {

            //Ensure model state is valid  
            if (ModelState.IsValid)
            {   //iterating through multiple file collection   
                foreach (HttpPostedFileBase file in files)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        var InputFileName = System.IO.Path.GetFileName(file.FileName);
                        var ServerSavePath = System.IO.Path.Combine(Server.MapPath("~/UploadedFiles/") + InputFileName);
                        //Save file to server folder  
                        file.SaveAs(ServerSavePath);
                        //assigning file uploaded status to ViewBag for showing message to user.  
                        ViewBag.UploadStatus = files.Count().ToString() + " files uploaded successfully.";
                    }

                }
            }
            return View();
        }

    }
}