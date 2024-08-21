using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using HG;
using System.Linq;
using RoR2.Skills;
using R2API.ScriptableObjects;
using R2API;

namespace FreeItemFriday
{
    public static class SerializableColorCatalog
    {
        public const string DAMAGE_COLOR_LABEL = "DamageColor";
        public const string COLOR_LABEL = "ColorCatalogEntry";

        public static readonly NamedAssetCollection<SerializableDamageColor> damageColors = new NamedAssetCollection<SerializableDamageColor>(ContentPack.getScriptableObjectName);
        public static readonly NamedAssetCollection<SerializableColorCatalogEntry> colors = new NamedAssetCollection<SerializableColorCatalogEntry>(ContentPack.getScriptableObjectName);

        [SystemInitializer]
        private static void Init()
        {
            foreach (SerializableDamageColor damageColor in damageColors)
            {
                ColorsAPI.AddSerializableDamageColor(damageColor);
            }
            foreach (SerializableColorCatalogEntry color in colors)
            {
                ColorsAPI.AddSerializableColor(color);
            }
        }

        public static void AddDamageColorsLoadOperation(this AddressablesLoadHelper loadHelper)
        {
            loadHelper.AddLoadOperation<SerializableDamageColor>(DAMAGE_COLOR_LABEL, damageColors.Add);
        }

        public static void AddColorsLoadOperation(this AddressablesLoadHelper loadHelper)
        {
            loadHelper.AddLoadOperation<SerializableColorCatalogEntry>(COLOR_LABEL, colors.Add);
        }
    }
}