namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		public abstract class SqlTable<TKey> : ISqlTable<TKey>, ITable
		{
			public static TEntry LoadOne<TEntry>(TKey id, bool createIfNone = false)
				where TEntry : class, ITable, new() 
				=> LoadOne<TEntry, TKey>(id, "Id", createIfNone);

			public static TEntry[] LoadAll<TEntry>(TKey id, string keyName = "Id")
				where TEntry : class, ITable, new() 
				=> SqliteHandler.LoadAll<TEntry, TKey>(id, keyName);

			public static TEntry[] LoadAll<TEntry, TForeign>(TForeign id, string keyName = "ForeignKey")
				where TEntry : class, ITable, new() 
				=> SqliteHandler.LoadAll<TEntry, TForeign>(id, keyName);

			public static void DeleteAll<TEntry, TForeign>(TForeign id, string keyName = "Id")
				where TEntry : class, ITable, new() 
				=> SqliteHandler.DeleteAll<TEntry, TForeign>(id, keyName);

			public static void Delete<TEntry>(TKey id)
				where TEntry : class, ITable, new()
				=> DeleteEntity<TEntry, TKey>(id);

			public TKey Id { get; set; }

			public virtual void Save() => SaveEntity(this, Id);

			public virtual void Delete() => DeleteEntity(GetType(), Id);
		}
	}
}
