using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using System;
using System.Reflection;

namespace TimeWarp {
    static class Main {
        public static int ID_NUM = 42;
        public static float speedMult = 1f;
        public static string input = "1";
        public static bool enabled;
        public static UnityModManager.ModEntry mod;

        static bool Load(UnityModManager.ModEntry modEntry) {
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            mod = modEntry;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            // Turning the mod on or off resets to normal speed
            UpdateTimeScale(1f);
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry) {
            GUILayout.Label("Current game speed multiplier:");
            GUILayout.Label(Time.timeScale.ToString(), GUILayout.Width(100f));
            GUILayout.Label("New game speed multiplier:");
            input = GUILayout.TextField(input, GUILayout.Width(100f));
            if (GUILayout.Button("Apply") && float.TryParse(input, out var t)) {
                UpdateTimeScale(t);
            }
        }

        public static void UpdateTimeScale(float newTimeScale) {
            if (newTimeScale < 0) {
                speedMult = 0f;
            } else {
                speedMult = newTimeScale;
            }
            if (Time.timeScale != speedMult) {
                Time.timeScale = speedMult;
                input = speedMult.ToString();
                // Send new time scale to other players in multiplayer (only works for other players that have the mod)
                try {
                    if (!PhotonNetwork.offlineMode) {
                        Corescript.CoreObject.GetComponent<PhotonView>().RPC("ReciveText", PhotonTargets.Others, new object[] {
                            speedMult.ToString(),
                            ID_NUM
                        });
                    }
                }
                catch (NullReferenceException e) {
                    // Catch Exception that happens if mod is enabled before a game is started
                }
            }
        }
    }
    [HarmonyPatch(typeof(IngameChat))]
    [HarmonyPatch("ReciveText")]
    static class TimeWarpReceive_Patch {
        static bool Prefix(IngameChat __instance, string SentText, int Team) {
            if (!Main.enabled)
                return true;

            try {
                if (Team == Main.ID_NUM) { // Use the Team number to identify if this mod is being sent data
                    if (float.TryParse(SentText, out var t)) {
                        Main.UpdateTimeScale(t);
                        return false; // Don't let the original "ReciveText" run
                    }
                }
            } catch (Exception e) {
                Main.mod.Logger.Error(e.ToString());
            }
            return true;
        }
    }
}