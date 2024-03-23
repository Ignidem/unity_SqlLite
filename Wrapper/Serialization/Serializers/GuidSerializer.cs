using Mono.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace SqlLite.Wrapper.Serialization
{
	public class GuidSerializer : SqlSerializer<Guid, byte[]>
	{
		protected override bool CanDeserializeNull => true;
		protected override bool CanSerializeNull => true;

		protected override Guid Deserialize(byte[] value)
		{
			return value == null ? default : new Guid(value);
		}

		protected override byte[] Serialize(Guid input)
		{
			return input == default ? null : input.ToByteArray();
		}
	}

	//For serializing a list of T : ISqlTable<Guid> into byte[] where each item's Guid key is listed.
	//owner key with Read All Where is a much better idea.
	public class ForeignGuidsSerializer<T> : SqlSerializer<T[], byte[]>
		where T : ISqlTable<Guid>
	{
		const string queryFormat =
		"with recursive split(i, d, o) as (values(1, @Data, @Data) UNION ALL" +
		"select(i+16), (substr(o, i, i+15)), (o) from split where i<length(o))" +
		"select * from {0} where Id in (select d from split limit -1 offset 1)";

		private static ISqliteHandler Handler => DefaultSqlite.Instance;
		private const int GUIDSIZE = 16;

		protected override byte[] Serialize(T[] input)
		{
			if (input == null || input.Length == 0)
				return default(Guid).ToByteArray();

			byte[] result = new byte[input.Length * GUIDSIZE];

			void AppendGuid(T entry, int index)
			{
				byte[] id = entry.Id.ToByteArray();
				Array.Copy(id, 0, result, index * GUIDSIZE, GUIDSIZE);
			}

			Handler.SaveMany<T, Guid>(input, AppendGuid);

			return result;
		}

		protected override async Task<byte[]> SerializeAsync(T[] input)
		{
			if (input == null || input.Length == 0)
				return default(Guid).ToByteArray();

			byte[] result = new byte[input.Length * GUIDSIZE];

			void AppendGuid(T entry, int index)
			{
				byte[] id = entry.Id.ToByteArray();
				Array.Copy(id, 0, result, index * GUIDSIZE, GUIDSIZE);
			}

			await Handler.SaveManyAsync<T, Guid>(input, AppendGuid);

			return result;
		}

		protected override T[] Deserialize(byte[] value)
		{
			if (value == null || value.Length == 0) 
				return new T[0];
			if (value.Length == 16 && new Guid(value) == default)
				return new T[0];

			string query = string.Format(queryFormat, DeserializedType.Name);

			void FormatCommand(SqliteCommand command)
			{
				command.Parameters.AddWithValue("@Data", value);
			}

			return Handler.ReadAll<T>(query, FormatCommand);
		}

		protected override async Task<T[]> DeserializeAsync(byte[] value)
		{
			if (value == null || value.Length == 0)
				return new T[0];
			if (value.Length == 16 && new Guid(value) == default)
				return new T[0];

			string query = string.Format(queryFormat, DeserializedType.Name);

			void FormatCommand(SqliteCommand command)
			{
				command.Parameters.AddWithValue("@Data", value);
			}

			return await Handler.ReadAllAsync<T>(query, FormatCommand);
		}
	}
}

