using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Data.Models
{
    public class User
    {
        [Column("id", TypeName = "serial"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [EmailAddress, Column("email", TypeName = "varchar(254)")]
        public string Email { get; set; }
        [Column("username", TypeName = "varchar(15)")]
        public string Username { get; set; }
        [Column("postal_code", TypeName = "char(6)")]
        public string Postal_Code { get; set; }
        [Column("password", TypeName = "varbinary(64)")]
        public byte[] Password { get; set; }
        [Column("created_at", TypeName = "datetime"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Created_At { get; set; }
        [Column("verified", TypeName = "tinyint(1)")]
        public bool IsVerified { get; set; }
        [NotMapped]
        public UserVerification Verification { get; set; }
    }
}
