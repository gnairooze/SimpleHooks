using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleTools.SimpleHooks.Assist.Models
{
    public class Credentials
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string IdentityServerUrl { get; set; } = string.Empty;
    }
}
