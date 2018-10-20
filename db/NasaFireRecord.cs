using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NASATest2018
{
	public class NasaFireReport
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int NasaFireReportId {get; set;}
		public decimal Longitude {get;set;}
		public decimal Latitude {get;set;}
		public DateTime Timestamp {get;set;}
		public decimal Confidence {get; set;}
	}
}

