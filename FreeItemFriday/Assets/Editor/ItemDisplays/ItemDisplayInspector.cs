#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using RoR2;
using System;
using UnityEngine.UIElements;
using System.Linq;
using HG;

namespace FreeItemFriday.Editor.ItemDisplays
{
    [CustomEditor(typeof(ItemDisplay))]
    public class ItemDisplayInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Populate"))
            {
                ItemDisplay itemDisplay = (ItemDisplay)serializedObject.targetObject;
                foreach (Renderer renderer in itemDisplay.GetComponentsInChildren<Renderer>().Where(x => x is MeshRenderer || x is SkinnedMeshRenderer))
                {
                    int index = Array.FindIndex(itemDisplay.rendererInfos, x => x.renderer == renderer);
                    if (index < 0)
                    {
                        ArrayUtils.ArrayAppend(ref itemDisplay.rendererInfos, new CharacterModel.RendererInfo
                        {
                            renderer = renderer,
                            defaultMaterial = renderer.sharedMaterial,
                            defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                            ignoreOverlays = false,
                        });
                    }
                    else
                    {
                        itemDisplay.rendererInfos[index].defaultMaterial = renderer.sharedMaterial;
                    }
                }
                EditorUtility.SetDirty(itemDisplay);
            }
            DrawDefaultInspector();
        }
    }
}
#endif