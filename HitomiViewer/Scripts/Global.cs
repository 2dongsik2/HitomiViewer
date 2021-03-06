﻿using HitomiViewer.Processor;
using HitomiViewer.Scripts;
using HitomiViewerLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HitomiViewer
{
    public class Global
    {
        public static readonly string rootDir = AppDomain.CurrentDomain.BaseDirectory;

        public static MainWindow MainWindow;
        public const string basicPatturn = "*.jpg";
        public static Color background = Colors.White;
        public static Color Menuground = Color.FromRgb(240, 240, 240);
        public static Color MenuItmclr = Colors.White;
        public static Color childcolor = Colors.White;
        public static Color imagecolor = Colors.LightGray;
        public static Color panelcolor = Colors.White;
        public static Color fontscolor = Colors.Black;
        public static Color outlineclr = Colors.Black;
        public static Color artistsclr = Colors.Blue;
        public const int Magnif = 4;
        public const int RandomStringLength = 16;
        public static string Password;
        public static ConfigFileData config = new ConfigFile().Load();
        public static Dispatcher dispatcher;
        public static BitmapImage NoImage { private get; set; }
        public static Func<string[], string[]> FolderSort;

        public class Config
        {
            public static readonly string path = Path.Combine(MainWindow.rootDir, "config.json");
            public static readonly string encryptpath = Path.Combine(MainWindow.rootDir, "config.lock");
        }
        public class Account
        {
            public static Pixiv Pixiv;
        }

        public static void Setup()
        {
            NoImage = Processor.ImageProcessor.FromResource("NoImage.jpg").ToImage();
        }
    }
}
