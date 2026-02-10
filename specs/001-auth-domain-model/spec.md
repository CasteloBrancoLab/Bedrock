# Feature Specification: Auth Domain Model — User Entity com Credenciais

**Feature Branch**: `001-auth-domain-model`
**Created**: 2026-02-09
**Status**: Draft
**Input**: Issue #138 — Auth: User + Credentials (Entity, Repository, Service, Use Case, Controller) — foco no modelo de domínio (Domain.Entities)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registrar Novo Usuário com Email e Senha (Priority: P1)

Um novo usuário fornece seu email e uma senha (já processada como hash pela camada de serviço) para criar uma conta no sistema. O sistema valida o formato do email, recebe o hash da senha como array de bytes (gerado externamente pelo building block de segurança, incluindo salt e pepper), define o username como igual ao email (regra de negócio inicial), e cria a entidade User com status ativo. A entidade de domínio não tem conhecimento sobre o algoritmo de hashing — ela apenas armazena o resultado opaco.

**Why this priority**: Esta é a funcionalidade fundamental — sem a capacidade de criar usuários com credenciais válidas, nenhum outro fluxo de autenticação é possível. A entidade User é o aggregate root de todo o domínio de autenticação.

**Independent Test**: Pode ser testado criando uma entidade User via factory method `RegisterNew` com email válido e hash de senha (array de bytes), verificando que o Id é gerado, o email é armazenado, o hash é armazenado corretamente, o status é Ativo e os campos de auditoria são preenchidos. Nenhuma dependência de criptografia é necessária nos testes da entidade.

**Acceptance Scenarios**:

1. **Given** dados válidos (email formatado corretamente, hash de senha como array de bytes não-vazio), **When** `User.RegisterNew` é invocado, **Then** uma entidade User é retornada com Id único, email validado, username igual ao email, hash armazenado, status Ativo e campos de auditoria preenchidos.
2. **Given** um email com formato inválido (ex: "sem-arroba.com"), **When** `User.RegisterNew` é invocado, **Then** o retorno é null e o ExecutionContext contém a mensagem de erro de validação do email.
3. **Given** um hash de senha vazio (array vazio ou nulo), **When** `User.RegisterNew` é invocado, **Then** o retorno é null e o ExecutionContext contém a mensagem de erro indicando que o hash é obrigatório.
4. **Given** um email e hash de senha ambos inválidos, **When** `User.RegisterNew` é invocado, **Then** o retorno é null e o ExecutionContext contém todas as mensagens de erro (validação completa via bitwise AND).

---

### User Story 2 - Verificar Credenciais de Usuário (Priority: P2)

Um usuário existente fornece email e senha para se autenticar. O domain service de autenticação localiza o usuário pelo email, delega a verificação da senha ao building block de segurança (que compara a senha fornecida contra o hash armazenado) e confirma ou rejeita as credenciais. Respostas de erro são genéricas para prevenir enumeração de contas.

**Why this priority**: A verificação de credenciais é o segundo pilar da autenticação — após o registro, o usuário precisa poder provar sua identidade. O domain service orquestra a interação entre a entidade User e o building block de segurança.

**Independent Test**: Pode ser testado usando o building block de segurança para gerar um hash, criando um User com esse hash, depois verificando via o serviço de segurança que a senha original corresponde ao hash armazenado na entidade.

**Acceptance Scenarios**:

1. **Given** um User existente com hash de senha armazenado, **When** a senha correta é verificada via o serviço de segurança, **Then** a verificação retorna verdadeiro.
2. **Given** um User existente com hash de senha armazenado, **When** uma senha incorreta é verificada via o serviço de segurança, **Then** a verificação retorna falso.
3. **Given** qualquer cenário de falha na verificação de credenciais, **When** o sistema reporta o erro, **Then** a mensagem é genérica ("credenciais inválidas") sem diferenciar se o problema é no email ou na senha.

---

### User Story 3 - Modificar Status do Usuário (Priority: P3)

Um administrador ou processo do sistema precisa alterar o status de um usuário (ex: suspender, bloquear, reativar). O sistema aplica a mudança de status seguindo o padrão Clone-Modify-Return, validando transições de estado permitidas e registrando a auditoria da mudança.

