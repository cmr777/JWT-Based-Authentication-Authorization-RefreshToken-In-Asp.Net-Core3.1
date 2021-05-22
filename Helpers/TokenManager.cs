using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Helpers;

namespace APIAuth.Helpers
{
    public interface ITokenManager
    {
        string generateAccessToken(UserMaster user);
        RefreshToken generateRefreshToken(string ipAddress);
    }

    public class TokenManager : ITokenManager
    {
        private readonly AppSettings _appSettings;

        public TokenManager(
            IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public string generateAccessToken(UserMaster user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.Now.AddMinutes(Convert.ToDouble(_appSettings.AccessTokenExpiration)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public RefreshToken generateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.Now.AddMinutes(Convert.ToDouble(_appSettings.RefreshTokenExpiration)),
                    Created = DateTime.Now,
                    CreatedByIp = ipAddress
                };
            }
        }
    }
}
