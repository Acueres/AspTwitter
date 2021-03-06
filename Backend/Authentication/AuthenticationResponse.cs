﻿using AspTwitter.Models;


namespace AspTwitter.Authentication
{
    public class AuthenticationResponse
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string About { get; set; }
        public string Token { get; set; }


        public AuthenticationResponse(User user, string token)
        {
            Id = user.Id;
            Name = user.Name;
            Username = user.Username;
            About = user.About;
            Token = token;
        }

        public AuthenticationResponse() { }
    }
}
