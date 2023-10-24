using System;
using Utilities.Conversions;

namespace SqlLite.Wrapper.Serialization
{
	public class SqlSerializer<TDeserialized, TSerialized> : ISqlSerializer
	{
		public Type DeserializedType => typeof(TDeserialized);
		public Type SerializedType => typeof(TSerialized);

		public SqlSerializer() { }

		public object Serialize(object input)
		{
			if (input is not TDeserialized _t)
			{
				throw new SqlSerializerInvalidTypeException(GetType(), DeserializedType, input.GetType(), "serializing");
			}

			object r = Serialize(_t);

			Type type = r.GetType();
			if (type != SerializedType)
			{
				throw new SqlSerializerInvalidTypeException(GetType(), SerializedType, type, 
					"returning value after serialization");
			}

			return r;
		}

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
			if (value is DBNull)
				return default(TSerialized);

			if (value is not TSerialized _s)
			{
				throw new SqlSerializerInvalidTypeException(GetType(), SerializedType, value.GetType(), "deserializing");
			}

			return Deserialize(_s);
		}

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
