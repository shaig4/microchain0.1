﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace blockchain1
{
    public class CryptoUtils
    {
        const int keySize = 1024;
        public static  Address CreateAddress()
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = keySize;

                var a1 = new Address();
                a1.privateKey = RSAParametersSerializable.Ser(rsa.ExportParameters(true));
                a1.publicKey = RSAParametersSerializable.Ser(rsa.ExportParameters(false));

                return a1;
            }
        }

        public static void Pay(Network net, RequestParent[] requestParents, RequestTrans trans, bool genesis = false)
        {
            decimal amounts = 0;
            if (!genesis)
                foreach (var parent in trans.parents)
                {
                    var coin = net.coins[parent];
                    amounts += coin.amount;
                    if (!VerifyData(coin.publicKey, coin.hash, parent.sig))
                        throw new ArgumentException("order could not unlock");

                    // coins.Add(coin);
                    //net.coins.Remove(order.publicKey);
                }

            if (trans.amount != amounts)
                throw new ArgumentException("orders amount not match");

             net.coins.Add(trans.publicKey, trans);
        }

        public static List<Trans> PayRequest(RequestParent[] requestParents, RequestTrans[] requestTrans, bool genesis = false)
        {
            if (requestParents.Length > 1 && requestTrans.Length > 1)
                throw new ArgumentException("cant move many to many");


            var res = new List<Trans>();
            foreach (var rt in requestTrans)
            {
                Trans t = new Trans
                {
                    amount = rt.amount,
                    data = rt.data,
                    parents = requestParents,
                    time = DateTime.UtcNow,
                    publicKey = rt.publicKey
                };
                t.hash = Hash(JsonConvert.SerializeObject(t)
                    + string.Join(";", requestParents.Select(a => a.sig)));
                res.Add(t);
            }
            return res;
        }
        //static string Encrypt(string value, string pwd)
        //{
        //    var pwdBytes = Encoding.UTF8.GetBytes(pwd);
        //    return Convert.ToBase64String(
        //        EncryptDecrypt(Encoding.UTF8.GetBytes(value),
        //        pwdBytes, pwdBytes.Take(10).ToArray(), true));
        //}
        //static string Decrypt(string value, string pwd)
        //{
        //    var pwdBytes = Encoding.UTF8.GetBytes(pwd);
        //    return Convert.ToBase64String(
        //        EncryptDecrypt(Encoding.UTF8.GetBytes(value),
        //        pwdBytes, pwdBytes.Take(10).ToArray(), true));
        //}

        //static byte[] EncryptDecrypt(byte[] value, byte[] pwd, byte[] salt, bool encrypt)
        //{
        //    var pdb = new PasswordDeriveBytes(pwd, salt);

        //    using (var rijndael = new RijndaelManaged())
        //    {
        //        rijndael.Key = pdb.GetBytes(32);
        //        rijndael.IV = pdb.GetBytes(16);
        //        return EncryptDecrypt(rijndael, value, encrypt);
        //    }
        //}
        //private static byte[] EncryptDecrypt(SymmetricAlgorithm alg, byte[] value, bool encrypt)
        //{
        //    if ((value == null) || (value.Length == 0))
        //        return value;

        //    if (alg == null)
        //        throw new ArgumentNullException("alg");

        //    using (MemoryStream stream = new MemoryStream())
        //    using (ICryptoTransform cryptor = (encrypt ? alg.CreateEncryptor() : alg.CreateDecryptor()))
        //    using (CryptoStream cryptoStream = new CryptoStream(stream, cryptor, CryptoStreamMode.Write))
        //    {
        //        cryptoStream.Write(value, 0, value.Length);
        //        cryptoStream.FlushFinalBlock();
        //        return stream.ToArray();
        //    }
        //}
        public static string Hash(string secret)
        {
            using (var sha256 = SHA256.Create())
            {
                var varhashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(secret));
                return Convert.ToBase64String(varhashedBytes);
            }
        }
        public static string Encrypt(string publicKey, string secret)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = keySize;
                rsa.ImportParameters(RSAParametersSerializable.DeSer(publicKey));
                //var   pr = rsa.ExportParameters(true);
                //  var k = ExportPrivateKey(pr);
                return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(secret), RSAEncryptionPadding.OaepSHA1));
            }
        }
        public static string Sign(string privateKey, string secret)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = keySize;
                var hash = (Encoding.UTF8.GetBytes(secret));
                rsa.ImportParameters(RSAParametersSerializable.DeSer(privateKey));
                return Convert.ToBase64String(rsa.SignData(hash,
                   HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1));
            }
        }
        public static bool VerifyData(string publicKey, string data, string sig)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = keySize;
                var hash = (Encoding.UTF8.GetBytes(data));
                rsa.ImportParameters(RSAParametersSerializable.DeSer(publicKey));
                return (rsa.VerifyData(hash, Convert.FromBase64String(sig),
                   HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1));
            }
        }
        public static string Decrypt(string privateKey, string secret)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = keySize;
                rsa.ImportParameters(JsonConvert.DeserializeObject<RSAParameters>(privateKey));
                return Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(secret), RSAEncryptionPadding.OaepSHA1));
            }
        }
        public static string GetUnlocker(string privateKey, string sig)
        {
            return CryptoUtils.Decrypt(privateKey, sig).Split(new char[] { ';' }, 2)[0];
        }
        private static string ExportPublicKey(RSAParameters parameters)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    innerWriter.Write((byte)0x30); // SEQUENCE
                    EncodeLength(innerWriter, 13);
                    innerWriter.Write((byte)0x06); // OBJECT IDENTIFIER
                    var rsaEncryptionOid = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
                    EncodeLength(innerWriter, rsaEncryptionOid.Length);
                    innerWriter.Write(rsaEncryptionOid);
                    innerWriter.Write((byte)0x05); // NULL
                    EncodeLength(innerWriter, 0);
                    innerWriter.Write((byte)0x03); // BIT STRING
                    using (var bitStringStream = new MemoryStream())
                    {
                        var bitStringWriter = new BinaryWriter(bitStringStream);
                        bitStringWriter.Write((byte)0x00); // # of unused bits
                        bitStringWriter.Write((byte)0x30); // SEQUENCE
                        using (var paramsStream = new MemoryStream())
                        {
                            var paramsWriter = new BinaryWriter(paramsStream);
                            EncodeIntegerBigEndian(paramsWriter, parameters.Modulus); // Modulus
                            EncodeIntegerBigEndian(paramsWriter, parameters.Exponent); // Exponent
                            var paramsLength = (int)paramsStream.Length;
                            EncodeLength(bitStringWriter, paramsLength);
                            bitStringWriter.Write(paramsStream.ToArray(), 0, paramsLength);
                        }
                        var bitStringLength = (int)bitStringStream.Length;
                        EncodeLength(innerWriter, bitStringLength);
                        innerWriter.Write(bitStringStream.ToArray(), 0, bitStringLength);
                    }
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.ToArray(), 0, length);
                }
                return Convert.ToBase64String(stream.ToArray());

            }
        }

        private static string ExportPrivateKey(RSAParameters parameters)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
                    EncodeIntegerBigEndian(innerWriter, parameters.D);
                    EncodeIntegerBigEndian(innerWriter, parameters.P);
                    EncodeIntegerBigEndian(innerWriter, parameters.Q);
                    EncodeIntegerBigEndian(innerWriter, parameters.DP);
                    EncodeIntegerBigEndian(innerWriter, parameters.DQ);
                    EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.ToArray(), 0, length);
                }

                return Convert.ToBase64String(stream.ToArray());

            }
        }

        private static void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }

        private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }
    }

}