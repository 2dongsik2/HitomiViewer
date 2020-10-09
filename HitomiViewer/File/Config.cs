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
    class ConfigFile : IDisposable
    {
        private readonly string path = Global.Config.path;
        private readonly string encryptpath = Global.Config.encryptpath;
        public bool encrypt = false;
        public ConfigFileData config;

        public ConfigFileData Load()
        {
            if (!File.Exists(path))
                return EncryptLoad();
            JObject data = JObject.Parse(File.ReadAllText(Global.Config.path));
            return data.ToObjectExceptNull<ConfigFileData>();
        }
        public ConfigFileData EncryptLoad()
        {
            if (File.Exists(path))
                return Load();
            if (!File.Exists(encryptpath)) throw new FileNotFoundException();
            byte[] BOrigin = File.ReadAllBytes(encryptpath);
            byte[] Decrypt = FileDecrypt.Default(BOrigin);
            string SOrigin = Encoding.UTF8.GetString(Decrypt);
            JObject data = JObject.Parse(SOrigin);
            return data.ToObjectExceptNull<ConfigFileData>();
        }
        public ConfigFile GetConfig()
        {
            config = Load();
            return this;
        }

        public bool Save()
        {
            if (Global.MainWindow != null){
                Visibility visibility = Visibility.Visible;
                if (config.password.Get<string>() == null || config.file_encrypt.Get<bool>() == false)
                    visibility = Visibility.Collapsed;
                Global.MainWindow.Encrypt.Visibility = visibility;
                Global.MainWindow.Decrypt.Visibility = visibility;
            }
            string path = encrypt ? Global.Config.encryptpath : Global.Config.path;
            byte[] bytes = Encoding.UTF8.GetBytes(JObject.FromObject(config).ToString());
            if (encrypt)
                bytes = FileEncrypt.Encrypt(bytes, Global.OriginPassword);
            File.WriteAllBytes(path, bytes);
            return true;
        }
        public bool Save(JObject data)
        {
            this.config = data.ToObjectExceptNull<ConfigFileData>();
            return Save();
        }

        public void Dispose()
        {
            config = null;
        }
    }

    public class ConfigFileData
    {
        public readonly ConfigFileType password = new ConfigFileType(null, "password");
        public readonly ConfigFileType favorites = new ConfigFileType(null, "favorites");
        public readonly ConfigFileType block_tags = new ConfigFileType(null, "block-tags");
        public readonly ConfigFileType except_tags = new ConfigFileType(null, "except-tags");
        public readonly ConfigFileType cache_search = new ConfigFileType(null, "cachesearch", false);
        public readonly ConfigFileType file_encrypt = new ConfigFileType(null, "file-encrypt", false);
        public readonly ConfigFileType random_title = new ConfigFileType(null, "random-title", false);
        public readonly ConfigFileType encrypt_title = new ConfigFileType(null, "encrypt-title", false);
        public readonly ConfigFileType origin_thumb = new ConfigFileType(null, "originalthumbnail", false);
        public readonly ConfigFileType download_file_encrypt = new ConfigFileType(null, "download-file-encrypt", false);
        public readonly ConfigFileType download_folder = new ConfigFileType(null, "download-file", "hitomi_downloaded");
    }
    public class ConfigFileType
    {
        public object Data { get; set; }
        public string Name { get; private set; }
        public object Default { get; private set; }

        public ConfigFileType(object data, string name, object Default = null) => (Data, Name, this.Default) = (data, name, Default);
        public T Get<T>()
        {
            if (Data == null)
                return (T)Default;
            if (Data.GetType() != typeof(T))
                throw new InvalidCastException();
            if (Data.GetType() == typeof(string) && (string)Data == "")
                return (T)Default;
            return (T)(Data ?? Default);
        }
    }
}
