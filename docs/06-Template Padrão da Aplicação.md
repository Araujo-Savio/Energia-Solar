# Template Padrão da Aplicação

O site é desenvolvido em ASP.NET Core MVC (C#) com Razor Views, usando HTML, CSS e JavaScript no front-end, EF Core para acesso ao banco de dados, Identity para autenticação/autorização e Bootstrap/jQuery para layout responsivo e interações.

# Iconografia — Solar Energy

* Início / Dashboard – fa-solid fa-gauge-high
* Busca de Empresas – fa-solid fa-building (alt: fa-magnifying-glass-location)
* Comparador – fa-solid fa-scale-balanced
* Calculadora – fa-solid fa-calculator
* Simulação – fa-solid fa-sliders (alt: fa-wand-magic-sparkles)
* Propostas (Quotes) – fa-solid fa-file-signature
* Leads – fa-solid fa-bullseye (alt: fa-user-plus)
* Clientes – fa-solid fa-users
* Avaliações / Moderação – fa-solid fa-star-half-stroke (alt: fa-clipboard-check)
* Relatórios – fa-solid fa-chart-line
* Painel da Empresa (CompanyPanel) – fa-solid fa-briefcase (alt: fa-solar-panel, fa-sun)
* Perfil do Usuário – fa-solid fa-user
* Configurações – fa-solid fa-gear
* Admin Dashboard – fa-solid fa-sitemap
* Contato/FAQ – fa-solid fa-circle-question
* Adicionar fa-solid fa-plus
* Editar fa-solid fa-pen
* Excluir fa-solid fa-trash
* Visualizar fa-solid fa-eye
* Baixar / Exportar fa-solid fa-download / fa-solid fa-file-export
* Aprovar / Rejeitar fa-solid fa-circle-check / fa-solid fa-circle-xmark
* Pesquisar / Filtrar / Ordenar fa-solid fa-magnifying-glass / fa-solid fa-filter / fa-solid fa-arrow-up-wide-short
* Enviar fa-solid fa-paper-plane
* Upload fa-solid fa-upload
* Localização / CEP fa-solid fa-location-dot
* Novo – fa-regular fa-circle + .chip chip--new
* Qualificado – fa-solid fa-badge-check (se não houver, usar fa-circle-check) + .chip chip--qualified
* Enviado – fa-solid fa-paper-plane + .chip chip--sent
* Ganho – fa-solid fa-circle-check + .chip chip--won
* Perdido – fa-solid fa-circle-xmark + .chip chip--lost
* Em revisão – fa-solid fa-hourglass-half
* Ativo – fa-solid fa-toggle-on
* Encerrado – fa-solid fa-lock

# TELAS

# LANDING PAGE
* Cores: #2563EB, #EA6D13, #FFCC00, #4DA3FF, #121212, #1e1e1e, #2a2a2a
* Backgrounds: #0d1b2a, #1b263b, gradiente hero, sobreposições com rgba(37,99,235,.1)
* Font-family: Segoe UI, Arial, sans-serif
* Font-size: 0.9rem, 1rem, 1.2rem, 1.5rem, 1.8rem, 2.2rem, 2.5rem
   * Seções
*	Hero: proposta de valor “Economize com Energia Solar” + CTA “Simular economia”.
*	Como funciona: aluguel vs instalação (cards comparativos).
*	Comparador: tabela resumida (CPL/CPV/CPA quando aplicável).
*	Calculadora: entrada consumo (kWh), tarifa média, resultado estimado.
*	Empresas parceiras: grid com selo verificado.
*	Depoimentos & Perguntas: prova social + FAQ.
*	CTA final: “Solicitar orçamento”.
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/57fa13e4-cd6a-4f28-81c7-2f83b88dfe9a" />
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/4b66082b-b39f-4711-b517-663b7797f013" />
<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/28d06a06-800e-408e-bf73-e1b2f386c779" />

# LOGIN 

* Cores: rgba(0,0,0,.3), #f2f2f29a (painel), #e0e0e0 (borda), #333 (texto), #FF0000 (erro)
* Font-family: Arial, sans-serif
* Font-size: 12px, 14px

  <img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/02a33e44-d48c-4a92-a94a-90ae5c2aea21" />


# CADASTRO

* Cores: rgba(0,0,0,.3), #f2f2f29a (painel), #e0e0e0 (borda), #333 (texto), #FF0000 (erro)
* Font-family: Arial, sans-serif
* Font-size: 12px, 14px

  <img width="1916" height="1079" alt="image" src="https://github.com/user-attachments/assets/72a0cb0a-7d16-463a-b9c7-0faf08728d13" />
  <img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/9c7ca2c1-dbf6-47ed-b8f6-4f384df0db8a" />



# DASHBOARD

* Cores: #F5F5F5 (fundo), #FFFFFF (cards), #121212 (texto), #666 (texto secundário), #2563EB (primária), #EA6D13 (secundária), #FFCC00 (aviso), #28A745 (sucesso), #E5E7EB (bordas), topo escuro #0d1b2a → #1b263b
* Font-family: Poppins, sans-serif
* Font-size: 14px, 16px, 18px, 24px
  <img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/eb94cabb-2d27-40df-88de-59bb09d59770" />


# LEADS

* Cores: #F5F5F5 (fundo), #FFFFFF (tabela/cards), #121212 (texto), #666 (secundário), #2563EB (ações/realces), chips: #4DA3FF (novo/info), #28A745 (convertido/sucesso), #EA6D13 (proposta enviada), #E5E7EB (borda)
* Font-family: Poppins, sans-serif
* Font-size: 14px, 16px, 18px, 24px

  <img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/174bc24f-fdb6-4ba1-8cdf-51b5c4d95c5d" />


# ADMIN 

* Cores: topo #0d1b2a → #1b263b, #F5F5F5 (fundo), #FFFFFF (cards), #121212/#666 (textos), indicadores: #28A745 (ok), #F59E0B (atenção), #FF0000 (crítico), acentos #EA6D13 / #2563EB
* Font-family: Poppins, sans-serif
* Font-size: 14px, 16px, 18px, 24px, 32px (títulos)

  <img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/958bcecb-8fd3-4783-a8c8-10096a6f5491" />


# SIMULADOR DE ECONOMIA

* Cores: #F5F5F5 (fundo), #FFFFFF (painéis), #121212 (texto), #666 (secundário), #2563EB (botões/inputs), gráfico e destaques em #EA6D13, #FFCC00 (marcadores/realces)
* Font-family: Poppins, sans-serif
* Font-size: 14px, 16px, 18px, 24px

<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/59d9d324-2494-4b81-ad37-19315ea06a04" />

# ORÇAMENTOS

* Cores: #F5F5F5 (fundo), #FFFFFF (tabela), #121212 (texto), #666 (secundário), #2563EB (links/ações), status: #FFCC00 (aguardando), #28A745 (aprovado), #666/cinza (revisão solicitada), #E5E7EB (borda)
* Font-family: Poppins, sans-serif
* Font-size: 14px, 16px, 18px, 24px

<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/96834e48-3fd2-48d0-94be-2e063aa2774b" />

# AVALIAÇÕES

* Cores: #F5F5F5 (fundo), #FFFFFF (cards/tabela), #121212 (texto), #666 (secundário), estrelas em #EA6D13, controles/links em #2563EB, #E5E7EB (borda)
* Font-family: Poppins, sans-serif
* Font-size: 14px, 16px, 18px, 24px

<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/9274cabf-db06-4308-83ef-6bfbcaaecc6b" />

# PAINEL DE EMPRESA 

* Cores: #F5F5F5 (fundo), #FFFFFF (widgets), #121212 e #666 (textos), gráficos com azul #2563EB e laranja #EA6D13, realces #FFCC00, #28A745, bordas #E5E7EB
* Font-family: Poppins, sans-serif
* Font-size: 14px, 16px, 18px, 24px

  <img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/5ae8c606-9baf-4a19-be5f-2aade4da1a53" />


# SOBRE

* Cores: #F5F5F5 (fundo), #FFFFFF (card de missão/visão), #121212 (texto), #666 (secundário), ícones e ênfases em #EA6D13, topo #0d1b2a → #1b263b
* Font-family: Poppins, sans-serif
* Font-size: 14px, 16px, 18px, 24px

<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/27d74a19-5eed-4c2d-b2a0-738eeb988324" />

# CONTATO

* Cores: #F5F5F5 (fundo), #FFFFFF (form/cards), #121212 (texto), #666 (secundário), botões/ícones em #EA6D13 e #2563EB, #E5E7EB (bordas), #FF0000 (erros de validação)
* Font-family: Poppins, sans-serif
* Font-size: 14px, 16px, 18px, 24px

  <img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/1df99e30-64d6-4929-ba96-1550426f89f8" />

