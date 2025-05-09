using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserNamespace = WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.Modules.CohortModule.Models
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public required string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("Cohort")]
        public Guid CohortId { get; set; }
        public required Cohort Cohort { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public required UserNamespace.ApplicationUser User { get; set; }
    }
}

