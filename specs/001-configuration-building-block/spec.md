# Especificacao da Feature: Configuration BuildingBlock

**Feature Branch**: `001-configuration-building-block`
**Criado em**: 2026-02-15
**Status**: Rascunho
**Input**: Descricao do usuario: "Criar um BuildingBlock de Configuration com ConfigurationManagerBase e pipeline de handlers no padrao mediator para Get/Set de configuracoes, com comportamento padrao (appsettings, env vars com separador '_', UserSecrets) e extensivel com handlers customizados (ex: AzureKeyVault). Handlers com classe base configuravel com LoadStrategy e opcoes especificas."

## Esclarecimentos

### Sessao 2026-02-15

- Q: Qual a granularidade do escopo do handler — chave exata, secao/prefixo, ou ambos? → A: Ambos — chave exata (ex: `Persistence:PostgreSql:ConnectionString`) e secao/prefixo (ex: todas as chaves sob `Persistence:PostgreSql`).
- Q: Quando um handler com escopo esta registrado, como ele se compoe com o pipeline padrao? → A: Adiciona ao pipeline padrao — handlers padrao executam normalmente e o handler com escopo executa na posicao registrada. Para chaves que nao correspondem ao escopo, o handler e simplesmente pulado.
- Q: O que acontece quando um handler StartupOnly falha durante a inicializacao? → A: Fail-fast — a excecao propaga e impede a aplicacao de subir. Handler StartupOnly que falha indica problema critico de infraestrutura.
- Q: Como o registro de handlers com escopo deve funcionar na pratica? → A: Via fluent API com expressoes type-safe (ex: `options.AddHandler<AzureKeyVaultHandler>().ToClass<MinhaClasse>().ToProperty(c => c.MinhaPropriedade)`). O escopo e definido por referencia direta a classe e propriedade, sem strings manuais. Tudo registrado via IoC.
- Q: Handlers e pipeline devem ser registrados via IoC? → A: Sim. Todo o sistema de configuracao (ConfigurationManagerBase, handlers, pipeline) DEVE ser registrado e resolvido via container de injecao de dependencia.
- Q: O que e registrado no IoC? → A: O ConfigurationManagerBase e os handlers. As classes de configuracao (objetos tipados) NAO sao registradas no IoC diretamente — sao gerenciadas internamente pelo ConfigurationManagerBase, que conhece o mapeamento classe-secao.
- Q: O ConfigurationManagerBase substitui ou estende o comportamento do IConfiguration? → A: Estende. O ConfigurationManagerBase possui um IConfiguration interno que cuida das fontes padrao (appsettings.json, env vars, user secrets). Todo acesso e baseado no IConfiguration, incluindo o Get tipado. Os handlers fazem o trabalho extra — sao extensoes que operam sobre os valores ja resolvidos pelo IConfiguration.
- Q: O handler recebe apenas o valor ou tambem a chave de configuracao? → A: O handler DEVE receber a chave (key/caminho completo) alem do valor, para poder tomar decisoes com base na chave sendo acessada.
- Q: Este BuildingBlock inclui handlers concretos como AzureKeyVault? → A: NAO. AzureKeyVault e apenas um exemplo ilustrativo. Este BuildingBlock depende apenas do IConfiguration e fornece a infraestrutura base (ConfigurationManagerBase, ConfigurationHandlerBase, pipeline, fluent API). Handlers concretos especializados (ex: `Bedrock.BuildingBlocks.Configuration.AzureKeyVault`) serao BuildingBlocks separados no futuro.

## Cenarios de Usuario e Testes *(obrigatorio)*

### Historia de Usuario 1 - Leitura de Configuracao com Fontes Padrao (Prioridade: P1)

Um desenvolvedor de framework cria uma subclasse de ConfigurationManagerBase que encapsula um IConfiguration interno configurado com as fontes padrao (arquivos JSON, variaveis de ambiente, user secrets). Os valores de configuracao sao lidos a partir do IConfiguration e mapeados para objetos fortemente tipados. O caminho de configuracao e derivado automaticamente do nome completo da classe e da propriedade (ex: `Persistence:PostgreSql:ConnectionString`), eliminando a necessidade de passar strings manuais. O sistema suporta tipos primitivos, nullable, e arrays. Sem handlers customizados, o comportamento e equivalente ao IConfiguration padrao.

