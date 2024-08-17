using BepInEx.Configuration;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SearchableAttribute = HG.Reflection.SearchableAttribute;

namespace FreeItemFriday
{
    public abstract class BaseContentPackProvider : IContentPackProvider
    {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        public class AddContentPackProviderAttribute : SearchableAttribute
        {
            public string group;
            public string name;

            public AddContentPackProviderAttribute(string group, string name)
            {
                this.group = group;
                this.name = name;
            }
        }

        public static class ContentGroup
        {
            public const string
                ITEMS = "Items",
                EQUIPMENT = "Equipment",
                SKILLS = "Skills",
                ARTIFACTS = "Artifacts";
        }

        public ContentPack contentPack;
        public ConfigFile config;

        public abstract string identifier { get; }

        public abstract IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args);

        public abstract IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args);

        public abstract IEnumerator FinalizeAsync(FinalizeAsyncArgs args);

        public AddressablesLoadHelper CreateLoadHelper()
        {
            return AddressablesLoadHelper.CreateUsingDefaultResourceLocator("ContentPack:" + identifier);
        }
    }
}