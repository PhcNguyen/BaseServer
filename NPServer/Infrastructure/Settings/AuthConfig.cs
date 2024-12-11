using NPServer.Infrastructure.Config;

namespace NPServer.Infrastructure.Settings
{
    public class AuthConfig : ConfigContainer
    {
        public string Address { get; private set; } = "localhost";
        public string Port { get; private set; } = "8080";
        public bool EnableWebApi { get; private set; } = true;
    }
}
