﻿using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

namespace NPServer.Infrastructure.Configuration.Utilties
{
    /// <summary>
    /// Một lớp bao bọc để đọc và ghi các tệp ini.
    /// </summary>
    public class IniFile
    {
        private readonly string _path;
        private readonly Dictionary<string, Dictionary<string, string>> _iniData;

        public bool ExistsFile => File.Exists(_path);

        /// <summary>
        /// Khởi tạo một phiên bản mới của <see cref="IniFile"/> cho đường dẫn được chỉ định.
        /// </summary>
        /// <param name="path">Đường dẫn tới tệp ini.</param>
        public IniFile(string path)
        {
            _path = path;
            _iniData = [];
            Load();
        }

        /// <summary>
        /// Ghi một giá trị vào tệp ini.
        /// </summary>
        /// <param name="section">Tên phần trong tệp ini.</param>
        /// <param name="key">Tên khóa trong phần.</param>
        /// <param name="value">Giá trị cần ghi.</param>
        public void WriteValue(string section, string key, object value)
        {
            if (!_iniData.TryGetValue(section, out Dictionary<string, string>? _value))
            {
                _value = ([]);
                _iniData[section] = _value;
            }

            _value[key] = value.ToString() ?? string.Empty;

            // Ghi lại toàn bộ dữ liệu vào tệp
            WriteFile();
        }

        /// <summary>
        /// Đọc dữ liệu từ tệp ini vào bộ nhớ.
        /// </summary>
        private void Load()
        {
            if (!ExistsFile) return;

            var currentSection = string.Empty;

            foreach (var line in File.ReadLines(_path))
            {
                var trimmedLine = line.Trim();

                // Bỏ qua dòng trống hoặc chú thích
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(';'))
                    continue;

                // Kiểm tra xem có phải là phần (section) không
                if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']'))
                {
                    currentSection = trimmedLine[1..^1].Trim();
                    if (!_iniData.ContainsKey(currentSection))
                    {
                        _iniData[currentSection] = [];
                    }
                }
                else
                {
                    // Nếu không phải là phần, giả sử đây là cặp khóa-giá trị
                    var keyValue = trimmedLine.Split(['='], 2);

                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim();

                        _iniData[currentSection][key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Lấy giá trị có khóa được chỉ định từ phần được chỉ định của <see cref="IniFile"/> này dưới dạng <see cref="string"/>.
        /// </summary>
        /// <param name="section">Tên phần trong tệp ini.</param>
        /// <param name="key">Tên khóa trong phần.</param>
        /// <returns>Giá trị dưới dạng chuỗi.</returns>
        public string GetString(string section, string key)
        {
            return _iniData.TryGetValue(section, out Dictionary<string, string>? value) && value.ContainsKey(key)
                ? value[key]
                : string.Empty;
        }

        /// <summary>
        /// Lấy giá trị có khóa được chỉ định từ phần được chỉ định của <see cref="IniFile"/> này dưới dạng <see cref="bool"/>.
        /// </summary>
        /// <param name="section">Tên phần trong tệp ini.</param>
        /// <param name="key">Tên khóa trong phần.</param>
        /// <returns>Giá trị dưới dạng boolean hoặc null nếu không thể chuyển đổi.</returns>
        public bool? GetBool(string section, string key)
        {
            var stringValue = GetString(section, key);
            return stringValue != null && bool.TryParse(stringValue, out bool parsedValue) ? parsedValue : null;
        }

        /// <summary>
        /// Lấy giá trị có khóa được chỉ định từ phần được chỉ định của <see cref="IniFile"/> này dưới dạng <see cref="int"/>.
        /// </summary>
        /// <param name="section">Tên phần trong tệp ini.</param>
        /// <param name="key">Tên khóa trong phần.</param>
        /// <returns>Giá trị dưới dạng int hoặc null nếu không thể chuyển đổi.</returns>
        public int? GetInt32(string section, string key)
        {
            var stringValue = GetString(section, key);
            return stringValue != null && int.TryParse(stringValue, out int parsedValue) ? parsedValue : null;
        }

        /// <summary>
        /// Lấy giá trị có khóa được chỉ định từ phần được chỉ định của <see cref="IniFile"/> này dưới dạng <see cref="uint"/>.
        /// </summary>
        /// <param name="section">Tên phần trong tệp ini.</param>
        /// <param name="key">Tên khóa trong phần.</param>
        /// <returns>Giá trị dưới dạng uint hoặc null nếu không thể chuyển đổi.</returns>
        public uint? GetUInt32(string section, string key)
        {
            var stringValue = GetString(section, key);
            return stringValue != null && uint.TryParse(stringValue, out uint parsedValue) ? parsedValue : null;
        }

        /// <summary>
        /// Lấy giá trị có khóa được chỉ định từ phần được chỉ định của <see cref="IniFile"/> này dưới dạng <see cref="long"/>.
        /// </summary>
        /// <param name="section">Tên phần trong tệp ini.</param>
        /// <param name="key">Tên khóa trong phần.</param>
        /// <returns>Giá trị dưới dạng long hoặc null nếu không thể chuyển đổi.</returns>
        public long? GetInt64(string section, string key)
        {
            var stringValue = GetString(section, key);
            return stringValue != null && long.TryParse(stringValue, out long parsedValue) ? parsedValue : null;
        }

        /// <summary>
        /// Lấy giá trị có khóa được chỉ định từ phần được chỉ định của <see cref="IniFile"/> này dưới dạng <see cref="ulong"/>.
        /// </summary>
        /// <param name="section">Tên phần trong tệp ini.</param>
        /// <param name="key">Tên khóa trong phần.</param>
        /// <returns>Giá trị dưới dạng ulong hoặc null nếu không thể chuyển đổi.</returns>
        public ulong? GetUInt64(string section, string key)
        {
            var stringValue = GetString(section, key);
            return stringValue != null && ulong.TryParse(stringValue, out ulong parsedValue) ? parsedValue : null;
        }

        /// <summary>
        /// Lấy giá trị có khóa được chỉ định từ phần được chỉ định của <see cref="IniFile"/> này dưới dạng <see cref="float"/>.
        /// </summary>
        /// <param name="section">Tên phần trong tệp ini.</param>
        /// <param name="key">Tên khóa trong phần.</param>
        /// <returns>Giá trị dưới dạng float hoặc null nếu không thể chuyển đổi.</returns>
        public float? GetSingle(string section, string key)
        {
            var stringValue = GetString(section, key);
            return stringValue != null && float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue)
                ? parsedValue
                : null;
        }

        /// <summary>
        /// Ghi lại nội dung của tệp ini vào tệp đích.
        /// </summary>
        private void WriteFile()
        {
            if (_iniData == null || _iniData.Count == 0)
                return;

            using var writer = new StreamWriter(_path);
            foreach (var section in _iniData)
            {
                writer.WriteLine($"[{section.Key}]");

                foreach (var keyValue in section.Value)
                {
                    writer.WriteLine($"{keyValue.Key}={keyValue.Value}");
                }

                writer.WriteLine();  // Dòng trống giữa các section
            }
        }
    }
}
