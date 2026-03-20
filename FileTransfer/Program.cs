using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string sourcePath = "C:\\Users\\kdo\\Downloads\\100MB.bin";
        string destinationPath = "testing.bin";

        //using FileStream sourceStream = File.OpenRead(sourcePath);
        using FileStream sourceStream = new(sourcePath, FileMode.Open, FileAccess.Read);

        using FileStream destinationStream = new(destinationPath, FileMode.Create, FileAccess.ReadWrite);
        
        int partSize = 1024;
        int bytesRead = 0;
        int totalBytes = 0;
        int position = 0;

        do
        {
            var part = new byte[partSize];
            bytesRead = await sourceStream.ReadAsync(part.AsMemory(0, partSize));
            totalBytes += bytesRead;

            byte[] hashedPart = MD5.HashData(part);

            bool success = await SendData(part, hashedPart, position, bytesRead, destinationStream);

            Console.WriteLine($"Position = {position}, MD5 = {BitConverter.ToString(hashedPart)}");

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
    private static async Task<bool> SendData(byte[] part, byte[] hashed, int position, int bytesRead, FileStream destinationStream)
    {
        int maxRetries = 3;
        int retryCount = 0;
        while (retryCount < 3)
        {
            retryCount++;

            destinationStream.Seek(position, SeekOrigin.Begin);
            await destinationStream.WriteAsync(part, 0, bytesRead);
            await destinationStream.FlushAsync();

            byte[] verifyPart = new byte[bytesRead];
            destinationStream.Seek(position, SeekOrigin.Begin);
            await destinationStream.ReadExactlyAsync(verifyPart.AsMemory(0, bytesRead));

            byte[] destinationHash = MD5.HashData(verifyPart);

            if (AreByteArraysEqual(hashed, destinationHash))
            {
                return true;
            }
            else
            {
                Console.WriteLine($"Retrying part at {position} (Retry {retryCount})...");
            }
        }

        return false;
    }

    private static bool AreByteArraysEqual(byte[] array1, byte[] array2)
    {
        if (array1 == null || array2 == null)
        {
            return array1 == array2;
        }

        if (array1.Length != array2.Length)
        {
            return false;
        }

        return array1.SequenceEqual(array2);
    }
}