using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Logger.Database.Table
{
	public class DbEntity
	{
		[Key]
		public int Id { get; set; }
	}
}
