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

			public int GetNextIndex(Type type)
			{
				const string id = nameof(__AutoIncrementIndexes.Id);
				__AutoIncrementIndexes indexer = LoadOne<__AutoIncrementIndexes, string>(type.Name, id, true);
				indexer.Index += 1;
				indexer.Save();
				return indexer.Index;
			}
		}
	}
}
