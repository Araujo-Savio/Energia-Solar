# Especificações do Projeto

## Personas

<img width="1920" height="1080" alt="1" src="https://github.com/user-attachments/assets/8ed2d8ee-3bb3-4501-a4d7-b90909e5ff3c" />

___

<img width="1920" height="1080" alt="2" src="https://github.com/user-attachments/assets/c85b204a-f54b-491d-afcd-5a6ddd05147e" /> 

___

<img width="1920" height="1080" alt="3" src="https://github.com/user-attachments/assets/2c5c248f-f720-4cbb-a243-4e02886bb100" />

___

<img width="1920" height="1080" alt="4" src="https://github.com/user-attachments/assets/51dc5b7d-89ac-4158-a7b4-941787ab5ddc" />

___

<img width="1920" height="1080" alt="5" src="https://github.com/user-attachments/assets/5f37989c-8e08-45f7-8d5c-b52138e9037e" />

---

## Histórias de Usuários

Com base na análise das personas forma identificadas as seguintes histórias de usuários:

|EU COMO... `PERSONA`| QUERO/PRECISO ... `FUNCIONALIDADE` |PARA ... `MOTIVO/VALOR`                 |
|--------------------|------------------------------------|----------------------------------------|
| Mariana | Acessar informações claras sobre aluguel de energia solar| Entender os benefícios e escolher a opção que caiba no meu orçamento|
| Mariana | Encontrar empresas confiáveis de forma rápida |Contratar o serviço sem correr riscos ou perder tempo|
| Carlos | Visualizar de forma clara e comparativa as diferenças entre aluguel e instalação de energia solar| Entender qual opção traz o melhor equilíbrio entre economia e impacto ambiental
| Carlos | Encontrar empresas que atuem na minha região e ofereçam energia limpa de forma transparente | Poder contratar um serviço sustentável com confiança e praticidade
| João | Visualizar de forma clara a economia e o tempo de retorno de uma solução de energia solar | Decidir se vale mais a pena alugar a energia ou parcelar a instalação
| João | Me conectar rapidamente apenas com empresas confiáveis | Contratar energia solar sem gastar tempo ou correr riscos de problemas futuros |
| Lucas | Receber apenas leads qualificados e realmente interessados | focar meu tempo e energia em oportunidades que têm chance real de fechamento |
| Lucas| Acessar um painel que mostre em tempo real os resultados do marketing | Acompanhar leads, custos por contato e taxas de conversão, otimizando meus investimentos |
| Helena | Receber apenas leads qualificados e realmente interessados | Poder focar meu tempo e energia em oportunidades que têm chance real de fechamento |
| Helena | Assinar pacotes de leads com possibilidade de desconto e acompanhar resultados | Otimizar investimentos e planejar a expansão da base de clientes para outras regiões de forma previsível

## Requisitos

As tabelas que se seguem apresentam os requisitos funcionais e não funcionais que detalham o escopo do projeto.

### Requisitos Funcionais

|ID      | Descrição do Requisito                                                                                  | Prioridade |
|--------|--------------------------------------------------------------------------------------------------------|-----------|
|RF-001  | A aplicação deve permitir que o usuário pesquise empresas de energia solar por localização.            | ALTA      |
|RF-002  | A aplicação deve exibir o perfil detalhado das empresas.   | ALTA      |
|RF-003  | A aplicação deve exibir as avaliações e área de atendimento das empresas | MÉDIA      |
|RF-004  | A aplicação deve permitir a comparação de orçamentos recebidos de diferentes empresas.                 | ALTA      |
|RF-005  | A aplicação deve permitir que o usuário visualize, avalie e comente sobre as empresas após a contratação.         | MÉDIA     |
|RF-006  | A aplicação deve permitir que empresas cadastrem e atualizem seu perfil e área de atendimento.         | ALTA      |
|RF-007  | A aplicação deve permitir que empresas respondam solicitações de orçamento e enviem propostas.         | ALTA      |
|RF-008  | A aplicação deve permitir que o usuário visualize o tempo estimado de retorno do investimento.         | MÉDIA     |
|RF-009  | A aplicação deve permitir que administradores moderem avaliações e verifiquem documentos das empresas. | MÉDIA     |
|RF-010  | A aplicação deve integrar mapas/geolocalização para exibir cobertura das empresas e facilitar buscas.  | ALTA      |
|RF-011  | A aplicação deve permitir a recuperação de senha para usuários e empresas.                             | MÉDIA     |
|RF-012  | A aplicação deve permitir que o usuário visualize o histórico de solicitações de orçamento realizadas. | MÉDIA     |
|RF-013  | A aplicação deve permitir que usuários e empresas recebam notificações sobre o status das solicitações.| MÉDIA     |
| RF-014 | A aplicação deve permitir a geração de relatório mensal da empresa, exibindo a quantidade de leads recebidos, leads convertidos e taxa de conversão. | ALTA |
| RF-015 | A aplicação deve permitir que o administrador gerencie **leads (CPL)**, incluindo registro, atribuição e acompanhamento. | ALTA |
| RF-016 | A aplicação deve permitir que o administrador **gerencie planos CPL e usuários**, podendo criar, editar, suspender e excluir. | ALTA |
| RF-017 | A aplicação deve permitir **calcular a cobertura geográfica** de empresas, integrando dados do sistema de mapas. | ALTA |
| RF-018 | A aplicação deve permitir que o usuário **agende uma visita técnica** com a empresa selecionada. | MÉDIA |
| RF-019 | A aplicação deve permitir a **comparação detalhada de orçamentos** recebidos de diferentes empresas. | ALTA |
| RF-020 | A aplicação deve permitir que empresas **enviem propostas detalhadas** em resposta às solicitações de orçamento. | ALTA |