**Why this priority**: A gestão de status é necessária para cenários de segurança (bloqueio após tentativas falhas, suspensão por violação de termos), mas pode ser implementada após os fluxos fundamentais de registro e verificação.

**Independent Test**: Pode ser testado criando um User ativo, invocando o método de mudança de status para Suspenso, e verificando que uma nova instância é retornada com o status alterado e a auditoria de mudança registrada, enquanto a instância original permanece inalterada.

**Acceptance Scenarios**:

1. **Given** um User com status Ativo, **When** o status é alterado para Suspenso, **Then** uma nova instância é retornada com status Suspenso, EntityVersion incrementada e LastChangedAt atualizado.
2. **Given** um User com status Ativo, **When** o status é alterado para Suspenso, **Then** a instância original permanece com status Ativo (imutabilidade via Clone-Modify-Return).
3. **Given** um User com status Bloqueado, **When** uma transição inválida é tentada, **Then** o retorno é null e o ExecutionContext contém a mensagem de erro da transição inválida.

---

### User Story 4 - Consultar Usuário por Email (Priority: P4)

O sistema precisa localizar um usuário pelo seu endereço de email para fluxos de autenticação e verificação de duplicidade. O repositório fornece uma abstração para busca por email que será implementada pela camada de persistência.

**Why this priority**: A busca por email é essencial para autenticação e prevenção de duplicatas, mas no escopo do domínio é uma interface de repositório — a implementação concreta virá na camada de persistência.

**Independent Test**: Pode ser testado verificando que a interface IUserRepository define o contrato correto com os métodos necessários e as assinaturas esperadas.

**Acceptance Scenarios**:

1. **Given** a interface IUserRepository definida, **When** o contrato é inspecionado, **Then** contém métodos para buscar por email, buscar por username, buscar por Id, verificar existência por email, verificar existência por username e registrar novo usuário.

---

### User Story 5 - Operações de Hashing de Senha via Building Block de Segurança (Priority: P5)

O building block de segurança fornece operações de hashing e verificação de senhas como serviço reutilizável do framework. Qualquer serviço de domínio que precise gerar ou verificar hashes de senha utiliza este building block, que encapsula o algoritmo Argon2id e seus parâmetros. A entidade User nunca interage diretamente com este building block — ela apenas recebe e armazena o resultado (array de bytes).

**Why this priority**: O building block de segurança é uma peça de infraestrutura reutilizável. Embora essencial para o fluxo completo, sua prioridade é menor porque a entidade User pode ser desenvolvida e testada independentemente, usando arrays de bytes arbitrários como hash.

**Independent Test**: Pode ser testado gerando um hash para uma senha conhecida, depois verificando que a mesma senha é validada contra o hash. Também pode ser testado que senhas diferentes produzem hashes diferentes (via salt) e que senhas incorretas falham na verificação.

**Acceptance Scenarios**:

1. **Given** uma senha em texto claro válida, **When** o serviço de hashing é invocado, **Then** um hash é gerado como array de bytes não-vazio.
2. **Given** um hash gerado previamente e a senha original, **When** a verificação é invocada, **Then** o resultado é verdadeiro.
3. **Given** um hash gerado previamente e uma senha diferente, **When** a verificação é invocada, **Then** o resultado é falso.
4. **Given** a mesma senha, **When** o hashing é invocado duas vezes, **Then** os hashes produzidos são diferentes (salt aleatório + pepper garantem unicidade).
5. **Given** um pepper ativo (versão N) e um novo pepper (versão N+1), **When** um usuário faz login com sucesso, **Then** o hash é re-gerado com o pepper mais recente e atualizado na entidade User de forma transparente.

---

### Edge Cases

