# Registro de Testes de Usabilidade

O registro de testes de usabilidade é um documento ou planilha onde são coletadas e organizadas as informações sobre a experiência dos usuários ao interagir com um sistema. Ele inclui dados como tempo de execução de tarefas, taxa de sucesso, dificuldades encontradas, erros cometidos e _feedback_ dos usuários. Esse registro permite identificar padrões de uso, obstáculos/dificuldades encontrados na interface e oportunidades de melhoria, fornecendo _insights_ quantitativos e qualitativos para otimizar a experiência do usuário. Além disso, serve como base para análises, correções e futuras iterações do sistema, garantindo que ele atenda às necessidades do público-alvo de forma eficiente.

## Perfil dos usuários que participaram do teste
- Usuário 1: 45 anos, ensino fundamental incompleto, conhecimento básico em tecnologia
- Usuário 2: 18 anos, ensino superior incompleto, conhecimento avançado em tecnologia
- Usuário 3: 70 anos, ensino fundamental incompleto, conhecimento básico em tecnologia
- Usuário 4: 25 anos, ensino superior completo, conhecimento avançado em tecnologia
- Usuário 5: 28 anos, ensino superior completo, conhecimento intermediário em tecnologia


## Registro dos testes de usabilidade

Os cenários abaixo foram definidos com base nas principais jornadas do sistema Solar Energy.

**Cenário 1**: Cadastro de novo usuário (consumidor ou empresa)
| **Usuário**   | **Tempo Total (seg)** | **Quantidade de cliques** | **Tarefa foi concluída?** | **Erros Cometidos** | **Feedback do Usuário** |
|-------------|--------------------|--------------------------|--------------------------|---------------------|-------------------------|
| Usuário 1   | 120                | 18                       | Sim                      | 1                   | "Tive dificuldade para entender o campo CNPJ, mas consegui concluir." |
| Usuário 2   | 60                 | 15                       | Sim                      | 0                   | "Cadastro rápido, interface clara." |
| Usuário 3   | 180                | 22                       | Não                      | 2                   | "Não entendi a diferença entre consumidor e empresa." |
| Usuário 4   | 70                 | 16                       | Sim                      | 0                   | "Fácil de usar, gostei do visual." |
| Usuário 5   | 90                 | 17                       | Sim                      | 1                   | "Faltou explicação sobre senha forte." |

**Cenário 2**: Login no sistema
| **Usuário**   | **Tempo Total (seg)** | **Quantidade de cliques** | **Tarefa foi concluída?** | **Erros Cometidos** | **Feedback do Usuário** |
|-------------|--------------------|--------------------------|--------------------------|---------------------|-------------------------|
| Usuário 1   | 40                 | 6                        | Sim                      | 0                   | "Login simples, mas demorei para achar o botão." |
| Usuário 2   | 20                 | 4                        | Sim                      | 0                   | "Muito rápido." |
| Usuário 3   | 60                 | 7                        | Não                      | 2                   | "Errei a senha e não entendi a mensagem de erro." |
| Usuário 4   | 25                 | 4                        | Sim                      | 0                   | "Tranquilo, sem dificuldades." |
| Usuário 5   | 30                 | 5                        | Sim                      | 0                   | "Gostei do layout." |

**Cenário 3**: Solicitar orçamento de energia solar
| **Usuário**   | **Tempo Total (seg)** | **Quantidade de cliques** | **Tarefa foi concluída?** | **Erros Cometidos** | **Feedback do Usuário** |
|-------------|--------------------|--------------------------|--------------------------|---------------------|-------------------------|
| Usuário 1   | 150                | 20                       | Não                      | 2                   | "Não achei fácil o botão de solicitar orçamento." |
| Usuário 2   | 60                 | 10                       | Sim                      | 0                   | "Processo intuitivo." |
| Usuário 3   | 200                | 25                       | Não                      | 3                   | "Fiquei confuso com as etapas." |
| Usuário 4   | 80                 | 12                       | Sim                      | 0                   | "Gostei do passo a passo." |
| Usuário 5   | 90                 | 13                       | Sim                      | 1                   | "Faltou confirmação visual ao enviar." |

