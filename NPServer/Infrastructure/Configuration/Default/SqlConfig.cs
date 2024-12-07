using NPServer.Infrastructure.Configuration.Abstract;

namespace NPServer.Infrastructure.Configuration.Default
{
    internal class SqlConfig : ConfigContainer
    {
        public string Server { get; private set; } = "localhost";
        public string Username { get; private set; } = "root";
        public string Password { get; private set; } = "1";
        public string DatabaseName { get; private set; } = "server";

        public bool Pooling { get; private set; } = true;
        public int MaxPoolSize { get; private set; } = 100;
        public int MinPoolSize { get; private set; } = 5;
        public int CommandTimeout { get; private set; } = 5;
    }
}