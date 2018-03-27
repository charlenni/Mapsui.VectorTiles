using System;
using System.Collections;
using System.Globalization;

namespace Mapsui.VectorTiles
{
    [ProtoBuf.ProtoContract(Name = @"value")]
    public sealed class Value : ProtoBuf.IExtensible
    {
        ValueType _type;
        string _stringValue = "";

        public bool HasStringValue { get; set; }
        public bool HasFloatValue { get; set; }
        public bool HasDoubleValue { get; set; }
        public bool HasIntValue { get; set; }
        public bool HasUIntValue { get; set; }
        public bool HasSIntValue { get; set; }
        public bool HasBoolValue { get; set; }

        [ProtoBuf.ProtoMember(1, IsRequired = false, Name = @"string_value", DataFormat = ProtoBuf.DataFormat.Default)]
        [System.ComponentModel.DefaultValue("")]
        public string StringValue
        {
            get => _stringValue;
            set
            {
                HasStringValue = true;
                _type = ValueType.String;
                _stringValue = value;
            }
        }

        float _floatValue;

        [ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"float_value", DataFormat = ProtoBuf.DataFormat.FixedSize)]
        [System.ComponentModel.DefaultValue(default(float))]
        public float FloatValue
        {
            get => _floatValue;
            set
            {
                _floatValue = value;
                _type = ValueType.Float;
                HasFloatValue = true;

            }
        }

        double _doubleValue;

        [ProtoBuf.ProtoMember(3, IsRequired = false, Name = @"double_value",
            DataFormat = ProtoBuf.DataFormat.TwosComplement)]
        [System.ComponentModel.DefaultValue(default(double))]
        public double DoubleValue
        {
            get => _doubleValue;
            set
            {
                _doubleValue = value;
                _type = ValueType.Double;
                HasDoubleValue = true;
            }
        }

        long _intValue;

        [ProtoBuf.ProtoMember(4, IsRequired = false, Name = @"int_value",
            DataFormat = ProtoBuf.DataFormat.TwosComplement)]
        [System.ComponentModel.DefaultValue(default(long))]
        public long IntValue
        {
            get => _intValue;
            set
            {
                _intValue = value;
                _type = ValueType.Int;
                HasIntValue = true;
            }
        }

        ulong _uintValue;

        [ProtoBuf.ProtoMember(5, IsRequired = false, Name = @"uint_value",
            DataFormat = ProtoBuf.DataFormat.TwosComplement)]
        [System.ComponentModel.DefaultValue(default(ulong))]
        public ulong UIntValue
        {
            get => _uintValue;
            set
            {
                _uintValue = value;
                _type = ValueType.UInt;
                HasUIntValue = true;
            }
        }

        long _sintValue;

        [ProtoBuf.ProtoMember(6, IsRequired = false, Name = @"sint_value", DataFormat = ProtoBuf.DataFormat.ZigZag)]
        [System.ComponentModel.DefaultValue(default(long))]
        public long SIntValue
        {
            get => _sintValue;
            set
            {
                _sintValue = value;
                _type = ValueType.SInt;
                HasSIntValue = true;
            }
        }

        bool _boolValue;

        [ProtoBuf.ProtoMember(7, IsRequired = false, Name = @"bool_value", DataFormat = ProtoBuf.DataFormat.Default)]
        [System.ComponentModel.DefaultValue(default(bool))]
        public bool BoolValue
        {
            get => _boolValue;
            set
            {
                _boolValue = value;
                _type = ValueType.Boolean;
                HasBoolValue = true;
            }
        }

        ProtoBuf.IExtension _extensionObject;

        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return ProtoBuf.Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }

        public double? Numeric
        {
            get
            {
                switch (_type)
                {
                    case ValueType.Double:
                        return DoubleValue;
                    case ValueType.Float:
                        return FloatValue;
                    case ValueType.Int:
                        return IntValue;
                    case ValueType.SInt:
                        return SIntValue;
                    case ValueType.UInt:
                        return UIntValue;
                }

                return null;
            }
        }

        public override bool Equals(object o)
        {
            if (!(o is Value other))
                return false;

            if (HasStringValue && other.HasStringValue && StringValue == other.StringValue)
            {
                return true;
            }

            if (HasBoolValue && other.HasBoolValue && BoolValue == other.BoolValue)
            {
                return true;
            }

            if (HasIntValue && other.HasIntValue && IntValue == other.IntValue)
            {
                return true;
            }

            if (HasSIntValue && other.HasSIntValue && SIntValue == other.SIntValue)
            {
                return true;
            }

            if (HasUIntValue && other.HasUIntValue && UIntValue == other.UIntValue)
            {
                return true;
            }

            if (HasFloatValue && other.HasFloatValue && Math.Abs(FloatValue - other.FloatValue) < float.Epsilon)
            {
                return true;
            }

            if (HasDoubleValue && other.HasDoubleValue && Math.Abs(DoubleValue - other.DoubleValue) < double.Epsilon)
            {
                return true;
            }

            return false;
        }

        public bool GreaterThanEquals(Value other)
        {
            if (other.HasBoolValue || other.HasStringValue)
                return false;

            return Numeric != null && other.Numeric != null && Numeric >= other.Numeric;
        }

        public bool GreaterThan(Value other)
        {
            if (other.HasBoolValue || other.HasStringValue)
                return false;

            return Numeric != null && other.Numeric != null && Numeric > other.Numeric;
        }

        public bool LessThanEquals(Value other)
        {
            if (other.HasBoolValue || other.HasStringValue)
                return false;

            return Numeric != null && other.Numeric != null && Numeric <= other.Numeric;
        }

        public bool LessThan(Value other)
        {
            if (other.HasBoolValue || other.HasStringValue)
                return false;

            return Numeric != null && other.Numeric != null && Numeric < other.Numeric;
        }

        public double? GetNumericValue()
        {
            if (HasDoubleValue)
            {
                return DoubleValue;
            }

            if (HasFloatValue)
            {
                return FloatValue;
            }

            if (HasIntValue)
            {
                return IntValue;
            }

            if (HasSIntValue)
            {
                return SIntValue;
            }

            if (HasUIntValue)
            {
                return UIntValue;
            }

            if (HasBoolValue)
            {
                return BoolValue ? 1 : 0;
            }

            if (HasStringValue)
            {
                if (double.TryParse(StringValue, 
                    NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var numericValue))
                {
                    return numericValue;
                }
            }

            return null;
        }

        public override string ToString()
        {
            if (HasDoubleValue)
            {
                return DoubleValue.ToString();
            }

            if (HasFloatValue)
            {
                return FloatValue.ToString();
            }

            if (HasIntValue)
            {
                return IntValue.ToString();
            }

            if (HasSIntValue)
            {
                return SIntValue.ToString();
            }

            if (HasUIntValue)
            {
                return UIntValue.ToString();
            }

            if (HasBoolValue)
            {
                return BoolValue.ToString();
            }

            if (HasStringValue)
            {
                return StringValue;
            }

            return "Unknown value type";
        }
    }
}