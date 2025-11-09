# Plano de Testes de Software

# Casos de Teste

| ID do teste | Requisito associado | Objetivo do teste | Passos | Critérios de êxito |
|-------------|---------------------|------------------|--------|---------------------|
| CT-001 | RNF-016 | Acessar URL protegida sem login (proteção de rotas) | 1) Abrir uma URL de área logada estando deslogado 2) Pressionar Enter | Redireciona para Login; após autenticar retorna à URL; nada sensível aparece deslogado |
| CT-002 | RNF-016 | Cadastro de CONSUMIDOR com dados válidos | 1) Criar Conta > CONSUMIDOR 2) Preencher obrigatórios 3) Aceitar termos 4) 'Criar Conta' | Conta criada; mensagem/redirect; registro no BD com status inicial correto |
| CT-003 | RNF-015 e 016| Cadastro com e-mail já existente | 1) Criar Conta > CONSUMIDOR 2) Usar e-mail já cadastrado | Erro 'e-mail já em uso'; |
| CT-004 | RNF-016 | Cadastro de EMPRESA com dados válidos | 1) Criar Conta > EMPRESA 2) Preencher Responsável, Razão Social, CNPJ, IE, contatos, senha | Empresa criada;  |
| CT-005 | RNF-015 E 016 | Validação de e-mail e CNPJ incorretos (empresa) | 1) Criar Conta > EMPRESA 2) Informar e-mail fora do padrão e CNPJ inválido | Envio bloqueado; mensagens por campo; nada persistido |
| CT-006 | RNF-013 | Mensagens de validação/erro no Login | 1) Digitar e-mail válido e senha incorreta 2) Clicar 'Entrar' | Não autentica; mensagem genérica; permanece na tela de login; tentativa registrada |
| CT-007 | RNF-004 | Política de senha — rejeitar senha fraca | 1) Em Criar Conta informar senha fora da política | Bloqueia envio; exibe critérios; nenhum usuário/empresa criado |
| CT-008 | RNF-016 | Login com credenciais válidas | 1) Acessar Login 2) Informar e-mail e senha válidos | Autentica; redireciona ao dashboard; sessão/cookie criado |
