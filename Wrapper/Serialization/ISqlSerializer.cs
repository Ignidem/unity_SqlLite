using System;
using System.Threading.Tasks;

namespace SqlLite.Wrapper.Serialization
{
	public interface ISqlSerializer
	{
		Type DeserializedType { get; }
		Type SerializedType { get; }

		object Serialize(object input);
		object Deserialize(object sqlEntry);

		Task<object> SerializeAsync(object input);
		Task<object> DeserializeAsync(object entry);
	}
}
