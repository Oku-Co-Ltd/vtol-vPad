/* Written by okureya // Omnith LLC */

using System;
using System.IO;
using Valve.Newtonsoft.Json;

namespace vPad.Util
{
    /// <summary> Interface to abstract away the mod settings. Makes it more modular. </summary>
    public interface IModSettings<out T>
    {
        /// <summary> Returns a <see cref="T"/> instance, set to default values.
        /// Implementation is dependent on derived type. </summary>
        T Default();
    }

    /// <summary>
    /// Utility helpers for managing mod settings.
    /// </summary>
    public static class ModSettingsUtil
    {
        /// <summary>
        /// Loads mod settings from a file, creating the file if it does not exist.
        /// </summary>
        public static T LoadFromFile<T>(string modFolder) where T : IModSettings<T>
        {
            ModDebug.Log("Checking for mod folder: " + modFolder);

            if (Directory.Exists(modFolder))
            {
                ModDebug.Log(modFolder + " exists!");
                try
                {
                    ModDebug.Log("Checking for settings: " + modFolder + @"\settings.json");
                    string temp = File.ReadAllText(modFolder + @"\settings.json");

                    var settings = JsonConvert.DeserializeObject<T>(temp);
                    vPadOku.Instance.HaveSettingsChanged.Value = false;
                    return settings;
                }
                catch
                {
                    ModDebug.Log("Settings file not found, creating one");
                    // calls constructor for derived type (can't just do `new T()`)
                    var settings = (T)Activator.CreateInstance(typeof(T));
                    SaveToFile(modFolder, settings);
                    return settings;
                }
            }
            ModDebug.LogWarning("Mod folder not found?");
            return (T)Activator.CreateInstance(typeof(T));
        }
        /// <summary>
        /// Saves mod settings to a file, creating a new one if it doesn't exist.
        /// </summary>
        public static void SaveToFile<T>(string modFolder, T settings) where T : IModSettings<T>
        {
            ModDebug.Log("Checking for mod folder: " + modFolder);

            if (Directory.Exists(modFolder))
            {
                ModDebug.Log("Saving settings to file");
                File.WriteAllText(modFolder + @"\settings.json", JsonConvert.SerializeObject(settings));
                vPadOku.Instance.HaveSettingsChanged.Value = false;
            }
            else
            {
                ModDebug.LogWarning("Mod folder not found?");
            }
        }
    }
}