**Por que esta prioridade**: Esta e a capacidade fundamental — leitura de configuracao e o caso de uso primario. Sem isso, nenhuma outra funcionalidade pode funcionar.

**Teste Independente**: Pode ser totalmente testado criando uma subclasse de ConfigurationManagerBase apenas com handlers padrao, mapeando uma secao de configuracao para um objeto tipado, e verificando que os valores sao resolvidos corretamente a partir de arquivos JSON, variaveis de ambiente (usando separador `_`) e user secrets — sem necessidade de informar caminhos de configuracao manualmente.

**Cenarios de Aceitacao**:

1. **Dado** uma subclasse de ConfigurationManagerBase sem handlers customizados, **Quando** o desenvolvedor le uma secao de configuracao, **Entao** os valores sao resolvidos pelo IConfiguration interno das fontes disponiveis na ordem de prioridade padrao (user secrets > env vars > JSON especifico do ambiente > JSON base).
2. **Dado** variaveis de ambiente com underline simples (`_`) como separador de hierarquia (ex: `Database_ConnectionString`), **Quando** o desenvolvedor le a chave de configuracao correspondente, **Entao** o valor e corretamente resolvido a partir da variavel de ambiente.
3. **Dado** que nao existe appsettings.json ou appsettings.{Environment}.json, **Quando** o desenvolvedor le a configuracao, **Entao** o sistema nao falha — ignora graciosamente as fontes ausentes.
4. **Dado** que assemblies de UserSecrets nao sao fornecidos, **Quando** o configuration manager e inicializado, **Entao** o carregamento de user secrets e ignorado sem erros.
5. **Dado** que assemblies de UserSecrets sao fornecidos como um array, **Quando** o configuration manager e inicializado, **Entao** os user secrets de todos os assemblies especificados sao carregados.
6. **Dado** um objeto de configuracao com a propriedade `ConnectionString` na classe que mapeia para a secao `Persistence:PostgreSql`, **Quando** o desenvolvedor acessa a propriedade via Get, **Entao** o sistema automaticamente resolve o caminho completo `Persistence:PostgreSql:ConnectionString` sem que o desenvolvedor precise informar a string do caminho manualmente.
7. **Dado** duas classes de configuracao diferentes que possuem uma propriedade com o mesmo nome (ex: `ConnectionString` em `Persistence:PostgreSql` e `ConnectionString` em `Persistence:MySql`), **Quando** cada propriedade e acessada via Get, **Entao** cada uma resolve seu caminho completo independente, sem colisao.
8. **Dado** uma propriedade que e um array de strings (ex: `AllowedOrigins` do tipo `string[]`), **Quando** o desenvolvedor acessa a propriedade via Get, **Entao** o sistema resolve o array corretamente a partir da fonte de configuracao (ex: secoes indexadas no JSON ou valores separados no ambiente).
9. **Dado** uma propriedade nullable (ex: `int?`), **Quando** a chave nao existe em nenhuma fonte, **Entao** o Get retorna null em vez de lancar excecao.
10. **Dado** um ConfigurationManagerBase registrado no IoC com seus handlers, **Quando** o desenvolvedor acessa um objeto de configuracao tipado atraves do manager, **Entao** todas as propriedades sao populadas a partir do pipeline de handlers usando o caminho derivado automaticamente da secao e propriedade.

---

### Historia de Usuario 2 - Extensao do Pipeline com Handlers Customizados (Prioridade: P2)

Um desenvolvedor de framework adiciona handlers customizados aos pipelines de Get e/ou Set via fluent API no registro de servicos (IoC), posicionando-os em posicoes especificas na ordem de execucao e direcionando-os a classes e propriedades especificas usando expressoes type-safe. Esses handlers estendem o comportamento do IConfiguration — recebem o valor ja resolvido pelo IConfiguration e podem transforma-lo, substitui-lo ou enriquece-lo. Por exemplo, um handler de AzureKeyVault e registrado apenas para a propriedade `ConnectionString` da classe `PostgreSqlConfig` usando `.ToClass<PostgreSqlConfig>().ToProperty(c => c.ConnectionString)`, enquanto as demais chaves continuam usando apenas o valor do IConfiguration sem handler extra. Handlers tambem podem ser registrados por classe inteira (ex: `.ToClass<JwtConfig>()` para todas as propriedades).

