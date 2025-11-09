using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace SolarEnergy.Migrations
{
    /// <inheritdoc />
    public partial class ForceCreateProposalsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primeiro, verificar se a tabela existe e dropá-la se existir
            migrationBuilder.Sql("DROP TABLE IF EXISTS [Proposals]");
            
            // Criar a tabela Proposals com todas as colunas necessárias
            migrationBuilder.Sql(@"
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
                    CONSTRAINT [PK_Proposals] PRIMARY KEY ([ProposalId])
                );
            ");
            
            // Adicionar foreign key se a tabela Quotes existe
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Quotes')
                BEGIN
                    ALTER TABLE [Proposals] ADD CONSTRAINT [FK_Proposals_Quotes_QuoteId] 
                    FOREIGN KEY ([QuoteId]) REFERENCES [Quotes] ([QuoteId]) ON DELETE CASCADE;
                    
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
