using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		public async Task BeginTransactionAsync()
		{
			if (uContext != null) return;

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
			await context.CommitAsync();
		}

		public async Task RollbackAsync()
		{
			if (uContext == null) return;

			using SqliteContext context = uContext;
			uContext = null;

			context.AllowDisposal();
			await context.RollbackAsync();
		}
	}
}
