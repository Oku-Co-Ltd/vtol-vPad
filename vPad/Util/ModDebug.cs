/* Copyright 2022 okureya, Omnith LLC. Attributes CC-BY 3.0 */

using System;
using UnityEngine;

namespace vPad.Util
{
    /// <summary> Wrapper utility for logging with tagged messages that relate to this mod. </summary>
    public static class ModDebug
    {
        private const string ModDebugSign = "[vPadOku]";

        public static void Log(string message) => Debug.Log($"{ModDebugSign} {message}");
        public static void LogWarning(string message) => Debug.LogWarning($"{ModDebugSign} {message}");
        public static void LogError(string message) => Debug.LogError($"{ModDebugSign} {message}");
        public static void LogException(Exception e) => Debug.LogException(e);
    }
}
