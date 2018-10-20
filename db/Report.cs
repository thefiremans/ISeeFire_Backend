using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace NASATest2018
{
	public class Report
	{
		public Guid ReportId {get; set;}
		public string SecretUserId {get;set;}
		public string TextOfComment {get;set;}
		public decimal Longitude {get;set;}
		public decimal Latitude {get;set;}
		public DateTime Timestamp {get;set;}
//		public


	}
}

