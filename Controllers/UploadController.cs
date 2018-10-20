
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
        public JsonResult UploadImage(string name)
        {
            var newFileName = string.Empty;

            if (HttpContext.Request.Form.Files != null)
            {
                var fileName = string.Empty;
                string PathDB = string.Empty;

                var files = HttpContext.Request.Form.Files;

                using(var fs = new System.IO.MemoryStream())
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            //Getting FileName
                            fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                            //Assigning Unique Filename (Guid)
                            var myUniqueFileName = Convert.ToString(Guid.NewGuid());

                            //Getting file Extension
                            var FileExtension = Path.GetExtension(fileName);

                            // concating  FileName + FileExtension
                            newFileName = myUniqueFileName + FileExtension;
                            
                            file.CopyTo(fs);


                        }

                    }
                
                }
            }
            return new JsonResult(new UploadImageResponseDTO());
        }
    }
}