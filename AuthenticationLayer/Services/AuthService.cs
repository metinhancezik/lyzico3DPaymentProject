﻿using AuthenticationLayer.Interfaces;
using AuthenticationLayer.Models;
using System.Security.Cryptography;
using System.Text;
using ServiceLayer.Abstract;
using EntityLayer.Concrete;
using DataAccesLayer.Abstract;
using AuthenticationLayer.Services.AuthenticationLayer.Services;

namespace AuthenticationLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserAuthService _userAuthService;
        private readonly IAuthTokensService _authTokensService;
        private readonly IUserDetailService _userDetailService;


        public AuthService(IUserAuthService userAuthService, IAuthTokensService authTokensService, IUserDetailService userDetailService)
        {
            _userAuthService = userAuthService;
            _authTokensService = authTokensService;
            _userDetailService = userDetailService;
        }


        public async Task<(bool Success, string Message)> RegisterUserAsync(UserRegistrationModel model)
        {
            
            if (_userDetailService.GetList().Any(u => u.UserMail == model.UserMail))
            {
                return (false, "E-posta adresi zaten kullanımda");
            }

         
            var userDetails = new UserDetails
            {
                UserName = model.UserName,
                UserSurname = model.UserSurname,
                UserMail = model.UserMail,
                UserPhone = model.UserPhone,
                CountryId = model.CountryId,
                CreatedTime = DateTime.UtcNow,
                UpdatedTime = DateTime.UtcNow,
                IsActive = true
            };

            _userDetailService.TAdd(userDetails);

          
            var userAuth = new UserAuth
            {
                UserId = userDetails.Id,
                PasswordHash = PasswordHasher.HashPassword(model.Password),
                LastLoginTime = DateTime.UtcNow,
                FailedLoginAttempts = 0,
                IsLocked = false,
                EmailConfirmed = false,
                PhoneConfirmed = false,
                LockoutEndTime = DateTime.MinValue
            };

            _userAuthService.TAdd(userAuth);

            var token = await GenerateTokenAsync(userDetails.Id);

            return (true, "Kullanıcı başarıyla kaydedildi.");
        }
        public async Task<LoginResult> ValidateCredentialsAsync(string email, string password)
        {
            var user = _userAuthService.GetList().FirstOrDefault(u => u.User.UserMail == email);
            if (user == null) return new LoginResult { Success = false };

            if (PasswordHasher.VerifyPassword(user.PasswordHash, password))
            {
                var token = await GenerateTokenAsync(user.UserId);
                return new LoginResult { Success = true, Token = token.AccessToken };
            }

            return new LoginResult { Success = false };
        }

        public async Task<TokenModel> GenerateTokenAsync(long userId)
        {
            var token = new TokenModel
            {
                AccessToken = GenerateRandomToken(),
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            var authToken = new AuthTokens
            {
                UserId = userId,
                AccessToken = token.AccessToken,
                ExpiresAt = token.ExpiresAt,
                CreatedAt = DateTime.UtcNow,
                Revoked = false
            };

            _authTokensService.TAdd(authToken);

            return token;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var authToken = _authTokensService.GetList().FirstOrDefault(t => t.AccessToken == token);
            if (authToken == null) return false;

            authToken.Revoked = true;
            _authTokensService.TUpdate(authToken);
            return true;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var authToken = _authTokensService.GetList().FirstOrDefault(t => t.AccessToken == token);
            return authToken != null && !authToken.Revoked && authToken.ExpiresAt > DateTime.UtcNow;
        }

        private string GenerateRandomToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }
    }
}