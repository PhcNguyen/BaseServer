using System.Diagnostics;
using NETServer.Infrastructure.Configuration;

namespace NETServer.Infrastructure.Services;

internal class OpenSsl
{
    private static readonly string OpenSSLPath = "openssl";

    public static void GenerateSslCertificate(string keyFilePath, string csrFilePath, string crtFilePath, string pfxFilePath, string password)
    {
        try
        {
            // Bước 1: Tạo khóa riêng (private key)
            // Bước 2: Tạo yêu cầu chứng chỉ (CSR)
            // Bước 3: Tạo chứng chỉ tự ký (self-signed certificate)
            // Bước 4: Chuyển đổi chứng chỉ và khóa thành tệp PFX

            var commands = new (string Command, string[] Args)[]
            {
                ("genpkey -algorithm RSA -out {0} -pkeyopt rsa_keygen_bits:2048", new[] { Setting.SslPrivateKeyPath }),
                ("req -new -key {0} -out {1}", new[] { Setting.SslPrivateKeyPath, Setting.SslCsrCertificatePath }),
                ("req -x509 -key {0} -in {1} -out {2} -days 365", new[] { Setting.SslPrivateKeyPath, Setting.SslCsrCertificatePath, Setting.SslCrtCertificatePath }),
                ("pkcs12 -export -out {0} -inkey {1} -in {2} -certfile {2} -password pass:{3}", new[] { Setting.SslPfxCertificatePath, Setting.SslPrivateKeyPath, Setting.SslCrtCertificatePath, Setting.SslPassword })
            };

            // Chạy các lệnh OpenSSL theo thứ tự
            foreach (var (command, args) in commands)
            {
                RunOpenSslCommand(string.Format(command, args));
            }
            Console.WriteLine("SSL Certificate generated successfully.");
        }
        catch (Exception ex)
        {
            // Ném ngoại lệ ra với thông báo chi tiết
            throw new Exception("Error generating SSL certificate: " + ex.Message, ex);
        }
    }

    public static void CheckOpenSslInstallation()
    {
        try
        {
            // Kiểm tra xem OpenSSL có cài đặt hay không
            string output = RunOpenSslCommand("version");
            if (string.IsNullOrEmpty(output))
            {
                throw new Exception("OpenSSL không có sẵn trong hệ thống.");
            }
            else
            {
                Console.WriteLine("OpenSSL đã được cài đặt: " + output);
            }
        }
        catch (Exception ex)
        {
            // Ném ngoại lệ ra với thông báo chi tiết
            throw new Exception("Error checking OpenSSL installation: " + ex.Message, ex);
        }
    }

    // Hàm chạy lệnh OpenSSL và trả về kết quả
    private static string RunOpenSslCommand(string arguments)
    {
        ProcessStartInfo pro = new ProcessStartInfo
        {
            FileName = OpenSSLPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using (Process process = Process.Start(pro)!)
            {
                if (process == null)
                {
                    throw new Exception("Không thể khởi động tiến trình OpenSSL.");
                }

                // Đọc kết quả từ StandardOutput
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Kiểm tra lỗi từ OpenSSL
                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception($"OpenSSL error: {error}");
                }

                process.WaitForExit();

                return output;
            }
        }
        catch (Exception ex)
        {
            // Ném ngoại lệ ra với thông báo chi tiết
            throw new Exception("Error running OpenSSL command: " + ex.Message, ex);
        }
    }
}