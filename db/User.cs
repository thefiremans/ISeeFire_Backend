using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NASATest2018
{
	public class User
	{
		public string UserId {get; set;}
		public string Name {get;set;}
		public string Phone {get;set;}
	}
}

