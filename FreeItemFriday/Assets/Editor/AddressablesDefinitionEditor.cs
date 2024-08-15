#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using ThunderKit.Core.Pipelines;
using System.Threading.Tasks;
using UnityEditor.AddressableAssets;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using ThunderKit.Core.Inspectors;
using UnityEditorInternal;

namespace FreeItemFriday.Editor
{
    [CustomEditor(typeof(AddressablesDefinition))]
    public class AddressablesDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty assetTypeLabels;
        private ReorderableList assetTypeLabelsList;
        private bool assetTypeLabelsFoldout;
        private SerializedProperty componentTypeLabels;
        private ReorderableList componentTypeLabelsList;
        private bool componentTypeLabelsFoldout;

        public void OnEnable()
        {
            assetTypeLabels = serializedObject.FindProperty(nameof(AddressablesDefinition.assetTypeLabels));
            assetTypeLabelsList = new ReorderableList(serializedObject, assetTypeLabels, true, false, true, true);
            assetTypeLabelsList.headerHeight = 0;
            assetTypeLabelsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = assetTypeLabels.GetArrayElementAtIndex(index);
                rect.width /= 2f;
                EditorGUI.PropertyField(rect, element.FindPropertyRelative(nameof(AddressablesDefinition.AssetTypeLabel.assetType)), GUIContent.none);
                rect.x += rect.width;
                EditorGUI.PropertyField(rect, element.FindPropertyRelative(nameof(AddressablesDefinition.AssetTypeLabel.label)), GUIContent.none);
            };
            componentTypeLabels = serializedObject.FindProperty(nameof(AddressablesDefinition.componentTypeLabels));
            componentTypeLabelsList = new ReorderableList(serializedObject, componentTypeLabels, true, false, true, true);
            componentTypeLabelsList.headerHeight = 0;
            componentTypeLabelsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = componentTypeLabels.GetArrayElementAtIndex(index);
                rect.width /= 2f;
                EditorGUI.PropertyField(rect, element.FindPropertyRelative(nameof(AddressablesDefinition.ComponentTypeLabel.componentType)), GUIContent.none);
                rect.x += rect.width;
                EditorGUI.PropertyField(rect, element.FindPropertyRelative(nameof(AddressablesDefinition.ComponentTypeLabel.label)), GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, "m_Script", nameof(AddressablesDefinition.assetTypeLabels), nameof(AddressablesDefinition.componentTypeLabels));
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();

            if (assetTypeLabelsFoldout = EditorGUILayout.Foldout(assetTypeLabelsFoldout, assetTypeLabels.displayName))
            {
                assetTypeLabelsList.DoLayoutList();
            }
            if (componentTypeLabelsFoldout = EditorGUILayout.Foldout(componentTypeLabelsFoldout, componentTypeLabels.displayName))
            {
                componentTypeLabelsList.DoLayoutList();
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif