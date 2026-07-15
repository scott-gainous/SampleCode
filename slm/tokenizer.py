"""Word-level tokenizer for the automotive interview corpus.

The corpus is small and domain-specific, so a word-level vocabulary
(~2-3k tokens) keeps the embedding table tiny and generation fast on CPU.
"""

import json
import re
from pathlib import Path

# Special tokens
PAD = "<pad>"
UNK = "<unk>"
Q = "<q>"    # marks the start of an interview question
A = "<a>"    # marks the start of the answer
END = "<end>"  # marks the end of a Q/A pair

SPECIAL_TOKENS = [PAD, UNK, Q, A, END]

_WORD_RE = re.compile(r"[a-z0-9]+|[^\sa-z0-9]")


def _split(text: str) -> list[str]:
    return _WORD_RE.findall(text.lower())


class WordTokenizer:
    def __init__(self, vocab: dict[str, int]):
        self.vocab = vocab
        self.inv_vocab = {i: t for t, i in vocab.items()}
        self.pad_id = vocab[PAD]
        self.unk_id = vocab[UNK]
        self.q_id = vocab[Q]
        self.a_id = vocab[A]
        self.end_id = vocab[END]

    @property
    def vocab_size(self) -> int:
        return len(self.vocab)

    @classmethod
    def train(cls, texts: list[str], min_freq: int = 1) -> "WordTokenizer":
        counts: dict[str, int] = {}
        for text in texts:
            for tok in _split(text):
                counts[tok] = counts.get(tok, 0) + 1
        vocab = {t: i for i, t in enumerate(SPECIAL_TOKENS)}
        for tok in sorted(counts, key=lambda t: (-counts[t], t)):
            if counts[tok] >= min_freq:
                vocab[tok] = len(vocab)
        return cls(vocab)

    def encode(self, text: str) -> list[int]:
        return [self.vocab.get(t, self.unk_id) for t in _split(text)]

    def decode(self, ids: list[int]) -> str:
        words = [self.inv_vocab.get(i, UNK) for i in ids]
        words = [w for w in words if w not in (PAD, Q, A, END)]
        out = ""
        for w in words:
            if out and re.match(r"[a-z0-9]", w):
                out += " "
            elif out and w in "([{":
                out += " "
            out += w
        # Capitalize sentence starts for readability.
        out = re.sub(r"(^|[.!?]\s+)([a-z])", lambda m: m.group(1) + m.group(2).upper(), out)
        return out

    def save(self, path: str | Path) -> None:
        Path(path).write_text(json.dumps(self.vocab, indent=0))

    @classmethod
    def load(cls, path: str | Path) -> "WordTokenizer":
        return cls(json.loads(Path(path).read_text()))
