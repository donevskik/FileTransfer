using System.Security.Cryptography;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string sourcePath = "source.bin";
        string destinationPath = "destination.bin";

        using FileStream stream = File.OpenRead(sourcePath);
        using MD5 md5 = MD5.Create();

        int partSize = 1024;
        int bytesRead = 0;
        int totalBytes = 0;
        int position = 0;

        do
        {
            var part = new byte[partSize];
            bytesRead = await stream.ReadAsync(part.AsMemory(0, partSize));
            totalBytes += bytesRead;

            byte[] hashedPart = MD5.HashData(part);

            bool success = await SendData(part, hashedPart, position, partSize, destinationPath);

            position += bytesRead;

        } while (bytesRead > 0);
    }
    
    /// <summary>
    /// sends the data
    /// </summary>
    /// <param name="part"></param>
    /// <param name="hashed"></param>
    /// <param name="position"></param>
    /// <returns>true if success, false otherwise</returns>
    private static async Task<bool> SendData(byte[] part, byte[] hashed, int position, int partSize, string destination)
    {

    }
}