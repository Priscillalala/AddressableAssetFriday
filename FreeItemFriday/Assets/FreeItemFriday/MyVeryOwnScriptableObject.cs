using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FreeItemFriday
{
	[CreateAssetMenu]
	public class MyVeryOwnScriptableObject : ScriptableObject
	{
		public string data;
		public AssetReferenceGameObject addressableAsset;
	}
}