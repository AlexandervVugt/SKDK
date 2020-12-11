using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Data.Models
{
    public class ImageData
    {
        [Column("id", TypeName = "serial"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Column("listing_id", TypeName = "bigint(20) unsigned")]
        public int ListingId { get; set; }

        [Column("image", TypeName = "LONGBLOB"), Required]
        public byte[] Image { get; set; }

        public Listing Listing { get; set; }

        public string GetImage()
        {
            // Convert byte arry to base64string   
            string base64string = Convert.ToBase64String(Image);
            return string.Format("data:image/png;base64,{0}", base64string);
        }
    }
}
