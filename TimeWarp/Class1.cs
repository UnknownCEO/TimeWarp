using UnityEngine;
using UnityModManagerNet;

namespace TimeWarp {
    static class Main {
        public static float speedMult = 1f;
        public static string input = "";

        static bool Load(UnityModManager.ModEntry modEntry) {
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            if (value) {
                Time.timeScale = speedMult;
            } else {
                Time.timeScale = 1f;
            }
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry) {
            GUILayout.Label("Game speed multiplier:");
            input = GUILayout.TextField(input, GUILayout.Width(100f));
            if (GUILayout.Button("Apply") && float.TryParse(input, out var t)) {
                if (t < 0) {
                    speedMult = 0f;
                } else {
                    speedMult = t;
                }
                Time.timeScale = speedMult;
            }
        }
    }
}