using System;
using System.Threading.Tasks;

namespace SqlLite.Wrapper.Attributes
{
	public class TriggerAttribute : Attribute
	{
		protected const string triggerFormat = "CREATE TRIGGER {0} {1} BEGIN {2}; END";

		protected string name;
		protected string trigger;
		protected string query;
		private string TriggerSQL => string.Format(triggerFormat, name, trigger, query);

		public TriggerAttribute(string name, string trigger, string query) 
		{
			this.name = name;
			this.trigger = trigger;
			this.query = query;
		}

		public async Task UpdateTrigger(SqliteHandler handler)
		{
			if (name == null || trigger == null || query == null)
				return;

			if (await handler.ExistsAsync(name, "trigger"))
				return;

			await handler.ExecuteQueryAsync(TriggerSQL);
		}

		public void UpdateTriggerSync(SqliteHandler handler)
		{
			if (name == null || trigger == null || query == null)
				return;

			if (handler.Exists(name, "trigger"))
				return;

			handler.ExecuteQuery(TriggerSQL);
		}
	}
}
