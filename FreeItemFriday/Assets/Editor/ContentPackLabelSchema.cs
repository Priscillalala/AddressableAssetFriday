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
        [SerializeField]
        private string _identifier;

        public string Identifier
        {
            get => _identifier;
            set
            {
                if (_identifier != value)
                {
                    _identifier = value;
                    SetDirty(true);
                }
            }
        }

        /*public override void OnGUI()
        {
            Identifier = EditorGUILayout.TextField("Identifier", Identifier);
        }*/

        public void ApplyLabel()
        {
            string label = "ContentPack:" + Identifier;
            foreach (AddressableAssetEntry entry in Group.entries)
            {
                entry.SetLabel(label, true, true);
            }
        }
    }
}
