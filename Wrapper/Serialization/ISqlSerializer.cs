using System;
using System.Threading.Tasks;

namespace SqlLite.Wrapper.Serialization
{
	public interface ISqlSerializer
	{
		Type DeserializedType { get; }
		Type SerializedType { get; }

		object SerializeObject(object input);
		object DeserializeObject(object sqlEntry);

		Task<object> SerializeObjectAsync(object input);
		Task<object> DeserializeObjectAsync(object entry);
	}
}
