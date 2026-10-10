using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowLevelSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'tenant_app_role') THEN
                        CREATE ROLE tenant_app_role NOSUPERUSER NOBYPASSRLS;
                    END IF;
                END
                $$;

                GRANT USAGE ON SCHEMA public TO tenant_app_role;
                GRANT ALL ON ALL TABLES IN SCHEMA public TO tenant_app_role;
                GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO tenant_app_role;
                ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO tenant_app_role;
                ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO tenant_app_role;

                ALTER TABLE ""Employees"" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE ""Employees"" FORCE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS employee_tenant_isolation_policy ON ""Employees"";
                CREATE POLICY employee_tenant_isolation_policy ON ""Employees""
                FOR ALL
                TO PUBLIC
                USING (
                    current_setting('app.bypass_rls', true) = 'on'
                    OR ""TenantId"" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid
                )
                WITH CHECK (
                    current_setting('app.bypass_rls', true) = 'on'
                    OR ""TenantId"" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid
                );

                ALTER TABLE ""AuditLogs"" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE ""AuditLogs"" FORCE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS auditlog_tenant_isolation_policy ON ""AuditLogs"";
                CREATE POLICY auditlog_tenant_isolation_policy ON ""AuditLogs""
                FOR ALL
                TO PUBLIC
                USING (
                    current_setting('app.bypass_rls', true) = 'on'
                    OR ""TenantId"" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid
                )
                WITH CHECK (
                    current_setting('app.bypass_rls', true) = 'on'
                    OR ""TenantId"" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid
                );

                ALTER TABLE ""CustomFieldDefinitions"" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE ""CustomFieldDefinitions"" FORCE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS customfielddefinition_tenant_isolation_policy ON ""CustomFieldDefinitions"";
                CREATE POLICY customfielddefinition_tenant_isolation_policy ON ""CustomFieldDefinitions""
                FOR ALL
                TO PUBLIC
                USING (
                    current_setting('app.bypass_rls', true) = 'on'
                    OR ""TenantId"" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid
                )
                WITH CHECK (
                    current_setting('app.bypass_rls', true) = 'on'
                    OR ""TenantId"" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS employee_tenant_isolation_policy ON ""Employees"";
                ALTER TABLE ""Employees"" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE ""Employees"" DISABLE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS auditlog_tenant_isolation_policy ON ""AuditLogs"";
                ALTER TABLE ""AuditLogs"" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE ""AuditLogs"" DISABLE ROW LEVEL SECURITY;

                DROP POLICY IF EXISTS customfielddefinition_tenant_isolation_policy ON ""CustomFieldDefinitions"";
                ALTER TABLE ""CustomFieldDefinitions"" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE ""CustomFieldDefinitions"" DISABLE ROW LEVEL SECURITY;

                REVOKE ALL ON ALL TABLES IN SCHEMA public FROM tenant_app_role;
                REVOKE ALL ON ALL SEQUENCES IN SCHEMA public FROM tenant_app_role;
                REVOKE USAGE ON SCHEMA public FROM tenant_app_role;
            ");
        }
    }
}