- O que acontece quando o email contém caracteres Unicode válidos no domínio (ex: "user@exämple.com")? O sistema aceita apenas emails ASCII conforme RFC 5321.
- O que acontece quando a senha contém exatamente 12 caracteres (limite mínimo)? A validação de comprimento da senha é responsabilidade do building block de segurança (antes do hashing). A entidade User recebe apenas o hash resultante.
- O que acontece quando o email é fornecido com case diferente (ex: "User@Email.COM")? O email é normalizado para lowercase antes de armazenar, garantindo busca case-insensitive.
- O que acontece quando se tenta criar dois usuários com o mesmo email? A validação de unicidade é responsabilidade da camada de persistência (constraint no banco), não da entidade de domínio.
- O que acontece quando o array de bytes do hash é extremamente grande? O sistema define um tamanho máximo aceitável para o hash, rejeitando valores fora do intervalo esperado.
- O que acontece quando o hash é um array de bytes válido mas corrompido (não gerado pelo algoritmo correto)? A entidade User não valida a integridade do hash — ela trata como dado opaco. A verificação de integridade é responsabilidade do building block de segurança no momento da comparação.
- O que acontece quando o pepper configurado é vazio ou nulo? O building block de segurança rejeita a operação com erro claro — hashing sem pepper não é permitido.
- O que acontece quando nenhuma versão de pepper está ativa? O building block de segurança rejeita a operação — pelo menos uma versão de pepper deve estar ativa.
- O que acontece quando a verificação é feita com um hash gerado por uma versão de pepper que já foi removida (não está mais na lista de versões ativas)? A verificação falha — peppers removidos não são mais aceitos. A rotação deve manter versões anteriores ativas até que todos os hashes sejam re-gerados.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer uma entidade User selada (sealed) seguindo o padrão EntityBase do framework Bedrock, com Id, Username (único), Email (único), PasswordHash (array de bytes opaco), Status, e metadados de auditoria gerenciados automaticamente (CreatedAt, UpdatedAt, EntityVersion). Username e Email são campos distintos com constraints de unicidade independentes; a regra de negócio inicial define Username = Email via factory method.
- **FR-002**: O sistema DEVE fornecer um value object Email que valide o formato do endereço de email, aceitando apenas emails ASCII conforme RFC 5321, e normalizando para lowercase.
- **FR-003**: O sistema DEVE armazenar o hash da senha como valor opaco (array de bytes) na entidade User, sem qualquer conhecimento ou dependência do algoritmo de hashing utilizado. A entidade valida apenas que o hash é não-vazio e está dentro de um tamanho aceitável.
- **FR-004**: O sistema DEVE fornecer um building block de segurança reutilizável que encapsule operações de hashing de senhas (gerar hash e verificar senha contra hash), utilizando exclusivamente Argon2id com salt automático por hash e pepper (chave secreta da aplicação) aplicado antes do hashing. O building block DEVE suportar rotação de pepper (múltiplas versões da chave ativas simultaneamente, re-hash gradual no próximo login bem-sucedido). Este building block referencia apenas o Core e pode ser referenciado por qualquer camada que necessite.
- **FR-005**: O sistema DEVE validar a política de senhas conforme NIST 800-63B no building block de segurança (antes do hashing): mínimo de 12 caracteres, máximo de 128 caracteres, sem restrições de tipos de caracteres (letras, números, símbolos e espaços são todos válidos).
- **FR-006**: O sistema DEVE fornecer um enumerador UserStatus com os estados: Ativo, Suspenso e Bloqueado, validando transições de estado permitidas.
- **FR-007**: O sistema DEVE fornecer factory methods `RegisterNew` (com validação completa) e `CreateFromExistingInfo` (sem validação, para reconstituição de dados persistidos), seguindo o padrão de dois construtores privados.
- **FR-008**: O sistema DEVE implementar o padrão Clone-Modify-Return para todas as operações de mudança de estado, garantindo que a instância original nunca é modificada.
- **FR-009**: O sistema DEVE usar o operador bitwise AND (&) em todas as validações, garantindo que todos os erros são coletados e retornados ao usuário de uma vez.
- **FR-010**: O sistema DEVE fornecer Input Objects (readonly record struct) para cada operação: RegisterNewInput (recebe apenas email e hash; username é derivado do email pela regra de negócio), ChangeStatusInput, ChangeUsernameInput e CreateFromExistingInfoInput.
- **FR-011**: O sistema DEVE fornecer uma interface IUserRepository com métodos para: buscar por Id, buscar por email, buscar por username, verificar existência por email, verificar existência por username, registrar novo usuário e atualizar usuário existente, restrita a aggregate roots.
- **FR-012**: O sistema DEVE fornecer um domain service no projeto Domain que orquestre a interação entre a entidade User e o building block de segurança, realizando o hashing da senha antes de passá-la à entidade como array de bytes.
- **FR-013**: O sistema DEVE fornecer métodos de validação públicos e estáticos (Validate*) na entidade User para email e hash de senha, permitindo validação antes da criação da entidade.
- **FR-014**: O sistema DEVE garantir que respostas de erro em cenários de autenticação sejam genéricas ("credenciais inválidas"), nunca diferenciando se o problema é no email ou na senha (anti-enumeração de contas).
- **FR-015**: O projeto Domain.Entities DEVE ter zero dependências do building block de segurança, recebendo o hash da senha exclusivamente como array de bytes.
- **FR-016**: O building block de segurança (`Bedrock.BuildingBlocks.Security`) DEVE seguir a mesma estrutura de testes do framework: projeto de UnitTests correspondente (relação 1:1), projeto de MutationTests correspondente (relação 1:1), threshold de mutação em 100%, cobertura de linhas em 100%. Testes devem cobrir: hashing, verificação, validação de política de senhas, rotação de pepper, e edge cases (senha vazia, senha no limite mínimo/máximo, pepper inválido).
- **FR-017**: Os projetos de Domain.Entities e Domain do Auth Service DEVEM ter projetos de UnitTests e MutationTests correspondentes (relação 1:1), com os mesmos thresholds de 100% cobertura e 100% mutantes eliminados. Os projetos de teste já existem como scaffolding da issue #137.

