#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using RoR2;
using System;

namespace FreeItemFriday.Editor.ItemDisplays
{
    [CustomPropertyDrawer(typeof(ItemDisplayRule))]
    public class ItemDisplayRuleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.height = EditorGUIUtility.singleLineHeight;

            void NextLine()
            {
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            }

            //if (EditorGUI.PropertyField(position, property, label, false))
            if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, "Item Display Rule", true))
            {
                EditorGUI.indentLevel++;

                var childName = property.FindPropertyRelative(nameof(ItemDisplayRule.childName));
                var localPos = property.FindPropertyRelative(nameof(ItemDisplayRule.localPos));
                var localAngles = property.FindPropertyRelative(nameof(ItemDisplayRule.localAngles));
                var localScale = property.FindPropertyRelative(nameof(ItemDisplayRule.localScale));

                NextLine();
                Rect buttonPos = position;
                buttonPos.x += EditorGUI.indentLevel * 15f;
                if (GUI.Button(buttonPos, "Paste Values"))
                {
                    string clipboardContent = GUIUtility.systemCopyBuffer;
                    clipboardContent = clipboardContent.Trim()
                        .Replace("new Vector3(", string.Empty)
                        .Replace(")", string.Empty)
                        .Replace("F", string.Empty)
                        .Replace("\"", string.Empty);
                    var args = clipboardContent.Split(new string[] { ", " }, StringSplitOptions.None);
                    childName.stringValue = args[0];
                    localPos.vector3Value = ParseVector3(args[1], args[2], args[3]);
                    localAngles.vector3Value = ParseVector3(args[4], args[5], args[6]);
                    localScale.vector3Value = ParseVector3(args[7], args[8], args[9]);
                    property.serializedObject.ApplyModifiedProperties();

                    Vector3 ParseVector3(string x, string y, string z)
                    {
                        return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
                    }
                }

                NextLine();
                EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(ItemDisplayRule.followerPrefab)));

                var limbMask = property.FindPropertyRelative(nameof(ItemDisplayRule.limbMask));
                NextLine();
                EditorGUI.PropertyField(position, limbMask);
                int newRuleType = limbMask.intValue == 0 ? 0 : 1;
                var ruleType = property.FindPropertyRelative(nameof(ItemDisplayRule.ruleType));
                if (ruleType.enumValueIndex != newRuleType)
                {
                    Debug.Log($"New rule type: {newRuleType}");
                    ruleType.enumValueIndex = newRuleType;
                    property.serializedObject.ApplyModifiedProperties();
                }
                //NextLine();
                //EditorGUI.PropertyField(position, ruleType);
                /*if ((ItemDisplayRuleType)ruleType.enumValueIndex == ItemDisplayRuleType.LimbMask)
                {
                    NextLine();
                    Rect limbMaskPos = position;
                    limbMaskPos.width /= 2f;
                    EditorGUI.PropertyField(limbMaskPos, ruleType);
                    limbMaskPos.x += limbMaskPos.width;
                    EditorGUI.PropertyField(limbMaskPos, , GUIContent.none);
                }
                else
                {
                    NextLine();
                    EditorGUI.PropertyField(position, ruleType);
                }*/

                string content = string.IsNullOrEmpty(childName.stringValue) ? "Transform" : childName.stringValue;
                NextLine();
                //Rect transformPos = position;
                //transformPos.width = Mathf.Min(transformPos.width / 2f, 50f);
                //transformPos.width /= 2f;
                //transformPos.x += transformPos.width;
                if (childName.isExpanded = EditorGUI.Foldout(position, childName.isExpanded, content, true))
                {
                    EditorGUI.indentLevel++;
                    NextLine();
                    EditorGUI.PropertyField(position, childName);
                    NextLine();
                    EditorGUI.PropertyField(position, localPos);
                    NextLine();
                    EditorGUI.PropertyField(position, localAngles);
                    NextLine();
                    EditorGUI.PropertyField(position, localScale);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                int lines = 5;
                if (property.FindPropertyRelative(nameof(ItemDisplayRule.childName)).isExpanded)
                {
                    lines += 4;
                }
                return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * lines;
            }
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}
#endif