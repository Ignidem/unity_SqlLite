using System.Threading.Tasks;
using Utilities.Conversions;

namespace SqlLite.Wrapper.Serialization
{
	public class AsEntrySerializer<TEntry, TKey> : SqlSerializer<object, TKey>
		where TEntry : ISqlTable<TKey>
	{
		public static ISqliteHandler Handler = DefaultSqlite.Instance;

		protected override TKey Serialize(object input)
		{
			if (!input.TryConvertTo(out TEntry entry))
				throw new System.InvalidCastException();

			(entry as ISqlTable).Save();
			return entry.Id;
		}

		protected override async Task<TKey> SerializeAsync(object input)
		{
			if (!input.TryConvertTo(out TEntry entry))
				throw new System.InvalidCastException();

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
