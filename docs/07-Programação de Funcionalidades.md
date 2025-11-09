# Programação de Funcionalidades

<span style="color:red">Pré-requisitos: <a href="02-Especificação do Projeto.md"> Especificação do Projeto</a></span>, <a href="04-Projeto de Interface.md"> Projeto de Interface</a>, <a href="03-Metodologia.md"> Metodologia</a>, <a href="05-Arquitetura da Solução.md"> Arquitetura da Solução</a>

Nesta seção são apresentadas as funcionalidades implementadas no sistema Solar Energy, relacionando os requisitos funcionais e não funcionais atendidos com os artefatos de código desenvolvidos. A aplicação utiliza a arquitetura MVC (Model-View-Controller) com ASP.NET Core e Entity Framework, seguindo as melhores práticas de segurança e desenvolvimento web.

## Tecnologias Utilizadas

**Tecnologias Obrigatórias Implementadas:**
- **Microsoft Visual Studio 2022** - IDE de desenvolvimento
- **ASP.NET Core MVC** - Framework web em C#
- **Entity Framework Core** - ORM para acesso a dados
- **SQL Server** - Sistema de gerenciamento de banco de dados
- **HTML5, CSS3** - Estruturação e estilização do frontend
- **JavaScript** - Interatividade no frontend
- **Bootstrap 5.3** - Framework CSS responsivo
- **ASP.NET Core Identity** - Sistema de autenticação e autorização
- **GitHub** - Controle de versão e documentação

## Funcionalidades Implementadas


A tabela a seguir relaciona os requisitos funcionais implementados com os respectivos artefatos de código produzidos. O projeto está em estágio avançado, com praticamente todos os fluxos principais implementados:

### Requisitos Funcionais Implementados

|ID    | Descrição do Requisito  | Status | Artefatos Produzidos |
|------|-------------------------|--------|---------------------|
|RF-001| Pesquisa de empresas por localização | ✅ **IMPLEMENTADO** | `HomeController.cs`, `SearchCompanies`, `CompanyDetails`, views de busca/listagem |
|RF-002| Perfil detalhado das empresas | ✅ **IMPLEMENTADO** | `HomeController.cs`, `CompanyDetails`, views de empresa |
|RF-003| Sistema de avaliações | ✅ **IMPLEMENTADO** | `HomeController.cs`, `AddReview`, `CompanyReview`, views de avaliações |
|RF-004| Comparação de orçamentos recebidos | ✅ **IMPLEMENTADO** | `QuoteController.cs`, `ViewQuoteDetails`, dashboards |
|RF-005| Visualização, avaliação e comentários após contratação | ✅ **IMPLEMENTADO** | `HomeController.cs`, `Evaluations`, `CompanyReview` |
|RF-006| Cadastro/atualização de perfil de empresas | ✅ **IMPLEMENTADO** | `AuthController.cs`, `Register.cshtml`, `ApplicationUser.cs`, `RegisterViewModel.cs` |
|RF-007| Solicitações e propostas de orçamento | ✅ **IMPLEMENTADO** | `QuoteController.cs`, `RequestQuote`, `MyQuotes`, `SendCompanyResponse` |
|RF-008| Visualização do tempo estimado de retorno do investimento | ✅ **IMPLEMENTADO** | `Simulation`, dashboards, views |
|RF-009| Moderação de avaliações e verificação de documentos | ✅ **IMPLEMENTADO** | `AdminDashboard`, views admin |
|RF-010| Integração com mapas/geolocalização | 🚧 **PARCIAL** | Placeholders visuais, falta integração real com API de mapas |
|RF-011| Recuperação de senha | ✅ **IMPLEMENTADO** | `AuthController.cs`, `LoginViewModel.cs`, `Login.cshtml` |
|RF-012| Histórico de solicitações de orçamento | ✅ **IMPLEMENTADO** | `QuoteController.cs`, `MyQuotes`, dashboards |
|RF-013| Notificações sobre status das solicitações | ✅ **IMPLEMENTADO** | Status em dashboards, chat, leads |
|RF-014| Relatórios mensais para empresas | ✅ **IMPLEMENTADO** | `ReportsController.cs`, `MonthlyReport`, `ExportReport` |
|RF-015| Gestão de leads (CPL) pelo administrador | ✅ **IMPLEMENTADO** | `LeadsController.cs`, `AdminDashboard`, views de gestão |
|RF-016| Gestão de planos CPL e usuários pelo administrador | ✅ **IMPLEMENTADO** | `AdminDashboard`, views admin |
|RF-017| Cálculo de cobertura geográfica das empresas | 🚧 **PARCIAL** | Placeholders visuais, falta cálculo dinâmico |
|RF-018| Agendamento de visita técnica | 🚧 **FALTA** | Não implementado |
|RF-019| Comparação detalhada de orçamentos | 🚧 **PARCIAL** | Comparação existe, falta visualização lado a lado |
|RF-020| Propostas detalhadas enviadas pelas empresas | ✅ **IMPLEMENTADO** | `QuoteController.cs`, `SendCompanyResponse`, `Proposal` |


