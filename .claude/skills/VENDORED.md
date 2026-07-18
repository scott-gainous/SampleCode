# Vendored skills — provenance

The engineering/productivity skills in this directory are vendored from
**[mattpocock/skills](https://github.com/mattpocock/skills)** (MIT license — see
`LICENSE-mattpocock-skills`), pinned at commit `9603c1cc8118d08bc1b3bf34cf714f62178dea3b`
(vendored 2026-07-18). Categories taken: `engineering/`, `productivity/`, plus
`misc/git-guardrails-claude-code`. Not taken: `personal/`, `deprecated/`, `in-progress/`,
course-specific skills.

To update: re-clone the repo and re-copy the same categories, or switch to the managed
plugin (`/plugin marketplace add mattpocock/skills` → `/plugin install
mattpocock-skills@mattpocock`) and delete the vendored copies.

**First-time setup:** run `/setup-matt-pocock-skills` once in this repo — it configures
the issue tracker, triage labels, and domain-doc locations the engineering skills assume.

Repo-native skills (not from that repo): `migration-status`, `analyze-legacy`,
`migrate-page`, `parity-check` — see `migration/STRATEGY.md`.

Note: the vendored `code-review` skill shadows Claude Code's built-in `/code-review` in
this repo; both review diffs, the vendored one follows Matt Pocock's process.
