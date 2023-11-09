using System.Data.Common;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		public async Task BeginTransactionAsync()
		{
			if (uContext != null)
			{
				throw new System.Exception("There is already a connection active.");
			}

			uContext = await CreateContext().OpenAsync();
			uContext.PreventDisposal();
			await uContext.BeginAsync();
		}

		public async Task CommitAsync()
		{
			if (uContext == null) return;

			using SqliteContext context = uContext;
			uContext = null;

			context.AllowDisposal();
			DbTransaction trans = context.GetModule<DbTransaction>();
			await trans.CommitAsync();
		}
	}
}
