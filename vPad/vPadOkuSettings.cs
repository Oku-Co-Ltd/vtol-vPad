/* Written by okureya // Omnith LLC */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;
using vPad.Util;

namespace vPad
{
    /// <summary> Serializable settings class specifically for the vPad mod. </summary>
    [Serializable]
    public sealed class vPadOkuSettings : IModSettings<vPadOkuSettings>
    {
        public string Version { get; set; }

        public SerializableDictionary<string, bool> Enabled;
        public float vPadScale = 1f;

        [NonSerialized] public Dictionary<string, UnityAction<bool>> EnableActions;
        [NonSerialized] public UnityAction<float> ScaleAction;

        public vPadOkuSettings()
        {
            Enabled = new SerializableDictionary<string, bool>();
            EnableActions = new Dictionary<string, UnityAction<bool>>();
            // set to default values
            Default();
        }

        /// <inheritdoc cref="IModSettings{vPadOkuSettings}"/>
        public vPadOkuSettings Default()
        {
            Version = typeof(vPadOkuSettings).Assembly.GetName().Version.ToString(3);
            Enabled.dictionary = new Dictionary<string, bool>
            {
                {"F/A-26B", true},
                {"F-45A", false},
                {"AV-42C", true},
                {"AH-94", false}
            };
            vPadScale = 1.0f;
            return this;
        }
    }
}
