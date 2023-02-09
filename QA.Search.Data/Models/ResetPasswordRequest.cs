using System;
using System.ComponentModel.DataAnnotations;

namespace QA.Search.Data.Models
{
    public class ResetPasswordRequest
    {
        public Guid Id { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        [Required]
        public User User { get; private set; }
        public bool IsActive { get; set; }

        protected ResetPasswordRequest() { }


        public ResetPasswordRequest(User user)
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            User = user;
            IsActive = true;
        }

        public void Invalidate()
        {
            IsActive = false;
        }
    }
}
