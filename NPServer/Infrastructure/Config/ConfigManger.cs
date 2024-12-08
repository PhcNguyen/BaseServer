using NPServer.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.IO;

namespace NPServer.Infrastructure.Config
{
    /// <summary>
    /// Một singleton cung cấp quyền truy cập vào các container giá trị cấu hình.
    /// </summary>
    public class ConfigManager
    {
        private readonly Dictionary<Type, ConfigContainer> _configContainerDict = [];
        private readonly IniFile _iniFile;

        /// <summary>
        /// Cung cấp quyền truy cập vào instance của <see cref="ConfigManager"/>.
        /// </summary>
        public static ConfigManager Instance { get; } = new();

        /// <summary>
        /// Khởi tạo một instance của <see cref="ConfigManager"/>.
        /// </summary>
        private ConfigManager()
        {
            string path = Path.Combine(PathConfig.DataDirectory, "Config.ini");
            _iniFile = new(path);
        }

        /// <summary>
        /// Khởi tạo nếu cần và trả về <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Kiểu của container cấu hình.</typeparam>
        /// <returns>Instance của kiểu <typeparamref name="T"/>.</returns>
        public T GetConfig<T>() where T : ConfigContainer, new()
        {
            if (_configContainerDict.TryGetValue(typeof(T), out ConfigContainer? container) == false)
            {
                container = new T();
                container.Initialize(_iniFile);

                _configContainerDict.Add(typeof(T), container);
            }

            return (T)container;
        }
    }
}