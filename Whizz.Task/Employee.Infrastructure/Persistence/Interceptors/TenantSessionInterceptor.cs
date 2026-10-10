using System.Data;
using System.Data.Common;
using Employee.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Employee.Infrastructure.Persistence.Interceptors
{
    public class TenantSessionInterceptor : DbCommandInterceptor
    {
        private readonly ITenantContext _tenantContext;

        public TenantSessionInterceptor(ITenantContext tenantContext)
        {
            _tenantContext = tenantContext;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            ConfigureTenantSession(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            await ConfigureTenantSessionAsync(command, cancellationToken);
            return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            ConfigureTenantSession(command);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            await ConfigureTenantSessionAsync(command, cancellationToken);
            return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            ConfigureTenantSession(command);
            return base.ScalarExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            await ConfigureTenantSessionAsync(command, cancellationToken);
            return await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void ConfigureTenantSession(DbCommand command)
        {
            if (command.Connection == null || command.Connection.State != ConnectionState.Open)
            {
                return;
            }

            if (command.CommandText.StartsWith("SET ROLE", StringComparison.OrdinalIgnoreCase) ||
                command.CommandText.StartsWith("RESET ROLE", StringComparison.OrdinalIgnoreCase) ||
                command.CommandText.StartsWith("SELECT set_config", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            using var sessionCmd = command.Connection.CreateCommand();
            sessionCmd.Transaction = command.Transaction;

            if (_tenantContext.TenantId.HasValue)
            {
                sessionCmd.CommandText = $"SET ROLE tenant_app_role; SELECT set_config('app.current_tenant_id', '{_tenantContext.TenantId.Value}', false), set_config('app.bypass_rls', 'off', false);";
            }
            else
            {
                sessionCmd.CommandText = "RESET ROLE; SELECT set_config('app.bypass_rls', 'on', false), set_config('app.current_tenant_id', '', false);";
            }

            sessionCmd.ExecuteNonQuery();
        }

        private async Task ConfigureTenantSessionAsync(DbCommand command, CancellationToken cancellationToken)
        {
            if (command.Connection == null || command.Connection.State != ConnectionState.Open)
            {
                return;
            }

            if (command.CommandText.StartsWith("SET ROLE", StringComparison.OrdinalIgnoreCase) ||
                command.CommandText.StartsWith("RESET ROLE", StringComparison.OrdinalIgnoreCase) ||
                command.CommandText.StartsWith("SELECT set_config", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            using var sessionCmd = command.Connection.CreateCommand();
            sessionCmd.Transaction = command.Transaction;

            if (_tenantContext.TenantId.HasValue)
            {
                sessionCmd.CommandText = $"SET ROLE tenant_app_role; SELECT set_config('app.current_tenant_id', '{_tenantContext.TenantId.Value}', false), set_config('app.bypass_rls', 'off', false);";
            }
            else
            {
                sessionCmd.CommandText = "RESET ROLE; SELECT set_config('app.bypass_rls', 'on', false), set_config('app.current_tenant_id', '', false);";
            }

            await sessionCmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
