using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Stexchange.Data.Models
{
    public class Rating
    {
        [Column("id", TypeName = "serial"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("quality", TypeName = "tinyint unsigned"), Range(1, 6)]
        public byte? Quality { get; set; }

        [Column("communication", TypeName = "tinyint unsigned"), Range(1, 6)]
        public byte Communication { get; set; }

        [Column("reviewer", TypeName = "bigint(20) unsigned")]
        public int ReviewerId { get; set; }

        [Column("reviewee", TypeName = "bigint(20) unsigned")]
        public int RevieweeId { get; set; }

        [NotMapped]
        public User Reviewer { get; set; }

        [NotMapped]
        public User Reviewee { get; set; }
    }
}