### Requisitos Não Funcionais Implementados

|ID     | Descrição do Requisito | Status | Implementação |
|-------|------------------------|--------|---------------|
|RNF-001| A aplicação deve ser responsiva | ✅ **IMPLEMENTADO** | Bootstrap 5.3, CSS responsivo |
|RNF-004| Garantir privacidade dos dados (LGPD) | ✅ **IMPLEMENTADO** | ASP.NET Core Identity, criptografia de senhas |
|RNF-005| Autenticação segura | ✅ **IMPLEMENTADO** | ASP.NET Core Identity, políticas de senha |
|RNF-007| Interface intuitiva e acessível | ✅ **IMPLEMENTADO** | Bootstrap, UX responsivo |
|RNF-009| Mensagens de erro claras | ✅ **IMPLEMENTADO** | ModelState, TempData, mensagens localizadas |
|RNF-010| Armazenamento seguro de dados | ✅ **IMPLEMENTADO** | Entity Framework, SQL Server |
|RNF-013| Mensagens anti-enumeração no login | ✅ **IMPLEMENTADO** | Mensagens genéricas de erro |
|RNF-014| Política de senha robusta | ✅ **IMPLEMENTADO** | Mínimo 8 caracteres, maiúscula, minúscula, dígito, especial |
|RNF-015| Mensagens padronizadas (pt-BR) | ✅ **IMPLEMENTADO** | Localização em português brasileiro |
|RNF-016| Logs estruturados | ✅ **IMPLEMENTADO** | ILogger, logs de autenticação |

## Detalhamento das Funcionalidades

### 1. Sistema de Autenticação (`AuthController.cs`)
**Funcionalidades:**
- **Login de usuários** com validação de credenciais
- **Registro diferenciado** para consumidores (CPF) e empresas (CNPJ) 
- **Logout seguro** com limpeza de sessão
- **Prevenção de ataques** (anti-enumeração, força bruta)
- **Validação robusta** de dados de entrada

**Artefatos:**
- `Controllers/AuthController.cs` - Lógica de autenticação
- `Models/LoginViewModel.cs` - Modelo para login
- `Models/RegisterViewModel.cs` - Modelo para registro
- `Views/Auth/Login.cshtml` - Interface de login
- `Views/Auth/Register.cshtml` - Interface de registro

### 2. Gestão de Perfil (`ProfileController.cs`)
**Funcionalidades:**
- **Visualização** de dados do perfil
- **Edição** de informações pessoais
- **Upload de foto** de perfil
- **Validação** de dados atualizados
- **Controle de acesso** (usuário autenticado)

**Artefatos:**
- `Controllers/ProfileController.cs` - Lógica de gestão de perfil
- `ViewModels/UserProfileViewModel.cs` - Modelo de visualização
- `Views/Profile/Index.cshtml` - Interface de perfil

