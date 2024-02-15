using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Domain.Entities
{
    public class VillaNumber
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Villa Number")]
        public int Villa_Number { get; set; }

        [ForeignKey("Villa")]
        [DisplayName("Villa Id")]
        public int VillaId { get; set; }
        public Villa Villa { get; set; }

        public string? SpecialDetails { get; set; }
    }
}
