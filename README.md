# Wopee.io prototype for C# Playwright tests

Example project to show how to integrate Wopee.io with C# Playwright tests for visual testing.

## Setup and Installation

1. Clone the repository.
2. Open the project in Visual Studio or Visual Studio Code.
3. Rename `.env.example` to `.env` and fill in the required values.
4. Run the tests: `dotnet test` or `dotnet test --logger "console;verbosity=detailed"` for more detailed output.

## Folder structure

- `GraphQL` folder to hold GraphQL client (WopeeGraphQLClient.cs).
- `Fixtures` folder to hold MSTest hooks (WopeeTestHooks.cs).
- `Tests` folder for your actual test classes (SampleTests.cs).

Feel free to adjust the structure based on your team’s conventions.

## Files

- `WopeeGraphQLClient.cs` - Purpose: Provide a minimal wrapper to send GraphQL requests to your Wopee.io backend.
- `WopeeTestHooks.cs` - Purpose: Define MSTest hooks (`[AssemblyInitialize]`, `[AssemblyCleanup]`, `[ClassInitialize]`, `[ClassCleanup]`) to manage suite- and scenario-level creation/stopping.
- Example Test Class: `WopeeExampleTests.cs` - Purpose: Show how an MSTest `[TestMethod]` can use `CreateStepAsync`. This can be called anywhere in your test to track a “step” in Wopee.io, linking it to the current scenario.
