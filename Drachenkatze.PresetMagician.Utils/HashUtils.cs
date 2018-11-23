﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.Utils
{
    public class HashUtils
    {
        public static string getFormattedSHA256Hash (string input)
        {
            return FormatHash(GetHash(SHA256.Create(), input));
        }

        public static string getFormattedSHA256Hash(byte[] input)
        {
            return FormatHash(GetHash(SHA256.Create(), input));
        }

        public static byte[] GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public static byte[] GetHash(HashAlgorithm hashAlgorithm, byte[] input)
        {
            // Convert the input string to a byte array and compute the hash.
            return hashAlgorithm.ComputeHash(input);
        }

        public static string FormatHash (byte[] data)
        {
            var sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

     }
}