using BepInEx;
using UnityEngine.AddressableAssets;
using RoR2;
using RoR2.ContentManagement;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using BepInEx.Logging;
using System.Linq;
using System.Collections.Generic;
using Path = System.IO.Path;
using SearchableAttribute = HG.Reflection.SearchableAttribute;
using BepInEx.Configuration;
using System;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace FreeItemFriday
{
	[BepInPlugin(GUID, NAME, VERSION)]
	[BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
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
				var providerGroups =
					SearchableAttribute.GetInstances<BaseContentPackProvider.AddContentPackProviderAttribute>()
					.Cast<BaseContentPackProvider.AddContentPackProviderAttribute>()
					.Where(x =>
                    {
						ConfigEntry<bool> include = Config.Bind(x.group, $"Include {x.name}", true);
						if (RiskOfOptionsInterop.Available)
                        {
							RiskOfOptionsInterop.AddCheckBoxOption(include, true);
                        }
						return include.Value;
					})
					.GroupBy(x => x.group);
				foreach (var grouping in providerGroups)
                {
					string configPath = Path.Combine(Paths.ConfigPath, $"{GUID}.{grouping.Key}.cfg");
					ConfigFile config = new ConfigFile(configPath, false, Info.Metadata);
					foreach (var attribute in grouping)
                    {
						var provider = (BaseContentPackProvider)Activator.CreateInstance((Type)attribute.target);
						provider.contentPack = new ContentPack { identifier = provider.identifier };
						provider.config = config;
						add(provider);
					}
                }
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