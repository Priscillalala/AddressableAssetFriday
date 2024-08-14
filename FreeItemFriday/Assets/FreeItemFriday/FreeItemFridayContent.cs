using RoR2.ContentManagement;
using System.Collections;

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