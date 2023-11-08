using System;
using System.Threading.Tasks;
using Utilities.Conversions;

namespace SqlLite.Wrapper.Serialization
{
	public class SqlSerializer<TDeserialized, TSerialized> : ISqlSerializer
	{
		public Type DeserializedType => typeof(TDeserialized);
		public Type SerializedType => typeof(TSerialized);

		private bool IsDeserializedNullable => !DeserializedType.IsValueType;
		private bool IsSerializedNullable => !SerializedType.IsValueType;

		public SqlSerializer() { }

		public object Serialize(object input)
		{
			object r = input switch
			{
				TDeserialized _t => Serialize(_t),
				null when IsDeserializedNullable => SerializeNull(),
				_ => throw new SqlSerializerInvalidTypeException
					(GetType(), DeserializedType, input?.GetType(), "serializing")
			};

			Type type = r?.GetType();
			if (type != SerializedType && (type != null || !IsDeserializedNullable))
			{
				throw new SqlSerializerInvalidTypeException(GetType(), SerializedType, type, 
					"returning value after serialization");
			}

			return r;
		}

		protected virtual TSerialized SerializeNull() => default;
		protected virtual TSerialized Serialize(TDeserialized input)
		{
			if (!input.TryConvertTo(out TSerialized r))
			{
				throw new SqlSerializerFailedConversionException(
					GetType(), SerializedType, input.GetType(), "ConvertibleUtils.TryConvertTo");
			}
			
			return r;
		}

		public object Deserialize(object value)
		{
			return value switch
			{
				DBNull or null when IsSerializedNullable => DeserializeNull(),
				TSerialized _s => Deserialize(_s),
				_ => throw new SqlSerializerInvalidTypeException
					(GetType(), SerializedType, value.GetType(), "deserializing")
			};
		}

		protected virtual TDeserialized DeserializeNull() => default;

		protected virtual TDeserialized Deserialize(TSerialized value)
		{
			if (!value.TryConvertTo(out TDeserialized r))
			{
				throw new SqlSerializerFailedConversionException(
					GetType(), DeserializedType, value.GetType(), "ConvertibleUtils.TryConvertTo");
			}

			return r;
		}
	}
}
