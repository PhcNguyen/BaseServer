using NPServer.Infrastructure.Configuration;
using NPServer.Infrastructure.Helper;
using NPServer.Infrastructure.Logging;
using NPServer.Infrastructure.Services.Time;
using NPServer.Models.Database;
using System;
using System.IO;
using System.Text.Json;

namespace NPServer.DatabaseAccess.Json
{
    /// <summary>
    /// Provides functionality for storing a single <see cref="DBAccount"/> instance in a JSON file using the <see cref="IDBManager"/> interface.
    /// </summary>
    public class JsonDBManager : IDBManager
    {
        private string? _accountFilePath;
        private DBAccount? _account;
        private JsonSerializerOptions? _jsonOptions;

        private int _maxBackupNumber;
        private TimeSpan _backupInterval;
        private TimeSpan _lastBackupTime;

        public static JsonDBManager Instance { get; } = new();

        public bool VerifyAccounts { get => false; }

        private JsonDBManager()
        { }

        public bool Initialize()
        {
            var config = ConfigManager.Instance.GetConfig<JsonDBManagerConfig>();
            _accountFilePath = Path.Combine(FileHelper.DataDirectory, config.FileName);

            _jsonOptions = new();
            _jsonOptions.Converters.Add(new DBEntityCollectionJsonConverter());

            if (File.Exists(_accountFilePath))
            {
                NPLog.Instance.Info<DBAccount>($"Found existing account file {FileHelper.GetRelativePath(_accountFilePath)}");

                try
                {
                    _account = FileHelper.DeserializeJson<DBAccount>(_accountFilePath, _jsonOptions);
                }
                catch (Exception e)
                {
                    NPLog.Instance.Error<DBAccount>($"Initialize(): Failed to load existing account data, resetting", e);
                }
            }

            if (_account == null)
            {
                // Initialize a new default account from config
                _account = new(config.PlayerName);
                _account.Player = new(_account.Id);

                NPLog.Instance.Info<DBAccount>($"Initialized default account {_account}");
            }
            else
            {
                _account.PlayerName = config.PlayerName;
                NPLog.Instance.Info<DBAccount>($"Loaded default account {_account}");
            }

            _maxBackupNumber = config.MaxBackupNumber;
            _backupInterval = TimeSpan.FromMinutes(config.BackupIntervalMinutes);
            _lastBackupTime = Clock.GameTime;

            return _account != null;
        }

        public bool TryQueryAccountByEmail(string email, out DBAccount account)
        {
            account = _account!;
            return true;
        }

        public bool QueryIsPlayerNameTaken(string playerName)
        {
            NPLog.Instance.Warning<DBAccount>("QueryIsPlayerNameTaken(): Operation not supported");
            return true;
        }

        public bool InsertAccount(DBAccount account)
        {
            NPLog.Instance.Warning<DBAccount>("InsertAccount(): Operation not supported");
            return false;
        }

        public bool UpdateAccount(DBAccount account)
        {
            NPLog.Instance.Warning<DBAccount>("UpdateAccount(): Operation not supported");
            return false;
        }

        public bool LoadPlayerData(DBAccount account)
        {
            // All JSON data is loaded at once (FIXME)
            return true;
        }

        public bool SavePlayerData(DBAccount account)
        {
            if (account != _account)
            {
                NPLog.Instance.Warning<DBAccount>("UpdateAccountData(): Attempting to update non-default account when bypass auth is enabled");
                return false;
            }

            NPLog.Instance.Info<DBAccount>($"Updated account file {FileHelper.GetRelativePath(_accountFilePath!)}");
            FileHelper.SerializeJson(_accountFilePath!, _account, _jsonOptions);

            TryCreateBackup();

            return true;
        }

        /// <summary>
        /// Creates a backup of the account file if enough time has passed since the last one.
        /// </summary>
        private void TryCreateBackup()
        {
            TimeSpan now = Clock.GameTime;

            if (now - _lastBackupTime < _backupInterval)
                return;

            if (FileHelper.CreateFileBackup(_accountFilePath!, _maxBackupNumber))
                NPLog.Instance.Info<DBAccount>("Created account file backup");

            _lastBackupTime = now;
        }
    }
}