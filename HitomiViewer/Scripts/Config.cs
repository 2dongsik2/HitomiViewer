using ExtensionMethods;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HitomiViewer.Scripts
{
    class Config
    {
        private readonly string path = Global.Config.path;
        private readonly string encryptpath = Global.Config.encryptpath;
        public bool encrypt = false;
        private JObject config;

        public JObject Load()
        {
            if (!File.Exists(path))
                return EncryptLoad();
            config = JObject.Parse(File.ReadAllText(Global.Config.path));
            return config;
        }
        public JObject EncryptLoad()
        {
            if (File.Exists(path))
                return Load();
            if (!File.Exists(encryptpath)) return new JObject();
            byte[] BOrigin = File.ReadAllBytes(encryptpath);
            byte[] Decrypt = FileDecrypt.Default(BOrigin);
            string SOrigin = Encoding.UTF8.GetString(Decrypt);
            return JObject.Parse(SOrigin);
        }
        public Config GetConfig()
        {
            config = Load();
            return this;
        }

        public string StringValue(string path)
        {
            if (config == null) return null;
            if(!config.ContainsKey(path)) return null;
            return config[path].ToString();
        }
        public bool? BoolValue(string path)
        {
            if (config == null) return null;
            if (!config.ContainsKey(path)) return null;
            return bool.Parse(config[path].ToString());
        }
        public IList<T> ArrayValue<T>(string path) where T : class
        {
            if (config == null) return new List<T>();
            if (!config.ContainsKey(path)) return new List<T>();
            return config[path].ToObject<List<T>>();
        }

        public bool Save()
        {
            Global.DownloadFolder = StringValue(Settings.download_folder) ?? "hitomi_downloaded";
            Global.FileEn = BoolValue(Settings.file_encrypt) ?? false;
            Global.AutoFileEn = BoolValue(Settings.download_file_encrypt) ?? false;
            Global.EncryptTitle = BoolValue(Settings.encrypt_title) ?? false;
            Global.RandomTitle = BoolValue(Settings.random_title) ?? false;
            Global.CacheSearch = BoolValue(Settings.cache_search) ?? false;
            Global.OriginThumb = BoolValue(Settings.origin_thumb) ?? false;
            if (Global.DownloadFolder == "") Global.DownloadFolder = "hitomi_downloaded";
            if (Global.MainWindow != null){
                Visibility visibility = Visibility.Visible;
                if (StringValue(Settings.password) == null || BoolValue(Settings.file_encrypt) == false)
                    visibility = Visibility.Collapsed;
                Global.MainWindow.Encrypt.Visibility = visibility;
                Global.MainWindow.Decrypt.Visibility = visibility;
            }
            string path = encrypt ? Global.Config.encryptpath : Global.Config.path;
            byte[] bytes = Encoding.UTF8.GetBytes(config.ToString());
            if (encrypt)
                bytes = FileEncrypt.Encrypt(bytes, Global.Password);
            File.WriteAllBytes(path, bytes);
            return true;
        }
        public bool Save(JObject data)
        {
            this.config = data;
            return Save();
        }
    }
}
