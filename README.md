# Observation

Observation is the repository for the Atya.Diagnostics.Observation NuGet package.

| | |
| --- | --- |
| Repository | [https://github.com/AtyaLibraries/Observation](https://github.com/AtyaLibraries/Observation) |
| NuGet | Atya.Diagnostics.Observation |
| License | MIT |

Thin diagnostics composition package over logging, tracing, and metrics.

## Layout

```text
.
|-- src/Observation/
|-- tests/Observation.UnitTests/
|-- samples/Observation.Samples.Console/
|-- benchmarks/Observation.Benchmarks/
|-- build/
\-- .github/
```

## Build and test

```bash
./build/build.ps1 -Configuration Release
./build/pack.ps1 -Configuration Release
```

Artifacts land in artifacts/packages/.

## Consumer guidance

Package-specific usage guidance lives in src/Observation/README.md.

## Release

CI runs restore, vulnerability audit, format verification, Release build, tests with coverage, package validation, and artifact upload.
Publishing to NuGet is handled by `.github/workflows/publish-nuget.yml`.
