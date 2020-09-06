using ExtensionMethods;
using HitomiViewer.Api;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Scripts
{
    class Account
    {
        public static void Load()
        {
            if (Global.OriginPassword == null) return;
            if (!File.Exists("Account.lock")) return;
            byte[] data = File.ReadAllBytes("Account.lock");
            data = FileDecrypt.Default(data);
            string sdata = Encoding.UTF8.GetString(data);
            JObject obj = JObject.Parse(sdata);
            if (obj["pixiv"] != null && obj["pixiv"]["username"] != null && obj["pixiv"]["password"] != null)
            {
                JToken pixv = obj["pixiv"];
                Pixiv pixiv = Global.Account.Pixiv ?? new Pixiv();
                _ = pixiv.Auth(pixv.StringValue("username"), pixv.StringValue("password"), true);
                Global.Account.Pixiv = pixiv;
            }
        }
        public static void Save(string parent, string username, string password)
        {
            if (Global.OriginPassword == null) return;
            JObject obj = new JObject();
            JToken token = new JObject();
            token["username"] = username;
            token["password"] = password;
            obj[parent] = token;
            byte[] data = Encoding.UTF8.GetBytes(obj.ToString());
            data = FileEncrypt.Default(data);
            File.WriteAllBytes("Account.lock", data);
        }
    }
}
