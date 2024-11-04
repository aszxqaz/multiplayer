using System;
using System.Diagnostics;
using System.Security.Cryptography;

public class CheckSum
{
    static public string Hex(byte[] bytes)
    {
        string hex;
        using (MD5 md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(bytes);
            hex = BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        return hex;
    }
}