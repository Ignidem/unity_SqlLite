namespace SqlLite.Wrapper
{
	public interface ISqlTable
	{
		void Save();
	}

	public interface ISqlTable<I> : ISqlTable
	{ 
		I Id { get; set; }

		void ISqlTable.Save() => SqlTableEx.Save(this);
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
