# Copilot Instructions

## Project Guidelines
- On Windows GitHub Actions runners, `dotnet nuget push` fails with a glob path that uses forward slashes (e.g. ./nupkg/*.nupkg), reporting 'File does not exist', even if the file exists. Use backslashes (.\\nupkg\\*.nupkg) instead.
- When asked to change the version, treat it as a new release: update `Api\\API.csproj`, add a new top-level section to `CHANGELOG.md`, preserve previous release entries, compare the current working-tree changes against committed code, and write detailed release notes that describe the actual code/API/behavior/documentation changes rather than only noting the version bump.
