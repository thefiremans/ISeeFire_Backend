using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NASATest2018
{
	public class Report
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ReportId {get; set;}
		public string SecretUserId {get;set;}
		public string TextOfComment {get;set;}
		public decimal Longitude {get;set;}
		public decimal Latitude {get;set;}
		public DateTime Timestamp {get;set;}
	}
}