### 3. Modelo de Dados (`ApplicationUser.cs`)
**Funcionalidades:**
- **Extensão** do IdentityUser padrão
- **Suporte** a consumidores e empresas
- **Campos específicos** para cada tipo de usuário
- **Validação** de dados com Data Annotations

**Características Implementadas:**
- Cadastro de consumidores com CPF
- Cadastro completo de empresas (CNPJ, razão social, nome fantasia, etc.)
- Sistema de roles (Client, Company, Administrator)
- Campos de perfil (foto, localização, telefone)

## Estrutura do Banco de Dados

O sistema utiliza **Entity Framework Code First** com as seguintes migrations implementadas:
- `InitialCreate` - Estrutura inicial do Identity
- `AddCNPJField` - Adição do campo CNPJ
- `UpdateUserTypeToEnglish` - Padronização dos tipos de usuário
- `AddUserProfileFields` - Campos de perfil do usuário
- `AddCompanyRegistrationFields` - Campos específicos para empresas

## Segurança Implementada

### Políticas de Senha (RNF-014)
- Mínimo 8 caracteres
- Pelo menos 1 letra maiúscula
- Pelo menos 1 letra minúscula  
- Pelo menos 1 dígito
- Pelo menos 1 caractere especial

### Medidas Anti-Enumeração (RNF-013)
- Mensagens genéricas para login inválido
- Tempo de resposta similar para credenciais válidas/inválidas
- Não exposição de existência de contas

### Logs de Segurança (RNF-016)
- Registros de login (sucesso/falha)
- Logs de criação de conta
- Monitoramento de tentativas de acesso
- Não armazenamento de dados sensíveis nos logs

## Próximas Implementações

### Requisitos Funcionais Pendentes
- **RF-010**: Integração real com mapas/geolocalização (API Google Maps ou similar)
- **RF-017**: Cálculo dinâmico de cobertura geográfica das empresas
- **RF-018**: Agendamento de visita técnica pelo usuário
- **RF-019**: Visualização detalhada lado a lado de orçamentos

Todos os demais requisitos funcionais já estão implementados ou parcialmente implementados.

# Instruções de Acesso

## Ambiente de Desenvolvimento

**Para executar a aplicação localmente:**

1. **Pré-requisitos:**
   - Visual Studio 2022 ou superior
   - .NET 8.0 SDK
   - SQL Server Express ou SQL Server

2. **Configuração:**
   ```bash
   # Clone o repositório
   git clone https://github.com/ICEI-PUC-Minas-PMV-ADS/pmv-ads-2025-2-e2-proj-int-t8-grupo-1-solar-energy.git
   
   # Navegue para o diretório do projeto
   cd src/SolarEnergy
   
   # Restaure as dependências
   dotnet restore
   
   # Execute as migrations
   dotnet ef database update
   
   # Execute a aplicação
   dotnet run
   ```

3. **Acesso:** `https://localhost:7047` ou `http://localhost:5073`

**Acesso rápido (produção)**
* URL da aplicação: [Solar Energy](https://solarenergy-byf3gvdndpetcec0.canadacentral-01.azurewebsites.net)
* Status: online
* Ambiente: homologação

## Usuários de Teste

**Consumidor:**
- Login: teste@teste.com
- Senha: Solar2610

## Funcionalidades Testáveis

### ✅ Funcionalidades Disponíveis:
- Registro de consumidores (com CPF)
- Registro de empresas (com CNPJ e dados completos)
- Login e logout
- Edição de perfil
- Upload de foto de perfil
- Validações de segurança
- Busca e listagem de empresas
- Visualização de perfil detalhado de empresas
- Sistema de avaliações e comentários
- Solicitação e comparação de orçamentos
- Painéis de usuário, empresa e admin
- Gestão de leads e planos CPL
- Relatórios mensais e exportação
- Simulador de economia
- Chat integrado a orçamentos

### 🚧 Em Desenvolvimento:
- Integração real com mapas/geolocalização
- Cálculo dinâmico de cobertura geográfica
- Agendamento de visita técnica
- Visualização detalhada lado a lado de orçamentos
