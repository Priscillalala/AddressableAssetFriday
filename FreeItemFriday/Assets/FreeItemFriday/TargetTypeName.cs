using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FreeItemFriday
{
    public class TargetTypeNameAttribute : TargetAssetNameAttribute
    {
        public TargetTypeNameAttribute(Type targetTypeName) : base(targetTypeName.FullName) { }
    }
}