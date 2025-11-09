# Solar Energy Platform - Código Fonte

## 📋 Sobre o Projeto

Plataforma web desenvolvida em ASP.NET Core MVC para conectar consumidores e empresas do setor de energia solar. O sistema facilita o acesso a informações, serviços e orçamentos de energia solar, promovendo transparência e sustentabilidade.

## 🛠️ Tecnologias Utilizadas

### Backend
- **ASP.NET Core MVC 8.0** - Framework web
- **C#** - Linguagem de programação
- **Entity Framework Core** - ORM para acesso ao banco
- **ASP.NET Identity** - Sistema de autenticação e autorização
- **Azure SQL Database** - Banco de dados na nuvem

### Frontend
- **HTML5** - Estruturação das páginas
- **CSS3** - Estilização e layout responsivo
- **Bootstrap 5** - Framework CSS responsivo
- **JavaScript (ES6+)** - Interatividade e validações
- **Font Awesome** - Biblioteca de ícones

## 📁 Estrutura do Projeto

```
SolarEnergy/
├── Controllers/
│   ├── AuthController.cs          # Autenticação (Login/Registro)
│   └── HomeController.cs          # Homepage e Dashboard
├── Data/
│   └── ApplicationDbContext.cs    # Contexto do Entity Framework
├── Migrations/                    # Migrations do banco de dados
│   ├── 20251020231355_InitialCreate.cs
│   └── 20251020235842_UpdateUserTypeToEnglish.cs
├── Models/
│   ├── ApplicationUser.cs         # Modelo de usuário extendido
│   ├── LoginViewModel.cs          # ViewModel para login
│   ├── RegisterViewModel.cs       # ViewModel para registro
│   └── ErrorViewModel.cs          # ViewModel para erros
├── Views/
│   ├── Auth/
│   │   ├── Login.cshtml           # Tela de login
│   │   └── Register.cshtml        # Tela de registro
│   ├── Home/
│   │   ├── Index.cshtml           # Homepage
│   │   ├── Dashboard.cshtml       # Dashboard do usuário
│   │   ├── About.cshtml           # Sobre nós
│   │   └── Contact.cshtml         # Contato
│   └── Shared/
│       ├── _Layout.cshtml         # Layout principal
│       └── Error.cshtml           # Página de erro
├── wwwroot/
│   ├── css/
│   │   └── site.css              # Estilos personalizados
│   ├── js/
│   │   └── register.js           # JavaScript do registro
│   └── lib/                      # Bibliotecas (Bootstrap, jQuery)
├── appsettings.json              # Configurações (sem credenciais)
├── appsettings.Example.json      # Exemplo com credenciais
└── Program.cs                    # Configuração da aplicação
```

## 🚀 Instalação e Configuração

### Pré-requisitos
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)
- Acesso ao Azure SQL Database

### 1. Configuração do Banco de Dados

1. **Copie o arquivo de configuração:**
   ```bash
   cp appsettings.Example.json appsettings.Development.json
   ```

2. **Edite `appsettings.Development.json` com suas credenciais:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=tcp:SEU_SERVIDOR.database.windows.net,1433;Initial Catalog=SEU_BANCO;User ID=SEU_USUARIO;Password=SUA_SENHA;..."
     }
   }
   ```

### 2. Executar Migrations

```bash
dotnet ef database update
```

### 3. Executar o Projeto

```bash
dotnet run
```

**Acesso:** https://localhost:5001

## 🎨 Design System

### Paleta de Cores
- **Azul Principal:** `#1e3a8a` (Baseado na logo Solar Energy)
- **Laranja Destaque:** `#ff6b35` (Cor de destaque da marca)
- **Gradiente:** `linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%)`

### Componentes Implementados
- Cards com sombra suave
- Botões com hover effects
- Layout responsivo split-screen
- Formulários com validação em tempo real
- Navegação moderna com dropdown

## 👥 Tipos de Usuário

1. **Client (Consumidor)**
   - Busca empresas de energia solar
   - Solicita orçamentos
   - Avalia empresas

2. **Company (Empresa)**
   - Oferece serviços de energia solar
   - Recebe leads qualificados
   - Gerencia propostas

3. **Administrator (Administrador)**
   - Modera a plataforma
   - Gerencia usuários e empresas
   - Visualiza relatórios

## ✅ Funcionalidades Implementadas

