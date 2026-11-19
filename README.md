# AspTwitter
An ASP.NET Core API and Blazor WebAssembly Twitter clone, originally completed in May 2021 and hosted on Heroku. The restored application runs locally on .NET 11 with SQLite.

## Run locally

Install the .NET SDK specified in `global.json` (11.0.100 or a later 11.0.1xx patch), then run from the repository root:

```powershell
dotnet restore AspTwitter.slnx
dotnet run --project AspTwitter.Api --launch-profile http
```

Open http://localhost:5055, choose **Sign up**, and register an account. Email is optional. The home page lets a signed-in user post the first tweet even when the database is empty.

The API serves the frontend at the same address; no separate frontend process or PostgreSQL installation is needed. SQLite creates `AspTwitter.Api/AppData/asptwitter.db` automatically. Accounts, tweets, and uploaded avatars survive restarts. The database is excluded from Git.

`ConnectionStrings:DefaultConnection` controls the database location. Relative paths are resolved against the API project directory. This restoration uses `EnsureCreated` for a fresh schema; it does not import the old Heroku database or apply schema upgrades to an existing SQLite file.

## Projects

- `AspTwitter.Api`: original controllers, authentication, and EF models; hosts the client and SQLite database.
- `AspTwitter.Frontend/AspTwitter.Frontend.Client`: restored original WebAssembly UI, including its original `Frontend` namespaces and styling.
- `AspTwitter.Tests`: original integration scenarios plus restoration regression tests, using isolated SQLite databases in memory.
- `Frontend`: untouched 2021 source retained as a reference. Edit the restored client for active development.
- `AspTwitter.Frontend/AspTwitter.Frontend`: unused server template, excluded from the solution. Start the API project instead.

Build and run the tests:

```powershell
dotnet build AspTwitter.slnx
dotnet test --project AspTwitter.Tests/AspTwitter.Tests.csproj
```

Stop and restart the API after rebuilding the client so the host and its fingerprinted static-asset manifest stay in sync.

## Configuration

The HTTP development launch profile uses a public, development-only JWT key from `appsettings.Development.json`. For a non-development environment, supply a private `JWT__Secret` of at least 32 bytes and configure HTTPS at the host or reverse proxy. The app validates the signing key at startup.

The reserved admin account is seeded with ID 1. The original `/admin/login` flow lets you set its initial password. The current milestone verifies ordinary registration, login, and posting; the remaining UI flows still need a browser verification pass.

## Bundled frontend assets

Bootstrap 4.6.0, jQuery 3.5.1, and Open Iconic 1.1.1 are checked in under the client's `wwwroot` so the app does not depend on a CDN at runtime. See `THIRD-PARTY-NOTICES.md` for their licenses and sources.
