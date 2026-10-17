using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Utility
{
    public class Directory
    {
        public static long Size(string path)
        {
            long Size = 0;
            DirectoryInfo DirectoryInfo = new DirectoryInfo(path);
            FileInfo[] FileInfos = DirectoryInfo.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo FileInfo in FileInfos) { Size += FileInfo.Length; }
            return Size;
        }
    }

    public class File
    {
        public static bool IsLocked(string path)
        {
            try
            {
                FileInfo FileInfo = new FileInfo(path);
                using (FileStream stream = FileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None)) { stream.Close(); }
            }
            catch (IOException) { return true; }
            return false;
        }

        public enum Types
        {
            Unknown,
            Text,

            Bmp,
            Jpeg,
            Png,
            Gif,

            Pdf,
            Zip,

            Mp4,
            WebM,
            Mov,
            Avi,
            Mkv,
            Flv,
            Mpg,
            Mpeg,
            Wmv,
            ThreeGp,
            Ogv,

            Mp3,
            Mp3_ID3,
            Wav,
            Flac,
            Ogg,
            Aac,
            Aac_ADTS,
            M4a,
            Aiff
        }

        private static readonly Dictionary<Types, byte[]> FileHeaders
        = new Dictionary<Types, byte[]>()
        {
            { Types.Text, new byte[] { 0xEF, 0xBB, 0xBF }},

            { Types.Bmp, new byte[] { 0x42, 0x4D }},
            { Types.Jpeg, new byte[] { 0xFF, 0xD8 }},
            { Types.Png, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }},
            { Types.Gif, new byte[] { 0x47, 0x49, 0x46 }},

            { Types.Pdf, new byte[] { 0x25, 0x50, 0x44, 0x46 } },
            { Types.Zip, new byte[] { 0x50, 0x4B, 0x03, 0x04 } },

            { Types.WebM, new byte[] { 0x1A, 0x45, 0xDF, 0xA3 } },
            { Types.Mp4, new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 } },
            { Types.Mov, new byte[] { 0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70, 0x71, 0x74, 0x20, 0x20 } },
            { Types.Avi, new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x41, 0x56, 0x49, 0x20 } },
            { Types.Mkv, new byte[] { 0x1A, 0x45, 0xDF, 0xA3 } },
            { Types.Flv, new byte[] { 0x46, 0x4C, 0x56, 0x01 } },
            { Types.Mpg, new byte[] { 0x00, 0x00, 0x01, 0xBA } },
            { Types.Mpeg, new byte[] { 0x00, 0x00, 0x01, 0xB3 } },
            { Types.Wmv, new byte[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C } },
            { Types.ThreeGp, new byte[] { 0x66, 0x74, 0x79, 0x70, 0x33, 0x67 } },
            { Types.Ogv, new byte[] { 0x4F, 0x67, 0x67, 0x53 } },

            { Types.Mp3, new byte[] { 0xFF, 0xFB } },
            { Types.Mp3_ID3, new byte[] { 0x49, 0x44, 0x33 } },
            { Types.Wav, new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45 } },
            { Types.Flac, new byte[] { 0x66, 0x4C, 0x61, 0x43, 0x00, 0x00, 0x00, 0x22 } },
            { Types.Ogg, new byte[] { 0x4F, 0x67, 0x67, 0x53 } },
            { Types.Aac, new byte[] { 0xFF, 0xF1 } },
            { Types.Aac_ADTS, new byte[] { 0xFF, 0xF9 } },
            { Types.M4a, new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x4D, 0x34, 0x41, 0x20 } },
            { Types.Aiff, new byte[] { 0x46, 0x4F, 0x52, 0x4D } }
        };

        public static Types Type(ReadOnlySpan<byte> Bytes)
        {
            foreach (KeyValuePair<Types, byte[]> KeyValuePair in FileHeaders)
            {
                Types Type = KeyValuePair.Key;
                byte[] _Bytes = KeyValuePair.Value;
                int Index = _Bytes.Length;

                switch (Bytes.Length >= Index)
                {
                    case true:
                        ReadOnlySpan<byte> ReadOnlySpan = Bytes.Slice(0, Index);
                        switch (ReadOnlySpan.SequenceEqual(_Bytes))
                        { case true: return Type; }
                        break;
                }
            }
            try
            {
                Encoding.UTF8.GetString(Bytes);
                return Types.Text;
            }
            catch (Exception Exception) { }
            return Types.Unknown;
        }
    }

    public class Suffixe
    {
        public static string Byte(long value, int decimalPlaces = 1)
        {
            string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + Byte(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }
            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << mag * 10);
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }
            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
        }

        public static string Number(long value)
        {
            if (value >= 100000000000)
                return (value / 1000000000).ToString("#,0") + " B";
            if (value >= 10000000000)
                return (value / 1000000000D).ToString("0.#") + " B";
            if (value >= 100000000)
                return (value / 1000000).ToString("#,0") + " M";
            if (value >= 10000000)
                return (value / 1000000D).ToString("0.#") + " M";
            if (value >= 100000)
                return (value / 1000).ToString("#,0") + " K";
            if (value >= 10000)
                return (value / 1000D).ToString("0.#") + " K";
            return value.ToString("#,0");
        }

        public static string Duration(string format, double seconds)
        {
            TimeSpan duration = TimeSpan.FromSeconds(seconds);
            return string.Format(format, duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);
        }
    }

    public class Random
    {
        public static string[] Numeric = new string[10] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        public static string[] HigherCaseAlphabet = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public static string[] LowerCaseAlphabet = new string[26] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        public static string Randomization(string[] data, int length)
        {
            StringBuilder StringBuilder = new StringBuilder();
            System.Random Random = new System.Random();
            for (int i = 0; i < length; i++) { StringBuilder.Append(data[Random.Next(0, data.Length)]); }
            return StringBuilder.ToString();
        }
    }

    public class Stream
    {
        public static T[] Slice<T>(T[] source, int index, int length, bool padToLength = false)
        {
            int n = length;
            T[] slice = null;
            if (source.Length < index + length)
            {
                n = source.Length - index;
                if (padToLength)
                {
                    slice = new T[length];
                }
            }
            if (slice == null) slice = new T[n];
            Array.Copy(source, index, slice, 0, n);
            return slice;
        }

        public static IEnumerable<T[]> Slices<T>(T[] source, int count, bool padToLength = false)
        {
            for (int i = 0; i < source.Length; i += count)
                yield return Slice(source, i, count, padToLength);
        }
    }

    public class Cryptography
    {
        public class Algorithm
        {
            public class AES
            {
                public string PrivateKey { get; set; }
                public byte[] InitializationVector { get; set; }

                public string Encrypt(string data)
                {
                    SHA256 PBKDF2SHA256 = SHA256.Create();
                    byte[] key = PBKDF2SHA256.ComputeHash(Encoding.ASCII.GetBytes(PrivateKey));
                    Aes encryptor = Aes.Create();
                    encryptor.Mode = CipherMode.CBC;
                    byte[] aesKey = new byte[32];
                    Array.Copy(key, 0, aesKey, 0, 32);
                    encryptor.Key = aesKey;
                    encryptor.IV = InitializationVector;
                    MemoryStream memoryStream = new MemoryStream();
                    ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);
                    byte[] plainBytes = Encoding.ASCII.GetBytes(data);
                    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    byte[] cipherBytes = memoryStream.ToArray();
                    memoryStream.Close();
                    cryptoStream.Close();
                    string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
                    return cipherText;
                }

                public string Decrypt(string data)
                {
                    SHA256 PBKDF2SHA256 = SHA256.Create();
                    byte[] key = PBKDF2SHA256.ComputeHash(Encoding.ASCII.GetBytes(PrivateKey));
                    Aes encryptor = Aes.Create();
                    encryptor.Mode = CipherMode.CBC;
                    byte[] aesKey = new byte[32];
                    Array.Copy(key, 0, aesKey, 0, 32);
                    encryptor.Key = aesKey;
                    encryptor.IV = InitializationVector;
                    MemoryStream memoryStream = new MemoryStream();
                    ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);
                    string plainText = string.Empty;
                    try
                    {
                        byte[] cipherBytes = Convert.FromBase64String(data);
                        cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        byte[] plainBytes = memoryStream.ToArray();
                        plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
                    }
                    finally
                    {
                        memoryStream.Close();
                        cryptoStream.Close();
                    }
                    return plainText;
                }
            }

            public class RSA
            {
                public string PrivateKey { get; set; }
                public string PublicKey { get; set; }

                public string Encrypt(string data, bool usePublicKey = true)
                {
                    var rsa = asRSACryptoServiceProvider(usePublicKey: usePublicKey);
                    byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(data), true);
                    var str = Convert.ToBase64String(encryptedData);
                    return str;
                }

                public string Decrypt(string data, bool usePublicKey = false)
                {
                    var rsa = asRSACryptoServiceProvider(usePublicKey: usePublicKey);
                    byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(data), true);
                    var str = Encoding.UTF8.GetString(decryptedData);
                    return str;
                }

                public string Hash(string data, bool usePublicKey = false)
                {
                    var rsa = asRSACryptoServiceProvider(usePublicKey: usePublicKey);
                    byte[] encryptedData = rsa.SignData(Encoding.UTF8.GetBytes(data), new SHA512CryptoServiceProvider());
                    var str = Convert.ToBase64String(encryptedData);
                    return str;
                }

                public void VerifyHash(string data, string hash, bool usePublicKey = false)
                {
                    var rsa = asRSACryptoServiceProvider(usePublicKey: usePublicKey);
                    var isValid = rsa.VerifyData(Encoding.UTF8.GetBytes(data), new SHA512CryptoServiceProvider(), Convert.FromBase64String(hash));
                    if (!isValid) { throw new CryptographicException("Invalid hash."); }
                }

                public RSACryptoServiceProvider asRSACryptoServiceProvider(bool usePublicKey = false)
                {
                    var keyStr = usePublicKey ? PublicKey : PrivateKey;
                    var pemReader = new PemReader(new StringReader(keyStr));
                    if (usePublicKey)
                    {
                        var asymmetricKey = (AsymmetricKeyParameter)pemReader.ReadObject();
                        pemReader.Reader.Close();
                        var rsaKey = (Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters)asymmetricKey;
                        var rsaKeyInfo = DotNetUtilities.ToRSAParameters(rsaKey);
                        var rsa = new RSACryptoServiceProvider();
                        rsa.ImportParameters(rsaKeyInfo);
                        return rsa;
                    }
                    else
                    {
                        var pemObject = pemReader.ReadObject();
                        Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters rsaKey;
                        if (pemObject is Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)
                        {
                            rsaKey = (Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)pemObject;
                        }
                        else
                        {
                            var asymmetricKey = (AsymmetricCipherKeyPair)pemObject;
                            rsaKey = (Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)asymmetricKey.Private;
                        }
                        pemReader.Reader.Close();
                        var rsaKeyInfo = DotNetUtilities.ToRSAParameters(rsaKey);
                        var rsa = new RSACryptoServiceProvider();
                        rsa.ImportParameters(rsaKeyInfo);
                        return rsa;
                    }
                }

                public static RSA GenerateKeyPair(string signatureAlgorithm = null, int rsaKeyLength = 2048)
                {
                    if (string.IsNullOrEmpty(signatureAlgorithm)) { signatureAlgorithm = PkcsObjectIdentifiers.Sha512WithRsaEncryption.Id; }
                    var rsaKeyPairGenerator = new RsaKeyPairGenerator();
                    rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), rsaKeyLength));
                    var pair = rsaKeyPairGenerator.GenerateKeyPair();
                    var configuration = new RSA();
                    var privateKeyStrBuilder = new StringBuilder();
                    var privateKeyPemWriter = new PemWriter(new StringWriter(privateKeyStrBuilder));
                    privateKeyPemWriter.WriteObject(pair.Private);
                    privateKeyPemWriter.Writer.Flush();
                    privateKeyPemWriter.Writer.Close();
                    configuration.PrivateKey = privateKeyStrBuilder.ToString();
                    var publicKeyStrBuilder = new StringBuilder();
                    var publicKeyPemWriter = new PemWriter(new StringWriter(publicKeyStrBuilder));
                    publicKeyPemWriter.WriteObject(pair.Public);
                    publicKeyPemWriter.Writer.Flush();
                    publicKeyPemWriter.Writer.Close();
                    configuration.PublicKey = publicKeyStrBuilder.ToString();
                    return configuration;
                }
            }
        }

        public class Signature
        {
            public static string MD5(byte[] bytes)
            {
                using (MD5 MD5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] dataBytes = bytes;
                    byte[] hashBytes = MD5.ComputeHash(dataBytes);
                    StringBuilder StringBuilder = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++) { StringBuilder.Append(hashBytes[i].ToString("X2")); }
                    return StringBuilder.ToString();
                }
            }

            public static string SHA256(byte[] bytes)
            {
                using (SHA256 SHA256 = System.Security.Cryptography.SHA256.Create())
                {
                    StringBuilder StringBuilder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++) { StringBuilder.Append(bytes[i].ToString("x2")); }
                    return StringBuilder.ToString();
                }
            }
        }
    }

    public class Net
    {
        public static IPAddress LocalIPAddress()
        {
            IPHostEntry IPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress IPAddress in IPHostEntry.AddressList) { switch (IPAddress.AddressFamily == AddressFamily.InterNetwork) { case true: return IPAddress; } }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static int AvailablePort(int startingPort)
        {
            IPGlobalProperties IPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var TcpConnectionEnumerable = IPGlobalProperties.GetActiveTcpConnections()
            .Where(n => n.LocalEndPoint.Port >= startingPort)
            .Select(n => n.LocalEndPoint.Port);
            var TcpListenerEnumerable = IPGlobalProperties.GetActiveTcpListeners()
            .Where(n => n.Port >= startingPort)
            .Select(n => n.Port);
            var UdpListenerEnumerable = IPGlobalProperties.GetActiveUdpListeners()
            .Where(n => n.Port >= startingPort)
            .Select(n => n.Port);
            int Index = Enumerable.Range(startingPort, ushort.MaxValue)
            .Where(i => !TcpConnectionEnumerable.Contains(i))
            .Where(i => !TcpListenerEnumerable.Contains(i))
            .Where(i => !UdpListenerEnumerable.Contains(i))
            .FirstOrDefault();
            return Index;
        }
    }
}

// Copyright © 2023 KendallDawson. All rights reserved.