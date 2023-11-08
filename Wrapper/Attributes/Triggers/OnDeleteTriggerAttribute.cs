namespace SqlLite.Wrapper.Attributes
{
	public class OnDeleteTriggerAttribute : TriggerAttribute
	{
		protected const string onDeleteFormat = "AFTER DELETE ON {0}";

		public OnDeleteTriggerAttribute(string name, string table, string query) 
			: base(name, string.Format(onDeleteFormat, table), query) { }
	}
}
