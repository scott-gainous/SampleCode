# legacy/ — READ ONLY

This tree is the system being migrated. It is **evidence of behavior**, not a workspace.

- Do not edit, reformat, or "improve" anything here. The only permitted change is
  deleting/stubbing a unit's files during its human-approved *retire* step.
- Requires Windows + .NET Framework 3.5 to build/run; on this machine you can only read.
- When you find a bug here, do NOT fix it. Record it as a quirk in the unit's spec
  (`migration/specs/<unit>.md`) with a preserve/fix decision.
- Business rules hide in code-behind event handlers and the DAL (e.g. the order status
  state machine in `Code/Dal/OrderDal.cs`) — read `.aspx` markup AND `.aspx.cs` AND the
  DAL for every page you analyze; validators in markup are part of the behavior.
