using System;

namespace SqlLite.Wrapper.Serialization
{
	public class SqlSerializerInvalidTypeException : Exception
	{
		public readonly Type serializerType;
		public readonly Type expectedType;
		public readonly Type receivedType;

		public SqlSerializerInvalidTypeException(Type serializerType, Type expectedType, Type receivedType, string action) 
			: base ($"{serializerType.Name} expected {expectedType.Name} while {action} {receivedType.Name}.")
		{
			this.serializerType = serializerType;
			this.expectedType = expectedType;
			this.receivedType = receivedType;
		}
	}

	public class SqlSerializerFailedConversionException : Exception
	{
		public readonly Type serializerType;
		public readonly Type targetType;
		public readonly Type receivedType;

		public SqlSerializerFailedConversionException(
			Type serializerType, Type targetType, Type receivedType, string method) 
			: base($"{serializerType.Name} failed to convert {receivedType} to {targetType} using {method}")
		{
			this.serializerType = serializerType;
			this.targetType = targetType;
			this.receivedType = receivedType;
		}
	}
}
