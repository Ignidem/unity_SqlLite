using System.Threading.Tasks;

namespace SqlLite.Wrapper.Serialization
{
	public class AsEntrySerializer<TEntry, TKey> : SqlSerializer<object, TKey>
		where TEntry : ISqlTable<TKey>
	{
		public static ISqliteHandler Handler = DefaultSqlite.Instance;

		protected override TKey Serialize(object input)
		{
			TEntry entry = (TEntry)input;
			(entry as ISqlTable).Save();
			return entry.Id;
		}

		protected override async Task<TKey> SerializeAsync(object input)
		{
			TEntry entry = (TEntry)input;
			await (entry as ISqlTable).SaveAsync();
			return entry.Id;
		}

		protected override object Deserialize(TKey value)
		{
			return Handler.ReadOne<TEntry>(value);
		}

		protected override async Task<object> DeserializeAsync(TKey value)
		{
			return await Handler.ReadOneAsync<TEntry>(value);
		}
	}
}
