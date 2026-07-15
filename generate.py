"""Generate an interview answer from the trained model and report tokens/sec.

Usage:
    python generate.py --question "How do you diagnose a parasitic battery drain?"
    python generate.py            # interactive prompt loop
"""

import argparse
import time

import torch

from slm.model import TinyGPT
from slm.tokenizer import WordTokenizer


def answer(model: TinyGPT, tok: WordTokenizer, question: str,
           max_new_tokens: int = 200, temperature: float = 0.7) -> tuple[str, float, int]:
    ids = [tok.q_id] + tok.encode(question) + [tok.a_id]
    idx = torch.tensor([ids], dtype=torch.long)
    t0 = time.perf_counter()
    out = model.generate(idx, max_new_tokens=max_new_tokens,
                         temperature=temperature, stop_token=tok.end_id)
    dt = time.perf_counter() - t0
    new_tokens = out.size(1) - len(ids)
    text = tok.decode(out[0, len(ids):].tolist())
    return text, new_tokens / dt, new_tokens


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--checkpoint", default="checkpoints/model.pt")
    ap.add_argument("--vocab", default="checkpoints/vocab.json")
    ap.add_argument("--question", default=None)
    ap.add_argument("--max-new-tokens", type=int, default=200)
    ap.add_argument("--temperature", type=float, default=0.7)
    args = ap.parse_args()

    torch.set_num_threads(torch.get_num_threads())
    model = TinyGPT.load(args.checkpoint)
    tok = WordTokenizer.load(args.vocab)

    def run(q: str) -> None:
        text, tps, n = answer(model, tok, q, args.max_new_tokens, args.temperature)
        print(f"\nQ: {q}\nA: {text}\n")
        print(f"[{n} tokens in {n / tps:.2f}s = {tps:.1f} tokens/sec]")

    if args.question:
        run(args.question)
    else:
        print("AutoTech interview model - type a question (Ctrl-D to exit)")
        while True:
            try:
                q = input("\nquestion> ").strip()
            except EOFError:
                break
            if q:
                run(q)


if __name__ == "__main__":
    main()
