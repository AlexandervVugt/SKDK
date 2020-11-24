﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Data.Models
{
    public class Filter
    {
        public Filter(string val)
        {
            Value = val;
        }

        public Filter(){}

        [Column("value", TypeName = "varchar(20)"), Key]
        public string Value { get; set; }
    }
}
