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

The test fixture launches one WPF app window for all test cases, finds its controls by their automation IDs, and verifies the displayed count for multiple click counts. After each case, it uses the reset button to return the counter to zero before the next case.

UI automation tests need an interactive Windows desktop session.
