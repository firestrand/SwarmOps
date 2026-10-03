# ATLAS Hardness Report: .NET 10 modernization

## Edge cases tested
SDK-style conversion preserves existing assembly metadata, project references, and source inclusion. Legacy x86 solution mappings now select Any CPU for portable execution.
Existing objective-function checks and offline convergence tests ran where present.

## Tool/service failure handling
Restore and compilation completed using SDK 10.0.401. Missing C# standards source was reported; official Microsoft guidance grounds framework and MSTest configuration.

## Concurrency / load considerations
No algorithm or RNG implementation changed in this migration.

## Security
No secrets added. Stable SDK pinned; obsolete built-in framework package references removed in SwarmOps.

## Verification commands + results
- `dotnet build -c Debug --nologo`: passed.
- `dotnet build -c Release --nologo`: passed.
- `dotnet test -c Release --no-build --nologo`: no test projects; benchmark examples compile but this command provides no correctness evidence.

## Summary
Framework migration builds successfully. Performance and numerical comparison evidence belongs to the subsequent optimization pass.

Remaining compiler warnings are recorded here for review:

- /home/firestrand/.dotnet/sdk/10.0.401/Microsoft.CSharp.CurrentVersion.targets(130,9): warning MSB3884: Could not find rule set file "AllRules.ruleset".
- /home/firestrand/.dotnet/sdk/10.0.401/Microsoft.CSharp.CurrentVersion.targets(130,9): warning MSB3884: Could not find rule set file "MinimumRecommendedRules.ruleset".
- /home/firestrand/Projects/particle-swarm/SwarmOps/RandomOps/HtmlDownload.cs(26,37): warning SYSLIB0014: 'WebRequest.Create(string)' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.' (https://aka.ms/dotnet-warnings/SYSLIB0014)
