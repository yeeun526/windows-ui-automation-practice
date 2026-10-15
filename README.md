# Windows UI Automation Practice

A small WPF counter app and an NUnit/FlaUI UI automation test project for learning Windows desktop testing.

## Requirements

- Windows
- .NET 10 SDK

## Run the sample app

From this directory:

```powershell
dotnet run --project CounterApp\CounterApp.csproj
```

The app displays a counter. Select **숫자 올리기** to increment it.

## Run the UI automation tests

```powershell
dotnet test CounterApp.UIItests\CounterApp.UIItests.csproj
```

The tests launch the WPF app, find its controls by their automation IDs, and verify the displayed count for multiple click counts.

UI automation tests need an interactive Windows desktop session.
