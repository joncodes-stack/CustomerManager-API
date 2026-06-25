using CustomerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Infra.EntityTypeConfigurations
{
    internal class CustomerEntityConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customer");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CardHolderName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Cpf)
                   .IsRequired()
                   .HasColumnType("char(20)");

            builder.Property(x => x.Status)
               .IsRequired()
               .HasDefaultValue(false);
        }
    }
}
