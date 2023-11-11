using System;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	public class AutoIncrementAttribute : Attribute
	{
#pragma warning disable IDE1006 // Naming Styles
		private class __AutoIncrementIndexes : ISqlTable<string>
#pragma warning restore IDE1006 // Naming Styles
		{
			public string Id { get; set; }
			public int Index { get; set; }
			public __AutoIncrementIndexes() { }
		}

		public static int GetNextIndex(SqliteHandler handler, Type type) => GetNextIndex(handler, type.Name);
		private static int GetNextIndex(SqliteHandler handler, string name)
		{
			const string id = nameof(__AutoIncrementIndexes.Id);
			__AutoIncrementIndexes indexer = handler.ReadOne<__AutoIncrementIndexes>(name, id, true);
			indexer.Index += 1;
			ISqlTable tab = indexer;
			tab.Save();
			return indexer.Index;
		}
		private static async Task<int> GetNextIndexAsync(SqliteHandler handler, string name)
		{
			const string id = nameof(__AutoIncrementIndexes.Id);
			__AutoIncrementIndexes indexer = await handler.ReadOneAsync<__AutoIncrementIndexes>(name, id, true);
			indexer.Index += 1;
			ISqlTable tab = indexer;
			await tab.SaveAsync();
			return indexer.Index;
		}

		public static void SetIncrement(SqliteHandler handler, string name, int value)
		{
			const string id = nameof(__AutoIncrementIndexes.Id);
			__AutoIncrementIndexes indexer = handler.ReadOne<__AutoIncrementIndexes>(name, id, true);
			indexer.Index = value;
			indexer.SaveEntry();
		}
		public static async Task SetIncrementAsync(SqliteHandler handler, string name, int value)
		{
			const string id = nameof(__AutoIncrementIndexes.Id);
			__AutoIncrementIndexes indexer = await handler.ReadOneAsync<__AutoIncrementIndexes>(name, id, true);
			indexer.Index = value;
			await (indexer as ISqlTable).SaveAsync();
		}

		private string table;

		public int GetNextIndex(SqliteHandler handler)
		{
			return GetNextIndex(handler, table);
		}

		public Task<int> GetNextIndexAsync(SqliteHandler handler)
		{
			return GetNextIndexAsync(handler, table);
		}

		public void SetNextIndex(SqliteHandler handler, int value)
		{
			SetIncrement(handler, table, value);
		}

		public Task SetNextIndexAsync(SqliteHandler handler, int value)
		{
			return SetIncrementAsync(handler, table, value);
		}

		internal void SetType(Type type)
		{
			table = type.Name;
		}

	}
}
