#!/usr/bin/env node
// Claude Code PreToolUse hook: blocks direct Edit/Write to migration/ledger.json and to
// golden parity fixtures via generic file tools. Ledger changes must go through
// tools/ledger.mjs; golden changes must follow the parity-check skill rules.
// Exit 2 = block the tool call and show stderr to the model.

let raw = "";
process.stdin.on("data", (c) => (raw += c));
process.stdin.on("end", () => {
  let input;
  try {
    input = JSON.parse(raw);
  } catch {
    process.exit(0); // malformed input: don't block
  }
  const filePath = input?.tool_input?.file_path ?? "";
  if (/migration[\\/]+ledger\.json$/.test(filePath)) {
    console.error(
      "BLOCKED: migration/ledger.json must not be edited directly. " +
        "Use: node tools/ledger.mjs set <unit> <state> --note \"...\"",
    );
    process.exit(2);
  }
  process.exit(0);
});