**Por que esta prioridade**: Extensibilidade e o diferencial principal — o padrao de pipeline de handlers permite integracao com fontes externas de configuracao como key vaults, servicos de configuracao remota ou transformacoes customizadas. O direcionamento por caminho permite controle granular sobre quais chaves passam por quais handlers.

**Teste Independente**: Pode ser testado criando um handler customizado direcionado a uma chave especifica, registrando-o em uma posicao especifica no pipeline, e verificando que apenas aquela chave passa pelo handler customizado enquanto as demais chaves sao resolvidas apenas pelo pipeline padrao.

**Cenarios de Aceitacao**:

1. **Dado** um handler customizado registrado como ultimo handler no pipeline de Get, **Quando** a configuracao e lida, **Entao** o handler customizado recebe o valor dos handlers anteriores e pode substituir, transformar ou repassar.
2. **Dado** multiplos handlers customizados no pipeline de Get, **Quando** a configuracao e lida, **Entao** os handlers executam na ordem registrada, cada um recebendo a saida do handler anterior.
3. **Dado** um handler customizado registrado no pipeline de Set, **Quando** a configuracao e escrita, **Entao** o handler customizado recebe o valor e pode transforma-lo ou persisti-lo antes de passar para o proximo handler.
4. **Dado** um handler que ignora o valor recebido e busca de uma fonte externa, **Quando** o pipeline de Get executa, **Entao** os handlers subsequentes recebem o valor buscado externamente.
5. **Dado** um handler registrado para a chave exata `Persistence:PostgreSql:ConnectionString`, **Quando** a chave `Persistence:PostgreSql:ConnectionString` e lida, **Entao** o handler e executado; **Quando** a chave `Persistence:PostgreSql:Schema` e lida, **Entao** o handler NAO e executado.
6. **Dado** um handler registrado para o prefixo `Security:Jwt`, **Quando** qualquer chave sob `Security:Jwt` e lida (ex: `Security:Jwt:Secret`, `Security:Jwt:Issuer`), **Entao** o handler e executado para todas essas chaves.
7. **Dado** um handler sem escopo definido (global), **Quando** qualquer chave e lida, **Entao** o handler e executado para todas as chaves.
8. **Dado** um handler registrado via fluent API com `.ToClass<PostgreSqlConfig>().ToProperty(c => c.ConnectionString)`, **Quando** a configuracao e lida, **Entao** o handler e executado apenas para a propriedade `ConnectionString` da classe `PostgreSqlConfig`, derivando o caminho completo automaticamente.
9. **Dado** um handler registrado via fluent API com `.ToClass<JwtConfig>()` sem especificar propriedade, **Quando** qualquer propriedade de `JwtConfig` e lida, **Entao** o handler e executado para todas as propriedades daquela classe.
10. **Dado** que todo o pipeline (handlers, manager, objetos de configuracao) e registrado via IoC, **Quando** o container resolve o ConfigurationManagerBase, **Entao** todos os handlers registrados sao injetados e o pipeline esta pronto para uso sem configuracao adicional.

---

### Historia de Usuario 3 - Configuracao de Comportamento do Handler com LoadStrategy (Prioridade: P3)

Um desenvolvedor de framework configura handlers individuais com estrategias de carregamento especificas e opcoes especificas do handler. Cada handler tem uma classe base que pode ser configurada com um LoadStrategy (controlando quando o handler carrega seus dados) e configuracoes customizadas adicionais.

**Por que esta prioridade**: Configuracao a nivel de handler permite controle granular sobre performance e comportamento — critico para cenarios de producao onde algumas fontes (como vaults remotos) devem ser carregadas apenas uma vez na inicializacao vs. a cada acesso.

