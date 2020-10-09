using HitomiViewer.Scripts;
using HitomiViewer.UserControls;
using HitomiViewerLibrary;
using HitomiViewerLibrary.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
                List<Hitomi.HTag> tags = HiyobiTags.Tags;
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
                    Hitomi.HTag tag = tags[i];
                    Thread th = new Thread(new ThreadStart(async () =>
                    {
                        try
                        {
                            string dir = Path.Combine(rootDir, tag.type.ToString());
                            string file = Path.Combine(dir, tag.tag + ".json");
                            if (!Directory.Exists(dir))
                                Directory.CreateDirectory(dir);
                            if (File.Exists(file))
                            {
                                //patcher.Invoke(() => progressBox.ProgressBar.Value++);
                                return;
                            }
                            InternetP parser = new InternetP();
                            int[] ids = parser.ByteArrayToIntArray(await parser.LoadNozomiTag(tag.type.ToString(), tag.tag, false, 0, 9999));
                            JArray arr = JArray.FromObject(ids);
                            File.WriteAllText(file, arr.ToString());
                            Console.WriteLine("{0}/{1}: {2}", i, tags.Count, tag.full);
                        }
                        catch (IOException) { Console.WriteLine("Faild {0}/{1}: {2}", i, tags.Count, tag.full); }
                        catch (Exception ex) { Console.WriteLine("Error {0} : {1}", tag.full, ex.Message); }
                        finally
                        {
                            patcher.Invoke(() =>
                            {
                                progressBox.ProgressBar.Value++;
                                if (progressBox.ProgressBar.Value == progressBox.ProgressBar.Maximum)
                                {
                                    progressBox.Close();
                                    MessageBox.Show("캐시 다운로드가 끝났습니다.");
                                }
                            });
                        }
                    }));
                    th.Start();
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
        public async void Test()
        {
            Hitomi.HTag tag = Hitomi.HTag.Parse("artist:2gou");
            InternetP parser = new InternetP();
            byte[] data = await parser.LoadNozomiTag(tag.type.ToString(), tag.tag, false, 0, 9999);
            int[] ids;
            if (tag.type == Hitomi.HTag.TType.artists.Name)
                ids = parser.ByteArrayToIntArrayBig(data);
            else
                ids = parser.ByteArrayToIntArray(data);
            Console.WriteLine(ids);
        }
    }
}
