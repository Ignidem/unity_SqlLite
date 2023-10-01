using System;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
		public class AutoIncrement : Attribute { }
	}
}
