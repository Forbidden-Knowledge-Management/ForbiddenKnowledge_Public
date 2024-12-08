using System.ComponentModel.DataAnnotations;

namespace ForbiddenKnowledge.Data
{
    public class LoginModel
    {
        [Required]
        [MaxLength(100)]
        public string Pseudonym { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

    }
}
