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
        public DbSet<TechnicalVisit> TechnicalVisits { get; set; }

        // Lead System
        public DbSet<CompanyLeadBalance> CompanyLeadBalances { get; set; }
        public DbSet<LeadPurchase> LeadPurchases { get; set; }
        public DbSet<LeadConsumption> LeadConsumptions { get; set; }

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

            // Configuração da entidade TechnicalVisit
            builder.Entity<TechnicalVisit>(entity =>
            {
                entity.ToTable("TechnicalVisits");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ClientId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(60);
                entity.Property(e => e.VisitDate).HasColumnType("date").IsRequired();
                entity.Property(e => e.VisitTime).HasColumnType("time").IsRequired();
                entity.Property(e => e.Address).HasMaxLength(200).IsRequired(false);
                entity.Property(e => e.Notes).HasMaxLength(500).IsRequired(false);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.CompanyId, e.VisitDate, e.VisitTime }).IsUnique();
                entity.HasIndex(e => e.VisitDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.ClientId);
            });

            // Configuração da entidade QuoteMessage
            builder.Entity<QuoteMessage>(entity =>
            {
                entity.ToTable("QuoteMessages");
                entity.HasKey(e => e.MessageId);
                entity.Property(e => e.SenderId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.SenderType).HasConversion<int>().IsRequired();
                
                // A propriedade IsRead é [NotMapped], então não configuramos aqui

                // Relacionamentos
                entity.HasOne(e => e.Quote)
                    .WithMany(q => q.Messages)
                    .HasForeignKey(e => e.QuoteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Sender)
                    .WithMany()
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índices
                entity.HasIndex(e => e.QuoteId);
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.SentDate);
                entity.HasIndex(e => e.ReadDate);
            });

            // Configuração das entidades do sistema de leads
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
        }
    }
}
