using System;
using System.Linq;
using QA.Search.Data.Utils;

namespace QA.Search.Data.Models
{
    /// <summary>
    /// Роль пользователя Админки
    /// </summary>
    public enum UserRole
    {
        Admin = 1,
        User = 2,
    }

    /// <summary>
    /// Пользователь Админки
    /// </summary>
    public class User
    {
        public int Id { get; private set; }
        public byte[] PasswordHash { get; private set; }
        public byte[] Salt { get; private set; }
        public string Email { get; private set; }
        public UserRole Role { get; private set; }

        protected User() { }


        public User(string email, string password, UserRole role)
        {
            Email = email.ToLower();
            Salt = SecurityHelper.GenerateSalt();
            PasswordHash = SecurityHelper.GetPasswordHash(password, Salt);
            Role = role;
        }

        public bool ValidatePassword(string password)
        {
            var hash = SecurityHelper.GetPasswordHash(password, Salt);
            return hash.SequenceEqual(PasswordHash);
        }

        public void SetPassword(string password)
        {
            Salt = SecurityHelper.GenerateSalt();
            PasswordHash = SecurityHelper.GetPasswordHash(password, Salt);
        }
    }
}
