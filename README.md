# AutoTech-SLM

A small language model (SLM), built from scratch in Python/PyTorch, that answers
automotive technician interview questions. It trains in a few minutes on CPU,
deploys to an Azure virtual machine with one script, and generates well over the
required **30 tokens per second** on a modest 4-vCPU VM.

## What's in the box

| Path | Purpose |
|---|---|
| `data/interview_qa.jsonl` | Training data: 118 automotive technician interview Q/A pairs (diagnostics, brakes, electrical, HVAC, ASE, behavioral) |
| `slm/model.py` | ~4M parameter decoder-only transformer (GPT-style, 4 layers, 256-dim) |
| `slm/tokenizer.py` | Word-level tokenizer trained on the corpus |
| `train.py` | Training script (saves `checkpoints/model.pt` + `checkpoints/vocab.json`) |
| `generate.py` | CLI generation with tokens/sec reporting |
| `serve.py` | FastAPI inference server (`/health`, `/generate`) |
| `benchmark.py` | Throughput gate — fails if median generation is below 30 tokens/sec |
| `azure/deploy.sh` | Creates the Azure VM and deploys everything via cloud-init |
| `azure/cloud-init.yaml` | VM provisioning: installs deps, trains, benchmarks, starts systemd service |

## Quick start (local)

```bash
pip install -r requirements.txt

# Train (~3-5 minutes on 4 CPU cores)
python train.py

# Verify the 30 tokens/sec requirement
python benchmark.py

# Ask a question
python generate.py --question "How do you diagnose a parasitic battery drain?"

# Or run the API
uvicorn serve:app --host 0.0.0.0 --port 8000
```

## Deploy to Azure

Requires the [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli),
logged in with `az login`.

```bash
./azure/deploy.sh                      # defaults: autotech-slm-rg / autotech-slm-vm / eastus
./azure/deploy.sh my-rg my-vm westus2  # or choose your own
```

The script creates a **Standard_D4s_v5** (4 vCPU / 16 GiB, ~$0.19/hr) Ubuntu 22.04
VM. Cloud-init then, on the VM itself:

1. installs Python dependencies (CPU-only PyTorch),
2. clones this repository and trains the model (~5 minutes),
3. runs `benchmark.py --target 30` as a deployment gate and logs the result to
   `/var/log/slm-benchmark.log`,
4. starts the API as a systemd service (`autotech-slm`) on port 8000.

If the repository is private, set `REPO_URL` to a URL the VM can clone (e.g. one
with a token) before running the script: `REPO_URL=https://<token>@github.com/... ./azure/deploy.sh`

End-to-end this takes about 10–15 minutes. The script prints the public IP plus
ready-made commands to watch setup, test the API, and tear everything down.

### Test the deployed API

```bash
curl http://<PUBLIC_IP>:8000/health

curl -X POST http://<PUBLIC_IP>:8000/generate \
  -H 'Content-Type: application/json' \
  -d '{"question": "What does a P0300 code indicate?"}'
```

The response includes the measured throughput for every request:

```json
{
  "answer": "A P0300 code indicates random or multiple cylinder misfires...",
  "tokens": 78,
  "seconds": 0.61,
  "tokens_per_second": 127.9
}
```

## Performance

The model is deliberately small (4 transformer layers, 256 embedding dim,
~2k-word vocabulary ⇒ ~4M parameters) so that CPU inference is fast:

- The 30 tokens/sec target is enforced by `benchmark.py`, which runs at provision
  time on the VM — deployment logs a FAIL if the box can't keep up.
- On a 4-vCPU machine, measured throughput is typically **100+ tokens/sec**,
  a comfortable 3× margin over the requirement.

## Extending the training data

`data/interview_qa.jsonl` is one JSON object per line:

```json
{"question": "How do you test a battery?", "answer": "I first check open circuit voltage..."}
```

Add more pairs and re-run `python train.py`. The tokenizer vocabulary is rebuilt
from the data automatically on every training run.
