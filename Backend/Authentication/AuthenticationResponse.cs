using AspTwitter.Models;


namespace AspTwitter.Authentication
{
    public class AuthenticationResponse
    {
        public int Id { get; set; }
        public string Token { get; set; }


        public AuthenticationResponse(User user, string token)
        {
            Id = user.Id;
            Token = token;
        }

        public AuthenticationResponse() { }
    }
}
