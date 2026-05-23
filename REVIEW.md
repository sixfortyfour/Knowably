# Review Notes

Observations, findings, and decisions recorded during development sessions.

---

## File Size Limit

**Finding:** The 5 MB upload limit in `FileDropZone.vue` and `IngestionOptions.MaxFileSizeMb` was
originally motivated by Upstash Redis's 1 MB per-value limit. The concern was storing raw file
content in Redis temporarily.

**Resolution:** The implementation saves uploaded files to `Path.GetTempPath()`, not Redis. Redis
only stores a short metadata hash (fileName, status, tempFilePath). The Redis limit is therefore
not a constraint on file size.

Limit raised to 20 MB. The practical ceiling is Ollama embedding throughput, not infrastructure
limits.

**Files changed:** `.env.example`, `appsettings.json`, `FileDropZone.vue`

---

## PDF Indexing Performance

### Root cause — serial embedding loop

`OllamaEmbeddingClient.EmbedBatchAsync` called `EmbedAsync` once per chunk in a `foreach` loop.
A 12 MB PDF at 512 words/chunk produces ~400–600 chunks, each requiring a separate HTTP
round-trip to Ollama. This was the dominant bottleneck.

### Fix 1 — true batch embedding

Ollama's `/api/embed` endpoint accepts an array for `input` and returns all embeddings in one
response. `EmbedBatchAsync` was rewritten to send all texts in a single request, eliminating N
sequential round-trips.

### Fix 2 — concurrent vector upserts

`UpstashVectorClient.UpsertAsync` previously sent all records in one HTTP request. For 500+
chunks this produces an oversized payload. Changed to split into batches of 100 and fire them
concurrently via `Task.WhenAll`.

**Known issue:** No concurrency cap on the upsert batches. At current scale (≤5 batches for a
large doc) this is fine. At significantly larger scale it could hit Upstash rate limits. A
`SemaphoreSlim` should be added if documents grow beyond ~10,000 chunks.

### Regression — 503 timeout

The single-giant-batch approach caused a 503: the default `HttpClient` timeout of 100 seconds
elapsed before Ollama finished embedding ~500 chunks in one request.

### Fix 3 — sub-batching with bounded concurrency + extended timeout

Replaced the single batch with concurrent sub-batches of 20 (max 3 in flight via
`SemaphoreSlim`). Each sub-batch completes well within any reasonable timeout. The Ollama
embedding `HttpClient` timeout was also extended to 10 minutes to handle slow hardware.

Ollama is single-threaded internally so high parallelism doesn't help — 3 concurrent requests of
20 chunks keeps it fed without overwhelming it.

**Known issue — `SemaphoreSlim` not disposed.** The semaphore is constructed locally in
`EmbedBatchAsync` and not disposed. `SemaphoreSlim` implements `IDisposable` and holds an
internal `ManualResetEventSlim` once waited on. It will be collected by the GC after the method
returns, but the clean fix is `using var sem = new SemaphoreSlim(...)`.

**Known issue — shared array written concurrently.** `results` is a pre-allocated `float[][]`
written from multiple tasks via `results[batchIndex * subBatchSize + i] = ...`. Each index is
written by exactly one task so there is no actual data race. However the code is non-obvious; a
`ConcurrentDictionary` keyed by chunk index would make the intent clearer (at a minor allocation
cost).

**Files changed:** `OllamaEmbeddingClient.cs`, `UpstashVectorClient.cs`,
`InfrastructureServiceExtensions.cs`

### Expected indexing time (12 MB PDF, ~400 chunks)

| Hardware | Estimate |
|---|---|
| Consumer GPU | 30–90 s |
| CPU only | 3–8 min |

---

## Background Processing for QStash Callbacks

### Problem

QStash retries `/internal/process-document` if the endpoint does not respond within its delivery
window. For large PDFs on slow hardware, the synchronous processing (embed + upsert) exceeds
ngrok's 30-second request timeout, producing `ERR_NGROK_3004`. QStash then retries, potentially
causing duplicate indexing.

### Fix — fire and forget with `IServiceScopeFactory`

Validation (signature verification, payload parse, Redis lookup) runs synchronously so bad
requests are still rejected before any work starts. The processing pipeline
(extract → chunk → embed → upsert) is handed off to `Task.Run` with a new DI scope, and the
controller returns `200` immediately.

`IServiceScopeFactory` is used rather than capturing the request-scoped services directly,
because scoped services (`ITextChunker`, `ITextExtractor`) are disposed when the request ends.

**Files changed:** `InternalController.cs`

### Known issues introduced

**1. QStash retry semantics are lost for transient failures.**
Once the controller returns `200`, QStash considers the message delivered. If the background task
fails (Ollama down, Vector unreachable), QStash will not retry. The document status is written to
`failed` in Redis and visible in the UI, but recovery requires manual re-upload.

**2. Exceptions from the catch block are silently swallowed.**
`ProcessInBackgroundAsync` catches all exceptions and attempts to write `failed` status to Redis.
If Redis is also unavailable, that write throws too, and the exception is fully unobserved — no
log entry, no status update, document stuck in `pending`. Mitigation: wrap the catch block's
Redis write in its own try/catch and log the fallback error.

**3. No logging in the background task.**
The original synchronous path surfaced exceptions through ASP.NET's exception-handling middleware
(`ExceptionHandlingMiddleware`), which logged them. The background task bypasses that middleware
entirely. A failure leaves no server-side trace unless the Redis write in the catch block
succeeds. `ILogger<InternalController>` should be injected and used inside
`ProcessInBackgroundAsync`.

**4. App shutdown can strand in-flight tasks.**
If the API process shuts down while a background task is running, the task is killed and the
document is left in `indexing` state permanently. QStash already delivered successfully and will
not retry. Proper mitigation requires registering a `CancellationToken` from
`IHostApplicationLifetime.ApplicationStopping` and wiring it through to the embed and upsert
calls.

---

## QStash Signature Verification

### Clock skew tolerance

`QStashSignatureVerifier` sets `ClockSkew = 5 minutes` to tolerate minor clock drift and QStash
retry delays. Without this, retried messages can fail signature verification due to the `nbf`/`exp`
claims drifting outside the default zero-tolerance window.

### Body hash check

The JWT `body` claim contains a SHA-256 hash of the raw request body. A mismatch is logged as a
warning but is not treated as a hard rejection. The JWT HMAC signature (signed with the QStash
signing key) is the authoritative security check — a valid HMAC proves the message came from
QStash. The body hash is a secondary integrity check and worth logging but not worth rejecting
over, since minor whitespace or encoding differences can cause false mismatches.
