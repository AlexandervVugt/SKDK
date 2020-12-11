using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Data.Models
{
    public class Filter
    {
        [Column("value", TypeName = "varchar(30)"), Key]
        public string Value { get; set; }
    }
}
