using System;

namespace SqlLite.Wrapper.Serialization
{
	public class GuidSerializer : SqlSerializer<Guid, byte[]>
	{
		protected override Guid Deserialize(byte[] value)
		{
			return new Guid(value);
		}

		protected override byte[] Serialize(Guid input)
		{
			return input.ToByteArray();
		}
	}
}
