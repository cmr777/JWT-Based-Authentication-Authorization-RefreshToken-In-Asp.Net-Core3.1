using System.Text.Json.Serialization;
using WebApi.Entities;

namespace WebApi.Models
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public AuthenticateResponse(UserMaster user, string accessToken, string refreshToken)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.Username;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}