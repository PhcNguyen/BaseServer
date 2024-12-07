namespace NPServer.Models.Database
{
    // NOTE: These enums are saved to the database, do not remove existing values

    public enum UserRole : byte
    {
        Guests = 0,
        User = 1,
        Moderator = 2,
        Admin = 3
    }
}
