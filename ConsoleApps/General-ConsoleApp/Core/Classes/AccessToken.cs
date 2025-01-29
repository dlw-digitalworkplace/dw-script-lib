namespace Core.Classes
{
    public class AccessToken
    {
        public AccessToken(string token, DateTimeOffset expiresOn)
        {
            Token = token;
            ExpiresOn = expiresOn;
        }

        public string Token { get; }

        public DateTimeOffset ExpiresOn { get; }
    }
}
