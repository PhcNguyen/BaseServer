﻿using NPServer.Core.Helpers;
using NPServer.Infrastructure.Services;
using System;

namespace NPServer.Database.Models
{
    // NOTE: These enums are saved to the database, do not remove existing values

    public enum AccountUserLevel : byte
    {
        User = 0,
        Moderator = 1,
        Admin = 2
    }

    [Flags]
    public enum AccountFlags
    {
        None = 0,
        IsBanned = 1 << 0,
        IsArchived = 1 << 1,
        IsPasswordExpired = 1 << 2,
        LinuxCompatibilityMode = 1 << 3,   // Disables session token verification
    }

    /// <summary>
    /// Represents an account stored in the account database.
    /// </summary>
    public class DBAccount
    {
        private static readonly IdGenerator IdGenerator = new(IdType.Player, 0);

        public long Id { get; set; }
        public string Email { get; set; }
        public string PlayerName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] Salt { get; set; }
        public AccountUserLevel UserLevel { get; set; }
        public AccountFlags Flags { get; set; }

        public DBPlayer Player { get; set; }

        // NOTE: init is required for collection properties to be compatible with JSON serialization
        public DBEntityCollection Avatars { get; init; } = new();

        public DBEntityCollection TeamUps { get; init; } = new();
        public DBEntityCollection Items { get; init; } = new();
        public DBEntityCollection ControlledEntities { get; init; } = new();

        /// <summary>
        /// Constructs an empty <see cref="DBAccount"/> instance.
        /// </summary>
        public DBAccount()
        { }

        /// <summary>
        /// Constructs a <see cref="DBAccount"/> instance with the provided data.
        /// </summary>
        public DBAccount(string email, string playerName, string password, AccountUserLevel userLevel = AccountUserLevel.User)
        {
            Id = (long)IdGenerator.Generate();
            Email = email;
            PlayerName = playerName;
            PasswordHash = CryptographyHelper.HashPassword(password, out byte[] salt);
            Salt = salt;
            UserLevel = userLevel;
        }

        /// <summary>
        /// Constructs a default <see cref="DBAccount"/> instance with the provided data.
        /// </summary>
        public DBAccount(string playerName)
        {
            // Default account is used when BypassAuth is enabled
            Id = 0x2000000000000001;
            Email = "default@mhserveremu";
            PlayerName = playerName;
            PasswordHash = [];
            Salt = [];
            UserLevel = AccountUserLevel.Admin;
        }

        public override string ToString()
        {
            return $"{PlayerName} (0x{Id:X})";
        }

        public void ClearEntities()
        {
            Avatars.Clear();
            TeamUps.Clear();
            Items.Clear();
            ControlledEntities.Clear();
        }
    }
}