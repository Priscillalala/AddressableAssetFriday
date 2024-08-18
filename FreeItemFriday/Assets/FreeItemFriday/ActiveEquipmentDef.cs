using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using HG;
using System.Linq;
using System.Reflection;

namespace FreeItemFriday
{
    [CreateAssetMenu(menuName = "FreeItemFriday/ActiveEquipmentDef")]
    public class ActiveEquipmentDef : EquipmentDef
    {
        [Serializable]
        public struct TargetMethod
        {
            [SerializableSystemType.RequiredBaseType(typeof(IContentPackProvider))]
            public SerializableSystemType targetType;
            public string targetMethodName;

            private SerializableSystemType cachedType;
            private string cachedTargetMethodName;
            private MethodInfo cachedMethodInfo;

            public MethodInfo GetMethod()
            {
                if (cachedTargetMethodName != targetMethodName || cachedType != targetType)
                {
                    cachedTargetMethodName = targetMethodName ?? string.Empty;
                    cachedType = targetType;
                    cachedMethodInfo = ((Type)targetType)?.GetMethod(targetMethodName ?? string.Empty);
                }
                return cachedMethodInfo;
            }
        }

        static ActiveEquipmentDef()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        private static bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef is ActiveEquipmentDef activeEquipmentDef)
            {
                MethodInfo method = activeEquipmentDef.fireMethod.GetMethod();
                if (method != null)
                {
                    return (bool)method.Invoke(null, new object[] { self });
                }
            }
            return orig(self, equipmentDef);
        }

        public TargetMethod fireMethod;
    }
}