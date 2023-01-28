using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using TMPro;
using I2.Loc;
using HarmonyLib;
using System.Reflection.Emit;

namespace RCG2_L10n
{

    public class L10nMod : MelonMod
    {
        private AssetBundle m_AssetBundle;
        private static Dictionary<string, FontReplacement> fontsDictionary;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("RCG2 l10n started");

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("rcg2_l10n.rcg2-l10n.assetbundle"))
            using (var tempStream = new MemoryStream((int)stream.Length))
            {
                stream.CopyTo(tempStream);
                m_AssetBundle = AssetBundle.LoadFromMemory(tempStream.ToArray(), 0);
            }


            fontsDictionary = new Dictionary<string, FontReplacement>
            {
                { "BalsamiqSans-Regular SDF", new FontReplacement(m_AssetBundle.LoadAsset<TMP_FontAsset>("BalsamiqSansRU-Regular SDF"), false) },
                { "BalsamiqSans-BoldItalic SDF", new FontReplacement(m_AssetBundle.LoadAsset<TMP_FontAsset>("BalsamiqSansRU-BoldItalic SDF"), false) },
                //{ "LiberationSans SDF", new FontReplacement(m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Medium SDF"), false) },

                { "NotoSansJP-Black SDF", new FontReplacement(m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Black SDF"), false) },
                { "NotoSansJP-Bold SDF", new FontReplacement(m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Bold SDF"), false) },
                { "NotoSansJP-Light SDF", new FontReplacement(m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Light SDF"), false) },
                { "NotoSansJP-Medium SDF", new FontReplacement(m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Medium SDF"), false) },

                { "NotoSansJP-Medium SDF Black Outline", new FontReplacement(m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Medium SDF Black Outline"), false) },
                { "NotoSansJP-Medium SDF Drop Shadow", new FontReplacement(m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Medium SDF Drop Shadow"), false) },
                { "NotoSansCJK-Black SDF BlackOutline", new FontReplacement(m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Black SDF Black Outline"), false) }
            };

            // Singleton:RCG.UI.ScreenManager/MainUICanvas(Clone)/UI_Splash/LegalPart1/Text/
            // Legal_Screen_1_Text

            LoggerInstance.Msg("PREINIT: Fonts loaded");

            LocalizationManager.UpdateSources();
            var translation_csv = m_AssetBundle.LoadAsset<TextAsset>("translation");
            // Remove garbage
            LocalizationManager.Sources[0].mLanguages.RemoveRange(9, 7);
            LocalizationManager.Sources[0].Import_CSV(string.Empty, translation_csv.text, eSpreadsheetUpdateMode.Replace, ';');
            // Need to manually define code
            LocalizationManager.Sources[0].mLanguages[9].Code = "ru";
            LocalizationManager.LocalizeAll(true);
            LoggerInstance.Msg("Updated translation");

            //TMP_ResourceManager
            UpdateFonts();

            HarmonyInstance.PatchAll(typeof(RCG.UI.Screens.UI_CellPhone_Settings));
            HarmonyInstance.PatchAll(typeof(RCG.UI.Screens.UI_Settings));
            LoggerInstance.Msg("Harmony patched");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Scene {sceneName} with build index {buildIndex} has been loaded!");
            UpdateFonts();
        }

        private void UpdateFonts()
        {
            TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var font in fonts)
            {
                LoggerInstance.Msg($"Found font: {font.name}");
                if (fontsDictionary.ContainsKey(font.name) && !fontsDictionary[font.name].Replaced)
                {
                    // Some fallback fonts fave thier fallbacks, so we insert ours before
                    font.fallbackFontAssetTable.Insert(0, fontsDictionary[font.name].FailbackFont);
                    var temp = fontsDictionary[font.name];
                    temp.Replaced = true;
                    fontsDictionary[font.name] = temp;
                    LoggerInstance.Msg($"Added failback {fontsDictionary[font.name].FailbackFont.name} to {font.name}");
                }
            }
        }

    }


    [HarmonyPatch(typeof(RCG.UI.Screens.UI_CellPhone_Settings))]
    [HarmonyPatch("Open")]
    public static class UI_CellPhone_Settings_Open_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var info = AccessTools.Field(typeof(RCG.UI.Screens.UI_CellPhone_Settings), "m_languageSelector");
            
            var startIndex = codes.FindIndex(ins => ins.opcode == OpCodes.Ldfld && ins.operand.Equals(info));
            codes[startIndex + 2].opcode = OpCodes.Ldc_I4_S;
            codes[startIndex + 2].operand = 9;

            return codes;
        }
    }

    [HarmonyPatch(typeof(RCG.UI.Screens.UI_Settings))]
    [HarmonyPatch("OnGainFocus")]
    public static class UI_Settings_Open_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var info = AccessTools.Field(typeof(RCG.UI.Screens.UI_Settings), "m_languageSelector");
            
            var startIndex = codes.FindIndex(ins => ins.opcode == OpCodes.Ldfld && ins.operand.Equals(info));
            codes[startIndex + 2].opcode = OpCodes.Ldc_I4_S;
            codes[startIndex + 2].operand = 9;

            return codes;
        }
    }
}
