/*
Generated from a ROR2EK Template. Feel free to remove this comment section.
0 = modName; 1 = Nicified mod name; 2 = authorName; 3 = using clauses; 4 = attributes; 
*/

using BepInEx;

using UnityEngine;

namespace FreeItemFriday
{

	[BepInPlugin(GUID, MODNAME, VERSION)]
	public class FreeItemFridayMain : BaseUnityPlugin
	{
		public const string GUID = "com.groovesalad.FreeItemFriday";
		public const string MODNAME = "FreeItemFriday";
		public const string VERSION = "0.0.1";

		public static FreeItemFridayMain Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
		}	
	}
}