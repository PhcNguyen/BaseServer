namespace NPServer.Database.Factory
{
    public class DbContextFactory
    {
        private static DbContextFactory? _instance;

        public static DbContextFactory GetInstance()
        {
            _instance ??= new DbContextFactory();

            return _instance;
        }
    }
}
