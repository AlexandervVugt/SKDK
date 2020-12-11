using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Data.Models
{
    public class Chat
    {
        [Column("id", TypeName = "serial"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("ad_id", TypeName = "bigint(20) unsigned")]
        public int AdId { get; set; }

        [Column("responder_id", TypeName = "bigint(20) unsigned")]
        public int ResponderId { get; set; }

        [NotMapped]
        public User Responder { get; set; }

        [NotMapped]
        public User Poster { get; set; }

        [NotMapped]
        public Listing Listing { get; set; }
        
        [NotMapped]
        public List<Message> Messages { get; set; }
    }
}
