using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

using WebApi.Models;
using WebApi.Entities;
using WebApi.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using APIAuth.Helpers;

namespace WebApi.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
        AuthenticateResponse RefreshToken(string token, string ipAddress);
        bool RevokeToken(string token, string ipAddress);
        IEnumerable<UserMaster> GetAll();
        UserMaster GetById(int id);
    }

    public class UserService : IUserService
    {
        private DataContext _context;
        private ITokenManager _tokenManager;
        private readonly AppSettings _appSettings;

        public UserService(
            DataContext context,
            IOptions<AppSettings> appSettings,ITokenManager tokenManager)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _tokenManager = tokenManager;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            var user =  _context.UserMaster.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt and refresh tokens
            var accessToken = _tokenManager.generateAccessToken(user);
            var refreshToken = _tokenManager.generateRefreshToken(ipAddress);
            refreshToken.Token = System.Web.HttpUtility.UrlEncode(refreshToken.Token);
            // save refresh token
            user.RefreshTokens = new List<RefreshToken>();
            user.RefreshTokens.Add(refreshToken);
            _context.Update(user);
            _context.SaveChanges();

            return new AuthenticateResponse(user, accessToken, refreshToken.Token);
        }

        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var refreshToken = _context.RefreshToken.SingleOrDefault(x => x.Token == token);

            if (refreshToken == null) return null;

             var user = _context.UserMaster.SingleOrDefault(x => x.Id == refreshToken.UserMasterId);
            // return null if token is no longer active
            if (!refreshToken.IsActive) return null;

            // replace old refresh token with a new one and save
            var newRefreshToken = _tokenManager.generateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.Now;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            _context.Update(user);
            _context.SaveChanges();

            // generate new jwt token
            var accessToken = _tokenManager.generateAccessToken(user);

            return new AuthenticateResponse(user, accessToken, newRefreshToken.Token);
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var refreshToken = _context.RefreshToken.SingleOrDefault(x => x.Token == token);

            if (refreshToken == null) return false;

            var user = _context.UserMaster.SingleOrDefault(x => x.Id == refreshToken.UserMasterId);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke jwt token and save
            refreshToken.Revoked = DateTime.Now;
            refreshToken.RevokedByIp = ipAddress;
            _context.Update(user);
            _context.SaveChanges();

            return true;
        }

        public IEnumerable<UserMaster> GetAll()
        {
            return _context.UserMaster;
        }

        public UserMaster GetById(int id)
        {
            return _context.UserMaster.FirstOrDefault(o=>o.Id==id);
        }
    }
}