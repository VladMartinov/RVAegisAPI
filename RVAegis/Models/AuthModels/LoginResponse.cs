﻿using RVAegis.Models.UserModels;

namespace RVAegis.Models.AuthModels
{
    public class LoginResponse
    {
        public bool IsLoggedIn { get; set; } = false;
        public string? JwtToken { get; set; }
        public string? RefreshToken { get; set; }
        public User? User { get; set; }
    }
}