**Teste Independente**: Pode ser testado criando handlers com diferentes valores de LoadStrategy, acessando configuracao multiplas vezes, e verificando que handlers StartupOnly executam apenas durante a inicializacao, LazyStartupOnly executam apenas no primeiro acesso, e AllTime executam a cada acesso.

**Cenarios de Aceitacao**:

1. **Dado** um handler configurado com LoadStrategy.StartupOnly, **Quando** a configuracao e acessada apos a inicializacao, **Entao** o handler usa seu resultado em cache e nao re-executa.
2. **Dado** um handler configurado com LoadStrategy.LazyStartupOnly, **Quando** a configuracao e acessada pela primeira vez, **Entao** o handler executa e armazena seu resultado em cache; acessos subsequentes usam o valor em cache.
3. **Dado** um handler configurado com LoadStrategy.AllTime, **Quando** a configuracao e acessada, **Entao** o handler executa a cada acesso.
4. **Dado** um handler customizado com opcoes especificas (ex: URL do vault, timeout), **Quando** o handler e registrado, **Entao** suas opcoes especificas sao configuraveis independentemente de outros handlers.
5. **Dado** um handler configurado com LoadStrategy.StartupOnly que falha durante a inicializacao (ex: fonte externa inacessivel), **Quando** o configuration manager e inicializado, **Entao** a excecao propaga e a aplicacao nao sobe (fail-fast).

---

### Historia de Usuario 4 - Escrita de Configuracao Atraves do Pipeline de Handlers (Prioridade: P4)

Um desenvolvedor de framework usa a operacao Set para atualizar valores de configuracao. O valor flui pelo pipeline de Set, onde cada handler pode validar, transformar, criptografar ou persistir o valor em uma fonte externa antes de passar para o proximo handler.

**Por que esta prioridade**: Operacoes de escrita complementam leituras e habilitam gerenciamento bidirecional de configuracao. Embora menos comuns que leituras, sao essenciais para cenarios como atualizacao de repositorios de configuracao remota ou manutencao de overrides locais.

**Teste Independente**: Pode ser testado definindo um valor de configuracao e verificando que ele flui por todos os handlers de Set registrados em ordem, com cada handler capaz de transformar o valor.

**Cenarios de Aceitacao**:

1. **Dado** um pipeline de Set com multiplos handlers, **Quando** um valor de configuracao e definido, **Entao** cada handler executa em ordem, recebendo a saida do handler anterior.
2. **Dado** um handler que persiste valores em uma fonte externa, **Quando** um valor e definido, **Entao** o handler escreve o valor na fonte externa.
3. **Dado** uma operacao Get subsequente apos um Set, **Entao** o pipeline de Get retorna o valor atualizado (respeitando o comportamento especifico do handler como LoadStrategy).

---

### Casos de Borda

- O que acontece quando dois handlers sao registrados na mesma posicao do pipeline? O sistema rejeita posicoes duplicadas com um erro claro no momento do registro.
- Como o sistema lida com um handler que lanca uma excecao durante a execucao do pipeline? O erro e propagado com contexto claro sobre qual handler falhou e sua posicao no pipeline.
- O que acontece quando um handler StartupOnly falha durante a inicializacao (ex: vault inacessivel)? A excecao propaga e impede a aplicacao de subir (fail-fast). Handler StartupOnly que falha indica problema critico de infraestrutura.
- O que acontece quando o primeiro acesso de um handler LazyStartupOnly falha (ex: vault inacessivel)? O erro e propagado — falhas nao sao cacheadas. O proximo acesso retenta o handler.
- O que acontece quando uma chave de configuracao nao existe em nenhuma fonte? O pipeline de Get retorna null/default, permitindo que handlers downstream decidam se fornecem um fallback.
- O que acontece quando o pipeline de Set e invocado mas nenhum handler de Set esta registrado? O sistema aplica o valor em memoria sem erros.
- O que acontece quando variaveis de ambiente usam tanto `_` quanto `__` como separadores? O sistema reconhece apenas `_` como separador de hierarquia — `__` e tratado como um literal de duplo underline nos nomes de chave.
- O que acontece quando uma chave corresponde tanto a um handler de chave exata quanto a um handler de secao/prefixo? Todos os handlers aplicaveis sao executados na ordem de suas posicoes registradas no pipeline — a ordem e determinada pela posicao, nao pelo nivel de escopo.
- O que acontece quando um handler com escopo e registrado mas a chave acessada nao corresponde ao escopo? O handler e ignorado (pulado) para aquela chave — o pipeline continua com os demais handlers aplicaveis.
- O que acontece quando uma propriedade de array esta vazia na fonte de configuracao? O Get retorna um array vazio, nao null.
- O que acontece quando uma propriedade de array contem elementos de tipos incompativeis na fonte? O erro e propagado com contexto claro sobre qual elemento falhou na conversao.
- O que acontece quando duas classes de configuracao diferentes tem uma propriedade com o mesmo nome? Nao ha colisao — o caminho completo inclui o nome da classe/secao (ex: `ClasseA:Prop` vs `ClasseB:Prop`), garantindo unicidade.

