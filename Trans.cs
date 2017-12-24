using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace blockchain1
{
    [DebuggerDisplay("{this.ToString()}")]
    [Serializable]
    public class Coin
    {
        public string[] parents;
        public string publicKey;
        public string hash;
        public decimal amount;
        public string data;
        public DateTime time;
        public bool available;

        public override string ToString()
        {
            return $"{amount}";
        }
    }
    public class RequestChild
    {
        public string data;
        public decimal amount;
        public string publicKey;
    }
    public class RequestParent
    {
        public string sig;
        public string unlocker;
        public string publicKey;
    }
}