**Cenário 4**: Avaliar uma empresa após contratação
| **Usuário**   | **Tempo Total (seg)** | **Quantidade de cliques** | **Tarefa foi concluída?** | **Erros Cometidos** | **Feedback do Usuário** |
|-------------|--------------------|--------------------------|--------------------------|---------------------|-------------------------|
| Usuário 1   | 60                 | 8                        | Sim                      | 0                   | "Fácil de avaliar, gostei das estrelas." |
| Usuário 2   | 30                 | 5                        | Sim                      | 0                   | "Muito rápido." |
| Usuário 3   | 90                 | 10                       | Não                      | 1                   | "Não entendi se a avaliação foi enviada." |
| Usuário 4   | 40                 | 6                        | Sim                      | 0                   | "Interface amigável." |
| Usuário 5   | 50                 | 7                        | Sim                      | 0                   | "Gostei do feedback visual." |

**Cenário 5**: Editar perfil e alterar foto
| **Usuário**   | **Tempo Total (seg)** | **Quantidade de cliques** | **Tarefa foi concluída?** | **Erros Cometidos** | **Feedback do Usuário** |
|-------------|--------------------|--------------------------|--------------------------|---------------------|-------------------------|
| Usuário 1   | 80                 | 12                       | Sim                      | 1                   | "Demorei para achar onde trocar a foto." |
| Usuário 2   | 40                 | 8                        | Sim                      | 0                   | "Muito fácil." |
| Usuário 3   | 120                | 15                       | Não                      | 2                   | "Não consegui salvar as alterações." |
| Usuário 4   | 50                 | 9                        | Sim                      | 0                   | "Gostei da opção de visualizar antes de salvar." |
| Usuário 5   | 60                 | 10                       | Sim                      | 0                   | "Interface intuitiva." |


## Relatório dos testes de usabilidade

### Análise Quantitativa

| Cenário | Taxa de Sucesso | Tempo Médio (seg) | Média de Erros | Taxa de Abandono |
|---------|-----------------|-------------------|----------------|------------------|
| 1       | 80%             | 104               | 0,8            | 20%              |
| 2       | 80%             | 35                | 0,4            | 20%              |
| 3       | 60%             | 116               | 1,2            | 40%              |
| 4       | 80%             | 54                | 0,2            | 20%              |
| 5       | 80%             | 70                | 0,6            | 20%              |

### Principais dificuldades identificadas
- Usuários com menor letramento digital tiveram dificuldade para diferenciar cadastro de consumidor e empresa.
- Alguns usuários não entenderam mensagens de erro no login e no cadastro.
- O botão de solicitar orçamento não estava visível para todos os perfis.
- Falta de confirmação visual clara após algumas ações (ex: envio de orçamento, avaliação).
- Dificuldade para localizar a opção de alterar foto de perfil.

### Tarefas concluídas sem problemas
- Login e logout para usuários com experiência digital.
- Edição de perfil e navegação no dashboard para usuários avançados.
- Avaliação de empresas após contratação.

### Sugestões de melhorias
- Adicionar textos de ajuda e exemplos nos campos de cadastro.
- Tornar o botão de solicitar orçamento mais destacado.
- Melhorar feedback visual após ações importantes (ex: envio de avaliação, orçamento).
- Simplificar mensagens de erro e torná-las mais didáticas.
- Destacar a opção de alterar foto de perfil.

### Priorização dos problemas
- **Crítico:** Dificuldade de cadastro (usuário não consegue concluir), botão de orçamento pouco visível.
- **Moderado:** Mensagens de erro pouco claras, falta de feedback visual.
- **Leve:** Melhorias de acessibilidade e textos de apoio.

### Propostas de ação
- Revisar e simplificar o fluxo de cadastro, com exemplos e textos explicativos.
- Destacar visualmente o botão de solicitar orçamento.
- Implementar mensagens de confirmação visual (toasts, banners) após ações.
- Revisar e padronizar mensagens de erro para linguagem simples.
- Melhorar a navegação para edição de perfil e troca de foto.
