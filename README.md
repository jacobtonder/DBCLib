# DBCLib

[![.NET 10.0](https://github.com/jacobtonder/DBCLib/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/jacobtonder/DBCLib/actions/workflows/dotnetcore.yml)
[![Publish NuGet](https://github.com/jacobtonder/DBCLib/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/jacobtonder/DBCLib/actions/workflows/nuget-publish.yml)
[![NuGet Version](https://img.shields.io/nuget/v/DBCLib.svg)](https://www.nuget.org/packages/DBCLib)

DBCLib is a .NET library for reading and writing Blizzard DBC files (for example files with `WDBC` signature).

## Features

- Load DBC files into strongly typed records.
- Add, remove, and replace records.
- Save modified records back to disk.
- Read string tables and localized string fields.

## NuGet

Package: https://www.nuget.org/packages/DBCLib

Install:

```powershell
dotnet add package DBCLib
```

## Quick Start

```csharp
using DBCLib;

// Replace with your own record type matching the DBC layout.
var dbc = new DBCFile<CharTitlesEntry>("CharTitles.dbc", "WDBC");

dbc.Load();

// Add or update entries.
dbc.AddEntry(9999, new CharTitlesEntry
{
	Id = 9999,
	ConditionId = 0,
	NameMale = "My Title %s",
	NameFemale = "My Title %s",
	TitleMaskId = 1
});

dbc.Save();
```

For a larger UI example, see [DBC Editor](https://github.com/jacobtonder/DBCEditorExample/).

## Target Framework

This project currently targets .NET 10 (`net10.0`).

## Development

Build and test from repository root:

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

## Release and Publishing

NuGet publishing is handled by GitHub Actions using trusted publishing (OIDC) in:

- `.github/workflows/nuget-publish.yml`

The publish workflow can be triggered by:

- Pushing a tag such as `v2.0.0`
- Manual workflow run with a version input

## License

MIT. See [LICENSE](LICENSE).
