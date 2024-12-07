﻿using NPServer.Database.Models;

namespace NPServer.Database
{
    /// <summary>
    /// Common interface for <see cref="DBAccount"/> storage implementations.
    /// </summary>
    public interface IDBManager
    {
        /// <summary>
        /// Set this to false to disable password and flag verification for accounts.
        /// </summary>
        public bool VerifyAccounts { get => true; }

        /// <summary>
        /// Initializes database connection.
        /// </summary>
        public bool Initialize();

        /// <summary>
        /// Queries a <see cref="DBAccount"/> from the database by its email.
        /// </summary>
        public bool TryQueryAccountByEmail(string email, out DBAccount account);

        /// <summary>
        /// Queries if the specified player name is already taken.
        /// </summary>
        public bool QueryIsPlayerNameTaken(string playerName);

        /// <summary>
        /// Inserts a new <see cref="DBAccount"/> with all of its data into the database.
        /// </summary>
        public bool InsertAccount(DBAccount account);

        /// <summary>
        /// Updates the Account table in the database with the provided <see cref="DBAccount"/>.
        /// </summary>
        public bool UpdateAccount(DBAccount account);

        /// <summary>
        /// Loads persistent game data stored in the database for the provided <see cref="DBAccount"/>.
        /// </summary>
        public bool LoadPlayerData(DBAccount account);

        /// <summary>
        /// Saves persistent game data stored in the database for the provided <see cref="DBAccount"/>.
        /// </summary>
        public bool SavePlayerData(DBAccount account);
    }
}