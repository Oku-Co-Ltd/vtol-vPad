/* Written by okureya // Omnith LLC */

using System.IO;
using Harmony;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using vPad.Util;

namespace vPad
{
    /// <summary> Main class for the mod. </summary>
    public sealed class vPadOku : VTOLMOD
    {
        ////*** statics ***////

        public static vPadOku Instance { get; private set; }
        public static vPadOkuSettings Settings;

        private const string vPadAssetBundle = "vpad_oku";
        private const string vPadPrefabName = "vPad.prefab";

        ////*** per instance ***////
        
        // the main vPad object
        public GameObject vPadPrefab;

        public readonly ReactiveProperty<bool> HaveSettingsChanged = new ReactiveProperty<bool>(false);

        /// <inheritdoc cref="VTOLMOD.ModLoaded"/>
        public override void ModLoaded()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("oku.vPad");
            harmonyInstance.PatchAll();

            Instance = this;

            // load prefab from asset bundle
            string pathToBundle = Path.Combine(ModFolder, vPadAssetBundle);
            ModDebug.Log($"Loading asset bundle from '{pathToBundle}'");
            var assetBundle = AssetBundle.LoadFromFile(pathToBundle);
            vPadPrefab = assetBundle.LoadAsset<GameObject>(vPadPrefabName);
            if (!vPadPrefab)
            {
                ModDebug.LogError("Couldn't load prefab from asset bundle");
            }
            ModDebug.Log("Successfully loaded asset bundle");

            // build settings, loading from file as necessary
            ModDebug.Log("Initializing settings");
            Settings = ModSettingsUtil.LoadFromFile<vPadOkuSettings>(ModFolder);
            // build a mod settings page
            ModDebug.Log("Building VTOL settings window");
            Settings modSettings = new Settings(this);
            modSettings.CreateCustomLabel("vPad Settings // cc. okureya");
            modSettings.CreateCustomLabel("");
            modSettings.CreateCustomLabel("Aircraft Enable");
            modSettings.CreateCustomLabel("> What aircraft should the vPad be enabled for?");
            // hook up some runtime stuff and build page at the same time
            foreach (var kvp in Settings.Enabled.dictionary)
            {
                ModDebug.Log($"Adding '{kvp.Key}' to settings...");
                void Action(bool val)
                {
                    Settings.Enabled.dictionary[kvp.Key] = val;
                    HaveSettingsChanged.Value = true;
                }
                // add delegate to the actions dict
                Settings.EnableActions.Add(kvp.Key, Action);
                // then build a setting entry that hooks up to it
                modSettings.CreateCustomLabel($"{kvp.Key}:");
                modSettings.CreateBoolSetting($"(Default = {kvp.Value.ToString()})", Settings.EnableActions[kvp.Key], Settings.Enabled.dictionary[kvp.Key]);
            }
            modSettings.CreateCustomLabel("<< modded plane support on the way! soon... >>");

            // when HaveSettingsChanged is modified, this will run every time
            HaveSettingsChanged.AsObservable().Subscribe(OnSettingsChanged);
            // then create the settings menu for the mod
            VTOLAPI.CreateSettingsMenu(modSettings);
            ModDebug.Log("ModLoaded() complete!");
        }

        /// <summary>
        /// Called once per frame, as per default Unity MonoBehaviour procedure.
        /// </summary>
        void Update()
        {
        }

        /// <summary>
        /// Called once per fixed time interval (e.g. physics), as per default Unity MonoBehaviour procedure.
        /// </summary>
        void FixedUpdate()
        {
        }

        /// <summary>
        /// Called when the application is exiting. Clean up stuff here!
        /// </summary>
        private void OnApplicationQuit()
        {
        }

        /// <summary>
        /// This function is called every time a scene is loaded, as defined in Awake().
        /// </summary>
        private void SceneLoaded(VTOLScenes scene)
        {
            //If you want something to happen in only one (or more) scenes, this is where you define it.

            //For example, lets say you're making a mod which only does something in the ready room and the loading scene. This is how your code could look:
            switch (scene)
            {
                case VTOLScenes.ReadyRoom:
                    //Add your ready room code here
                    break;
                case VTOLScenes.LoadingScene:
                    //Add your loading scene code here
                    break;
            }
        }

        /// <summary>
        /// Called when HaveSettingsChanged is modified, and if it is true, then save changes to file.
        /// </summary>
        private void OnSettingsChanged(bool haveChanged)
        {
            if (haveChanged)
            {
                ModDebug.Log("Settings were changed, saving changes!");
                ModSettingsUtil.SaveToFile(ModFolder, Settings);
            }
        }
    }
}