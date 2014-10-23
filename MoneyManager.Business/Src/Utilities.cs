﻿using System.Reflection;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace MoneyManager.Src
{
    internal class Utilities
    {
        public static string GetVersion()
        {
            return Assembly.Load(new AssemblyName("MoneyManager.WindowsPhone")).FullName.Split('=')[1].Split(',')[0];
        }

        public static string GetTranslation(string text)
        {
            return ResourceLoader.GetForViewIndependentUse().GetString(text);
        }

        public static int GetMaxId()
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            return roamingSettings.Values.Count;
        }
    }
}