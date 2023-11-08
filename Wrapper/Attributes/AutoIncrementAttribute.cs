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

		private string table;

		public int GetNextIndex(SqliteHandler handler)
		{
			return GetNextIndex(handler, table);
		}

		internal void SetType(Type type)
		{
			table = type.Name;
		}

	}
}
