using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;

namespace FreeItemFriday.ItemContent
{
	public class ThereminContent : IContentPackProvider
	{
        public static class Items
        {
            public static ItemDef Theremin;
        }

        private readonly ContentPack contentPack = new ContentPack();

        public string identifier => "FreeItemFriday.ItemContent.Theremin";

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            AddressablesLoadHelper loadHelper = AddressablesLoadHelper.CreateUsingDefaultResourceLocator("ContentPack:" + identifier);
            loadHelper.AddContentPackLoadOperation(contentPack);
            /*loadHelper.AddLoadOperation<Material>(null, assets =>
            {
                foreach (var material in assets)
                {
                    Debug.Log(material.name);
                }
            });*/
            loadHelper.AddGenericOperation(delegate
            {
                ContentLoadHelper.PopulateTypeFields(typeof(Items), contentPack.itemDefs);
            }, 0.05f);
            while (loadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(loadHelper.progress.value);
                yield return loadHelper.coroutine.Current;
            }
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            yield break;
        }
    }
}