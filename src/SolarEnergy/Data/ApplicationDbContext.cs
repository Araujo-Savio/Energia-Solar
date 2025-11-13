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
        public DbSet<CompanyCostProfile> CompanyCostProfiles { get; set; }
        public DbSet<CompanyCostItem> CompanyCostItems { get; set; }
        public DbSet<CompanySystemSizeCost> CompanySystemSizeCosts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurações adicionais do modelo podem ser adicionadas aqui
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

                // Configuração do enum ServiceType
                entity.Property(e => e.ServiceType)
                    .HasConversion<int>()
                    .IsRequired(false);

                // Índices únicos
                entity.HasIndex(e => e.CPF).IsUnique().HasFilter("[CPF] IS NOT NULL");
                entity.HasIndex(e => e.CNPJ).IsUnique().HasFilter("[CNPJ] IS NOT NULL");
            });

            // Configuração da entidade CompanyReview
            builder.Entity<CompanyReview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Comment).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ReviewerId).IsRequired().HasMaxLength(450);
                
                // Relacionamentos
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Reviewer)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índice único para garantir que um usuário só pode avaliar uma empresa uma vez
                entity.HasIndex(e => new { e.CompanyId, e.ReviewerId }).IsUnique();
            });

            // Configuração da entidade Quote
            builder.Entity<Quote>(entity =>
            {
                entity.ToTable("Quotes");
                entity.HasKey(e => e.QuoteId);
                entity.Property(e => e.ClientId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.MonthlyConsumptionKwh).IsRequired(); // Agora é int, não precisa de HasColumnType
                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Message).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

                // Relacionamentos
                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índice único para garantir que um cliente só pode solicitar um orçamento por empresa
                entity.HasIndex(e => new { e.ClientId, e.CompanyId }).IsUnique();
            });

            // Configuração da entidade Proposal
            builder.Entity<Proposal>(entity =>
            {
                entity.ToTable("Proposals");
                entity.HasKey(e => e.ProposalId);
                entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.EstimatedMonthlySavings).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

                // Relacionamento
                entity.HasOne(e => e.Quote)
                    .WithMany(q => q.Proposals)
                    .HasForeignKey(e => e.QuoteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CompanyCostProfile>(entity =>
            {
                entity.ToTable("CompanyCostProfiles");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CompanyId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.ProductionPerKilowattPeak)
                    .HasDefaultValue(140.0);

                entity.Property(e => e.MaintenanceRate)
                    .HasColumnType("decimal(18,6)")
                    .HasDefaultValue(0.015m);

                entity.Property(e => e.RentalRatePerKwh)
                    .HasColumnType("decimal(18,6)")
                    .HasDefaultValue(0.65m);

                entity.Property(e => e.RentalAnnualIncrease)
                    .HasColumnType("decimal(18,6)")
                    .HasDefaultValue(0.03m);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.CompanyId)
                    .IsUnique();
            });

            builder.Entity<CompanyCostItem>(entity =>
            {
                entity.ToTable("CompanyCostItems");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(120);

                entity.Property(e => e.Unit)
                    .HasMaxLength(60);

                entity.Property(e => e.ItemType)
                    .HasConversion<int>();

                entity.Property(e => e.Cost)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Notes)
                    .HasMaxLength(300);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasOne(e => e.Profile)
                    .WithMany(p => p.CostItems)
                    .HasForeignKey(e => e.CompanyCostProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CompanySystemSizeCost>(entity =>
            {
                entity.ToTable("CompanySystemSizeCosts");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.SystemSizeKwp)
                    .HasColumnType("decimal(9,2)");

                entity.Property(e => e.AverageCost)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Notes)
                    .HasMaxLength(200);

                entity.HasOne(e => e.Profile)
                    .WithMany(p => p.SystemSizeCosts)
                    .HasForeignKey(e => e.CompanyCostProfileId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.CompanyCostProfileId, e.SystemSizeKwp })
                    .IsUnique();
            });
        }
    }
}
