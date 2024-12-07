namespace NPServer.Models.Database
{
    [System.Flags]
    public enum AccountFlags
    {
        None = 0,
        IsBanned = 1 << 0,
        IsArchived = 1 << 1,
        IsPasswordExpired = 1 << 2,
        LinuxCompatibilityMode = 1 << 3,   // Disables session token verification
    }
}
