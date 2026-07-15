"""FastAPI inference server for the automotive interview model.

Endpoints:
    GET  /health              liveness probe
    POST /generate            {"question": "...", "max_new_tokens": 200, "temperature": 0.7}
                              -> {"answer": "...", "tokens": N, "tokens_per_second": X}

Run:
    uvicorn serve:app --host 0.0.0.0 --port 8000
"""

import threading
import time

import torch
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field

from slm.model import TinyGPT
from slm.tokenizer import WordTokenizer

CHECKPOINT = "checkpoints/model.pt"
VOCAB = "checkpoints/vocab.json"

app = FastAPI(title="AutoTech Interview SLM", version="1.0.0")

model = TinyGPT.load(CHECKPOINT)
tok = WordTokenizer.load(VOCAB)
# Single-threaded generation lock: the model is small enough that serialized
# requests still comfortably exceed the throughput target.
_lock = threading.Lock()


class GenerateRequest(BaseModel):
    question: str = Field(min_length=1, max_length=500)
    max_new_tokens: int = Field(default=200, ge=1, le=512)
    temperature: float = Field(default=0.7, gt=0.0, le=2.0)


class GenerateResponse(BaseModel):
    answer: str
    tokens: int
    seconds: float
    tokens_per_second: float


@app.get("/health")
def health() -> dict:
    return {"status": "ok", "model_parameters": model.num_params()}


@app.post("/generate", response_model=GenerateResponse)
def generate(req: GenerateRequest) -> GenerateResponse:
    ids = [tok.q_id] + tok.encode(req.question) + [tok.a_id]
    if len(ids) >= model.cfg.block_size:
        raise HTTPException(status_code=400, detail="question too long for model context")
    idx = torch.tensor([ids], dtype=torch.long)
    with _lock:
        t0 = time.perf_counter()
        out = model.generate(idx, max_new_tokens=req.max_new_tokens,
                             temperature=req.temperature, stop_token=tok.end_id)
        dt = time.perf_counter() - t0
    n = out.size(1) - len(ids)
    return GenerateResponse(
        answer=tok.decode(out[0, len(ids):].tolist()),
        tokens=n,
        seconds=round(dt, 3),
        tokens_per_second=round(n / dt, 1),
    )
