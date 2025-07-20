using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PersonalAI
{
    public static class SecurityHelper
    {
        // คีย์เริ่มต้นสำหรับการเข้ารหัส (ในการใช้งานจริงควรเก็บไว้ในที่ปลอดภัย)
        private static readonly byte[] DefaultKey = new byte[32] 
        { 
            0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 
            0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01,
            0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01,
            0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01
        };
        
        private static readonly byte[] DefaultIV = new byte[16] 
        { 
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF,
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF
        };

        // เข้ารหัสข้อความ
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = DefaultKey;
                aes.IV = DefaultIV;

                using MemoryStream output = new();
                using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
                
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                cryptoStream.FlushFinalBlock();
                
                return Convert.ToBase64String(output.ToArray());
            }
            catch
            {
                // ในกรณีที่มีข้อผิดพลาด ส่งคืนข้อความเดิม
                return plainText;
            }
        }

        // ถอดรหัสข้อความ
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(encryptedText);
                
                using Aes aes = Aes.Create();
                aes.Key = DefaultKey;
                aes.IV = DefaultIV;

                using MemoryStream input = new(cipherBytes);
                using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using StreamReader reader = new(cryptoStream);
                
                return reader.ReadToEnd();
            }
            catch
            {
                // ในกรณีที่มีข้อผิดพลาด ส่งคืนข้อความเดิม
                return encryptedText;
            }
        }

        // สร้าง hash ของข้อความ
        public static string ComputeHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            
            return builder.ToString();
        }

        // ตรวจสอบความปลอดภัยของข้อความ (เช่น ไม่มีคำหยาบ)
        public static bool IsSafeContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return true;

            // รายการคำที่ไม่เหมาะสม (ในการใช้งานจริงควรมีรายการที่ครอบคลุมมากกว่านี้)
            string[] unsafeWords = { "badword1", "badword2", "badword3" };
            
            foreach (var word in unsafeWords)
            {
                if (content.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}