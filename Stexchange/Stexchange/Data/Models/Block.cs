using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Data.Models
{
    public class Block
    {
        [Column("blocked_id", TypeName = "bigint(20) unsigned")]
        public int BlockedId { get; set; }

        [Column("blocker_id", TypeName = "bigint(20) unsigned")]
        public int BlockerId { get; set; }

        [NotMapped]
        public User Blocked { get; set; }

        [NotMapped]
        public User Blocker { get; set; }
    }
}
