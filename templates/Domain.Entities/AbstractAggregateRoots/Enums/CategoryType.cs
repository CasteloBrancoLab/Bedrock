namespace Templates.Domain.Entities.AbstractAggregateRoots.Enums;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Convenções de Enumerações no Domínio
═══════════════════════════════════════════════════════════════════════════════

Enumerações representam conjuntos finitos de valores válidos para propriedades.
Seguem convenções específicas para clareza, performance e manutenibilidade.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Nomenclatura - Sem Sufixo "Enum"
───────────────────────────────────────────────────────────────────────────────

O nome da enum DEVE ser simples e direto, SEM sufixo "Enum":
✅ CategoryType, PersonType, OrderStatus
❌ CategoryTypeEnum, PersonTypeEnumeration

RAZÃO: O contexto de uso já indica que é uma enumeração.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Tipo Subjacente - Usar byte Quando Possível
───────────────────────────────────────────────────────────────────────────────

SEMPRE especifique o tipo subjacente explicitamente:
✅ public enum CategoryType : byte
❌ public enum CategoryType (usa int por padrão)

PREFERÊNCIA DE TIPOS:
- byte (0-255): maioria dos casos, até 255 valores
- short (±32k): quando byte não é suficiente
- int: apenas quando necessário compatibilidade externa

RAZÃO: Otimização de memória em coleções grandes e serialização.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Valores Explícitos - Sempre Definir
───────────────────────────────────────────────────────────────────────────────

SEMPRE defina valores explicitamente para cada membro:
✅ TypeA = 1, TypeB = 2
❌ TypeA, TypeB (valores implícitos)

RAZÃO: Evita quebra de compatibilidade se a ordem dos membros for alterada.
Valores persistidos em banco permanecem consistentes.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Valor Inicial - Começar em 1 (Não Zero)
───────────────────────────────────────────────────────────────────────────────

SEMPRE comece valores em 1, NÃO em 0:
✅ TypeA = 1, TypeB = 2
❌ TypeA = 0, TypeB = 1

RAZÃO: Zero é o valor default de tipos numéricos. Começar em 1 permite
distinguir entre "valor não definido" (0) e "primeiro valor válido" (1).

EXCEÇÃO: Use 0 apenas para representar explicitamente "Unknown" ou "None".

═══════════════════════════════════════════════════════════════════════════════
*/
public enum CategoryType : byte
{
    TypeA = 1,
    TypeB = 2
}
