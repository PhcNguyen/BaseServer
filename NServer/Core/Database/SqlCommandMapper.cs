using System;
using System.Collections.Generic;

namespace NServer.Core.Database
{
    public static class SqlCommandMapper
    {
        private static readonly Dictionary<SqlCommand, string> SqlQueries = new()
        {
            { SqlCommand.INSERT_ACCOUNT, "INSERT INTO account (email, password) VALUES (@params0, @params1)" },
            { SqlCommand.UPDATE_ACCOUNT_PASSWORD, "UPDATE account SET password = @params1 WHERE email = @params0" },
            { SqlCommand.UPDATE_ACCOUNT_ACTIVE, "UPDATE account SET active = @params1 WHERE email = @params0" },
            { SqlCommand.UPDATE_LAST_LOGIN, "UPDATE account SET last_login = NOW() WHERE email = @params0"},
            { SqlCommand.SELECT_ACCOUNT, "SELECT * FROM account WHERE email = @params0" },
            { SqlCommand.SELECT_ACCOUNT_COUNT, "SELECT COUNT(*) FROM account WHERE email = @param0" },
            { SqlCommand.SELECT_ACCOUNT_PASSWORD, "SELECT password FROM account WHERE email = @param0" },
            { SqlCommand.SELECT_LAST_LOGIN, "SELECT last_login FROM account WHERE email = @params0" },
            { SqlCommand.DELETE_ACCOUNT, "DELETE FROM account WHERE email = @params0" },
        };

        public static string Get(SqlCommand command)
        {
            if (!SqlQueries.TryGetValue(command, out var query))
            {
                throw new ArgumentOutOfRangeException(nameof(command), $"No SQL query defined for {command}. Please add it to the SqlQueries dictionary.");
            }

            return query;
        }
    }
}