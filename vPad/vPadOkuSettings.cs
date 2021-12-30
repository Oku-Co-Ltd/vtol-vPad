/* Copyright 2022 okureya, Omnith LLC. Attributes CC-BY 3.0 */

using System;
using System.Collections.Generic;
using UnityEngine.Events;
using vPad.Util;

namespace vPad
{
    /// <summary> Serializable settings class specifically for the vPad mod. </summary>
    [Serializable]
    public sealed class vPadOkuSettings : IModSettings<vPadOkuSettings>
    {
        public SerializableDictionary<string, bool> Enabled;

        [NonSerialized] public Dictionary<string, UnityAction<bool>> EnableActions;

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
            Enabled.dictionary = new Dictionary<string, bool>
            {
                {"F/A-26B", true},
                {"F-45A", false},
                {"AV-42C", false},
                {"AH-94", false}
            };
            return this;
        }
    }
}
