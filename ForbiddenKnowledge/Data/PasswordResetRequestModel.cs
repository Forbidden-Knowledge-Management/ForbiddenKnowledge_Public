using System.ComponentModel.DataAnnotations;

namespace ForbiddenKnowledge.Data
{
    public class PasswordResetRequestModel
    {
        [MaxLength(256)]
        public string PseudonymOrEmail { get; set; } = string.Empty;

    }
}
