/*
This file is part of River City Girls 2 L10n project
SPDX-License-Identifier: GPL-3.0-or-later
(c) 2023 Azamat H. Hackimov <azamat.hackimov@gmail.com>
*/

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
        private static Dictionary<string, TMP_FontAsset> m_FontsDictionary;
        private static List<TMP_FontAsset> m_FontsReplaced;

        public L10nMod()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("rcg2_l10n.rcg2-l10n.assetbundle"))
            using (var tempStream = new MemoryStream((int)stream.Length))
            {
                stream.CopyTo(tempStream);
                m_AssetBundle = AssetBundle.LoadFromMemory(tempStream.ToArray(), 0);
            }

            m_FontsDictionary = new Dictionary<string, TMP_FontAsset>
            {
                { "BalsamiqSans-Regular SDF", m_AssetBundle.LoadAsset<TMP_FontAsset>("BalsamiqSansRU-Regular SDF") },
                { "BalsamiqSans-BoldItalic SDF", m_AssetBundle.LoadAsset<TMP_FontAsset>("BalsamiqSansRU-BoldItalic SDF") },
                { "DotGothic16-Regular SDF", m_AssetBundle.LoadAsset<TMP_FontAsset>("TerminusTTF SDF") },
                { "LiberationSans SDF", m_AssetBundle.LoadAsset<TMP_FontAsset>("LiberationSansRU SDF") },
                { "FugazOne-Regular SDF", m_AssetBundle.LoadAsset<TMP_FontAsset>("Lobster-Regular SDF") },

                { "NotoSansJP-Black SDF", m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Black SDF") },
                { "NotoSansJP-Bold SDF", m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Bold SDF") },
                { "NotoSansJP-Light SDF", m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Light SDF") },
                { "NotoSansJP-Medium SDF", m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Medium SDF") },

                { "NotoSansJP-Medium SDF Black Outline", m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Medium SDF Black Outline") },
                { "NotoSansJP-Medium SDF Drop Shadow", m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Medium SDF Drop Shadow") },
                { "NotoSansCJK-Black SDF BlackOutline", m_AssetBundle.LoadAsset<TMP_FontAsset>("NotoSans-Black SDF Black Outline") }
            };
            m_FontsReplaced = new List<TMP_FontAsset>();
        }

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("RCG2 l10n started");

            UpdateFonts();
            UpdateLocalization();
            PatchHarmony();
        }

        private void UpdateFonts()
        {
            // Preload all resources so we can catch all fonts to fallback insertion
            var entries = Resources.LoadAll("");
            TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var font in fonts)
            {
                // LoggerInstance.Msg($"Found font: {font.name}");
                if (m_FontsDictionary.ContainsKey(font.name))
                {
                    // Some fonts has fallbacks that have fallbacks, so we push own first to avoid clashing
                    font.fallbackFontAssetTable.Insert(0, m_FontsDictionary[font.name]);
                    // Push it into static
                    m_FontsReplaced.Add(font);
                    // LoggerInstance.Msg($"Added failback {m_FontsDictionary[font.name].name} to {font.name}");
                }
            }
            Resources.UnloadUnusedAssets();
            LoggerInstance.Msg("Fonts loaded");
        }

        private void UpdateLocalization()
        {
            LocalizationManager.UpdateSources();
            var translation_csv = m_AssetBundle.LoadAsset<TextAsset>("translation");
            // Remove garbage
            LocalizationManager.Sources[0].mLanguages.RemoveRange(9, 7);
            LocalizationManager.Sources[0].Import_CSV(string.Empty, translation_csv.text, eSpreadsheetUpdateMode.Replace, ';');
            // Need to manually define code
            LocalizationManager.Sources[0].mLanguages[9].Code = "ru";
            LocalizationManager.LocalizeAll(true);
            LoggerInstance.Msg("Updated translation");
        }

        private void PatchHarmony()
        {
            HarmonyInstance.PatchAll(typeof(RCG.UI.Screens.UI_CellPhone_Settings));
            HarmonyInstance.PatchAll(typeof(RCG.UI.Screens.UI_Settings));
            LoggerInstance.Msg("Harmony patched");
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
