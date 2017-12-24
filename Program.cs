using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace blockchain1
{
    class Program
    {
        static void Main(string[] args)
        {
            var a1 = CryptoUtils.CreateAddress();
            var net = new Network();

            var genesis =  CryptoUtils.PayRequest(net, new RequestParent[] { new RequestParent {amount=100 } }, new string[] { a1.publicKey }, genesis:true);

            var start = DateTime.Now;
            var a2 = CryptoUtils.CreateAddress();
            var order = new RequestParent
            {
                publicKey = genesis.publicKey,
                amount = 10,
                sig=CryptoUtils.Sign(a1.privateKey, genesis.hash)
            };
            CryptoUtils.Pay(net,new RequestParent[] { order },a2.publicKey);

            var ms = DateTime.Now.Subtract(start).TotalMilliseconds;

           
        }

     }
}