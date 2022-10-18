using System;
using QA.Search.Data.Models;

namespace QA.Search.Admin.Models
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
    }
}
