using System;
using System.Threading.Tasks;
using DatingApp.api.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.api.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _dbContext;
        public AuthRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> Login(string userName, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(x=>x.UserName == userName);

            if(user == null)
                return null;

            if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)){
                    var computePasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                    for(var i =0 ; i < computePasswordHash.Length; i++)
                    {
                        if(computePasswordHash[i] != passwordHash[i])
                            return false;
                    }
            }
            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512){
                    passordSalt = hmac.Key;
                    passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExist(string userName)
        {
            if(await _dbContext.Users.AnyAsync(x=> x.UserName == userName))
                return true;
            
            return false;
            
        }
    }
}