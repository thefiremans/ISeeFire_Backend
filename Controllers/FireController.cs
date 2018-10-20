using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NASATest2018.Models;

namespace NASATest2018.Controllers
{
    public class FireController : Controller
    {
        private bool checkSecretUserId(string SecretUserId)
        {
            using(var context = new IsfContext())
            {
                var findResult = context.Users.FirstOrDefault(x => x.UserId == SecretUserId);
                return findResult != null;
            }
        }

        [HttpPost]
        public JsonResult ReportTheFire([FromBody] ReportTheFireParameterDTO param)
        {

            var response = new ReportTheFireResponseDTO
            {
            };

            //check secret user id
            if(checkSecretUserId(param.SecretUserId))
            {
                using(var context = new IsfContext())
                {
                    var added = context.Reports.Add( 
                        new Report
                        {
                            SecretUserId = param.SecretUserId,
                            Longitude = param.Longitude,
                            Latitude = param.Latitude,
                            TextOfComment = param.TextOfComment,
                            Timestamp = DateTime.UtcNow
                        }

                    );
                    context.SaveChanges();
                    response.ReportId = added.Entity.ReportId;
                }
            }
            else
            {
                response.Error = $"Unknown secret user id: \"{param.SecretUserId}\"";
        private void parseCSV(MemoryStream stream)
        {
            bool isFirstLine = true;
            string[] headers = null;
            int latitude, longitude,acq_date,acq_time,confidence;
            using (StreamReader reader = new StreamReader(stream))
            {

                string line = reader.ReadLine();
                if(isFirstLine)
                {
                    headers = line.Split(',', options: StringSplitOptions.RemoveEmptyEntries);
                    isFirstLine = false;
                    for(int i = 0; i < headers.Length; i++)
                    {
                        string actualHeader = headers[i];
                        if(actualHeader == nameof(latitude))
                        {
                            latitude = i;
                        }
                        if(actualHeader == nameof(longitude))
                        {
                            longitude = i;
                        }
                        if(actualHeader == nameof(acq_date))
                        {
                            acq_date = i;
                        }
                        if(actualHeader == nameof(acq_time))
                        {
                            acq_time = i;
                        }
                        if(actualHeader == nameof(confidence))
                        {
                            confidence = i;
                        }
                    }
                }

            }
            
        }

            }
            
            return new JsonResult(response);
        }
    }
}
