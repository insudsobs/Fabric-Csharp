# Contributing to FabricCsharp

Welcome, and thank you for your interest in contributing to FabricCsharp! This guide will help you set up your development environment and understand our contribution workflow.

## Table of Contents

- [Development Environment Setup](#development-environment-setup)
- [Coding Conventions](#coding-conventions)
- [Project Structure](#project-structure)
- [How to Add New C# to Java Type Mappings](#how-to-add-new-c-to-java-type-mappings)
- [How to Add New Analyzer Rules](#how-to-add-new-analyzer-rules)
- [How to Run Tests](#how-to-run-tests)
- [Pull Request Process](#pull-request-process)
- [Issue Reporting Guidelines](#issue-reporting-guidelines)

## Development Environment Setup

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A C# IDE (JetBrains Rider, Visual Studio 2022+, or VS Code with C# Dev Kit)
- Git

### Getting Started

1. Fork the repository on GitHub.
2. Clone your fork locally:

   ```bash
   git clone https://github.com/YOUR_USERNAME/FabricCsharp.git
   cd FabricCsharp
   ```

3. Restore dependencies and build:

   ```bash
   dotnet restore
   dotnet build
   ```

4. Run the tests to verify everything works:

   ```bash
   dotnet test
   ```

5. Create a branch for your changes:

   ```bash
   git checkout -b feature/your-feature-name
   ```

## Coding Conventions

- **File-scoped namespaces**: Always use file-scoped namespace declarations (`namespace Foo;`).
- **XML doc comments**: All public types, methods, and properties must have XML documentation comments (`/// <summary>...</summary>`).
- **Match existing style**: Follow the code style already established in the project. When in doubt, look at surrounding code.
- **Naming**: Use PascalCase for public members and types, camelCase for private fields (prefixed with `_`), local variables, and parameters.
- **Implicit usings**: Rely on the SDK-level implicit usings where configured; add explicit using directives only when necessary.
- **Async naming**: Asynchronous methods should end with the `Async` suffix.
- **Nullable reference types**: The project uses nullable reference types. Always annotate your code appropriately and avoid null-reference warnings.

## Project Structure

```
FabricCsharp/
├── src/
│   ├── FabricCsharp/              # Core transpiler library
│   ├── FabricCsharp.Analyzers/    # Roslyn analyzers and code fixes
│   ├── FabricCsharp.Templates/    # dotnet new fabric-mod template
│   └── FabricCsharp.Tool/         # CLI tool
├── test/
│   └── FabricCsharp.Tests/        # Unit and integration tests
├── samples/
│   └── SapphireEquipment/         # Sample mod: Sapphire Equipment
├── docs/                          # Documentation and API reference
└── .github/                       # CI/CD and governance files
```

## How to Add New C# to Java Type Mappings

Type mappings translate .NET/C# types into their Java/Fabric equivalents. To add a new mapping:

1. Locate the type mapping configuration, typically in `src/FabricCsharp/TypeMapping/` or a similar directory.
2. Add a new entry to the mapping dictionary (or mapping configuration class) with the C# type as the key and the Java type as the value.
3. If the type requires special handling (e.g., generic parameters, namespace imports), implement the corresponding logic in the type resolver.
4. Add a unit test in `test/FabricCsharp.Tests/TypeMapping/` covering the new mapping:

   ```csharp
   [Fact]
   public void Maps_YourNewType_To_ExpectedJavaType()
   {
       var result = Transpile("YourType x = default;");
       Assert.Contains("ExpectedJavaType", result);
   }
   ```

5. Update the API Reference documentation in `docs/` if the type mapping is user-facing.

## How to Add New Analyzer Rules

FabricCsharp uses Roslyn analyzers to detect and report unsupported C# features. To add a new rule:

1. Open `src/FabricCsharp.Analyzers/` and identify the pattern of existing analyzers.
2. Create a new analyzer class implementing `DiagnosticAnalyzer`, decorated with `[DiagnosticAnalyzer(LanguageNames.CSharp)]`.
3. Define your diagnostic descriptor using the `FC###` numbering convention (e.g., `FC011`). Include a descriptive ID, title, message format, and category.
4. Override `Initialize` to register syntax node actions for the syntax kinds you need to inspect.
5. In your analysis callback, detect the unsupported pattern and report a diagnostic.
6. Add tests in `test/FabricCsharp.Tests/Analyzers/` to verify:
   - The diagnostic is raised for the unsupported pattern.
   - The diagnostic is not raised for valid C# that FabricCsharp does support.
7. Update the list of analyzer rules in the README.

## How to Run Tests

```bash
# Run all tests
dotnet test

# Run tests in a specific project
dotnet test test/FabricCsharp.Tests/

# Run with verbose output
dotnet test --verbosity normal

# Run a specific test class or method
dotnet test --filter "FullyQualifiedName~YourTestClassName"
dotnet test --filter "FullyQualifiedName~YourTestMethodName"

# Run tests in Release configuration
dotnet test --configuration Release
```

## Pull Request Process

1. **Fork the repository** (if you haven't already) and create your branch from `main`.
2. **Make your changes** following the coding conventions above.
3. **Add or update tests** to cover your changes.
4. **Ensure all tests pass** (`dotnet test`).
5. **Ensure the build succeeds** (`dotnet build --configuration Release`).
6. **Commit with a clear, descriptive message** (e.g., `Add mapping for List<T> to ArrayList<T>`).
7. **Push your branch** and open a pull request against the `main` branch.
8. **Fill out the PR template** completely, including a description of the changes and any related issues.
9. **Wait for review**: A maintainer will review your PR. Be responsive to feedback and be prepared to make adjustments.
10. **Merge**: Once approved and CI passes, a maintainer will merge your PR.

### PR Checklist

Before submitting your pull request, confirm the following:

- [ ] I have read the contributing guide.
- [ ] My code follows the project's coding conventions.
- [ ] I have added XML doc comments for new public APIs.
- [ ] I have added or updated tests that prove my change works.
- [ ] All new and existing tests pass locally.
- [ ] I have not introduced compiler warnings.

## Issue Reporting Guidelines

### Bug Reports

Use the Bug Report issue template and include:

- A clear, descriptive title.
- Steps to reproduce the issue (a minimal code sample goes a long way).
- Expected behavior vs. actual behavior.
- Your .NET SDK version (`dotnet --version`).
- Your operating system and architecture.

### Feature Requests

Use the Feature Request issue template and include:

- A clear description of the feature.
- The use case or problem it solves.
- Any alternatives or workarounds you've considered.
- Whether you are willing to contribute the implementation.

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.
