using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesApp.Data
{
    public class Director
    {
        [Key]
        public int DirectorId { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [DisplayName("Director's name")]
        [Column(TypeName = "varchar(50)")]
        public string DirectorName { get; set; }
    }
}