## Requisitos *(obrigatorio)*

### Requisitos Funcionais

- **RF-001**: O sistema DEVE fornecer uma classe abstrata ConfigurationManagerBase que encapsula um IConfiguration interno e serve como ponto de entrada para leitura e escrita de valores de configuracao. O IConfiguration cuida das fontes padrao; o ConfigurationManagerBase estende esse comportamento com um pipeline de handlers que operam sobre os valores ja resolvidos pelo IConfiguration.
- **RF-002**: O sistema DEVE implementar um pipeline de handlers para operacoes Get. O valor inicial do pipeline e o valor resolvido pelo IConfiguration interno. Cada handler recebe a chave de configuracao (caminho completo) e a saida do handler anterior (ou do IConfiguration, no caso do primeiro handler), e pode transformar, substituir ou repassar (padrao mediator). A chave permite ao handler tomar decisoes com base no caminho sendo acessado.
- **RF-003**: O sistema DEVE implementar um pipeline de handlers para operacoes Set seguindo o mesmo padrao mediator do Get. Cada handler recebe a chave de configuracao e o valor, podendo tomar decisoes com base na chave.
- **RF-004**: O IConfiguration interno DEVE ser configurado com as fontes padrao: arquivos de configuracao JSON (appsettings.json, appsettings.{Environment}.json), variaveis de ambiente e user secrets. Essas fontes NAO sao handlers — sao a base sobre a qual os handlers operam.
- **RF-005**: Todas as fontes de configuracao padrao (arquivos JSON, user secrets) DEVEM ser opcionais — o sistema NAO DEVE falhar se qualquer fonte estiver ausente.
- **RF-006**: O handler de variaveis de ambiente DEVE usar um underline simples (`_`) como separador de hierarquia ao inves do padrao de duplo underline (`__`).
- **RF-007**: O handler de user secrets DEVE aceitar um array opcional de assemblies dos quais carregar secrets.
- **RF-008**: Desenvolvedores DEVEM poder registrar handlers customizados em posicoes especificas (ordenacao) nos pipelines de Get e Set, com escopo opcional: global (todas as chaves), por chave exata ou por secao/prefixo. O escopo DEVE ser definido via fluent API com expressoes type-safe que referenciam diretamente a classe e propriedade do objeto de configuracao (ex: `.ToClass<PostgreSqlConfig>().ToProperty(c => c.ConnectionString)`), sem necessidade de strings manuais de caminho. Handlers sem escopo definido sao globais por padrao.
- **RF-009**: Cada handler DEVE estender uma classe base de handler que suporte opcoes de configuracao especificas do handler.
- **RF-010**: O sistema DEVE suportar configuracao de LoadStrategy por handler com pelo menos tres modos: StartupOnly (executar uma vez na inicializacao), LazyStartupOnly (executar uma vez no primeiro acesso) e AllTime (executar a cada acesso).
- **RF-011**: Valores de configuracao DEVEM ser mapeaveis para objetos fortemente tipados por secao de configuracao.
- **RF-012**: O pipeline de handlers DEVE propagar erros com contexto claro sobre qual handler falhou.
- **RF-013**: Handlers no pipeline de Get DEVEM poder ignorar completamente o valor recebido e fornecer um novo valor de uma fonte externa.
- **RF-014**: O sistema DEVE rejeitar posicoes duplicadas de handlers dentro do mesmo pipeline com um erro claro no momento do registro.
- **RF-015**: As operacoes Get e Set DEVEM derivar automaticamente o caminho completo de configuracao a partir do nome da classe e da propriedade do objeto de configuracao (ex: classe que mapeia `Persistence:PostgreSql` com propriedade `ConnectionString` resulta no caminho `Persistence:PostgreSql:ConnectionString`). O desenvolvedor NAO DEVE precisar informar strings de caminho manualmente.
- **RF-016**: O caminho derivado automaticamente (RF-015) DEVE ser usado como chave para corresponder handlers com escopo (RF-008), garantindo que propriedades com mesmo nome em classes diferentes nao colidam (ex: `Persistence:PostgreSql:ConnectionString` e `Persistence:MySql:ConnectionString` sao chaves distintas).
- **RF-017**: O sistema DEVE suportar propriedades de tipo array (ex: `string[]`, `int[]`) nos objetos de configuracao, resolvendo corretamente a partir de secoes indexadas na fonte de configuracao.
- **RF-018**: O sistema DEVE suportar tipos nullable nos objetos de configuracao (ex: `int?`, `string?`), retornando null quando a chave nao existe em nenhuma fonte ao inves de lancar excecao.
- **RF-019**: O registro de handlers, pipeline e ConfigurationManagerBase DEVE ser feito via container de injecao de dependencia (IoC). Toda a configuracao do pipeline DEVE ser declarativa via fluent API no momento do registro dos servicos.
- **RF-020**: A fluent API de registro DEVE suportar definicao de escopo por secao/classe inteira (ex: `.ToClass<PostgreSqlConfig>()` para todas as propriedades da classe) alem de por propriedade especifica (ex: `.ToClass<PostgreSqlConfig>().ToProperty(c => c.ConnectionString)`).
- **RF-021**: O registro no IoC DEVE incluir o ConfigurationManagerBase e os handlers. As classes de configuracao (objetos tipados) NAO sao registradas no IoC diretamente — sao gerenciadas internamente pelo ConfigurationManagerBase, que conhece o mapeamento entre classe e secao de configuracao.

