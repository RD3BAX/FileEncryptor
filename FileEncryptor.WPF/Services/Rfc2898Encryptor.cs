using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FileEncryptor.WPF.Services.Interfaces;

namespace FileEncryptor.WPF.Services
{
    internal class Rfc2898Encryptor : IEncryptor
    {
        private static readonly byte[] __Salt =
        {
            0x26, 0xdc, 0xff, 0x00,
            0xad, 0xed, 0x7a, 0xee,
            0xc5, 0xfe, 0x07, 0xaf,
            0x4d, 0x08, 0x22, 0x3c
        };

        private static ICryptoTransform GetEncryptor(string password, byte[] Slat = null)
        {
            var pdb = new Rfc2898DeriveBytes(password, Slat ?? __Salt);
            var algorithm = Rijndael.Create();
            algorithm.Key = pdb.GetBytes(32);
            algorithm.IV = pdb.GetBytes(16);
            return algorithm.CreateEncryptor();
        }

        private static ICryptoTransform GetDecryptor(string password, byte[] Slat = null)
        {
            var pdb = new Rfc2898DeriveBytes(password, Slat ?? __Salt);
            var algorithm = Rijndael.Create();
            algorithm.Key = pdb.GetBytes(32);
            algorithm.IV = pdb.GetBytes(16);
            return algorithm.CreateDecryptor();
        }

        public void Encrypt(string SourcePath, string DestinationPath, string Password, int BufferLength = 104200)
        {
            var encryptor = GetEncryptor(Password/*, Encoding.UTF8.GetBytes(SourcePath)*/);

            using var destination_encrypted = File.Create(DestinationPath, BufferLength);
            using var destination = new CryptoStream(destination_encrypted, encryptor, CryptoStreamMode.Write);
            using var source = File.OpenRead(SourcePath);

            var buffer = new byte[BufferLength];
            int readed;
            do
            {
                Thread.Sleep(1); // Тестовое замедление
                readed = source.Read(buffer, 0, BufferLength);
                destination.Write(buffer, 0, readed);
            } 
            while (readed > 0);
            destination.FlushFinalBlock();
        }

        public bool Decrypt(string SourcePath, string DestinationPath, string Password, int BufferLength = 104200)
        {
            var decryptor = GetDecryptor(Password);

            using var destination_decrypted = File.Create(DestinationPath, BufferLength);
            using var destination = new CryptoStream(destination_decrypted, decryptor, CryptoStreamMode.Write);
            using var encrypted_source = File.OpenRead(SourcePath);

            var buffer = new byte[BufferLength];
            int readed;
            do
            {
                readed = encrypted_source.Read(buffer, 0, BufferLength);
                destination.Write(buffer, 0, readed);
            } while (readed > 0);

            try
            {
                destination.FlushFinalBlock();
            }
            catch (CryptographicException)
            {
                return false;
            }

            return true;
        }

        public async Task EncryptAsync(string SourcePath, string DestinationPath, string Password, int BufferLength = 104200)
        {
            if (!File.Exists(SourcePath))
                throw new FileNotFoundException("Файл-источник для процесса шифрования не найден", SourcePath);
            if (BufferLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(BufferLength), BufferLength,
                    "Размер буфера чтения должен быть больше 0");

            var encryptor = GetEncryptor(Password/*, Encoding.UTF8.GetBytes(SourcePath)*/);

            await using var destination_encrypted = File.Create(DestinationPath, BufferLength);
            await using var destination = new CryptoStream(destination_encrypted, encryptor, CryptoStreamMode.Write);
            await using var source = File.OpenRead(SourcePath);

            var buffer = new byte[BufferLength];
            int readed;
            do
            {
                readed = await source.ReadAsync(buffer, 0, BufferLength).ConfigureAwait(false);
                // дополнительные действия по завершению асинхронной операции
                await destination.WriteAsync(buffer, 0, readed).ConfigureAwait(false);                
                Thread.Sleep(1); // Тестовое замедление
            }
            while (readed > 0);
            destination.FlushFinalBlock();
        }

        public async Task<bool> DecryptAsync(string SourcePath, string DestinationPath, string Password, int BufferLength = 104200)
        {
            if (!File.Exists(SourcePath))
                throw new FileNotFoundException("Файл-источник для процесса дешифрирования не найден", SourcePath);
            if (BufferLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(BufferLength), BufferLength,
                    "Размер буфера чтения должен быть больше 0");

            var decryptor = GetDecryptor(Password);

            await using var destination_decrypted = File.Create(DestinationPath, BufferLength);
            await using var destination = new CryptoStream(destination_decrypted, decryptor, CryptoStreamMode.Write);
            await using var encrypted_source = File.OpenRead(SourcePath);

            var buffer = new byte[BufferLength];
            int readed;
            do
            {
                readed = await encrypted_source.ReadAsync(buffer, 0, BufferLength).ConfigureAwait(false);

                await destination.WriteAsync(buffer, 0, readed).ConfigureAwait(false);
            } 
            while (readed > 0);

            try
            {
                destination.FlushFinalBlock();
            }
            catch (CryptographicException)
            {
                //return Task.FromResult(false);
                return false;
            }

            return true;
        }

    }
}
