using BepInEx;
using Path = System.IO.Path;
using UnityEngine.AddressableAssets;
using RoR2;

namespace FreeItemFriday
{
	[BepInPlugin(GUID, NAME, VERSION)]
	public class FreeItemFridayPlugin : BaseUnityPlugin
	{
		public const string 
			GUID = "groovesalad." + NAME,
			NAME = "FreeItemFriday",
			VERSION = "1.0.0";

		public static FreeItemFridayPlugin Instance { get; private set; }
		public static string RuntimeDirectory { get; private set; }

		private void Awake()
		{
			Instance = this;
			RuntimeDirectory = Path.GetDirectoryName(Info.Location);

			string catalogPath = Path.Combine(RuntimeDirectory, "aa", $"catalog_{NAME}.json");
			var locator = Addressables.LoadContentCatalogAsync(catalogPath).WaitForCompletion();
			foreach (var key in locator.Keys)
            {
				Logger.LogInfo(key);
            }
			var theremin = Addressables.LoadAssetAsync<ItemDef>("FreeItemFriday/ItemContent/Theremin/Theremin.asset").WaitForCompletion();
			Logger.LogMessage(theremin.nameToken);
			var testObject = Addressables.LoadAssetAsync<MyVeryOwnScriptableObject>("FreeItemFriday/Base/TestObject.asset").WaitForCompletion();
			Logger.LogMessage(testObject.data);
		}	
	}
}