using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Models;

namespace SolarEnergy.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CompanyReview> CompanyReviews { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<QuoteMessage> QuoteMessages { get; set; }
        public DbSet<CompanyParameters> CompanyParameters { get; set; }

        // Lead System
        public DbSet<CompanyLeadBalance> CompanyLeadBalances { get; set; }
        public DbSet<LeadPurchase> LeadPurchases { get; set; }
        public DbSet<LeadConsumption> LeadConsumptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---- CONFIGURAÇÃO DE USUÁRIO (ApplicationUser) ----
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CPF).HasMaxLength(14);
                entity.Property(e => e.CNPJ).HasMaxLength(18);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.CompanyLegalName).HasMaxLength(120);
                entity.Property(e => e.CompanyTradeName).HasMaxLength(120);
                entity.Property(e => e.StateRegistration).HasMaxLength(30);
                entity.Property(e => e.CompanyPhone).HasMaxLength(20);
                entity.Property(e => e.CompanyWebsite).HasMaxLength(200);
                entity.Property(e => e.CompanyDescription).HasMaxLength(500);
                entity.Property(e => e.ResponsibleName).HasMaxLength(100);
                entity.Property(e => e.ResponsibleCPF).HasMaxLength(14);
                entity.Property(e => e.Location).HasMaxLength(120);
                entity.Property(e => e.ProfileImagePath).HasMaxLength(260);

                entity.Property(e => e.ServiceType)
                    .HasConversion<int>()
                    .IsRequired(false);

                entity.HasIndex(e => e.CPF).IsUnique().HasFilter("[CPF] IS NOT NULL");
                entity.HasIndex(e => e.CNPJ).IsUnique().HasFilter("[CNPJ] IS NOT NULL");
            });

            // ---- COMPANY REVIEW ----
            builder.Entity<CompanyReview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Comment).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ReviewerId).IsRequired().HasMaxLength(450);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Reviewer)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.CompanyId, e.ReviewerId }).IsUnique();
            });

            // ---- QUOTE ----
            builder.Entity<Quote>(entity =>
            {
                entity.ToTable("Quotes");
                entity.HasKey(e => e.QuoteId);
                entity.Property(e => e.ClientId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.MonthlyConsumptionKwh).IsRequired();
                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Message).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ClientId, e.CompanyId }).IsUnique();
            });

            // ---- PROPOSAL ----
            builder.Entity<Proposal>(entity =>
            {
                entity.ToTable("Proposals");
                entity.HasKey(e => e.ProposalId);
                entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.EstimatedMonthlySavings).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Quote)
                    .WithMany(q => q.Proposals)
                    .HasForeignKey(e => e.QuoteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- QUOTE MESSAGE ----
            builder.Entity<QuoteMessage>(entity =>
            {
                entity.ToTable("QuoteMessages");
                entity.HasKey(e => e.MessageId);
                entity.Property(e => e.SenderId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.SenderType).HasConversion<int>().IsRequired();

                entity.HasOne(e => e.Quote)
                    .WithMany(q => q.Messages)
                    .HasForeignKey(e => e.QuoteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Sender)
                    .WithMany()
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.QuoteId);
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.SentDate);
                entity.HasIndex(e => e.ReadDate);
            });

            // ---- LEAD SYSTEM ----
            builder.Entity<CompanyLeadBalance>(entity =>
            {
                entity.ToTable("CompanyLeadBalances");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.AvailableLeads).IsRequired();
                entity.Property(e => e.ConsumedLeads).IsRequired();
                entity.Property(e => e.TotalPurchasedLeads).IsRequired();

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.CompanyId).IsUnique();
            });

            builder.Entity<LeadPurchase>(entity =>
            {
                entity.ToTable("LeadPurchases");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.LeadQuantity).IsRequired();
                entity.Property(e => e.UnitPrice).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.DiscountPercentage).IsRequired().HasPrecision(5, 2);
                entity.Property(e => e.TotalAmount).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.PurchaseDate);
                entity.HasIndex(e => e.TransactionId);
            });

            builder.Entity<LeadConsumption>(entity =>
            {
                entity.ToTable("LeadConsumptions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.QuoteId).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(200);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Quote)
                    .WithMany()
                    .HasForeignKey(e => e.QuoteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.QuoteId);
                entity.HasIndex(e => new { e.CompanyId, e.QuoteId }).IsUnique();
                entity.HasIndex(e => e.ConsumedAt);
            });

            // ---- COMPANY PARAMETERS (FINAL, CORRIGIDO) ----
            builder.Entity<CompanyParameters>(entity =>
            {
                entity.ToTable("CompanyParameters");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CompanyId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.SystemPricePerKwp)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.MaintenancePercent)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.InstallDiscountPercent)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.RentalFactorPercent)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.RentalMinMonthly)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.RentalSetupPerKwp)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.RentalAnnualIncreasePercent)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.RentalDiscountPercent)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.ConsumptionPerKwp)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.MinSystemSizeKwp)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime2");

                entity.HasOne(e => e.Company)
                    .WithOne(c => c.Parameters)
                    .HasForeignKey<CompanyParameters>(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.CompanyId).IsUnique();
            });
        }
    }
}
