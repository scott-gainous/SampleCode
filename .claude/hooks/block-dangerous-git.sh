#!/bin/bash
# Git guardrails (adapted from mattpocock/skills git-guardrails-claude-code).
# Repo-specific customization, chosen 2026-07-19: plain `git push` is ALLOWED so
# Claude Code sessions can deliver work to their claude/* feature branch;
# force-pushes (except --force-with-lease, which the repo's merged-branch-restart
# workflow needs), pushes to master/main, and history-destroying commands are blocked.

INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty' 2>/dev/null)
[ -z "$COMMAND" ] && exit 0

DANGEROUS_PATTERNS=(
  "git push[^|;&]*--force([^-]|$)"
  "git push[^|;&]* -f( |$)"
  "git push[^|;&]* (origin +)?(master|main)( |$)"
  "git reset --hard"
  "git clean -f"
  "git branch -D"
  "git checkout \."
  "git restore \."
)

for pattern in "${DANGEROUS_PATTERNS[@]}"; do
  if echo "$COMMAND" | grep -qE "$pattern"; then
    echo "BLOCKED: '$COMMAND' matches dangerous pattern '$pattern'. The user has prevented you from doing this." >&2
    exit 2
  fi
done

exit 0
