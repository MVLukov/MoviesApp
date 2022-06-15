using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesApp.Data
{
    public class Movie : IValidatableObject
    {
        [Key]
        public int MovieId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [DisplayName("Movie's name")]
        [Column(TypeName = "varchar(100)")]
        public string MovieName { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 2)]
        [DisplayName("Movie's synopsis")]
        [Column(TypeName = "varchar(500)")]
        public string MovieSynopsis { get; set; }

        [Required]
        [DisplayName("Movie's genre")]
        [Column(TypeName = "varchar(10)")]
        public string MovieGenre { get; set; }

        [Required]
        [DisplayName("Movie's year")]
        [Column(TypeName = "varchar(4)")]
        public string MovieYear { get; set; }

        [Required]
        [Range(0.00, 10)]
        [DisplayName("Movie's rating")]
        [Column(TypeName = "float")]
        public float MovieRating { get; set; }

        // [Required(ErrorMessage = "Movie's director is required!")]
        [Column(TypeName = "int")]
        [DisplayName("Movie's director")]
        public int? DirectorId { get; set; } = null;

        public Director Director { get; set; }

        public ICollection<Actor> Actors { get; set; } = new List<Actor>();

        [NotMapped]
        public List<int> ActorsSelect { get; set; } = new List<int>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MovieYear.Length != 4)
            {
                yield return new ValidationResult("Movie's year is not valid! (eg. 2001)", new[] { nameof(MovieYear) });
            }
        }
    }
}