using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        
        private List<NasaFireReport> parseNasaDataCSV(MemoryStream stream)
        {
            var result = new List<NasaFireReport>();
            string[] headers = null;
            int latitude = 0
                ,longitude = 0
                ,acq_date = 0
                ,acq_time = 0
                ,confidence = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = reader.ReadLine();
                headers = line.Split(',', options: StringSplitOptions.RemoveEmptyEntries);

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

                string[] confidenceLevels = new [] {"nominal", "low", "high"};

                bool isViirsData = false;
                bool isSecondRow = true;

                while( !reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if(string.IsNullOrEmpty(line))
                    {
                        break;
                    }

                    var content = line.Split(',', options: StringSplitOptions.RemoveEmptyEntries);

                    CultureInfo provider = CultureInfo.InvariantCulture;

                    string date =$"{content[acq_date]}-{content[acq_time].Substring(0,2)}:{content[acq_time].Substring(2)}";

                    string localConfidence = content[confidence];

                    decimal parsedConfidence = 0;

                    if(isSecondRow)
                    {
                        isSecondRow = false;
                        isViirsData = confidenceLevels.Contains(localConfidence);
                    }

                    if(isViirsData)
                    {
                        if(localConfidence.StartsWith("l"))
                        {
                            parsedConfidence = 10;
                        }
                        if(localConfidence.StartsWith("n"))
                        {
                            parsedConfidence = 40;
                        }
                        if(localConfidence.StartsWith("h"))
                        {
                            parsedConfidence = 80;
                        }
                    }
                    else
                    {
                        parsedConfidence = Decimal.Parse(localConfidence);
                    }

                    var x = new NasaFireReport
                    {
                        Latitude = Decimal.Parse( content[latitude]),
                        Longitude = Decimal.Parse(content[longitude]),
                        Timestamp = DateTime.ParseExact(date, "yyyy-MM-dd-HH:mm", provider),
                        Confidence = parsedConfidence
                    };
                    result.Add(x);
                 } 
             }
             return result;
            
        }

        private bool isLastImportNASAFilesDateIsActual()
        {
            return false;
        }

         [HttpPost]
        public JsonResult DownloadFilesFromNASA()
         {
             string[] filesFromNASA = new [] 
             {
                 "https://firms.modaps.eosdis.nasa.gov/data/active_fire/c6/csv/MODIS_C6_Global_24h.csv",
                 "https://firms.modaps.eosdis.nasa.gov/data/active_fire/viirs/csv/VNP14IMGTDL_NRT_Global_24h.csv"
             };
             DownloadFilesFromNASAResponseDTO response = new DownloadFilesFromNASAResponseDTO
             {
 
             };
 
             if(isLastImportNASAFilesDateIsActual())
             {
                 response.Error = "No need to import NASA files.";
                 return new JsonResult(response);
             }
 
             using (var client = new WebClient())
             {
                 foreach(var path in filesFromNASA)
                 {
                     try
                     {
                        MemoryStream stream = new MemoryStream(client.DownloadData(path));
                        var result = parseNasaDataCSV(stream);
                        using(var context = new IsfContext())
                        {
                            try
                            {
                            context.NasaFireReports.AddRange(result);
                            context.SaveChanges(); 
                            }
                            catch(Exception ex)
                            {
                            Console.WriteLine(ex.Message);
                            }

                            
                        }
                     }
                     catch(Exception ex)
                     {
                         Console.WriteLine(ex.Message);
                     }
                     
 
                 }
                 
             }
             
 
 
             return new JsonResult(response);
         }
        
     }
 }
