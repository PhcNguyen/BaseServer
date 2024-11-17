namespace NETServer.Database
{
    internal class PostgreConfig
    {
        public readonly static string Host = "192.168.1.11";
        public readonly static string Username = "ROOT";
        public readonly static string Password = "APNxH8x5a";
        public readonly static string DatabaseName = "Server";
        public readonly static string ConnectionString = $"Host={Host};Username=postgres;Password={Password};Database={DatabaseName};Pooling=true;Max Pool Size=100;Min Pool Size=10;CommandTimeout=10;";

        public readonly static string AccountTableSchema = @"
        CREATE TABLE IF NOT EXISTS account (
            id SERIAL PRIMARY KEY,
            email VARCHAR(255) UNIQUE NOT NULL,
            password VARCHAR(255) NOT NULL,
            ban BOOLEAN DEFAULT FALSE,
            role BOOLEAN DEFAULT FALSE,
            active BOOLEAN DEFAULT FALSE,
            last_login TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        );";


    }
}
