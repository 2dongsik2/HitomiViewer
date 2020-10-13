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
            byte[] bytes = Encoding.UTF8.GetBytes(ToJObject().ToString());//JObject.FromObject(config).ToString());
            if (encrypt)
                bytes = FileEncrypt.Encrypt(bytes, Global.Password);
            File.WriteAllBytes(path, bytes);
            return true;
        }
        public JObject ToJObject()
        {
            JObject data = new JObject();
            var items = typeof(ConfigFileData).GetFields()
                .ToList();
            foreach (var item in items)
            {
                ConfigFileType type = (ConfigFileType)item.GetValue(this.config);
                data[type.GetName()] = (JToken)type.GetDynamic();
            }
            return data;
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
        public readonly ConfigFileType password = new ConfigFileType(null, "password", typeof(string));
        public readonly ConfigFileType favorites = new ConfigFileType(null, "favorites", typeof(Array));
        public readonly ConfigFileType block_tags = new ConfigFileType(null, "block-tags", typeof(bool), false);
        public readonly ConfigFileType except_tags = new ConfigFileType(null, "except-tags", typeof(Array));
        public readonly ConfigFileType cache_search = new ConfigFileType(null, "cachesearch", typeof(bool), false);
        public readonly ConfigFileType file_encrypt = new ConfigFileType(null, "file-encrypt", typeof(bool), false);
        public readonly ConfigFileType random_title = new ConfigFileType(null, "random-title", typeof(bool), false);
        public readonly ConfigFileType encrypt_title = new ConfigFileType(null, "encrypt-title", typeof(bool), false);
        public readonly ConfigFileType origin_thumb = new ConfigFileType(null, "originalthumbnail", typeof(bool), false);
        public readonly ConfigFileType download_file_encrypt = new ConfigFileType(null, "download-file-encrypt", typeof(bool), false);
        public readonly ConfigFileType download_folder = new ConfigFileType(null, "download-file", typeof(string), "hitomi_downloaded");
    }
    public class ConfigFileType
    {
        public dynamic Data { get; set; }
        public Type Type { get; set; }
        public string Name { get; private set; }
        public object Default { get; private set; }

        public ConfigFileType(object data, string name, Type type, object Default = null) => (Data, Name, Type, this.Default) = (data, name, type, Default);
        public T Get<T>()
        {
            if (Data == null)
                return (T)Default;
            if (Type != typeof(T))
                throw new InvalidCastException();
            if (Data.GetType() != typeof(T))
                throw new InvalidCastException();
            if (Data.GetType() == typeof(string) && (string)Data == "")
                return (T)Default;
            return (T)(Data ?? Default);
        }
        public dynamic GetDynamic() => Data ?? Default;
        public string GetName() => Name;
        public void Set(object data)
        {
            if (data == null || Data == null)
                Data = data;
            if (Type != data.GetType())
                throw new InvalidCastException();
            if (Data.GetType() != data.GetType())
                throw new InvalidCastException();
            Data = data;
        }
    }
}
