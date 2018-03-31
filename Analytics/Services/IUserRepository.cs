using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IUserRepository
    {
        bool Create(string email, string name, string password);
        User ValidatePassword(string email, string password);
        bool UserExists(string email);
        User GetUserByEmail(string email);
        bool Save();
    }
}
