﻿using RestWithASPNETUdemy.Configurations;
using RestWithASPNETUdemy.Data.VO;
using RestWithASPNETUdemy.Model;
using RestWithASPNETUdemy.Model.Context;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RestWithASPNETUdemy.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly MySQLContext _context;

        public UserRepository(MySQLContext context)
        {
            _context = context;
        }

        public User ValidateCredentials(UserVO user)
        {
            var pass = ComputeHash(user.Password, new SHA256CryptoServiceProvider());

            return _context.Users.FirstOrDefault(u => (u.UserName == user.UserName) && (u.Password == pass));
        }
        public User ValidateCredentials(string userName)
        {
            return _context.Users.SingleOrDefault(u => (u.UserName == userName));
        }
        public bool RevokeToken(string userName)
        {
            var user = _context.Users.SingleOrDefault(u => (u.UserName == userName));
            if (user is null) return false;
            user.RefreshToken = "0";
            TokenConfiguration token = new TokenConfiguration();
            _context.SaveChanges();

            return true;
        }
        public User RefreshUserInfo(User user)
        {
            if (!_context.Users.Any(u => u.id.Equals(user.id))) return null;
            var result = _context.Users.SingleOrDefault(p => p.id.Equals(user.id));
            
            if (result != null)
            {
                try
                {
                    _context.Entry(result).CurrentValues.SetValues(user);
                    _context.SaveChanges();
                    return user;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return result;
        }

        private string ComputeHash(string input, SHA256CryptoServiceProvider algorithm)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);

            return BitConverter.ToString(hashedBytes);
        }
    }
}