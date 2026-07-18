#!/usr/bin/env node
// Migration ledger CLI — the only supported way to read/update migration/ledger.json.
//
//   node tools/ledger.mjs status
//   node tools/ledger.mjs set <unit> <state> [--note "..."] [--force] [--approved-by <name>]
//   node tools/ledger.mjs verify
//   node tools/ledger.mjs check-db-contract <dbObjectName>

import { readFileSync, writeFileSync, existsSync, readdirSync, statSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const ROOT = join(dirname(fileURLToPath(import.meta.url)), "..");
const LEDGER_PATH = join(ROOT, "migration", "ledger.json");

const STATES = [
  "not-started", "analyzed", "api-complete", "ui-complete",
  "parity-passed", "cutover", "retired",
];
const HUMAN_GATED = new Set(["cutover", "retired"]);

function loadLedger() {
  return JSON.parse(readFileSync(LEDGER_PATH, "utf8"));
}

function saveLedger(ledger) {
  ledger.updatedUtc = new Date().toISOString().replace(/\.\d{3}Z$/, "Z");
  writeFileSync(LEDGER_PATH, JSON.stringify(ledger, null, 2) + "\n");
}

function fail(msg) {
  console.error(`ERROR: ${msg}`);
  process.exit(1);
}

function cmdStatus() {
  const ledger = loadLedger();
  const rows = Object.entries(ledger.units).map(([id, u]) => ({
    unit: id,
    status: u.status,
    note: u.note ?? "",
  }));
  const done = rows.filter((r) => r.status === "retired").length;
  console.log(`Migration ledger (updated ${ledger.updatedUtc}) — ${done}/${rows.length} units retired\n`);
  const w1 = Math.max(...rows.map((r) => r.unit.length), 4);
  const w2 = Math.max(...rows.map((r) => r.status.length), 6);
  console.log(`${"UNIT".padEnd(w1)}  ${"STATUS".padEnd(w2)}  NOTE`);
  for (const r of rows) {
    console.log(`${r.unit.padEnd(w1)}  ${r.status.padEnd(w2)}  ${r.note}`);
  }
  const next = rows.find((r) => r.status !== "retired");
  if (next) console.log(`\nNext up: ${next.unit} (${next.status})`);
}

function cmdSet(args) {
  const [unitId, state] = args._;
  if (!unitId || !state) fail("usage: set <unit> <state> [--note ...] [--force] [--approved-by <name>]");
  if (!STATES.includes(state)) fail(`unknown state '${state}'. Valid: ${STATES.join(", ")}`);

  const ledger = loadLedger();
  const unit = ledger.units[unitId];
  if (!unit) fail(`unknown unit '${unitId}'. Units: ${Object.keys(ledger.units).join(", ")}`);

  const from = STATES.indexOf(unit.status);
  const to = STATES.indexOf(state);
  if (to < from && !args.force) {
    fail(`'${unitId}' is '${unit.status}'; moving backwards to '${state}' requires --force.`);
  }
  if (to > from + 1 && !args.force) {
    fail(`'${unitId}' is '${unit.status}'; skipping to '${state}' requires --force (one state at a time).`);
  }
  if (HUMAN_GATED.has(state) && !args["approved-by"]) {
    fail(`state '${state}' is human-gated: pass --approved-by <maintainer> after explicit approval.`);
  }

  unit.status = state;
  if (args.note !== undefined) unit.note = args.note;
  if (args["approved-by"]) unit.approvedBy = args["approved-by"];
  saveLedger(ledger);
  console.log(`${unitId}: -> ${state}${args.note ? ` (${args.note})` : ""}`);
}

function cmdVerify() {
  const ledger = loadLedger();
  const problems = [];
  const procsSql = readFileSync(join(ROOT, "db", "schema", "002_procs.sql"), "utf8");

  for (const [id, u] of Object.entries(ledger.units)) {
    if (!STATES.includes(u.status)) {
      problems.push(`${id}: unknown status '${u.status}'`);
      continue;
    }
    const idx = STATES.indexOf(u.status);
    if (idx >= STATES.indexOf("analyzed") && !existsSync(join(ROOT, u.spec))) {
      problems.push(`${id}: status '${u.status}' requires spec file ${u.spec}`);
    }
    for (const proc of u.sprocs ?? []) {
      if (!procsSql.includes(proc)) {
        problems.push(`${id}: references sproc '${proc}' not found in db/schema/002_procs.sql`);
      }
    }
    for (const f of u.legacy ?? []) {
      const p = join(ROOT, "legacy", "ContosoOrders.Web", f);
      if (u.status !== "retired" && !existsSync(p)) {
        problems.push(`${id}: legacy file missing but unit not retired: ${f}`);
      }
    }
  }

  if (problems.length) {
    console.error("Ledger verification FAILED:");
    for (const p of problems) console.error(`  - ${p}`);
    process.exit(1);
  }
  console.log("Ledger OK.");
}

function grepTree(dir, needle, hits) {
  for (const entry of readdirSync(dir)) {
    if (["node_modules", "bin", "obj", ".next", ".git"].includes(entry)) continue;
    const p = join(dir, entry);
    if (statSync(p).isDirectory()) {
      grepTree(p, needle, hits);
    } else if (readFileSync(p, "latin1").includes(needle)) {
      hits.push(p.slice(ROOT.length + 1));
    }
  }
}

function cmdCheckDbContract(args) {
  const [objectName] = args._;
  if (!objectName) fail("usage: check-db-contract <dbObjectName>");
  const ledger = loadLedger();

  const blockers = Object.entries(ledger.units)
    .filter(([, u]) => u.status !== "retired" && (u.sprocs ?? []).includes(objectName))
    .map(([id, u]) => `${id} (${u.status})`);

  const hits = [];
  for (const dir of ["legacy", "modern"]) grepTree(join(ROOT, dir), objectName, hits);

  console.log(`Object: ${objectName}`);
  console.log(`Non-retired units declaring it: ${blockers.length ? blockers.join(", ") : "none"}`);
  console.log(`Code references:${hits.length ? "\n  " + hits.join("\n  ") : " none"}`);
  if (blockers.length || hits.some((h) => h.startsWith("legacy/"))) {
    console.error("\nNOT SAFE to contract: still referenced by non-retired unit(s) or legacy code.");
    process.exit(1);
  }
  console.log("\nSafe to contract (no non-retired unit and no legacy reference).");
}

function parseArgs(argv) {
  const args = { _: [] };
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === "--force") args.force = true;
    else if (a.startsWith("--")) args[a.slice(2)] = argv[++i];
    else args._.push(a);
  }
  return args;
}

const [cmd, ...rest] = process.argv.slice(2);
const args = parseArgs(rest);
switch (cmd) {
  case "status": cmdStatus(); break;
  case "set": cmdSet(args); break;
  case "verify": cmdVerify(); break;
  case "check-db-contract": cmdCheckDbContract(args); break;
  default:
    console.log("usage: node tools/ledger.mjs <status|set|verify|check-db-contract> ...");
    process.exit(cmd ? 1 : 0);
}