### Entidades Principais

- **ConfigurationManagerBase**: A classe base abstrata que encapsula um IConfiguration interno e gerencia o pipeline de handlers. Todo acesso a configuracao e baseado no IConfiguration — o Get tipado le do IConfiguration e depois passa o valor pelo pipeline de handlers para extensao. Estendida por desenvolvedores para configurar seu pipeline especifico de handlers.
- **ConfigurationHandlerBase**: A classe base abstrata para todos os handlers de extensao. Cada handler recebe a chave de configuracao (caminho completo) e o valor atual, podendo tomar decisoes com base em ambos. Contem configuracao especifica do handler (LoadStrategy, escopo de chave/propriedade, opcoes customizadas). Estendida para criar handlers que adicionam comportamento alem do IConfiguration (ex: handler de key vault, handler de transformacao, handler de criptografia). O escopo e definido via fluent API com expressoes type-safe — pode ser global, direcionado a uma classe inteira, ou direcionado a uma propriedade especifica de uma classe.
- **LoadStrategy**: Define quando um handler carrega ou atualiza seus dados. Valores: StartupOnly (carregar uma vez na inicializacao e cachear), LazyStartupOnly (carregar no primeiro acesso e cachear), AllTime (carregar a cada acesso).
- **IConfiguration (interno)**: A instancia de configuracao padrao encapsulada pelo ConfigurationManagerBase. Cuida das fontes padrao (JSON, env vars, user secrets). Fornece o valor inicial que entra no pipeline de handlers.
- **Pipeline de Handlers**: Uma cadeia ordenada de handlers de extensao que processa operacoes Get ou Set sequencialmente. O valor inicial vem do IConfiguration; cada handler recebe a saida do anterior e pode transforma-lo, substitui-lo ou repassar (padrao mediator).
- **Objeto de Configuracao**: Um objeto fortemente tipado que mapeia para uma secao de configuracao. Operacoes Get/Set neste objeto passam pelo pipeline do ConfigurationManagerBase. O caminho completo de configuracao e derivado automaticamente do nome da classe + propriedade, eliminando strings manuais e evitando colisao de nomes. Suporta tipos primitivos, nullable e arrays.

