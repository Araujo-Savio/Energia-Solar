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
        public DbSet<CompanyParameters> CompanyParameters { get; set; }

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

            // Configuração da entidade CompanyParameters
            builder.Entity<CompanyParameters>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.PricePerKwP).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AnnualMaintenance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.InstallationDiscount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RentalPercent).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MinRentalValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RentalSetupFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AnnualRentIncrease).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RentDiscount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.KwhPerKwp).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MinSystemPower).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UpdatedAt).IsRequired();

                entity.HasIndex(e => e.CompanyId).IsUnique();
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
        }
    }
}
