using HitomiViewer.Scripts;
using HitomiViewer.Structs;
using HitomiViewer.UserControls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HitomiViewer.Processor.Cache
{
    class Cache
    {
        public readonly string rootDir = Path.Combine(Global.rootDir, "Cache");

        public Cache()
        {
            if (!Directory.Exists(rootDir))
                Directory.CreateDirectory(rootDir);
        }

        public void TagCache()
        {
            if (HiyobiTags.Tags == null)
                HiyobiTags.LoadTags();
            try
            {
                List<Tag> tags = HiyobiTags.Tags;
                Dispatcher patcher = Global.dispatcher;
                ProgressBox progressBox = null;
                patcher.Invoke(() => {
                    progressBox = new ProgressBox();
                    progressBox.Title = "태그 캐시 다운로드";
                    progressBox.Show();
                    progressBox.ProgressBar.Maximum = tags.Count;
                });
                for (int i = 0; i < tags.Count; i++)
                {
                    Tag tag = tags[i];
                    Thread th = new Thread(new ThreadStart(async () =>
                    {
                        try
                        {
                            if (!tag.Hitomi) return;
                            string dir = Path.Combine(rootDir, tag.types.ToString());
                            string file = Path.Combine(dir, tag.name + ".json");
                            if (!Directory.Exists(dir))
                                Directory.CreateDirectory(dir);
                            if (File.Exists(file))
                            {
                                patcher.Invoke(() => progressBox.ProgressBar.Value++);
                                return;
                            }
                            InternetP parser = new InternetP();
                            int[] ids = parser.ByteArrayToIntArray(await parser.LoadNozomiTag(tag.types.ToString(), tag.name, false));
                            JArray arr = JArray.FromObject(ids);
                            File.WriteAllText(file, arr.ToString());
                            Console.WriteLine("{0}/{1}: {2}", i, tags.Count, tag.full);
                            //patcher.Invoke(() => progressBox.ProgressBar.Value++);
                        }
                        catch (IOException) { Console.WriteLine("Faild {0}/{1}: {2}", i, tags.Count, tag.full); }
                        catch (Exception) { Console.WriteLine(tag.full); }
                        finally
                        {
                            patcher.Invoke(() =>
                            {
                                progressBox.ProgressBar.Value++;
                                if (progressBox.ProgressBar.Value == progressBox.ProgressBar.Maximum)
                                    progressBox.Close();
                            });
                        }
                    }));
                    th.Start();
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
}
