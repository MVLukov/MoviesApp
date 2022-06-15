using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesApp.Data
{
    public class Actor
    {
        [Key]
        public int ActorId { get; set; }
        
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [DisplayName("Actor's name")]
        [Column(TypeName = "varchar(50)")]
        public string ActorName { get; set; }
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}