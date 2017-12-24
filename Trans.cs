using System;
using System.Collections.Generic;
using System.Text;

namespace blockchain1
{
    public class Trans
    {
        public string[] parents;
        public string publicKey;
        public string hash;
        public decimal amount;
        public string data;
        public DateTime time;
    }
    public class RequestTrans
    {
        public string data;
        public decimal amount;
        public string publicKey;
        public RequestParent[] parents;
    }
    public class RequestParent
    {
        public string sig;
        public decimal amount;
        public string publicKey;
    }
}