### Sistema de Autenticação
- [x] Login com validação
- [x] Registro de usuários (Consumidor/Empresa)
- [x] Validação de CPF/CNPJ
- [x] Validação de força da senha
- [x] Sistema de roles (Client, Company, Administrator)

### Interface do Usuário
- [x] Homepage responsiva
- [x] Design baseado na identidade visual Solar Energy
- [x] Formulários com validação em tempo real
- [x] Máscaras para CPF, CNPJ e telefone
- [x] Dashboard personalizado por tipo de usuário

### Infraestrutura
- [x] Integração com Azure SQL Database
- [x] Entity Framework com migrations
- [x] Configuração segura (credenciais protegidas)
- [x] Logging configurado

## 🔒 Segurança

### Implementações de Segurança
- Senhas criptografadas com ASP.NET Identity
- Proteção contra CSRF com tokens
- Validação de entrada em client e server-side
- Connection strings protegidas
- Lockout após tentativas de login falhadas

### Configuração Segura
- Credenciais nunca commitadas no Git
- Arquivo `.gitignore` configurado para .NET
- Exemplo de configuração em `appsettings.Example.json`

## 📱 Responsividade

O projeto é totalmente responsivo e otimizado para:
- **Desktop** (1920px+)
- **Tablet** (768px - 1919px)
- **Mobile** (até 767px)

## 🚧 Próximas Funcionalidades

### Planejadas para as próximas versões:
- [ ] Sistema de busca de empresas por localização
- [ ] Simulador de economia energética
- [ ] Sistema completo de orçamentos e propostas
- [ ] Avaliações e comentários
- [ ] Dashboard de empresa com métricas
- [ ] Painel administrativo completo
- [ ] Integração com APIs de mapas (Google Maps)
- [ ] Sistema de notificações
- [ ] Relatórios e analytics
- [ ] Sistema de leads CPL (Custo por Lead)

## 🧪 Como Testar

### Testar Registro
1. Acesse `/Auth/Register`
2. Selecione tipo: Consumidor ou Empresa
3. Preencha os dados (CPF para consumidor, CNPJ para empresa)
4. Teste validações em tempo real

### Testar Login
1. Acesse `/Auth/Login`
2. Use credenciais de um usuário registrado
3. Verifique redirecionamento para dashboard

### Testar Responsividade
1. Redimensione a janela do navegador
2. Teste em diferentes dispositivos
3. Verifique se o layout se adapta corretamente

## 📊 Banco de Dados

### Tabelas Principais
- **AspNetUsers** - Usuários do sistema (estendida com ApplicationUser)
- **AspNetRoles** - Roles (Client, Company, Administrator)
- **AspNetUserRoles** - Relacionamento usuário-role

### Campos Customizados em ApplicationUser
- `FullName` - Nome completo
- `CPF` - CPF (para consumidores)
- `CNPJ` - CNPJ (para empresas)
- `Phone` - Telefone
- `UserType` - Tipo do usuário (enum)
- `CreatedAt` - Data de criação
- `IsActive` - Status ativo/inativo

## 🔧 Configurações Avançadas

### Configuração do Identity
```csharp
options.Password.RequiredLength = 8;
options.Password.RequireDigit = true;
options.Password.RequireLowercase = true;
options.Lockout.MaxFailedAccessAttempts = 5;
options.User.RequireUniqueEmail = true;
```

### Configuração do Entity Framework
```csharp
options.UseSqlServer(connectionString);
context.Database.EnsureCreated();
```

## Histórico de Versões

### [0.1.0] - 20/10/2024
#### Adicionado
- Sistema de autenticação completo com ASP.NET Identity
- Telas de login e registro responsivas
- Homepage com informações sobre energia solar
- Dashboard básico personalizado por tipo de usuário
- Integração com Azure SQL Database
- Design system baseado na identidade visual Solar Energy
- Validações em tempo real com JavaScript
- Estrutura MVC organizada e documentada
- Sistema de roles (Client, Company, Administrator)
- Configuração segura para produção

#### Tecnologias Implementadas
- ASP.NET Core MVC 8.0
- Entity Framework Core
- Bootstrap 5
- Font Awesome
- Azure SQL Database
- JavaScript ES6+

#### Segurança
- Proteção de credenciais com .gitignore
- Configuração de exemplo para desenvolvimento
- Criptografia de senhas com Identity
- Proteção CSRF