using Employee;
using Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;


namespace Employee.Infrastructure.Persistence.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Domain.Entities.Employee>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Domain.Entities.Employee> builder)
        {
            builder.ToTable("Employees");

            builder.HasKey(e => e.Id);

            builder.Property(x => x.FirstName).IsRequired();
            builder.Property(x => x.LastName).IsRequired();
            builder.Property(x => x.Email).IsRequired();
            builder.Property(x => x.Department).IsRequired();

            builder.Property(x => x.Status).HasConversion<string>();

            builder.Property(e => e.CustomData)
                   .HasColumnType("jsonb")
                   .IsRequired()
                   .HasDefaultValue("{}");

            builder.OwnsOne(e => e.Salary, salary =>
            {
                salary.Property(m => m.AmountMinor)
                    .HasColumnName("SalaryAmountMinor");
                salary.Property(m => m.CurrencyCode)
                    .HasColumnName("SalaryCurrencyCode")
                    .HasMaxLength(3);
            });

            builder.HasIndex(e => new { e.TenantId, e.Email })
                   .IsUnique()
                   .HasFilter("\"DeletedAt\" IS NULL");

            builder.HasIndex(e => e.TenantId);

            builder.HasOne(e => e.Tenant)
                   .WithMany()
                   .HasForeignKey(e => e.TenantId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
