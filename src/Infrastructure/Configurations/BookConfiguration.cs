using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.ToTable("Books", table =>
            table.HasCheckConstraint(
                "CK_Books_Title_Length",
                "LEN([Title]) >= 3"));

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .IsRequired();

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.AuthorId)
                .IsRequired();

            builder.HasOne<Author>()
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .IsRequired();

            builder.Property(e => e.SubTitle)
                .IsRequired(false);
        }
    }
}
