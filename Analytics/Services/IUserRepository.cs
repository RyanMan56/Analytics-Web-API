using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IUserRepository
    {
        bool Create(string email, string name, string password, string securityQuestion, string securityAnswer);
        User ValidatePassword(string email, string password);
        User ValidateSecurityAnswer(string email, string securityAnswer);
        bool UserExists(string email);
        User GetUser(string email);
        User GetUser(int id);
        bool ResetPassword(string email, string securityAnswer, string newPassword);
        bool Delete(int id);
        bool Save();
    }
}
