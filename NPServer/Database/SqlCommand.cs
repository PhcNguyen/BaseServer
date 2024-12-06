namespace NPServer.Database
{
    public enum SqlCommand
    {
        INSERT_ACCOUNT,
        DELETE_ACCOUNT,
        SELECT_ACCOUNT,

        SELECT_ACCOUNT_COUNT,
        SELECT_ACCOUNT_PASSWORD,
        UPDATE_ACCOUNT_PASSWORD,
        UPDATE_ACCOUNT_ACTIVE,
        UPDATE_LAST_LOGIN,
        SELECT_LAST_LOGIN,
    }
}