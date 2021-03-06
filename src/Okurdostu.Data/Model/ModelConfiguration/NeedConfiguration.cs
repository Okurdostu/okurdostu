﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.ModelConfiguration
{
    public class NeedConfiguration : IEntityTypeConfiguration<Need>
    {
        public void Configure(EntityTypeBuilder<Need> entity)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            entity.HasIndex(e => e.Title)
                    .HasName("Unique_Key_Title")
                    .IsUnique();

            entity.HasIndex(e => e.FriendlyTitle)
                    .HasName("Unique_Key_FriendlyTitle")
                    .IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.CreatedOn).HasColumnType("smalldatetime");

            entity.Property(e => e.Description).IsRequired();

            entity.Property(e => e.FinishedOn).HasColumnType("smalldatetime");

            entity.Property(e => e.FriendlyTitle)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.LastCheckOn).HasColumnType("datetime");

            entity.Property(e => e.StartedOn).HasColumnType("smalldatetime");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(75);

            entity.Property(e => e.TotalCharge).HasColumnType("money");

            entity.Property(e => e.TotalCollectedMoney).HasColumnType("money");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Need)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Need_User");
        }
    }
}