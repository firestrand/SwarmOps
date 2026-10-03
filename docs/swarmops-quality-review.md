# SwarmOps trace and statistics review

## Findings and implemented changes
- **Single responsibility:** concrete trace writers previously closed streams supplied by callers. `Write(TextWriter)` now borrows and leaves the writer open. `WriteToFile` owns its file and uses a disposal scope, including when an overridden `Write` throws.
- **DRY:** mean-fitness and feasibility traces repeated the same accumulator allocation and clearing loops. Both now use the internal `AccumulatorTraceStorage` helper while retaining their protected `Trace` fields and existing inheritance.
- **KISS:** `Quartiles.ComputeUnsorted` clones its input and delegates sorting/computation to `ComputeUnsortedInplace`; the sorting path is maintained in one place.
- **Trace invariants:** nonpositive iteration/interval counts now produce `ArgumentOutOfRangeException` with the relevant parameter name. Samples before the trace offset are ignored by that trace, while chained traces still receive them. Negative indices cannot reach the log implementation.

The public type/member signatures, optimizer formulas, RNG calls, scheduling, trace column names, formatting, statistical update order and quartile convention are unchanged. Writer ownership and invalid-input behavior are intentional contract corrections: callers using `Write(writer)` must dispose their own writer; callers using `WriteToFile` receive a complete, closed file.

## Review performed
Reviewed all changed source, the internal helper, regression fixtures and the baseline-runner integration. Checked the three concrete FitnessTrace subclasses and repository export call sites. Existing examples use `WriteToFile` and retain the same export workflow. No new public hierarchy or dependency was introduced.

The review deliberately retains explicit property delegation in the problem/optimizer wrappers: merging their inheritance would disturb public APIs for little gain. The mutually delegating default overloads on Problem/Optimizer remain a separate extension-contract design concern; existing implementations override the required paths, and changing that contract is outside this focused trace/storage cleanup. This pass does not claim every library design concern is resolved.

The required standards library has no C#/.NET guide; these findings are assessed against the requested SOLID/DRY/KISS principles rather than claimed formal standards compliance.

## ATLAS hardness report
### Edge cases tested
- Baseline-exact trace text for all three trace types over five iteration counts, four interval counts and five runs; clear and reuse included.
- Caller-owned writers remain open on success and injected I/O failure.
- File exports flush completely and release their handle; exceptions from custom trace writers still dispose the owned file writer.
- Zero/negative iteration and interval counts, samples before a nonzero offset, negative/late samples and continued delivery to a child trace.
- Quartile values and input mutation against the baseline for arrays of lengths 0 through 128 and a NaN/infinity/signed-zero fixture.
- Existing sequential/parallel PSO/MOL traces and final results remain covered by the regression runner.

### Tool/service failure handling
The runner terminates on API, output, numeric or lifetime assertion failures and on restore/build failures. Temporary baseline directories and test output files are cleaned on normal and exceptional paths.

### Concurrency / load considerations
The storage helper has no shared mutable state. Trace classes retain their existing lack of synchronization; they should not be concurrently updated without caller coordination. The optimizer comparison includes both deterministic one-worker runs and four-worker runs.

### Security and artifacts
No new packages, credentials or external services. Plans, logs, snapshots and raw measurements stay outside commits. Permanent regression code is in `verification/TraceRegression.cs` and loaded by the existing runner.

### Verification commands and results
- `dotnet build -c Debug --nologo`: passed.
- `dotnet build -c Release --nologo`: passed.
- `dotnet test -c Release --no-build --nologo`: no test projects; this is not counted as correctness evidence.
- `python3 verification/run-ablation.py /tmp/swarmops-quality-results.json`: **662,418** bit-exact optimizer/fitness comparisons and **20,580** trace/output/lifetime/failure/quartile checks passed. Maximum finite numeric difference: **0**.
- Reflection comparison against the pinned baseline: **1,041** public type/member signatures unchanged.
- `git diff --cached --check`: passed before commit.

### Summary
Implemented the focused responsibility, duplication and simplicity improvements with verified output/numerical compatibility. Existing obsolete WebRequest and missing legacy rule-set warnings remain. Performance is not claimed for these maintainability changes.
