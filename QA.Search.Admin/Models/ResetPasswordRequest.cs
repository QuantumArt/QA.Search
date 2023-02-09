using System.ComponentModel.DataAnnotations;

namespace QA.Search.Admin.Models
{
    public class ResetPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
