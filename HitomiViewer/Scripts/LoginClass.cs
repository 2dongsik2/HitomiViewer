using HitomiViewer.Encryption;
using HitomiViewer.Scripts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HitomiViewer.Scripts
{
    class LoginClass
    {
        private byte[] BOrigin;
        private byte[] Decrypt;

        /// <summary>
        /// 로그인 실행
        /// </summary>
        public void Run()
        {
            if (!File.Exists(Global.Config.path) && File.Exists(Global.Config.encryptpath))
                Encrypted();
            else
                Plain();
        }
        /// <summary>
        /// 암호화된 json 파일로 로그인
        /// </summary>
        private void Encrypted()
        {
            BOrigin = File.ReadAllBytes(Global.Config.encryptpath);
            LoginWindow lw = new LoginWindow();
            lw.CheckPassword = CheckPassword1;
            if (lw.ShowDialog().Value)
            {
                try
                {
                    byte[] Decrypt = FileDecrypt.Decrypt(BOrigin, FilePassword.Default(lw.Password.Password));
                    string SOrigin = Encoding.UTF8.GetString(Decrypt);
                    Global.Password = lw.Password.Password;
                }
                catch { Environment.Exit(0); }
            }
        }
        /// <summary>
        /// 비암호화된 json 파일로 로그인
        /// </summary>
        private void Plain()
        {
            ConfigFileData config = new ConfigFile().Load();
            if (config.password.Data != null)
            {
                LoginWindow lw = new LoginWindow();
                lw.CheckPassword = CheckPassword2;
                if (!lw.ShowDialog().Value) Environment.Exit(0);
                Global.Password = lw.Password.Password;
            }
        }

        private bool CheckPassword2(string password)
        {
            return SHA256.Hash(password) == new ConfigFile().Load().password.Get<string>();
        }

        private bool CheckPassword1(string password)
        {
            bool @try = FileDecrypt.TryDecrypt(ref Decrypt, BOrigin, FilePassword.Default(password));
            return @try;
        }
    }
}
