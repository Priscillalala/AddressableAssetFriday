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
using TMPro;
using RoR2.UI;
using BepInEx.Bootstrap;
using UnityEngine.AddressableAssets;
using System.Runtime.CompilerServices;

namespace FreeItemFriday
{
	public static class RiskOfOptionsInterop
    {
        const MethodImplOptions SAFE = MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization;

        public static bool Available { get; private set; }

        public static void Init()
        {
            Available = Chainloader.PluginInfos.ContainsKey(PluginInfo.PLUGIN_GUID);
            if (Available)
            {
                FreeItemFridayPlugin.Logger.LogMessage("Found Risk of Options!");
                SetModIcon();
            }
        }

        [MethodImpl(SAFE)]
        public static void SetModIcon()
        {
            Addressables.LoadAssetAsync<Sprite>("FreeItemFriday/Base/texFreeItemFridayExpansionIcon.png").Completed += handle =>
            {
                ModSettingsManager.SetModIcon(handle.Result);
            };
        }

        [MethodImpl(SAFE)]
        public static void AddFloatFieldOption(ConfigEntry<float> configEntry)
        {
            ModSettingsManager.AddOption(new FloatFieldOption(configEntry), FreeItemFridayPlugin.GUID, FreeItemFridayPlugin.NAME);
        }

        [MethodImpl(SAFE)]
        public static void AddFloatSliderOption(ConfigEntry<float> configEntry, float min, float max)
        {
            ModSettingsManager.AddOption(new SliderOption(configEntry, new SliderConfig 
            {
                min = min,
                max = max
            }), FreeItemFridayPlugin.GUID, FreeItemFridayPlugin.NAME);
        }

        [MethodImpl(SAFE)]
        public static void AddPercentOption(ConfigEntry<Percent> configEntry)
        {
            PercentOption percentOption = new PercentOption(configEntry);
            ModSettingsManager.AddOption(percentOption, FreeItemFridayPlugin.GUID, FreeItemFridayPlugin.NAME);
            percentOption.dummy = false;
        }

        [MethodImpl(SAFE)]
        public static void AddCheckBoxOption(ConfigEntry<bool> configEntry, bool restartRequired = false)
        {
            ModSettingsManager.AddOption(new CheckBoxOption(configEntry, restartRequired), FreeItemFridayPlugin.GUID, FreeItemFridayPlugin.NAME);
        }

        class PercentOption : ChoiceOption
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

                var floatSettingsField = floatField.GetComponentInChildren<ModSettingsFloatField>();
                var settingsField = floatSettingsField.gameObject.AddComponent<ModSettingsPercent>();
                settingsField.nameLabel = floatSettingsField.nameLabel;
                settingsField.valueText = floatSettingsField.valueText;
                UnityEngine.Object.DestroyImmediate(floatSettingsField);

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
                    return configEntry.Value;
                }
                set => configEntry.BoxedValue = value;
            }
        }

        class PercentConfig : NumericFieldConfig<Percent>
        {
            public override Percent Min { get; set; } = float.MinValue;
            public override Percent Max { get; set; } = float.MaxValue;
        }

        class ModSettingsPercent : ModSettingsControl<object>
        {
            public TMP_InputField valueText;
            public Percent min;
            public Percent max;
            public string formatString;
            private NumberFormatInfo numberFormatInfo;

            protected override void Awake()
            {
                base.Awake();

                valueText.onEndEdit.AddListener(OnTextEdited);
                valueText.onSubmit.AddListener(OnTextEdited);

                if (!LanguageSystem.languageNumberFormatting.TryGetValue(Language.currentLanguageName, out numberFormatInfo))
                {
                    numberFormatInfo = Separator.GetCultureInfo().NumberFormat;
                }
            }

            protected override void Disable()
            {
                foreach (var button in GetComponentsInChildren<HGButton>())
                {
                    button.interactable = false;
                }
            }

            protected override void Enable()
            {
                foreach (var button in GetComponentsInChildren<HGButton>())
                {
                    button.interactable = true;
                }
            }

            protected override void OnUpdateControls()
            {
                base.OnUpdateControls();

                Percent currentValue = (Percent)GetCurrentValue();
                if (currentValue < min)
                {
                    currentValue = min;
                }
                else if (currentValue > max)
                {
                    currentValue = max;
                }
                if (valueText)
                {  
                    valueText.text = string.Format(Separator.GetCultureInfo(), formatString, currentValue.ToString(numberFormatInfo));
                }
            }

            private void OnTextEdited(string newText)
            {
                if (float.TryParse(newText, NumberStyles.Any, Separator.GetCultureInfo(), out var num))
                {
                    SubmitValue(new Percent(num / 100f));
                }
                else
                {
                    SubmitValue(GetCurrentValue());
                }
            }
        }
    }
}