using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TradeMarket.DataTransfering.Bitmex.Rest
{
    public class UserAuthentication
    {
        [DataMember(Name = "api-key")]
        public string Key;

        [DataMember(Name = "api-expires")]
        public string Expires;

        [DataMember(Name = "api-signature")]
        public string Signature;

        public UserAuthentication(string key,string secret,HttpMethod method,Uri uri,String postData)
        {
            this.Key = key;
            this.Expires = GetExpires().ToString();
            string message = method + uri.AbsoluteUri + Expires + postData;
            byte[] signatureBytes = hmacsha256(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(message));
            this.Signature = ByteArrayToString(signatureBytes);

        }

        public string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private object GetExpires()
        {
            //пока логин захардожен на час
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600; 
        }

        private byte[] hmacsha256(byte[] keyByte, byte[] messageBytes)
        {
            using (var hash = new HMACSHA256(keyByte))
            {
                return hash.ComputeHash(messageBytes);
            }
        }
    }
}