## Criterios de Sucesso *(obrigatorio)*

### Resultados Mensuraveis

- **CS-001**: Desenvolvedores conseguem ler valores de configuracao de todas as fontes padrao (arquivos JSON, variaveis de ambiente com separador `_`, user secrets) com uma unica configuracao do manager em menos de 5 minutos de tempo de desenvolvimento.
- **CS-002**: Adicionar um handler customizado ao pipeline requer apenas estender a classe base do handler e registra-lo via fluent API no IoC — sem modificacao no manager ou em outros handlers.
- **CS-003**: Todas as operacoes do pipeline de handlers (Get e Set) completam sua cadeia completa para fontes em memoria em menos de 1 milissegundo por chave.
- **CS-004**: 100% das chaves de configuracao sao resolviveis atraves do pipeline de handlers, independentemente do numero ou tipo de handlers registrados.
- **CS-005**: Opcoes especificas do handler (LoadStrategy, configuracoes customizadas) sao configuraveis independentemente sem afetar outros handlers no pipeline.
- **CS-006**: O BuildingBlock segue todas as convencoes existentes do Bedrock (nomenclatura, testes, testes de mutacao a 100%, regras de arquitetura).

## Fora de Escopo

- Handlers concretos especializados (ex: AzureKeyVault, AWS Secrets Manager, HashiCorp Vault) NAO fazem parte deste BuildingBlock. Serao criados como BuildingBlocks separados no futuro (ex: `Bedrock.BuildingBlocks.Configuration.AzureKeyVault`).
- Este BuildingBlock fornece apenas a infraestrutura base: ConfigurationManagerBase, ConfigurationHandlerBase, pipeline, LoadStrategy, fluent API de registro.
- A unica dependencia externa e o IConfiguration (Microsoft.Extensions.Configuration).
- Todas as referencias a "KeyVault" ou "AzureKeyVault" na spec sao meramente exemplos ilustrativos de como um handler futuro poderia ser usado.

## Premissas

- O separador `_` (underline simples) para variaveis de ambiente e intencional e difere do padrao .NET (`__`). Esta e uma convencao especifica do projeto.
- Operacoes "Set" sao em memoria por padrao, mas handlers podem opcionalmente persistir em fontes externas. O pipeline em si nao exige persistencia.
- A prioridade padrao das fontes e gerenciada pelo IConfiguration interno e segue a precedencia padrao: user secrets > variaveis de ambiente > JSON especifico do ambiente > JSON base. Handlers customizados operam sobre o valor ja resolvido pelo IConfiguration, estendendo o comportamento sem substituir as fontes padrao.
- O ConfigurationManagerBase estende o IConfiguration, nao o substitui. O IConfiguration e a base para leitura de configuracoes; os handlers sao extensoes que adicionam comportamento extra (ex: transformar valores, criptografar). Sem handlers, o comportamento e identico ao IConfiguration padrao.
- O ConfigurationManagerBase segue o padrao template method existente no Bedrock (Initialize/ConfigureInternal) visto em outras classes base como MigrationManagerBase.
- LoadStrategy.LazyStartupOnly usa inicializacao lazy (primeiro acesso dispara o carregamento, acessos subsequentes usam o valor em cache). Thread-safety para primeiro acesso concorrente e garantida.
- Objetos de configuracao sao mapeados por secao (ex: secao "Persistence:PostgreSql" mapeia para um objeto PostgreSqlConfig), nao por chave individual. O mapeamento classe-secao e gerenciado internamente pelo ConfigurationManagerBase — os objetos de configuracao nao sao registrados no IoC diretamente.
- O caminho de configuracao e derivado automaticamente do nome completo da classe e propriedade. O desenvolvedor nao precisa (e nao deve) informar strings de caminho manualmente nas operacoes Get/Set. Isso garante consistencia e previne colisao de nomes.
