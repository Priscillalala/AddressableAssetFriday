using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using RoR2.ContentManagement;
using System.Linq;
using System.ComponentModel;

namespace FreeItemFriday.Editor
{
    [DisplayName("Content Pack Label")]
    public class ContentPackLabelSchema : AddressableAssetGroupSchema
    {
        public string Identifier { get; set; }

        private string Label => "ContentPack:" + Identifier;

        public override void OnGUI()
        {
            string identifier = EditorGUILayout.TextField("Identifier", Identifier);
            if (Identifier != identifier)
            {
                if (Group)
                {
                    Group.
                }
                Identifier = identifier;
            }
        }
    }
}
