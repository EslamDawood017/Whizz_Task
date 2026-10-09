using Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee.Infrastructure.Persistence.Configurations
{
    internal class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
    {
        public void Configure(EntityTypeBuilder<CustomFieldDefinition> builder)
        {
            builder.ToTable("CustomFieldDefinitions");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.FieldName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DataType).HasMaxLength(50).IsRequired();

            builder.HasIndex(x => new {x.TenantId , x.FieldName}).IsUnique();

            builder.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
