using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace SolarEnergy.Migrations
{
    /// <inheritdoc />
    public partial class EnsureProposalsTableExists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verificar se a tabela Proposals não existe e criá-la
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Proposals')
                BEGIN
                    CREATE TABLE [Proposals] (
                        [ProposalId] int IDENTITY(1,1) NOT NULL,
                        [QuoteId] int NOT NULL,
                        [Value] decimal(18,2) NOT NULL,
                        [Description] nvarchar(2000) NULL,
                        [InstallationTimeframe] int NULL,
                        [Warranty] int NULL,
                        [EstimatedMonthlySavings] decimal(18,2) NULL,
                        [ProposalDate] datetime2 NOT NULL,
                        [ValidUntil] datetime2 NULL,
                        [Status] nvarchar(50) NOT NULL,
                        CONSTRAINT [PK_Proposals] PRIMARY KEY ([ProposalId]),
                        CONSTRAINT [FK_Proposals_Quotes_QuoteId] FOREIGN KEY ([QuoteId]) REFERENCES [Quotes] ([QuoteId]) ON DELETE CASCADE
                    );
                    
                    CREATE INDEX [IX_Proposals_QuoteId] ON [Proposals] ([QuoteId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS [Proposals]");
        }
    }
}
