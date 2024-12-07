using NPServer.Database.Models;
using NPServer.Infrastructure.Logging;
using System;
using System.IO;
using System.Text;

namespace NPServer.Database.Json
{
    /// <summary>
    /// Experimental binary serializer for <see cref="DBAccount"/>. Stores data more efficiently than JSON.
    /// </summary>
    public static class DBAccountBinarySerializer
    {
        private const string Magic = "534156";  // SAV
        private const byte SerializerVersion = 1;

        public static void SerializeToFile(string path, DBAccount dbAccount)
        {
            using FileStream fs = new(path, FileMode.Create);
            Serialize(fs, dbAccount);
        }

        public static DBAccount? DeserializeFromFile(string path)
        {
            try
            {
                using FileStream fs = new(path, FileMode.Open);
                return Deserialize(fs);
            }
            catch (Exception e)
            {
                NPLog.Instance.Error<DBAccount>($"DeserializeFromFile(): Exception occured, path={path}", e);
                return null;
            }
        }

        public static void Serialize(Stream stream, DBAccount dbAccount)
        {
            using (BinaryWriter writer = new(stream))
            {
                WriteFileHeader(writer);

                writer.Write(dbAccount.Id);
                WriteString(writer, dbAccount.Email);
                WriteString(writer, dbAccount.PlayerName);
                WriteByteArray(writer, dbAccount.PasswordHash);
                WriteByteArray(writer, dbAccount.Salt);
                writer.Write((byte)dbAccount.UserLevel);
                writer.Write((int)dbAccount.Flags);

                WriteDBPlayer(writer, dbAccount.Player);

                WriteDBEntityCollection(writer, dbAccount.Avatars);
                WriteDBEntityCollection(writer, dbAccount.TeamUps);
                WriteDBEntityCollection(writer, dbAccount.Items);
                WriteDBEntityCollection(writer, dbAccount.ControlledEntities);
            }
        }

        public static DBAccount? Deserialize(Stream stream)
        {
            try
            {
                using BinaryReader reader = new(stream);
                if (!ReadFileHeader(reader))
                {
                    NPLog.Instance.Error<DBAccount>("Deserialize(): File header error");
                    return null;
                }

                DBAccount dbAccount = new()
                {
                    Id = reader.ReadInt64(),
                    Email = ReadString(reader),
                    PlayerName = ReadString(reader),
                    PasswordHash = ReadByteArray(reader),
                    Salt = ReadByteArray(reader),
                    UserLevel = (AccountUserLevel)reader.ReadByte(),
                    Flags = (AccountFlags)reader.ReadInt32(),

                    Player = ReadDBPlayer(reader)
                };

                ReadDBEntityCollection(reader, dbAccount.Avatars);
                ReadDBEntityCollection(reader, dbAccount.TeamUps);
                ReadDBEntityCollection(reader, dbAccount.Items);
                ReadDBEntityCollection(reader, dbAccount.ControlledEntities);

                return dbAccount;
            }
            catch (Exception e)
            {
                NPLog.Instance.Error<DBAccount>("Deserialize(): Exception occured", e);
                return null;
            }
        }

        private static void WriteFileHeader(BinaryWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes(Magic));  // Chuyển Magic thành byte[]
            writer.Write(SerializerVersion);
        }

        private static bool ReadFileHeader(BinaryReader reader)
        {
            byte[] magicBytes = reader.ReadBytes(3);  // Đọc đúng 3 byte
            string magic = Encoding.ASCII.GetString(magicBytes);  // Chuyển đổi thành chuỗi

            if (magic != Magic)
            {
                NPLog.Instance.Warning<DBAccount>($"ReadFileHeader(): Invalid file header - Expected {Magic}, but found {magic}");
                return false;
            }

            byte version = reader.ReadByte();
            if (version != SerializerVersion)
            {
                NPLog.Instance.Warning<DBAccount>($"ReadFileHeader(): Unsupported file version (found {version}, expected {SerializerVersion})");
                return false;
            }

            return true;
        }

        private static void WriteByteArray(BinaryWriter writer, byte[] array)
        {
            if (array.Length > ushort.MaxValue) throw new OverflowException("Byte array size exceeds the allowed limit.");

            writer.Write((ushort)array.Length);
            writer.Write(array);
        }

        private static byte[] ReadByteArray(BinaryReader reader)
        {
            int length = reader.ReadUInt16();
            return reader.ReadBytes(length);
        }

        private static void WriteString(BinaryWriter writer, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteByteArray(writer, bytes);
        }

        private static string ReadString(BinaryReader reader)
        {
            byte[] bytes = ReadByteArray(reader);
            return Encoding.UTF8.GetString(bytes);
        }

        private static void WriteDBPlayer(BinaryWriter writer, DBPlayer dbPlayer)
        {
            writer.Write(dbPlayer.DbGuid);
            WriteByteArray(writer, dbPlayer.ArchiveData);
            writer.Write(dbPlayer.StartTarget);
            writer.Write(dbPlayer.StartTargetRegionOverride);
            writer.Write(dbPlayer.AOIVolume);
        }

        private static DBPlayer ReadDBPlayer(BinaryReader reader)
        {
            DBPlayer dbPlayer = new()
            {
                DbGuid = reader.ReadInt64(),
                ArchiveData = ReadByteArray(reader),
                StartTarget = reader.ReadInt64(),
                StartTargetRegionOverride = reader.ReadInt64(),
                AOIVolume = reader.ReadInt32()
            };
            return dbPlayer;
        }

        private static void WriteDBEntityCollection(BinaryWriter writer, DBEntityCollection dbEntityCollection)
        {
            if (dbEntityCollection.Count > ushort.MaxValue) throw new OverflowException("Entity collection size exceeds the allowed limit.");

            writer.Write((ushort)dbEntityCollection.Count);
            foreach (DBEntity dbEntity in dbEntityCollection)
                WriteDBEntity(writer, dbEntity);
        }

        private static void ReadDBEntityCollection(BinaryReader reader, DBEntityCollection dbEntityCollection)
        {
            dbEntityCollection.Clear();

            int numEntries = reader.ReadUInt16();
            for (int i = 0; i < numEntries; i++)
            {
                DBEntity dbEntity = ReadDBEntity(reader);
                dbEntityCollection.Add(dbEntity);
            }
        }

        private static void WriteDBEntity(BinaryWriter writer, DBEntity dbEntity)
        {
            writer.Write(dbEntity.DbGuid);
            writer.Write(dbEntity.ContainerDbGuid);
            writer.Write(dbEntity.InventoryProtoGuid);
            writer.Write(dbEntity.Slot);
            writer.Write(dbEntity.EntityProtoGuid);
            WriteByteArray(writer, dbEntity.ArchiveData);
        }

        private static DBEntity ReadDBEntity(BinaryReader reader)
        {
            DBEntity dbEntity = new()
            {
                DbGuid = reader.ReadInt64(),
                ContainerDbGuid = reader.ReadInt64(),
                InventoryProtoGuid = reader.ReadInt64(),
                Slot = reader.ReadUInt32(),
                EntityProtoGuid = reader.ReadInt64(),
                ArchiveData = ReadByteArray(reader)
            };
            return dbEntity;
        }
    }
}