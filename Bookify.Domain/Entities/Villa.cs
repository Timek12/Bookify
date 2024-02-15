using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Domain.Entities
{
    public class Villa
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public required string Name { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        [DisplayName("Price per night")]
        [Range(1, 100000)]
        public double Price { get; set; }
        [Range(1, 10000)]
        public int Sqft { get; set; }
        [Range(1, 20)]
        public int Occupancy { get; set; }
        [DisplayName("Image Url")]
        public string? ImageUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
