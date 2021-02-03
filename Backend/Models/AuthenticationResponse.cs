using System;


namespace AspTwitter.Models
{
    public class AuthenticationResponse
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }


        public AuthenticationResponse(User user, string token)
        {
            Id = user.Id;
            Username = user.Username;
            Token = token;
        }
    }
}
