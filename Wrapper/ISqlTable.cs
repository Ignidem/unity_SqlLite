namespace SqlLite.Wrapper
{
	public interface ISqlTable<I> 
	{ 
		I Id { get; set; }
	}

	public static class SqlTableEx
	{
		public static void Save<I>(this ISqlTable<I> entry)
		{
			SqliteHandler.SaveEntity(entry, entry.Id);
		}

		public static void Delete<T, I>(this T entry)
			where T : ISqlTable<I>
		{
			SqliteHandler.DeleteEntity<T, I>(entry.Id);
		}
	}
}
