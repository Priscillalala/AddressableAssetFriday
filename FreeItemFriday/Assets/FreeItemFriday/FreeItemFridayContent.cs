using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FreeItemFriday
{
	public class FreeItemFridayContent : IContentPackProvider
	{
		private readonly ContentPack contentPack = new ContentPack();

        public string identifier => "FreeItemFriday.BaseContent";

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            AddressablesLoadHelper loadHelper = AddressablesLoadHelper.CreateUsingDefaultResourceLocator("ContentPack:" + identifier);
            loadHelper.AddContentPackLoadOperation(contentPack);
            loadHelper.AddGenericOperation(SetDisabledIcon());
            while (loadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(loadHelper.progress.value);
                yield return loadHelper.coroutine.Current;
            }

            IEnumerator SetDisabledIcon()
            {
                var texUnlockIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png");
                while (!texUnlockIcon.IsDone)
                {
                    yield return null;
                }
                contentPack.expansionDefs.Find("FreeItemFriday").disabledIconSprite = texUnlockIcon.Result;
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