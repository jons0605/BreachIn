# Contributing to BreachIn

Thank you for helping improve BreachIn. Contributions from developers of all experience levels are welcome.

## Before You Start

For bug fixes and small improvements, feel free to open a pull request. Please discuss major features or architectural changes in a GitHub Issue before implementation so that effort can be coordinated.

Security vulnerabilities must not be reported publicly. Follow the private reporting guidance in [SECURITY.md](SECURITY.md).

## Contribution Workflow

1. **Fork the repository**

   Select **Fork** on GitHub to create a copy under your account, then clone your fork:

   ```bash
   git clone <your-fork-url>
   cd BreachIn
   ```

2. **Create a feature branch**

   Create a short, descriptive branch from the latest default branch:

   ```bash
   git checkout -b feature/short-description
   ```

3. **Run the application locally**

   Install the .NET 10 SDK, then run:

   ```bash
   dotnet restore
   dotnet run --project BreachIn/BreachIn.csproj
   ```

   Open the local address shown in the terminal.

4. **Make focused changes**

   Keep each contribution limited to one bug fix or feature. Follow the existing project style, prefer simple and readable code, and update documentation or tests when appropriate.

5. **Test your changes**

   Contributors must test their changes before submitting a pull request. At minimum, build the solution and manually verify the affected functionality:

   ```bash
   dotnet build BreachIn.slnx
   ```

6. **Commit your changes**

   Use a clear commit message that describes the change:

   ```bash
   git add <changed-files>
   git commit -m "Add opportunity search validation"
   ```

7. **Push and open a pull request**

   Push your branch to your fork:

   ```bash
   git push origin feature/short-description
   ```

   Open a pull request on GitHub. Explain what changed, why it is needed, and how you tested it. Link any related issues.

## Protect Sensitive Information

Never commit:

- API keys
- Passwords
- Secrets or access tokens
- Connection strings containing credentials
- Personal CV data
- Personal information

Use safe local configuration and sample data instead. Review your changes carefully before committing.

## License

By contributing to BreachIn, you agree that your contributions will be licensed under the project's [MIT License](LICENSE).
