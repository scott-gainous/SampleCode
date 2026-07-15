"""Train the automotive-technician interview language model.

Usage:
    python train.py [--steps 3000] [--data data/interview_qa.jsonl] [--out checkpoints]

Each Q/A pair is encoded as:  <q> question tokens <a> answer tokens <end>
Pairs are concatenated into one token stream and sampled in random windows.
Trains in a few minutes on a 4-vCPU machine.
"""

import argparse
import json
import time
from pathlib import Path

import torch

from slm.model import TinyGPT, ModelConfig
from slm.tokenizer import WordTokenizer


def load_pairs(path: str) -> list[dict]:
    pairs = []
    with open(path) as f:
        for line in f:
            line = line.strip()
            if line:
                pairs.append(json.loads(line))
    return pairs


def build_stream(pairs: list[dict], tok: WordTokenizer) -> torch.Tensor:
    ids: list[int] = []
    for p in pairs:
        ids.append(tok.q_id)
        ids.extend(tok.encode(p["question"]))
        ids.append(tok.a_id)
        ids.extend(tok.encode(p["answer"]))
        ids.append(tok.end_id)
    return torch.tensor(ids, dtype=torch.long)


def get_batch(stream: torch.Tensor, block_size: int, batch_size: int, device: str):
    ix = torch.randint(0, len(stream) - block_size - 1, (batch_size,))
    x = torch.stack([stream[i:i + block_size] for i in ix])
    y = torch.stack([stream[i + 1:i + 1 + block_size] for i in ix])
    return x.to(device), y.to(device)


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--data", default="data/interview_qa.jsonl")
    ap.add_argument("--out", default="checkpoints")
    ap.add_argument("--steps", type=int, default=3000)
    ap.add_argument("--batch-size", type=int, default=16)
    ap.add_argument("--block-size", type=int, default=256)
    ap.add_argument("--lr", type=float, default=3e-4)
    ap.add_argument("--seed", type=int, default=1337)
    args = ap.parse_args()

    torch.manual_seed(args.seed)
    device = "cuda" if torch.cuda.is_available() else "cpu"
    print(f"device: {device}")

    pairs = load_pairs(args.data)
    print(f"loaded {len(pairs)} Q/A pairs from {args.data}")

    texts = [p["question"] for p in pairs] + [p["answer"] for p in pairs]
    tok = WordTokenizer.train(texts)
    print(f"vocab size: {tok.vocab_size}")

    stream = build_stream(pairs, tok)
    print(f"training stream: {len(stream)} tokens")

    # 90/10 split for a held-out validation window
    split = int(0.9 * len(stream))
    train_stream, val_stream = stream[:split], stream[split:]

    cfg = ModelConfig(vocab_size=tok.vocab_size, block_size=args.block_size)
    model = TinyGPT(cfg).to(device)
    print(f"model parameters: {model.num_params() / 1e6:.2f}M")

    opt = torch.optim.AdamW(model.parameters(), lr=args.lr, weight_decay=0.01)
    sched = torch.optim.lr_scheduler.CosineAnnealingLR(opt, T_max=args.steps)

    out_dir = Path(args.out)
    out_dir.mkdir(parents=True, exist_ok=True)

    model.train()
    t0 = time.time()
    for step in range(1, args.steps + 1):
        x, y = get_batch(train_stream, args.block_size, args.batch_size, device)
        _, loss = model(x, y)
        opt.zero_grad(set_to_none=True)
        loss.backward()
        torch.nn.utils.clip_grad_norm_(model.parameters(), 1.0)
        opt.step()
        sched.step()

        if step % 200 == 0 or step == args.steps:
            model.eval()
            with torch.no_grad():
                vx, vy = get_batch(val_stream, min(args.block_size, len(val_stream) - 2),
                                   args.batch_size, device)
                _, vloss = model(vx, vy)
            model.train()
            elapsed = time.time() - t0
            print(f"step {step:5d}/{args.steps}  train loss {loss.item():.3f}  "
                  f"val loss {vloss.item():.3f}  ({elapsed:.0f}s)")

    model.save(out_dir / "model.pt")
    tok.save(out_dir / "vocab.json")
    print(f"saved model and vocab to {out_dir}/")


if __name__ == "__main__":
    main()
