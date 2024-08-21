using HG.GeneralSerializer;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FreeItemFriday
{
	public static class Extensions
    {
        public static bool HasItem(this CharacterBody characterBody, ItemDef itemDef, out int stack)
        {
            if (characterBody && characterBody.inventory)
            {
                return (stack = characterBody.inventory.GetItemCount(itemDef)) > 0;
            }
            stack = 0;
            return false;
        }

        public static bool HasItem(this CharacterBody characterBody, ItemDef itemDef) => characterBody && characterBody.inventory && characterBody.inventory.GetItemCount(itemDef) > 0;

        public static bool HasItem(this CharacterMaster characterMaster, ItemDef itemDef, out int stack)
        {
            if (characterMaster && characterMaster.inventory)
            {
                return (stack = characterMaster.inventory.GetItemCount(itemDef)) > 0;
            }
            stack = 0;
            return false;
        }

        public static bool HasItem(this CharacterMaster characterMaster, ItemDef itemDef) => characterMaster && characterMaster.inventory && characterMaster.inventory.GetItemCount(itemDef) > 0;

        public static bool HasBuff(this CharacterBody characterBody, BuffDef buffDef, out int count)
        {
            if (characterBody)
            {
                count = characterBody.GetBuffCount(buffDef);
                return count > 0;
            }
            count = 0;
            return false;
        }

        public static void SetValue<T>(this EntityStateConfiguration entityStateConfiguration, string fieldName, T value)
        {
            int index = Array.FindIndex(entityStateConfiguration.serializedFieldsCollection.serializedFields, x => x.fieldName == fieldName);
            if (index < 0)
            {
                throw new KeyNotFoundException(fieldName);
            }
            ref SerializedField serializedField = ref entityStateConfiguration.serializedFieldsCollection.serializedFields[index];
            Type type = typeof(T);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                serializedField.fieldValue.objectValue = value as UnityEngine.Object;
            }
            else if (StringSerializer.CanSerializeType(type))
            {
                serializedField.fieldValue.stringValue = StringSerializer.Serialize(type, value);
            }
            else
            {
                throw new Exception($"Cannot serialize type \"{type.FullName}\".");
            }
        }

        public static T GetValue<T>(this EntityStateConfiguration entityStateConfiguration, string fieldName)
        {
            int index = Array.FindIndex(entityStateConfiguration.serializedFieldsCollection.serializedFields, x => x.fieldName == fieldName);
            if (index < 0)
            {
                throw new KeyNotFoundException(fieldName);
            }
            ref SerializedField serializedField = ref entityStateConfiguration.serializedFieldsCollection.serializedFields[index];
            Type type = typeof(T);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return (T)(object)serializedField.fieldValue.objectValue;
            }
            else if (StringSerializer.CanSerializeType(type))
            {
                return (T)StringSerializer.Deserialize(type, serializedField.fieldValue.stringValue);
            }
            else
            {
                throw new Exception($"Cannot deserialize type \"{type.FullName}\".");
            }
        }

        public static bool TryFind(this Transform transform, string n, out Transform child)
        {
            return child = transform.Find(n);
        }
    }
}