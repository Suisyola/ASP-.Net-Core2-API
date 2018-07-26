using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Entities
{
    public class PointOfInterest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        // The foreign key of City will be CityId. It is Ok to leave out 
        // this ForeignKey data annotation as EF can implicitly determine 
        // CityId as the foreign key.
        [ForeignKey("CityId")]
        public City City { get; set; }

        public int CityId { get; set; }
    }
}
