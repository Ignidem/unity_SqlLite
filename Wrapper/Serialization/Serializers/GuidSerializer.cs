using System;
using System.Threading.Tasks;

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

	public class ForeignGuidSerializer<T> : SqlSerializer<T, string>
		where T : ISqlTable<Guid>
	{
		private static SqliteHandler Handler => DefaultSqlite.Instance;

		protected override string Serialize(T input)
		{
			Handler.Save(input);
			return input.Id.ToString();
		}

		protected override async Task<string> SerializeAsync(T input)
		{
			await Handler.SaveAsync(input);
			return input.Id.ToString();
		}

		protected override T Deserialize(string value)
		{
			return Handler.ReadOne<T>(value);
		}

		protected override Task<T> DeserializeAsync(string value)
		{
			return Handler.ReadOneAsync<T>(value);
		}
	}
}
