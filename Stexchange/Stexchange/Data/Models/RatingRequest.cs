using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stexchange.Data.Models
{
    public class RatingRequest
    {
        [Column("id", TypeName = "serial"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("reviewer", TypeName = "bigint(20) unsigned")]
        public int ReviewerId { get; set; }

        [Column("reviewee", TypeName = "bigint(20) unsigned")]
        public int RevieweeId { get; set; }

        [Column("created_at", TypeName = "datetime"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public User Reviewer { get; set; }

        [NotMapped]
        public User Reviewee { get; set; }
    }
}
