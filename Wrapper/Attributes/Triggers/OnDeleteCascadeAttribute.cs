using System;

namespace SqlLite.Wrapper.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public class OnDeleteCascadeAttribute : OnDeleteTriggerAttribute
	{
		private const string deleteFormat = "DELETE FROM {0} WHERE {1}=old.{2}";
		protected const string nameFormat = "OnDeleteCascade_{0}_{1}";

		public OnDeleteCascadeAttribute(string parentTable, string parentField, string childTable, string childField)
			: base(string.Format(nameFormat, childTable, parentTable), 
				  parentTable, 
				  string.Format(deleteFormat, childTable, childField, parentField))
		{ }

		public OnDeleteCascadeAttribute(string name, string parentTable, string parentField, string childTable, string childField)
			: base(name,  parentTable,
				  string.Format(deleteFormat, childTable, childField, parentField))
		{ }
	}
}
