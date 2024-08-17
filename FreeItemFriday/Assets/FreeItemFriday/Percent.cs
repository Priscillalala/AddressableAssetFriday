using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace FreeItemFriday
{
	public readonly struct Percent : IComparable<float>, IEquatable<float>, IFormattable
    {
        public static readonly NumberFormatInfo defaultNumberFormat = new NumberFormatInfo
        {
            PercentPositivePattern = 1,
            PercentNegativePattern = 1,
            PercentDecimalDigits = 0
        };

        static Percent()
        {
            TomlTypeConverter.AddConverter(typeof(Percent), new TypeConverter
            {
                ConvertToString = (obj, type) => obj.ToString(),
                ConvertToObject = (str, type) =>
                {
                    string percentSymbol = defaultNumberFormat.PercentSymbol;
                    int index = str.IndexOf(percentSymbol);
                    if (index < 0)
                    {
                        return new Percent(float.Parse(str, defaultNumberFormat));
                    }
                    str = str.Remove(index, percentSymbol.Length);
                    return new Percent(float.Parse(str, defaultNumberFormat) / 100f);
                }
            });
        }

        public float Value { get; }

        public Percent(float value)
        {
            Value = value;
        }

        public int CompareTo(float other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(float other)
        {
            return Value.Equals(other);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString("p", formatProvider);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return Value.ToString("p", formatProvider);
        }

        public override string ToString()
        {
            return Value.ToString("p", defaultNumberFormat);
        }

        public override bool Equals(object obj)
        {
            if (obj is Percent percent)
            {
                return Value == percent.Value;
            }
            if (obj is float other)
            {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator Percent(float value)
        {
            return new Percent(value);
        }

        public static implicit operator float(Percent percent)
        {
            return percent.Value;
        }
    }
}