---

### Requisitos Não Funcionais

|ID       | Descrição do Requisito                                                                 | Prioridade |
|---------|---------------------------------------------------------------------------------------|-----------|
|RNF-001  | A aplicação deve ser responsiva, adaptando-se a diferentes tamanhos de tela e dispositivos. | ALTA     |
|RNF-002  | A aplicação deve processar requisições do usuário em no máximo 3 segundos.             | BAIXA     |
|RNF-003  | A aplicação deve estar disponível 99% do tempo, exceto em períodos programados de manutenção. | ALTA      |
|RNF-004  | A aplicação deve garantir a privacidade dos dados dos usuários, seguindo a LGPD.       | ALTA      |
|RNF-005  | A aplicação deve utilizar autenticação segura para acesso de empresas e administradores.| ALTA      |
|RNF-006  | A aplicação deve ser compatível com os principais navegadores modernos (Chrome, Firefox, Edge, Safari). | MÉDIA     |
|RNF-007  | A interface deve ser intuitiva, com navegação clara e acessível para todos os perfis de usuário. | ALTA      |
|RNF-008  | O sistema deve permitir fácil escalabilidade para suportar aumento de usuários e empresas cadastradas. | MÉDIA     |
|RNF-009  | A aplicação deve apresentar mensagens de erro claras e orientativas ao usuário.         | MÉDIA     |
|RNF-010  | O sistema deve armazenar dados de forma segura, utilizando criptografia para informações sensíveis. | ALTA      |
|RNF-011  | O tempo de carregamento das páginas não deve exceder 2 segundos em conexões banda larga.| MÉDIA     |
|RNF-012  | A aplicação deve possuir testes automatizados para as principais funcionalidades.      | BAIXA     |
|RNF-013  | Mensagens de erro no Login devem ser genéricas (anti-enumeração) e com tempo de resposta similar para credenciais válidas/ inválidas.      | ALTA     |
|RNF-014  | Política de senha: mínimo 8 caracteres, contendo maiúscula, minúscula, dígito e caractere especial.    | ALTA     |
|RNF-015  | Mensagens e validações devem ser padronizadas e localizadas (pt-BR), sem expor detalhes sensíveis.  | MEDIA     |
|RNF-016  | Registrar logs estruturados de login (sucesso/falha), criação de conta, bloqueios e validações; métricas e correlação; sem dados sensíveis.  | MEDIA     |


## Restrições

O projeto está restrito pelos itens apresentados na tabela a seguir.

|ID| Restrição                                                                                   |
|--|--------------------------------------------------------------------------------------------|
|01| O projeto deverá ser entregue até o final do semestre                                      |
|02| O desenvolvimento deve utilizar o padrão arquitetural MVC (Model-View-Controller)          |
|03| As tecnologias permitidas para o desenvolvimento são: HTML, CSS, JavaScript, C#, .NET, Visual Studio, Bootstrap e SQL Server |
|04| Não é permitido o uso de frameworks ou linguagens de backend diferentes de C#/.NET         |
|05| O banco de dados deve ser obrigatoriamente o SQL Server                                    |
|06| O frontend deve ser desenvolvido utilizando HTML, CSS, JavaScript e Bootstrap              |
|07| O projeto deve ser desenvolvido e gerenciado utilizando o Visual Studio                    |


## Diagrama de Casos de Uso


<img width="993" height="710" alt="diagrama" src="https://github.com/user-attachments/assets/4f28204b-0d97-4dbc-9ef1-94dee940b38e" />



