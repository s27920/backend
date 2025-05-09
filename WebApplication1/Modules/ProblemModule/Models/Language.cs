using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Modules.ProblemModule.Models
{
    public class Language
    {
        [Key]
        public Guid LanguageId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(256)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(256)]
        public required string Version { get; set; }
        
        public ICollection<UserSolution> UserSolutions { get; set; } = new List<UserSolution>();
    }
}