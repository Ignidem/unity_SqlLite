namespace SqlLite.Wrapper.QueryExtensions
{
	public readonly struct Condition
	{
		private readonly string field;
		private readonly string op;
		public readonly object value;
		public readonly string valueParameterName;

		public Condition(string field, object value) : this(field, "=", value) { }

		public Condition(string field, string op, object value)
		{
			this.field = field;
			this.op = op;
			this.value = value;
			valueParameterName = '@' + field;
		}

		public string ToSQL()
		{
			return string.Format("{0} {1} {2}", field, op, valueParameterName);
		}
	}
}
