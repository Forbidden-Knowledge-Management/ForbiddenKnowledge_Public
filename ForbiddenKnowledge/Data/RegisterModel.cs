using System.ComponentModel.DataAnnotations;

namespace ForbiddenKnowledge.Data
{
    public class RegisterModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(12)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Pseudonym { get; set; } = string.Empty;

        public string UppercasedPseudonym => Pseudonym.ToUpperInvariant();

    }
}
