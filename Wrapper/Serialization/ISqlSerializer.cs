using System;

namespace SqlLite.Wrapper.Serialization
{
	public interface ISqlSerializer
	{
		Type DeserializedType { get; }
		Type SerializedType { get; }

		object Serialize(object input);

		object Deserialize(object sqlEntry);
	}
}
