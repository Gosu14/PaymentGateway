using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Infrastructure.Database
{
    public class PaymentConfirmationConfiguration : IEntityTypeConfiguration<PaymentConfirmation>
    {
        public void Configure(EntityTypeBuilder<PaymentConfirmation> builder)
        {
            builder.Property(pc => pc.Id)
                .IsRequired(true);

            builder.Property(pc => pc.Currency)
                .IsRequired(true)
                .HasMaxLength(3);

            builder.Property(pc => pc.Amount)
                .IsRequired(true);

            builder.Property(pc => pc.Status)
                .IsRequired(true)
                .HasMaxLength(50);

            //builder.Property(pc => pc.Type)
            //    .IsRequired(true);

            builder.Property(pc => pc.CardBrand)
                .IsRequired(true);

            builder.Property(pc => pc.CardCountry)
                .IsRequired(true)
                .HasMaxLength(2);

            builder.Property(pc => pc.CardExpiryYear)
                .IsRequired(true);

            builder.Property(pc => pc.Last4)
                .IsRequired(true)
                .HasMaxLength(4);
        }
    }
}
