﻿using ExtensionMethods;
using HitomiViewer.Processor;
using HitomiViewer.Structs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Processor
{
    class HiyobiTags
    {
        public static readonly string path = Path.Combine(MainWindow.rootDir, "tagdata.json");
        public static List<Tag> Tags = null;

        public static async void LoadTags()
        {
            if (!File.Exists(path))
            {
                InternetP parser = new InternetP();
                JArray tags = await parser.HiyobiTags();
                Tags = tags.Select(x => Tag.Parse(x.ToString())).ToList();
                Tags.Add(Tag.Parse("language:korean"));
                //Tags = tags.Select(x => new Tag { name = x.ToString(), types = Tag.ParseTypes(x.ToString()), Hitomi = Tag.isHitomi(x.ToString()) }).ToList();
                File.WriteAllText(path, tags.ToString());
            }
            else
            {
                JArray tags = JArray.Parse(File.ReadAllText(path));
                Tags = tags.Select(x => Tag.Parse(x.ToString())).ToList();
                //Tags = tags.Select(x => new Tag { name = x.ToString(), types = Tag.ParseTypes(x.ToString()), Hitomi = Tag.isHitomi(x.ToString()) }).ToList();
            }
        }
    }
}
