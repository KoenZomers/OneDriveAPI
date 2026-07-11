# Copilot Instructions

## Project Guidelines
- On Windows GitHub Actions runners, `dotnet nuget push` fails with a glob path that uses forward slashes (e.g. ./nupkg/*.nupkg), reporting 'File does not exist', even if the file exists. Use backslashes (.\\nupkg\\*.nupkg) instead.