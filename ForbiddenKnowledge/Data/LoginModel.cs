using System.ComponentModel.DataAnnotations;

namespace ForbiddenKnowledge.Data
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Please enter your pseudonym")]
        [MaxLength(100)]
        public string Pseudonym { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password")]
        public string Password { get; set; } = string.Empty;

        public string UppercasedPseudonym => Pseudonym.ToUpperInvariant();

    }
}
