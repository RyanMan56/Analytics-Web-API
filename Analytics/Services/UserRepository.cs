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

        public bool Create(string email, string name, string password, string securityQuestion, string securityAnswer)
        {
            if (UserExists(email))
            {
                return false;
            }

            User user = new User
            {
                Username = email,
                Name = name,
                Password = cryptoService.Compute(password),
                PasswordSalt = cryptoService.Salt,
                SecurityQuestion = securityQuestion,
                SecurityAnswer = cryptoService.Compute(securityAnswer),
                SecurityAnswerSalt = cryptoService.Salt
            };

            context.Users.Add(user);
            return true;
        }

        public User ValidatePassword(string email, string password)
        {
            var user = GetUser(email);
            if (user == null)
            {
                return null;
            }
            string hashed = cryptoService.Compute(password, user.PasswordSalt);
            if (!hashed.Equals(user.Password))
            {
                return null;
            }
            return user;
        }

        public User ValidateSecurityAnswer(string email, string securityAnswer)
        {
            var user = GetUser(email);
            if (user == null)
            {
                return null;
            }
            string hashed = cryptoService.Compute(securityAnswer, user.SecurityAnswerSalt);
            if (hashed != user.SecurityAnswer)
            {
                return null;
            }
            return user;
        }

        public bool UserExists(string email)
        {
            return context.Users.Any(u => u.Username == email);
        }

        public bool UserExists(int id)
        {
            return context.Users.Any(u => u.Id == id);
        }

        public User GetUser(string email)
        {
            return context.Users.Where(u => u.Username.Equals(email)).SingleOrDefault(); // OrDefault means that null is returned (for reference types) instead of an exception
        }

        public User GetUser(int id)
        {
            return context.Users.Where(u => u.Id == id).SingleOrDefault();            
        }

        public bool ResetPassword(string email, string securityAnswer, string newPassword)
        {
            var user = ValidateSecurityAnswer(email, securityAnswer);
            if (user == null)
            {
                Debug.WriteLine("User doesn't exist");
                return false;
            }
            user.Password = cryptoService.Compute(newPassword);
            user.PasswordSalt = cryptoService.Salt;
            return true;
        }

        public bool Delete(int id)
        {
            var user = GetUser(id);
            if (user == null)
            {
                return false;
            }
            context.Users.Remove(user);
            return true;
        }        
    }
}
