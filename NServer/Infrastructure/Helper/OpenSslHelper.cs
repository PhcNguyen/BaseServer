﻿using NServer.Infrastructure.Configuration;
using System;
using System.Diagnostics;

namespace NServer.Infrastructure.Helper;

public static class OpensslHelper
{
    private static readonly string OpenSSLPath = "openssl";

    /// <summary>
    /// Generates an SSL certificate using OpenSSL by executing a series of commands.
    /// </summary>
    public static void GenerateSslCertificate()
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
                var formattedCommand = string.Format(command, args);
                RunOpenSslCommand(formattedCommand);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error generating SSL certificate: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Checks if OpenSSL is installed and accessible.
    /// </summary>
    public static void CheckOpenSslInstallation()
    {
        try
        {
            string output = RunOpenSslCommand("version");
            if (string.IsNullOrEmpty(output))
            {
                throw new InvalidOperationException("OpenSSL is not available on the system.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error checking OpenSSL installation: " + ex.Message, ex);
        }
    }

    // Hàm chạy lệnh OpenSSL và trả về kết quả
    private static string RunOpenSslCommand(string arguments)
    {
        ProcessStartInfo pro = new()
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
            using Process process = Process.Start(pro)! ?? throw new Exception("Unable to start the OpenSSL process.");

            // Đọc kết quả từ StandardOutput
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            // Kiểm tra lỗi từ OpenSSL
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException($"OpenSSL error: {error} while executing command: {arguments}");
            }

            process.WaitForExit();

            return output;
        }
        catch (Exception ex)
        {
            // Ném ngoại lệ ra với thông báo chi tiết, bao gồm lệnh gặp lỗi
            throw new InvalidOperationException($"Error running OpenSSL command: {arguments} - " + ex.Message, ex);
        }
    }
}