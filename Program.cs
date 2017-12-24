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

           var  genesisTran=  CryptoUtils.Pay(net, 
               new RequestParent[] { new RequestParent { } },
               new RequestChild[] { new RequestChild { amount = 100, publicKey = a1.publicKey } },genesis:true);

            var start = DateTime.Now;
            var a2 = CryptoUtils.CreateAddress();
            var a3 = CryptoUtils.CreateAddress();
            var a23Coins= CryptoUtils.Pay(net,new RequestParent[] {
                new RequestParent{ publicKey = a1.publicKey, sig=CryptoUtils.Sign(a1.privateKey, genesisTran.First().hash) }
            }, new RequestChild[] {
                new RequestChild { amount = 10, publicKey = a2.publicKey },
                new RequestChild { amount = 90, publicKey = a3.publicKey },
            });

            var a4 = CryptoUtils.CreateAddress();
            CryptoUtils.Pay(net, new RequestParent[] {
                new RequestParent{ publicKey = a2.publicKey, sig=CryptoUtils.Sign(a2.privateKey, a23Coins[0].hash) },
                new RequestParent{ publicKey = a3.publicKey, sig=CryptoUtils.Sign(a3.privateKey, a23Coins[1].hash) }
            }, new RequestChild[] {
                new RequestChild { amount = 100, publicKey = a4.publicKey },
            });
            var ms = DateTime.Now.Subtract(start).TotalMilliseconds;
           
        }

     }
}