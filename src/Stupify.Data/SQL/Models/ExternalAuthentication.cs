using System;
using Stupify.Data.Models;

namespace Stupify.Data.SQL.Models
{
    internal class ExternalAuthentication
    {
        public int ExternalAuthenticationId { get; set; }
        public User User { get; set; }
        public ExternalService Service { get; set; }
        public DateTime LastRefreshed { get; set; }
        public string AccessTokenAes { get; set; }
        public string TokenTypeAes { get; set; }
        public string ScopeAes { get; set; }
        public string ExpiresInAes { get; set; }
        public string RefreshTokenAes { get; set; }
    }
}