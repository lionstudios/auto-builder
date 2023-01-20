using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class CryptoHelper //Copied from King Wing
{
    public static string Encrypt(string textToEncrypt)
    {
        if (string.IsNullOrEmpty(textToEncrypt))
        {
            return textToEncrypt;
        }
        
        string encryptionKey = "NotNull!30";
        byte[] encryptionSalt = new byte[]
            {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76};

        byte[] encryptionBytes = Encoding.Unicode.GetBytes(textToEncrypt);

        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, encryptionSalt);
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(encryptionBytes, 0, encryptionBytes.Length);
                    cs.Close();
                }

                textToEncrypt = Convert.ToBase64String(ms.ToArray());
            }
        }

        return textToEncrypt;
    }

    public static string Decrypt(string textToDecrypt)
    {
        if (string.IsNullOrEmpty(textToDecrypt))
        {
            return textToDecrypt;
        }

        try
        {
            string decryptionKey = "NotNull!30";
            byte[] decryptionSalt = new byte[]
                {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76};

            textToDecrypt = textToDecrypt.Replace(" ", "+");
            byte[] decryptionBytes = Convert.FromBase64String(textToDecrypt);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(decryptionKey, decryptionSalt);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(decryptionBytes, 0, decryptionBytes.Length);
                        cs.Close();
                    }

                    textToDecrypt = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        return textToDecrypt;
    }
}