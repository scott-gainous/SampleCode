#!/usr/bin/env node
// Parity runner: replays golden checks (migration/parity/golden/<unit>.json) against the
// modern API and reports divergences. See migration/parity/README.md.
//
//   node tools/parity/run-parity.mjs <unit|all> [--api http://localhost:5080]

import { readFileSync, writeFileSync, readdirSync, mkdirSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const ROOT = join(dirname(fileURLToPath(import.meta.url)), "..", "..");
const GOLDEN_DIR = join(ROOT, "migration", "parity", "golden");
const RESULTS_DIR = join(ROOT, "migration", "parity", "results");

const argv = process.argv.slice(2);
const target = argv.find((a) => !a.startsWith("--")) ?? "all";
const apiFlag = argv.indexOf("--api");
const API = apiFlag >= 0 ? argv[apiFlag + 1] : "http://localhost:5080";

// Subset match: every field of `expected` must equal `actual`; extra actual fields are
// fine. Arrays: equal length (ordering is legacy behavior), element-wise subset.
function diff(expected, actual, path = "$") {
  const out = [];
  if (Array.isArray(expected)) {
    if (!Array.isArray(actual)) return [`${path}: expected array, got ${typeof actual}`];
    if (expected.length !== actual.length) {
      out.push(`${path}: expected ${expected.length} items, got ${actual.length}`);
    }
    expected.forEach((e, i) => {
      if (i < actual.length) out.push(...diff(e, actual[i], `${path}[${i}]`));
    });
    return out;
  }
  if (expected !== null && typeof expected === "object") {
    if (actual === null || typeof actual !== "object") {
      return [`${path}: expected object, got ${JSON.stringify(actual)}`];
    }
    for (const [k, v] of Object.entries(expected)) {
      out.push(...diff(v, actual[k], `${path}.${k}`));
    }
    return out;
  }
  if (expected !== actual) {
    out.push(`${path}: expected ${JSON.stringify(expected)}, got ${JSON.stringify(actual)}`);
  }
  return out;
}

async function runCheck(check) {
  const { method = "GET", path, body } = check.request;
  let res;
  try {
    res = await fetch(API + path, {
      method,
      headers: body ? { "content-type": "application/json" } : undefined,
      body: body ? JSON.stringify(body) : undefined,
    });
  } catch (err) {
    return { name: check.name, ok: false, divergences: [`request failed: ${err.message}`] };
  }

  const divergences = [];
  if (check.expectStatus !== undefined && res.status !== check.expectStatus) {
    divergences.push(`HTTP status: expected ${check.expectStatus}, got ${res.status}`);
  }
  if (check.expect !== undefined) {
    let json;
    try {
      json = await res.json();
    } catch {
      divergences.push("response was not JSON");
    }
    if (json !== undefined) divergences.push(...diff(check.expect, json));
  }
  return { name: check.name, deliberate: check.deliberate, ok: divergences.length === 0, divergences };
}

async function runUnit(unit) {
  const golden = JSON.parse(readFileSync(join(GOLDEN_DIR, `${unit}.json`), "utf8"));
  const results = [];
  for (const check of golden.checks) {
    results.push(await runCheck(check));
  }
  const failed = results.filter((r) => !r.ok);
  mkdirSync(RESULTS_DIR, { recursive: true });
  writeFileSync(
    join(RESULTS_DIR, `${unit}.json`),
    JSON.stringify({ unit, api: API, ranUtc: new Date().toISOString(), results }, null, 2) + "\n",
  );

  console.log(`\n== ${unit}: ${results.length - failed.length}/${results.length} checks passed`);
  for (const r of results) {
    console.log(`  ${r.ok ? "PASS" : "FAIL"}  ${r.name}${r.deliberate ? "  [deliberate change]" : ""}`);
    for (const d of r.divergences) console.log(`          ${d}`);
  }
  return failed.length === 0;
}

const units =
  target === "all"
    ? readdirSync(GOLDEN_DIR).filter((f) => f.endsWith(".json")).map((f) => f.replace(/\.json$/, ""))
    : [target];

console.log(`Parity run against ${API} — units: ${units.join(", ")}`);
let allOk = true;
for (const unit of units) {
  allOk = (await runUnit(unit)) && allOk;
}
console.log(allOk ? "\nParity: ALL GREEN" : "\nParity: DIVERGENCES FOUND");
process.exit(allOk ? 0 : 1);
