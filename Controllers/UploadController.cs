
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NASATest2018.Models;
using System;
using System.IO;
using System.Net.Http.Headers;


namespace NASATest2018.Controllers
{
    public class UploadController: Controller
    {
        //UploadImage

        [HttpPost]
        public JsonResult UploadImage()
        {
            string name = "";
            var newFileName = string.Empty;

            var response = new UploadImageResponseDTO()
            {
                TotalUploadedSize = 0
                
            };

            if(HttpContext.Request.Form.ContainsKey("name"))
            {
                name = HttpContext.Request.Form["name"];
            }

            if (HttpContext.Request.Form.Files != null)
            {
                var fileName = string.Empty;
                string PathDB = string.Empty;

                var files = HttpContext.Request.Form.Files;

                //Assigning Unique Filename (Guid)
                var myUniqueFileName = Convert.ToString(Guid.NewGuid());

                string  FileExtension = "";

                using(var fs = new System.IO.MemoryStream())
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            //Getting FileName
                            fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                            

                            //Getting file Extension
                            FileExtension = Path.GetExtension(fileName);
                            
                            file.CopyTo(fs);


                        }

                    }

                    newFileName = myUniqueFileName + FileExtension;
                    response.TotalUploadedSize = fs.Length;
                    response.GeneratedImageName = newFileName+"#####" + name;
                
                }
            }
            return new JsonResult(response);
        }
    }
}