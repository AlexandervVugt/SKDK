using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Data.Models
{
    public class Message
    {
        [Column("id", TypeName = "serial"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("chat_id", TypeName = "bigint(20) unsigned")]
        public int ChatId { get; set; }

        [Column("content", TypeName = "varchar(1024)")]
        public string Content { get; set; }

        [Column("sender", TypeName = "bigint(20) unsigned")]
        public int SenderId { get; set; }

        [Column("created_at", TypeName = "timestamp"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Timestamp { get; set; }

        [NotMapped]
        public User Sender { get; set; }

    }
}
