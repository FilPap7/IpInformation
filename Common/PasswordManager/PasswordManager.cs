using System;
using Microsoft.AspNetCore.Identity;

namespace Common.PasswordManager
{
    public class PasswordManager : IPasswordManager
    {
        private readonly PasswordHasher<object> _passwordHasher = new();

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }

        public bool ValidatePassword(string password, string hashedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
