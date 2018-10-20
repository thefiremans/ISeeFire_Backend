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
    public class DebugController : Controller
    {
        [HttpPost]
        public JsonResult GetUserList()
        {
            Console.WriteLine("GetUserList called");

            var result = new List<string>();

            using(var context = new IsfContext())
            {
                foreach(var user in context.Users)
                result.Add($"{user.UserId} - {user.Name} - {user.Phone}");
            }
            
            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult GetReportsList()
        {
            Console.WriteLine("GetReportsList called");

            var result = new List<string>();

            using(var context = new IsfContext())
            {
                foreach(var report in context.Reports)
                result.Add($"{report.ReportId} - {report.Latitude} - {report.Longitude} - {report.Timestamp}");
            }
            
            return new JsonResult(result);
        }
    }
}
