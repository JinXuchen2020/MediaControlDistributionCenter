using System.Security.Cryptography;
using System.Text;

namespace MediaControlDistributionCenter.Helpers
{
    class EncryptionHelper
    { // 使用 SHA-256 加密密码
        public static string GetSha256Hash(string input)
        {
            byte[] data = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new();
            foreach (byte byteValue in data)
            {
                sb.Append(byteValue.ToString("x2"));  // 转换为 16 进制
            }
            return sb.ToString();
        }

        // 比较输入的密码哈希值与存储的密码哈希值是否匹配
        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            // 对输入的密码进行哈希处理
            string inputHash = GetSha256Hash(inputPassword);

            // 比较哈希值
            return inputHash.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
        }

        // 使用盐（Salt）增强密码的安全性
        public static string GetSha256HashWithSalt(string input, string salt)
        {
            string saltedInput = input + salt;  // 将盐值与密码合并
            byte[] data = SHA256.HashData(Encoding.UTF8.GetBytes(saltedInput));
            StringBuilder sb = new();
            foreach (byte byteValue in data)
            {
                sb.Append(byteValue.ToString("x2"));
            }
            return sb.ToString();
        }

        // 验证带盐的密码哈希
        public static bool VerifyPasswordWithSalt(string inputPassword, string storedHash, string salt)
        {
            // 对输入的密码加盐后进行哈希处理
            string inputHash = GetSha256HashWithSalt(inputPassword, salt);

            // 比较哈希值
            return inputHash.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
        }

        // MD5 加密
        public static string GetMd5Hash(string input)
        {
            // 创建 MD5 实例
            byte[] data = MD5.HashData(Encoding.UTF8.GetBytes(input)); // 获取字节数组

            // 转换字节数组为十六进制字符串
            StringBuilder sb = new();
            foreach (byte byteValue in data)
            {
                sb.Append(byteValue.ToString("x2")); // 将每个字节转换为两位十六进制数
            }

            return sb.ToString();
        }

        // 验证输入的字符串与原始密码是否匹配
        public static bool VerifyMd5Hash(string input, string hash)
        {
            string hashOfInput = GetMd5Hash(input); // 对输入进行加密
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashOfInput, hash) == 0; // 比较哈希值
        }
    }
}
