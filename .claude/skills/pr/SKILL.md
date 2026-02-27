---
name: pr
description: "Fase 7: Criar PR, acompanhar CI e merge. Requer numero da issue."
argument-hint: "[issue-number]"
disable-model-invocation: true
allowed-tools: Read, Bash(gh *), Bash(git *)
---

# /pr - Fase 7: Pull Request

Voce esta na fase PR. Crie a PR e acompanhe ate o merge.

## Argumentos

$ARGUMENTS

Deve conter o numero da issue (ex: `/pr 42`).

## Fluxo

1. Verificar que a pipeline local passou:
   - `artifacts/pending/SUMMARY.txt` deve ter zero pendencias
   - Se nao, informar o usuario para rodar `/pipeline` primeiro
2. Commitar alteracoes pendentes (se houver):
   - `git add` dos arquivos relevantes (NAO usar `git add -A`)
   - Commit com mensagem descritiva
3. Push para o remote: `git push -u origin <branch>`
4. Criar PR: `gh pr create --title "..." --body "Closes #<issue>"`
5. Aguardar pipeline GitHub Actions: `gh pr checks <number> --watch`
6. Se pipeline **passou**:
   - `gh pr merge <number> --squash --delete-branch`
   - `git checkout main && git pull`
   - `git branch -d <branch-local>`
7. Se pipeline **falhou** (max 5 tentativas):
   - `gh run view <run-id> --log-failed`
   - Corrigir localmente
   - Commitar e push
   - Voltar ao passo 5

## Regras de Commit

- Mensagem segue conventional commits: `feat:`, `fix:`, `refactor:`, `test:`
- Body com `Closes #<issue>` para auto-close
- NAO usar `git add -A` (pode incluir .env, credenciais)
- NAO usar `--no-verify` ou `--force`

## Regras de PR

- Titulo descritivo (max 70 chars)
- Body com:
  - `## Summary` (1-3 bullet points)
  - `## Test plan` (checklist)
  - `Closes #<issue>`

## Apos Merge

- Atualizar labels da issue: `status:approved` → issue fechada automaticamente
- Limpar branch local