### Key Entities

- **User**: Aggregate root do domínio de autenticação. Representa um usuário do sistema com identidade única, username, email, credenciais de acesso (hash de senha como array de bytes opaco) e estado de ciclo de vida (Ativo, Suspenso, Bloqueado). Username e Email são campos distintos — a regra de negócio inicial define Username = Email, encapsulada no factory method `RegisterNew`. O Email é a chave única de negócio (identidade canônica do usuário). Possui metadados de auditoria completos (criação, última alteração, versão) gerenciados automaticamente pela infraestrutura base do Bedrock. Não possui conhecimento sobre o algoritmo de hashing utilizado.
- **Email (Value Object)**: Representa um endereço de email validado e normalizado. Implementado como readonly struct, garante formato ASCII conforme RFC 5321 e armazenamento em lowercase. Suporta extração de parte local e domínio. Reutiliza o `EmailAddress` existente no BuildingBlocks.Core.
- **PasswordHash (Value Object)**: Encapsula o hash criptográfico de uma senha como array de bytes opaco. Implementado como readonly struct, não possui conhecimento do algoritmo utilizado. Valida apenas que o conteúdo é não-vazio e está dentro de um tamanho aceitável. Fornece comparação segura em tempo constante.
- **UserStatus (Enumeração)**: Define os estados possíveis do ciclo de vida do usuário: Ativo (pode autenticar), Suspenso (temporariamente impedido de autenticar) e Bloqueado (permanentemente impedido até intervenção administrativa).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Todas as operações de criação e modificação da entidade User validam 100% dos campos de entrada, coletando todos os erros em uma única resposta (zero erros silenciosos).
- **SC-002**: A entidade User mantém imutabilidade completa — nenhuma operação de modificação altera a instância original, verificável por comparação de referência antes e após cada operação.
- **SC-003**: O value object Email rejeita 100% dos endereços com formato inválido e aceita 100% dos endereços com formato válido conforme RFC 5321.
- **SC-004**: A política de senhas aceita qualquer senha entre 12 e 128 caracteres independente dos tipos de caracteres utilizados, e rejeita 100% das senhas fora deste intervalo.
- **SC-005**: O serviço de hashing gera hashes distintos para a mesma senha (via salt + pepper) e verifica corretamente senhas válidas contra seus hashes em 100% dos casos, incluindo hashes gerados com versões anteriores do pepper.
- **SC-006**: Nenhuma mensagem de erro de autenticação expõe informação sobre a existência ou não de um email no sistema.
- **SC-007**: A cobertura de testes unitários atinge 100% de linhas e 100% de mutantes eliminados (zero mutantes sobreviventes) para todos os projetos afetados, incluindo: (a) Domain.Entities — entidade User, value objects Email e PasswordHash, enumeração UserStatus, Input Objects; (b) Domain — domain service de autenticação, interface IUserRepository; (c) Bedrock.BuildingBlocks.Security — serviço de hashing com Argon2id, validação de política de senhas, rotação de pepper. Cada projeto DEVE ter projetos de UnitTests e MutationTests correspondentes em relação 1:1, com threshold de mutação em 100%.
- **SC-008**: Todas as transições de estado do UserStatus são validadas — transições permitidas são executadas com sucesso e transições não permitidas são rejeitadas com mensagem de erro clara.
- **SC-009**: O projeto Domain.Entities compila e é testável sem nenhuma referência ao building block de segurança, confirmando o desacoplamento total.

