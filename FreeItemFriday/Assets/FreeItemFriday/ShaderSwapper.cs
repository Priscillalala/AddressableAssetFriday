using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace FreeItemFriday
{
	public static class ShaderSwapper
    {
        const string PREFIX = "Stubbed";
        const int PREFIX_LENGTH = 7;

        private static UnityEngine.Object[] _ = Array.Empty<UnityEngine.Object>();

        public static void AddUpgradeStubbedShadersOperation(this AddressablesLoadHelper loadHelper)
        {
            loadHelper.AddLoadOperation<Material>("Material", assets => loadHelper.AddGenericOperation(UpgradeStubbedShaders(assets)));
        }
        
        public static IEnumerator UpgradeStubbedShaders(Material[] materials)
        {
            int materialCount = materials.Length;
            if (materialCount <= 0)
            {
                yield break;
            }

            List<AsyncOperationHandle> loadResourceLocationsOperations = new List<AsyncOperationHandle>(materialCount);
            for (int i = materialCount - 1; i >= 0; i--)
            {
                string cachedShaderName = materials[i].shader.name;
                if (cachedShaderName.StartsWith(PREFIX))
                {
                    loadResourceLocationsOperations.Add(Addressables.LoadResourceLocationsAsync(cachedShaderName.Substring(PREFIX_LENGTH) + ".shader", typeof(Shader)));
                }
                else
                {
                    materialCount--;
                    for (int j = i; j < materialCount; j++)
                    {
                        materials[j] = materials[j + 1];
                    }
                }
            }
            if (materialCount <= 0)
            {
                yield break;
            }

            AsyncOperationHandle<IList<AsyncOperationHandle>> loadResourceLocationsGroup = Addressables.ResourceManager.CreateGenericGroupOperation(loadResourceLocationsOperations, true);
            while (!loadResourceLocationsGroup.IsDone)
            {
                yield return null;
            }

            List<IResourceLocation> resourceLocations = new List<IResourceLocation>(materialCount);
            for (int i = materialCount - 1; i >= 0; i--)
            {
                IList<IResourceLocation> result = (IList<IResourceLocation>)loadResourceLocationsGroup.Result[i].Result;
                if (result.Count > 0)
                {
                    resourceLocations.Add(result[0]);
                }
                else
                {
                    materialCount--;
                    for (int j = materialCount - i; j < materialCount; j++)
                    {
                        materials[j] = materials[j + 1];
                    }
                }
            }
            if (materialCount <= 0)
            {
                yield break;
            }

            AsyncOperationHandle<IList<Shader>> loadShaders = Addressables.LoadAssetsAsync<Shader>(resourceLocations, null, false);
            while (!loadShaders.IsDone)
            {
                yield return null;
            }
            int startIndex = _.Length;
            Array.Resize(ref _, startIndex + materialCount);
            for (int i = 0; i < materialCount; i++)
            {
                SwapShader(materials[i], loadShaders.Result[i]);
                _[startIndex + i] = materials[i];
            }
        }

        private static void SwapShader(Material material, Shader shader)
        {
            int renderQueue = material.renderQueue;
            material.shader = shader;
            material.renderQueue = renderQueue;
        }
    }
}