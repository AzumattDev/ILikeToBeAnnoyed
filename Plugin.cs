using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ILikeToBeAnnoyed
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ILikeToBeAnnoyedPlugin : BaseUnityPlugin
    {
        internal const string ModName = "ILikeToBeAnnoyed";
        internal const string ModVersion = "1.0.1";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        internal static string ConnectionError = "";

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource ILikeToBeAnnoyedLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        #region ConfigOptions

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        #endregion
    }
    
    [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Start))]
    static class MonsterAIAwakePatch
    {
        static void Postfix(MonsterAI __instance)
        {
            if (!__instance.m_nview || !__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner())
                return;

            if (ZNetScene.instance == null)
                return;

            if (Utils.GetPrefabName(__instance.gameObject).StartsWith("Wolf"))
            {
                __instance.m_idleSound = new EffectList()
                {
                    m_effectPrefabs = new EffectList.EffectData[]
                    {
                        new()
                        {
                            m_prefab = ZNetScene.instance.GetPrefab("sfx_wolf_haul"),
                            m_enabled = true,
                            m_variant = -1,
                            m_attach = false,
                            m_inheritParentRotation = false,
                            m_inheritParentScale = false,
                            m_randomRotation = false,
                            m_scale = false,
                            m_childTransform = ""
                        },
                    }
                };
            }
        }
    }
    
    
    [HarmonyPatch(typeof(ZNetScene),nameof(ZNetScene.Awake))]
    static class ZNetSceneAwakePatch
    {
        static void Prefix(ZNetScene __instance)
        {
            foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (gameObject.name != "sfx_wolf_haul") continue; 
                // The default game doesn't have this registered in the ZNetScene. Force it.
                __instance.m_prefabs.Add(gameObject);
                break;
            }
        }
    }

}