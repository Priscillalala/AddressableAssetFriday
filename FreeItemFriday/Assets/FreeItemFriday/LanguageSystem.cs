using BepInEx.Configuration;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace FreeItemFriday
{
	public static class LanguageSystem
    {
        public static Dictionary<string, ConfigEntryBase[]> tokenArgs = new Dictionary<string, ConfigEntryBase[]>();
        public static Dictionary<Language, Dictionary<string, string>> formatStrings = new Dictionary<Language, Dictionary<string, string>>();
        public static readonly Dictionary<string, NumberFormatInfo> languageNumberFormatting = new Dictionary<string, NumberFormatInfo>
        {
            ["en"] = new NumberFormatInfo
            {
                PercentDecimalDigits = 0,
                PercentPositivePattern = 1,
            },

        };

        public static void Init()
        {
            On.RoR2.Language.LoadStrings += Language_LoadStrings;
            On.RoR2.Language.UnloadStrings += Language_UnloadStrings;
        }

        public static string GetFormattedString(string formatString, ConfigEntryBase[] args, string language)
        {
            if (!languageNumberFormatting.TryGetValue(language, out NumberFormatInfo numberFormatInfo))
            {
                numberFormatInfo = languageNumberFormatting["en"];
            }
            return string.Format(numberFormatInfo, formatString, args.Select(x => x.BoxedValue).ToArray());
        }

        public static void SetArgs(string token, params ConfigEntryBase[] args)
        {
            tokenArgs[token] = args;
            foreach (ConfigEntryBase configEntry in args)
            {
                configEntry.ConfigFile.SettingChanged += (sender, eventArgs) =>
                {
                    if (eventArgs.ChangedSetting == configEntry)
                    {
                        foreach (var languageFormatStringsPair in formatStrings)
                        {
                            if (languageFormatStringsPair.Value.TryGetValue(token, out string formatString))
                            {
                                languageFormatStringsPair.Key.SetStringByToken(token, GetFormattedString(formatString, args, languageFormatStringsPair.Key.name));
                            }
                        }
                    }
                };
            }
        }

        private static void Language_LoadStrings(On.RoR2.Language.orig_LoadStrings orig, Language self)
        {
            orig(self);
            if (formatStrings.ContainsKey(self))
            {
                return;
            }
            Dictionary<string, string> formatStringsByToken = new Dictionary<string, string>();
            formatStrings.Add(self, formatStringsByToken);
            foreach (var tokenArgsPair in tokenArgs)
            {
                string token = tokenArgsPair.Key;
                if (self.stringsByToken.TryGetValue(token, out string formatString))
                {
                    formatStringsByToken[token] = formatString;
                    self.SetStringByToken(token, GetFormattedString(formatString, tokenArgsPair.Value, self.name));
                }
            }
        }

        private static void Language_UnloadStrings(On.RoR2.Language.orig_UnloadStrings orig, Language self)
        {
            orig(self);
            formatStrings.Remove(self);
        }
    }
}