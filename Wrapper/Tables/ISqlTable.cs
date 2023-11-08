using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public interface ISqlTable
	{
		Task SaveAsync();
		void Save();
		Task DeleteAsync();
		void Delete();
	}

	public interface ISqlTable<I> : ISqlTable
	{
		SqliteHandler Handler => DefaultSqlite.Instance;

		I Id { get; set; }

		void ISqlTable.Save()
		{
			DefaultSqlite.SaveEntry(this);
		}

		void ISqlTable.Delete()
		{
			DefaultSqlite.DeleteEntry(this);
		}

		Task ISqlTable.SaveAsync()
		{
			return DefaultSqlite.SaveEntryAsync(this);
		}

		Task ISqlTable.DeleteAsync()
		{
			return DefaultSqlite.DeleteEntryAsync(this);
		}
	}

	public static class DefaultSqlite
	{
		public static SqliteHandler Instance = new SqliteHandler();

		public static Task SaveEntryAsync<T>(this ISqlTable<T> entry)
		{
			return entry.Handler.SaveAsync(entry);
		}

		public static Task DeleteEntryAsync<T>(this ISqlTable<T> entry)
		{
			return entry.Handler.DeleteAsync(entry);
		}

		public static void SaveEntry<T>(this ISqlTable<T> entry)
		{
			entry.Handler.Save(entry);
		}

		public static void DeleteEntry<T>(this ISqlTable<T> entry)
		{
			entry.Handler.Delete(entry);
		}
	}
}
