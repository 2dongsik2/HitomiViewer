using HitomiViewer.UserControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HitomiViewer.Plugin
{
    internal static class PluginHandler
    {
        private static readonly List<IPlugin> loadedPlugins = new List<IPlugin>();

        public static void LoadPlugins()
        {
            string pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            if (!Directory.Exists(pluginDirectory))
                Directory.CreateDirectory(pluginDirectory);
            Console.WriteLine("Plugin Loading Start!");
            List<Assembly> loadedAssemblies = new List<Assembly>();
            string[] pluginFiles = Directory.GetFiles(pluginDirectory, "*", SearchOption.AllDirectories);
            foreach (string pluginFile in pluginFiles)
            {
                if (Path.GetExtension(pluginFile).ToLower() == ".dll")
                {
                    try
                    {
                        Assembly loadedAssembly = Assembly.UnsafeLoadFrom(pluginFile);
                        loadedAssemblies.Add(loadedAssembly);
                        Console.WriteLine("Plugin Loaded " + pluginFile);
                    }
                    catch
                    {
                        Console.WriteLine("Plugin Error" + pluginFile);
                    }
                }
            }
            Type dmpInterfaceType = typeof(IPlugin);

            foreach (Assembly loadedAssembly in loadedAssemblies)
            {
                Type[] loadedTypes = loadedAssembly.GetExportedTypes();
                foreach (Type loadedType in loadedTypes)
                {
                    Type[] typeInterfaces = loadedType.GetInterfaces();
                    bool containsDMPInterface = false;
                    foreach (Type typeInterface in typeInterfaces)
                    {
                        if (typeInterface == dmpInterfaceType)
                        {
                            containsDMPInterface = true;
                        }
                    }
                    if (containsDMPInterface)
                    {
                        Console.WriteLine("Loading plugin: " + loadedType.FullName);

                        try
                        {
                            IPlugin pluginInstance = CreatePlugin(loadedType);

                            if (pluginInstance != null)
                            {
                                Console.WriteLine("Loaded plugin: " + loadedType.FullName);

                                loadedPlugins.Add(pluginInstance);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error loading plugin " + loadedType.FullName + "(" + loadedType.Assembly.FullName + ") Exception: " + ex.ToString());
                        }
                    }
                }
            }
            Console.WriteLine("Plugin Loading End!");
        }

        private static IPlugin CreatePlugin(Type loadedType)
        {
            try
            {
                IPlugin pluginInstance = Activator.CreateInstance(loadedType) as IPlugin;
                return pluginInstance;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error to activate plugin \"" + loadedType.Name + "\", Message: " + e.Message);
                return null;
            }
        }

        public static void FireOnInit(MainWindow main)
        {
            foreach (var plugin in loadedPlugins)
            {
                try
                {
                    plugin.OnInit(main);
                }
                catch(Exception e)
                {
                    Type type = plugin.GetType();
                    Console.WriteLine("Error in OnInit event for " + type.FullName + " (" + type.Assembly.FullName + "), Exception: " + e);
                }
            }
        }
        public static void FireOnDelayInit(MainWindow main)
        {
            foreach (var plugin in loadedPlugins)
            {
                try
                {
                    plugin.OnDelayInit(main);
                }
                catch (Exception e)
                {
                    Type type = plugin.GetType();
                    Console.WriteLine("Error in OnDelayInit event for " + type.FullName + " (" + type.Assembly.FullName + "), Exception: " + e);
                }
            }
        }
        public static void FireOnHitomiPanelInit(HitomiPanel panel)
        {
            foreach (var plugin in loadedPlugins)
            {
                try
                {
                    plugin.OnHitomiPanelInit(panel);
                }
                catch (Exception e)
                {
                    Type type = plugin.GetType();
                    Console.WriteLine("Error in OnHitomiPanelInit event for " + type.FullName + " (" + type.Assembly.FullName + "), Exception: " + e);
                }
            }
        }
        public static void FireOnHitomiPanelDelayInit(HitomiPanel panel)
        {
            foreach (var plugin in loadedPlugins)
            {
                try
                {
                    plugin.OnHitomiPanelDelayInit(panel);
                }
                catch (Exception e)
                {
                    Type type = plugin.GetType();
                    Console.WriteLine("Error in OnHitomiPanelDelayInit event for " + type.FullName + " (" + type.Assembly.FullName + "), Exception: " + e);
                }
            }
        }
        public static void FireOnHitomiChangeColor(HitomiPanel panel)
        {
            foreach (var plugin in loadedPlugins)
            {
                try
                {
                    plugin.OnHitomiChangeColor(panel);
                }
                catch (Exception e)
                {
                    Type type = plugin.GetType();
                    Console.WriteLine("Error in OnHitomiChangeColor event for " + type.FullName + " (" + type.Assembly.FullName + "), Exception: " + e);
                }
            }
        }

    }
}
