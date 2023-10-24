using System;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
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

			public static int GetNextIndex(Type type) => GetNextIndex(type.Name);
			private static int GetNextIndex(string name)
			{
				const string id = nameof(__AutoIncrementIndexes.Id);
				__AutoIncrementIndexes indexer = LoadOne<__AutoIncrementIndexes, string>(name, id, true);
				indexer.Index += 1;
				indexer.Save();
				return indexer.Index;
			}

			private string table;

			public int GetNextIndex()
			{
				return GetNextIndex(table);
			}

			internal void SetType(Type type)
			{
				table = type.Name;
			}
		}
	}
}
