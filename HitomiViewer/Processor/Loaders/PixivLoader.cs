using ExtensionMethods;
using HitomiViewer.Api;
using HitomiViewer.Structs;
using HitomiViewer.UserControls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static HitomiViewer.Api.Pixiv;
using Tag = HitomiViewer.Structs.Tag;

namespace HitomiViewer.Processor.Loaders
{
    public class PixivLoaders
    {
        public class Illust : ILoader
        {
            public Action<Pixiv.Illust, int, int> update = null;

            public override void Default()
            {
                base.Default();
                this.update = (Pixiv.Illust h, int index, int max) =>
                {
                    Global.MainWindow.label.Content = $"{index}/{max}";
                    Global.MainWindow.MainPanel.Children.Add(new UserControls.Panels.PixivIllustPanel(h));
                };
            }

            public void Parser(JObject data)
            {
                JArray items = JArray.FromObject(data["illusts"]);
                start(items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    //x_restrict == R-18

                    Pixiv.Illust h = items.ToObject<Pixiv.Illust>();
                    update(h, i, items.Count);
                }
                end();
            }
        }
        public class User : ILoader
        {
            public Action<PixivUser, int, int> update = null;

            public override void Default()
            {
                base.Default();
                this.update = (PixivUser h, int index, int max) =>
                {
                    Global.MainWindow.label.Content = $"{index}/{max}";
                    Global.MainWindow.MainPanel.Children.Add(new UserControls.PixivUserPanel(h));
                };
            }

            public void Parser(JObject data)
            {
                JArray jusers = data["user_previews"] as JArray;
                start(jusers.Count);
                for (int i = 0; i < jusers.Count; i++)
                {
                    JToken juser = jusers[i];
                    PixivUser user = new PixivUser();
                    user.user = juser["user"].ToObject<PixivUser.User>();
                    JArray illusts = juser["illusts"] as JArray;
                    List<Pixiv.Illust> illustlist = new List<Pixiv.Illust>();
                    for (int j = 0; j < illusts.Count; j++)
                    {
                        illustlist.Add(illusts[j].ToObject<Pixiv.Illust>());
                    }
                    user.illusts = illustlist.ToArray();
                    update(user, i, jusers.Count);
                }
                end();
            }
        }
    }
}
