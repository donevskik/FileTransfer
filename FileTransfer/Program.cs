using System.Security.Cryptography;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string sourcePath = "C:\\Users\\kdo\\Downloads\\1GB.bin";
        string destinationPath = "testing.bin";

        using FileStream sourceStream = new(sourcePath, FileMode.Open, FileAccess.Read);

        using FileStream destinationStream = new(destinationPath, FileMode.Create, FileAccess.ReadWrite);
        
        int partSize = 1024000;
        int bytesRead = 0;
        int position = 0;
        byte[] part = new byte[partSize];

        while ((bytesRead = await sourceStream.ReadAsync(part.AsMemory(0, partSize))) > 0)
        {
            byte[] hashedPart = MD5.HashData(part);

            bool success = await SendData(part, hashedPart, position, bytesRead, partSize, destinationStream);

            Console.WriteLine($"Position = {position}, Skip = {bytesRead}, MD5Hash = {BitConverter.ToString(hashedPart)}");

            position += bytesRead;

            part = new byte[partSize];

        }

        if (VerifyFile(sourceStream, destinationStream))
        {
            Console.WriteLine("File transferred successfully");
        }
        else
        {
            Console.WriteLine("File transfer failed");
        }

    }

    private static async Task<bool> SendData(byte[] part, byte[] hashed, int position, int bytesRead, int partSize, FileStream destinationStream)
    {
        int maxRetries = 3;
        int retryCount = 0;
        while (retryCount < 3)
        {
            retryCount++;

            destinationStream.Seek(position, SeekOrigin.Begin);
            await destinationStream.WriteAsync(part, 0, bytesRead);
            await destinationStream.FlushAsync();

            byte[] verifyPart = new byte[partSize];
            destinationStream.Seek(position, SeekOrigin.Begin);
            await destinationStream.ReadAsync(verifyPart.AsMemory(0, partSize));

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

    private static bool VerifyFile(FileStream sourceFile, FileStream destinationFile)
    {
        SHA256 sha256 = SHA256.Create();

        byte[] source = sha256.ComputeHash(sourceFile);
        byte[] destination = sha256.ComputeHash(destinationFile);

        return AreByteArraysEqual(source, destination);
    }
}