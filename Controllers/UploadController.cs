using System.Linq;
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
            string SecretUserId = "";
            string ReportId = "";
            var newFileName = string.Empty;

            var response = new UploadImageResponseDTO()
            {
                TotalUploadedSize = 0
                
            };

            if(HttpContext.Request.Form.ContainsKey("SecretUserId"))
            {
                SecretUserId = HttpContext.Request.Form["SecretUserId"];
            }

            if(HttpContext.Request.Form.ContainsKey("ReportId"))
            {
                ReportId = HttpContext.Request.Form["ReportId"];
            }

            int parsedReportId = 0;

            bool parseResult = int.TryParse(ReportId, out parsedReportId);
            if(!parseResult)
            {
                response.Error = "Failed to parse ReportId";
                return new JsonResult(response);
            }

            using(var context = new IsfContext())
            {
                var findResult = context.Reports.FirstOrDefault(x => x.SecretUserId == SecretUserId && x.ReportId ==  parsedReportId);
                if(findResult == null)
                {
                    response.Error = "No such portfolio binded to specified user.";
                    return new JsonResult(response);
                }
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
                    string pathToImage = System.IO.Path.Combine("./Images/", newFileName);
                    try
                    {
                        using(var fileWrite = new FileStream(pathToImage, FileMode.CreateNew))
                        {
                            var bytes = fs.ToArray();
                            fileWrite.Write(bytes, 0, bytes.Length);
                            fileWrite.Flush();

                            response.TotalUploadedSize = bytes.Length;
                            response.GeneratedImageName = $"fileName: {pathToImage}, SecretUser: {SecretUserId}, Report: {ReportId}";
                        }

                        using(var context = new IsfContext())
                        {
                            var findResult = context.Reports.FirstOrDefault(x => x.SecretUserId == SecretUserId && x.ReportId ==  parsedReportId);
                            findResult.ImagePath = pathToImage;
                            context.SaveChanges();
                        }

                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                        response.Error = "Failed to save image file";
                        return new JsonResult(response);

                    }

                    

                    
                    
                
                }
            }
            return new JsonResult(response);
        }
    }
}