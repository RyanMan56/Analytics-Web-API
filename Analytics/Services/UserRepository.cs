using Analytics.Entities;
using SimpleCrypto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private ICryptoService cryptoService;

        public UserRepository(AnalyticsContext context)
        {
            this.context = context;
            cryptoService = new PBKDF2();
        }

        public bool Create(string email, string name, string password)
        {
            if (UserExists(email))
            {
                return false;
            }
            var cryptoPassword = cryptoService.Compute(password);
            var cryptoSalt = cryptoService.Salt;

            Debug.WriteLine(cryptoPassword);
            Debug.WriteLine(cryptoSalt);
            
            User user = new User
            {
                Email = email,
                Name = name,
                Password = cryptoPassword,
                PasswordSalt = cryptoSalt
            };

            context.Users.Add(user);
            return true;
        }

        public User ValidatePassword(string email, string password)
        {
            var user = GetUserByEmail(email);
            Debug.WriteLine(user);
            string hashed = cryptoService.Compute(password, user.PasswordSalt);
            return user;
        }

        public bool UserExists(string email)
        {
            return context.Users.Any(u => u.Email == email);
        }

        public User GetUserByEmail(string email)
        {
            var user = context.Users.Where(u => u.Email.Equals(email)).SingleOrDefault(); // OrDefault means that null is returned (for reference types) instead of an exception
            Debug.WriteLine(user);
            return user;
        }
    }
}
