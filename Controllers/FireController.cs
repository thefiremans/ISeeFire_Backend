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
            }
            
            return new JsonResult(response);
        }
    }
}
