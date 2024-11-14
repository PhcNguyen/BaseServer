namespace NETServer.Database
{
    internal class SqlFileReader
    {
        public static async Task<string> ParseSqlCommandsAsync(string filePath)
        {
            var sqlLines = new List<string>();

            using (var reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // Loại bỏ dòng trống và các dòng bắt đầu bằng --
                    line = line.Trim();
                    if (!string.IsNullOrEmpty(line) && !line.StartsWith("--"))
                    {
                        sqlLines.Add(line);
                    }
                }
            }

            // Nối các dòng SQL lại thành một câu lệnh duy nhất, phân cách bằng dấu cách
            return string.Join(" ", sqlLines);  // Kết hợp các câu lệnh thành 1 chuỗi, nếu cần
        }
    }
}