## Clarifications

### Session 2026-02-09

- Q: O hashing de senha inclui pepper (chave secreta da aplicação) além do salt automático do Argon2id? → A: Sim — Salt automático (Argon2id) + Pepper (chave secreta da aplicação) + Suporte a rotação de pepper (múltiplas versões da chave, re-hash gradual no login). Máxima segurança.
- Q: O User deve ter campos Username e Email separados ou apenas Email? → A: Campos separados. A regra de negócio inicial define Username = Email, encapsulada num factory method `RegisterNew` que recebe apenas email e preenche ambos. Isso permite futura dissociação sem mudança estrutural.
- Q: Username também é campo único, ou apenas Email? → A: Ambos únicos. Username e Email são campos de identificação distintos, sem duplicatas em nenhum deles.
- Q: Critérios de teste, cobertura e mutação se aplicam ao building block Security também? → A: Sim — todos os critérios de teste do framework (100% cobertura, 100% mutantes eliminados, relação 1:1 UnitTests/MutationTests) se aplicam igualmente ao Bedrock.BuildingBlocks.Security, Domain.Entities e Domain do Auth Service.

## Assumptions

- A validação de email reutiliza o value object `EmailAddress` já existente no BuildingBlocks.Core, estendendo-o com validação de formato no nível da entidade User conforme o padrão do Bedrock (value objects são containers, entidades validam).
- O building block de segurança (`Bedrock.BuildingBlocks.Security`) é um novo projeto no framework que referencia apenas `Bedrock.BuildingBlocks.Core`. Ele encapsula Argon2id com parâmetros padrão recomendados pela OWASP (memory: 19 MiB, iterations: 2, parallelism: 1), salt automático por hash (gerado pelo Argon2id) e pepper (chave secreta da aplicação, configurável externamente). Suporta rotação de pepper com múltiplas versões ativas e re-hash transparente no login.
- Username e Email são campos separados na entidade User, ambos com constraints de unicidade independentes. A regra de negócio inicial define Username = Email, encapsulada no factory method `RegisterNew` que recebe apenas email e hash. Futuramente, a regra pode ser alterada para permitir usernames diferentes do email sem mudança estrutural na entidade. A unicidade de ambos é validada na camada de persistência (constraints no banco).
- A verificação contra senhas vazadas (haveibeenpwned) mencionada na issue #138 é escopo de camada de aplicação (use case), não do domínio — o domínio valida apenas comprimento e formato.
- A interface IUserRepository será definida no projeto Domain (não Domain.Entities), seguindo a separação de building blocks do Bedrock onde Domain.Entities tem zero dependências de infraestrutura.
- O domain service de autenticação no projeto Domain referencia tanto Domain.Entities quanto o building block de Security, orquestrando: (1) receber senha em texto claro, (2) delegar ao Security para gerar hash, (3) passar hash como byte[] para a entidade User.
- As transições de status permitidas são: Ativo → Suspenso, Ativo → Bloqueado, Suspenso → Ativo, Suspenso → Bloqueado, Bloqueado → Ativo. A transição Bloqueado → Suspenso não é permitida (bloqueio requer reativação completa).
- O fluxo de dependências entre os building blocks é: Domain.Entities → Core (apenas), Domain → Domain.Entities + Core + Security, garantindo que entidades de domínio permanecem livres de acoplamento com infraestrutura de segurança.
