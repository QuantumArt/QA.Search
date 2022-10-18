using System;
using QA.Search.Data.Models;

namespace QA.Search.Admin.Models
{
    public class CreateUserRequest
    {
        public string Email { get; set; }
        public UserRole Role { get; set; }
    }
}
