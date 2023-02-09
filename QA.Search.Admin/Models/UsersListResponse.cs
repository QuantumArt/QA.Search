using System.Collections.Generic;

namespace QA.Search.Admin.Models
{
    public class UsersListResponse
    {
        public int TotalCount { get; set; }
        public ICollection<UserResponse> Data { get; set; }
    }
}
