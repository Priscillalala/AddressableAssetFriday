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
    [CustomEditor(typeof(ModelPanelParameters))]
    public class ModelPanelParametersInspector : UnityEditor.Editor
    {
        public bool toggleEuler = true;
        public SerializedProperty modelRotation;

        public void OnEnable()
        {
            modelRotation = serializedObject.FindProperty(nameof(ModelPanelParameters.modelRotation));
        }

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, nameof(ModelPanelParameters.modelRotation));
            if (toggleEuler = EditorGUILayout.Toggle("Show Euler Angles", toggleEuler))
            {
                Vector3 eulerAngles = EditorGUILayout.Vector3Field("Model Rotation", modelRotation.quaternionValue.eulerAngles);
                if (eulerAngles != modelRotation.quaternionValue.eulerAngles)
                {
                    modelRotation.quaternionValue = Quaternion.Euler(eulerAngles);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUILayout.PropertyField(modelRotation, true);
            }
        }
    }
}
#endif