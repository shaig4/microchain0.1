﻿using Merkator.BitCoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace blockchain1
{

    [Serializable]
    public class RSAParametersSerializable : ISerializable
    {
        public static string Ser(RSAParameters p)
        {
            using (var ms = new MemoryStream())
            {
                var ser = new BinaryFormatter();
                ser.Serialize(ms, new RSAParametersSerializable(p));
                return Base58Encoding.EncodeWithCheckSum(ms.ToArray());
            }
          //  return JsonConvert.SerializeObject(new RSAParametersSerializable(p));
        }
        public  static RSAParameters DeSer(string data)
        {
            using (var ms = new MemoryStream(Base58Encoding.DecodeWithCheckSum(data)))
            {
                var ser = new BinaryFormatter();
                return ((RSAParametersSerializable)ser.Deserialize(ms)).RSAParameters;
            }
            //return JsonConvert.DeserializeObject<RSAParametersSerializable>(json).RSAParameters;
        }
        private RSAParameters _rsaParameters;

        public RSAParameters RSAParameters
        {
            get
            {
                return _rsaParameters;
            }
        }

        public RSAParametersSerializable(RSAParameters rsaParameters)
        {
            _rsaParameters = rsaParameters;
        }

        private RSAParametersSerializable()
        {
        }

        public byte[] D { get { return _rsaParameters.D; } set { _rsaParameters.D = value; } }

        public byte[] DP { get { return _rsaParameters.DP; } set { _rsaParameters.DP = value; } }

        public byte[] DQ { get { return _rsaParameters.DQ; } set { _rsaParameters.DQ = value; } }

        public byte[] Exponent { get { return _rsaParameters.Exponent; } set { _rsaParameters.Exponent = value; } }

        public byte[] InverseQ { get { return _rsaParameters.InverseQ; } set { _rsaParameters.InverseQ = value; } }

        public byte[] Modulus { get { return _rsaParameters.Modulus; } set { _rsaParameters.Modulus = value; } }

        public byte[] P { get { return _rsaParameters.P; } set { _rsaParameters.P = value; } }

        public byte[] Q { get { return _rsaParameters.Q; } set { _rsaParameters.Q = value; } }

        public RSAParametersSerializable(SerializationInfo information, StreamingContext context)
        {
            _rsaParameters = new RSAParameters()
            {
                D = (byte[])information.GetValue("D", typeof(byte[])),
                DP = (byte[])information.GetValue("DP", typeof(byte[])),
                DQ = (byte[])information.GetValue("DQ", typeof(byte[])),
                Exponent = (byte[])information.GetValue("Exponent", typeof(byte[])),
                InverseQ = (byte[])information.GetValue("InverseQ", typeof(byte[])),
                Modulus = (byte[])information.GetValue("Modulus", typeof(byte[])),
                P = (byte[])information.GetValue("P", typeof(byte[])),
                Q = (byte[])information.GetValue("Q", typeof(byte[]))
            };
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("D", _rsaParameters.D);
            info.AddValue("DP", _rsaParameters.DP);
            info.AddValue("DQ", _rsaParameters.DQ);
            info.AddValue("Exponent", _rsaParameters.Exponent);
            info.AddValue("InverseQ", _rsaParameters.InverseQ);
            info.AddValue("Modulus", _rsaParameters.Modulus);
            info.AddValue("P", _rsaParameters.P);
            info.AddValue("Q", _rsaParameters.Q);
        }
    }
}
