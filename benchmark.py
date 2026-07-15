"""Benchmark generation throughput and verify the >= 30 tokens/sec requirement.

Usage:
    python benchmark.py [--runs 5] [--tokens 200] [--target 30]

Exits nonzero if the median throughput is below the target, so this can
gate a deployment (it runs on the Azure VM at provision time).
"""

import argparse
import statistics
import sys
import time

import torch

from slm.model import TinyGPT
from slm.tokenizer import WordTokenizer

QUESTIONS = [
    "How do you diagnose a parasitic battery drain?",
    "What does a P0300 code indicate?",
    "A vehicle cranks but will not start. What do you check?",
    "How do you test a charging system?",
    "What causes brake pedal pulsation?",
]


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--checkpoint", default="checkpoints/model.pt")
    ap.add_argument("--vocab", default="checkpoints/vocab.json")
    ap.add_argument("--runs", type=int, default=5)
    ap.add_argument("--tokens", type=int, default=200)
    ap.add_argument("--target", type=float, default=30.0)
    args = ap.parse_args()

    model = TinyGPT.load(args.checkpoint)
    tok = WordTokenizer.load(args.vocab)

    # Warmup run (first pass pays allocation/JIT costs)
    ids = torch.tensor([[tok.q_id] + tok.encode(QUESTIONS[0]) + [tok.a_id]])
    model.generate(ids, max_new_tokens=32)

    results = []
    for i in range(args.runs):
        q = QUESTIONS[i % len(QUESTIONS)]
        ids = torch.tensor([[tok.q_id] + tok.encode(q) + [tok.a_id]])
        t0 = time.perf_counter()
        out = model.generate(ids, max_new_tokens=args.tokens, stop_token=None)
        dt = time.perf_counter() - t0
        n = out.size(1) - ids.size(1)
        tps = n / dt
        results.append(tps)
        print(f"run {i + 1}: {n} tokens in {dt:.2f}s = {tps:.1f} tokens/sec")

    median = statistics.median(results)
    print(f"\nmedian throughput: {median:.1f} tokens/sec (target: {args.target:.0f})")
    if median >= args.target:
        print("PASS: meets throughput requirement")
    else:
        print("FAIL: below throughput requirement")
        sys.exit(1)


if __name__ == "__main__":
    main()
