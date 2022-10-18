using System;
using System.ComponentModel.DataAnnotations;

namespace QA.Search.Admin.Models
{
    public class ChangePasswordRequest
    {
        [Required]
        public Guid EmailId { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
