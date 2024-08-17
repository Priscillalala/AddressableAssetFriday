using BepInEx.Configuration;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using System.Reflection;
using RiskOfOptions.Components.Options;
using RiskOfOptions.Components.RuntimePrefabs;

namespace FreeItemFriday
{
	public static class RiskOfOptionsInterop
    {
        public class PercentOption : ChoiceOption
        {
            public enum Dummy { }

            static readonly FieldInfo _configEntry = typeof(ChoiceOption).GetField("_configEntry", BindingFlags.Instance | BindingFlags.NonPublic);

            protected new PercentConfig config;
            protected ConfigEntry<Percent> configEntry;
            public bool dummy = true;

            public PercentOption(ConfigEntry<Percent> configEntry) : this(configEntry, new PercentConfig()) { }
            public PercentOption(ConfigEntry<Percent> configEntry, bool restartRequired) : this(configEntry, new PercentConfig { restartRequired = restartRequired }) { }
            public PercentOption(ConfigEntry<Percent> configEntry, PercentConfig config) : base(null, configEntry.BoxedValue)
            {
                this.configEntry = configEntry;
                _configEntry.SetValue(this, configEntry);
                this.config = config;
            }

            public override string OptionTypeName { get; protected set; } = "percent";

            public override BaseOptionConfig GetConfig() => config;

            public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent)
            {
                prefab = RuntimePrefabManager.Get<FloatFieldPrefab>().FloatField;
                var floatField = UnityEngine.Object.Instantiate(prefab, parent);

                var settingsField = floatField.GetComponentInChildren<ModSettingsFloatField>();

                settingsField.nameToken = GetNameToken();
                settingsField.settingToken = Identifier;

                settingsField.min = config.Min;
                settingsField.max = config.Max;
                settingsField.formatString = config.FormatString;

                settingsField.name = $"Mod Options Percent, {Name}";

                return floatField;
            }

            public override object Value
            { 
                get
                {
                    if (dummy)
                    {
                        return (Dummy)0;
                    }
                    return (float)configEntry.Value;
                }
                set => configEntry.Value = (float)value;
            }
        }

        public class PercentConfig : NumericFieldConfig<Percent>
        {
            public override Percent Min { get; set; } = float.MinValue;
            public override Percent Max { get; set; } = float.MaxValue;
        }
    }
}