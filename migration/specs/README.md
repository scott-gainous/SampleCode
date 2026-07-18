# Behavior specs

One spec per migration unit, written during `/analyze-legacy`, **before** modern code.
The spec is the authority the modern implementation is built against; legacy code is the
evidence it cites.

Template:

```markdown
# Unit: <id>

## Sources (legacy evidence)
<files read, with line references>

## Behavior
<numbered, testable statements of what the unit does>

## Quirks
| # | Quirk | Decision | Rationale |
|---|-------|----------|-----------|
|   |       | preserve / fix |     |

## Deliberate changes
<each "fix" decision, its new intended behavior, and the golden-fixture checks that assert it>

## Data contract
<tables/sprocs touched, and in what mode (read/write)>

## Out of scope
```
