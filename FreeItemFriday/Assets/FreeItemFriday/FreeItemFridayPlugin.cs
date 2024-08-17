using BepInEx;
using Path = System.IO.Path;
using UnityEngine.AddressableAssets;
using RoR2;
using RoR2.ContentManagement;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using BepInEx.Logging;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace FreeItemFriday
{
	[BepInPlugin(GUID, NAME, VERSION)]
	[BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
	public class FreeItemFridayPlugin : BaseUnityPlugin
	{
		public const string 
			GUID = "groovesalad." + NAME,
			NAME = "FreeItemFriday",
			VERSION = "1.0.0";

		public static FreeItemFridayPlugin Instance { get; private set; }
		public static string RuntimeDirectory { get; private set; }
		public static new ManualLogSource Logger { get; private set; }

		private void Awake()
		{
			Instance = this;
			RuntimeDirectory = Path.GetDirectoryName(Info.Location);
			Logger = base.Logger;

			string catalogPath = Path.Combine(RuntimeDirectory, "aa", $"catalog_{NAME}.json");
			var locator = Addressables.LoadContentCatalogAsync(catalogPath).WaitForCompletion();
			
			ContentManager.collectContentPackProviders += add =>
			{
				add(new FreeItemFridayContent());
				add(new ItemContent.ThereminContent());
				add(new SkillContent.RebootContent());
			};
			Language.collectLanguageRootFolders += list =>
			{
				list.Add(Path.Combine(RuntimeDirectory, "Language"));
			};
			LanguageSystem.Init();
			RiskOfOptionsInterop.Init();
		}
	}
}