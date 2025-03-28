using Microsoft.AspNetCore.Identity;
using System;

namespace Common.PasswordManager
{
    internal interface IPasswordManager
    {
        string HashPassword(string password);
        bool ValidatePassword(string password, string hashedPassword);
    }
}
