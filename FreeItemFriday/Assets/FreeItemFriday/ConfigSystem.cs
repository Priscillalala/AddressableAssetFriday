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
	public static class ConfigSystem
    {
        public static ConfigEntry<float> BindWithOptions(this ConfigFile config, string section, string key, float defaultValue, string description = null, AcceptableValueBase acceptableValues = null)
        {
            ConfigDescription configDescription = null;
            if (!string.IsNullOrEmpty(description) || acceptableValues != null)
            {
                configDescription = new ConfigDescription(description ?? string.Empty, acceptableValues);
            }
            var configEntry = config.Bind(section, key, defaultValue, configDescription);
            if (RiskOfOptionsInterop.Available)
            {
                if (acceptableValues is AcceptableValueRange<float> acceptableValuesRange)
                {
                    RiskOfOptionsInterop.AddFloatSliderOption(configEntry, acceptableValuesRange.MinValue, acceptableValuesRange.MaxValue);
                }
                else
                {
                    RiskOfOptionsInterop.AddFloatFieldOption(configEntry);
                }
            }
            return configEntry;
        }

        public static ConfigEntry<Percent> BindWithOptions(this ConfigFile config, string section, string key, Percent defaultValue, string description = null)
        {
            var configEntry = config.Bind(section, key, defaultValue, !string.IsNullOrEmpty(description) ? new ConfigDescription(description) : null);
            if (RiskOfOptionsInterop.Available)
            {
                RiskOfOptionsInterop.AddPercentOption(configEntry);
            }
            return configEntry;
        }

        public static ConfigEntry<bool> BindWithOptions(this ConfigFile config, string section, string key, bool defaultValue, string description = null)
        {
            var configEntry = config.Bind(section, key, defaultValue, !string.IsNullOrEmpty(description) ? new ConfigDescription(description) : null);
            if (RiskOfOptionsInterop.Available)
            {
                RiskOfOptionsInterop.AddCheckBoxOption(configEntry);
            }
            return configEntry;
        }
    }
}