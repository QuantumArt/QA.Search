using System;
using QA.Search.Data.Models;

namespace QA.Search.Admin.Models
{
    public class UsersListRequest
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Email { get; set; }
        public UserRole? Role { get; set; }
    }
}
