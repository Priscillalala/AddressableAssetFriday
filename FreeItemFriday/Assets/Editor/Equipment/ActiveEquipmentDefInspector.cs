#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using RoR2;
using System;
using System.Linq;
using HG;
using System.Reflection;
using RoR2EditorKit.Inspectors;
using RoR2EditorKit.VisualElements;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using RoR2EditorKit;
using ContextMenuData = RoR2EditorKit.ContextMenuData;

namespace FreeItemFriday.Editor.ItemDisplays
{
    [CustomEditor(typeof(ActiveEquipmentDef))]
    public sealed class ActiveEquipmentDefInspector : ScriptableObjectInspector<ActiveEquipmentDef>
    {
        ValidatingPropertyField buffDefValidator;
        bool DoesNotAppear => (!TargetType.appearsInMultiPlayer && !TargetType.appearsInSinglePlayer);

        protected override bool HasVisualTreeAsset => true;

        protected override bool ValidateUXMLPath(string path) => true;

        VisualElement inspectorData = null;
        VisualElement tokenHolder = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorData = container.Q<VisualElement>("InspectorDataContainer");
                tokenHolder = inspectorData.Q<Foldout>("TokenContainer");
                buffDefValidator = inspectorData.Q<ValidatingPropertyField>("passiveBuffDef");
            };
        }


        protected override void DrawInspectorGUI()
        {
            var cooldown = inspectorData.Q<PropertyField>("cooldown");
            cooldown.RegisterCallback<ChangeEvent<float>>(OnCooldownSet);
            OnCooldownSet();

            SetupBuffValidator();

            var dropOnDeathChance = inspectorData.Q<PropertyField>("dropOnDeathChance");
            dropOnDeathChance.AddSimpleContextMenu(new ContextMenuData(
                "Set to Elite drop chance",
                x => TargetType.dropOnDeathChance = 0.0025f,
                callback =>
                {
                    if (TargetType.passiveBuffDef && TargetType.passiveBuffDef.eliteDef)
                        return DropdownMenuAction.Status.Normal;
                    return DropdownMenuAction.Status.Disabled;
                }));

            tokenHolder.AddSimpleContextMenu(new ContextMenuData(
                "Set Tokens",
                SetTokens,
                callback =>
                {
                    var tokenPrefix = Settings.tokenPrefix;
                    if (string.IsNullOrEmpty(tokenPrefix))
                        return DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                }));
        }

        private void OnCooldownSet(ChangeEvent<float> evt = null)
        {
            float value = evt == null ? TargetType.cooldown : evt.newValue;

            if (value < 0)
            {
                TargetType.cooldown = 0;
                Debug.LogError($"Cannot set an equipment's cooldown to a number less than 0");
                return;
            }
            TargetType.cooldown = value;
        }

        private void SetupBuffValidator()
        {
            buffDefValidator.AddValidator(() =>
            {
                var buffDef = GetBuffDef();
                return buffDef && buffDef.eliteDef && !buffDef.eliteDef.eliteEquipmentDef;
            },
            $"The assigned Passive Buff Def has an EliteDef assigned, but the EliteDef has no EliteEquipmentDef!", MessageType.Error);

            buffDefValidator.AddValidator(() =>
            {
                var buffDef = GetBuffDef();
                return buffDef && buffDef.eliteDef && buffDef.eliteDef.eliteEquipmentDef != TargetType;
            },
            $"The assigned Passive Buff Def has an EliteDef assigned, but the EliteDef's EliteEquipmentDef does not point to the inspected EquipmentDef!", MessageType.Error);

            BuffDef GetBuffDef() => buffDefValidator.ChangeEvent == null ? TargetType.passiveBuffDef : (BuffDef)buffDefValidator.ChangeEvent.newValue;
        }

        private void SetTokens(DropdownMenuAction act)
        {
            if (Settings.tokenPrefix.IsNullOrEmptyOrWhitespace())
                throw ErrorShorthands.NullTokenPrefix();

            string tokenBase = $"{Settings.GetPrefixUppercase()}_EQUIP_{TargetType.name.ToUpperInvariant().Replace(" ", "")}_";
            TargetType.nameToken = $"{tokenBase}NAME";
            TargetType.pickupToken = $"{tokenBase}PICKUP";
            TargetType.descriptionToken = $"{tokenBase}DESC";
            TargetType.loreToken = $"{tokenBase}LORE";
        }
    }
}
#endif