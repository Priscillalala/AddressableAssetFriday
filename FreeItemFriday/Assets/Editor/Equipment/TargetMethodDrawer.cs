#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using RoR2;
using System;
using System.Linq;
using HG;
using System.Reflection;

namespace FreeItemFriday.Editor.ItemDisplays
{
    [CustomPropertyDrawer(typeof(ActiveEquipmentDef.TargetMethod))]
    public class TargetMethodDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var targetType = property.FindPropertyRelative(nameof(ActiveEquipmentDef.TargetMethod.targetType));
            var targetMethodName = property.FindPropertyRelative(nameof(ActiveEquipmentDef.TargetMethod.targetMethodName));
            var assemblyQualifiedName = targetType.FindPropertyRelative("assemblyQualifiedName");

            position.width /= 2;
            EditorGUI.PropertyField(position, targetType, GUIContent.none);
            position.x += position.width;

            string[] displayedOptions = Array.Empty<string>();
            if (!string.IsNullOrEmpty(assemblyQualifiedName.stringValue))
            {
                Type type = Type.GetType(assemblyQualifiedName.stringValue);

                bool MethodFilter(MethodInfo method)
                {
                    if (!method.IsStatic || method.ReturnType != typeof(bool))
                    {
                        return false;
                    }
                    var parameters = method.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(EquipmentSlot));
                }

                displayedOptions = type.GetMethods().Where(MethodFilter).Select(x => x.Name).ToArray();
            }

            int index = EditorGUI.Popup(position, Array.IndexOf(displayedOptions, targetMethodName.stringValue), displayedOptions);
            if (index < 0 && displayedOptions.Length > 0)
            {
                index = 0;
            }
            string newTargetMethodName = index >= 0 ? displayedOptions[index] : string.Empty;
            if (newTargetMethodName != targetMethodName.stringValue)
            {
                targetMethodName.stringValue = newTargetMethodName;
                property.serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif