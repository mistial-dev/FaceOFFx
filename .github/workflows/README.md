# GitHub Actions Workflows

This directory contains the CI/CD workflows for FaceOFFx.

## Workflows

### CI Build (`ci.yml`)
- **Triggers**: Push to main/develop branches, PRs to main
- **Purpose**: Build and test on multiple platforms (Ubuntu, Windows, macOS)
- **Actions**:
  - Build solution in Release mode
  - Run all tests
  - Pack NuGet packages (Ubuntu only)
  - Test tool installation (Ubuntu only)

### Publish NuGet (`publish.yml`)
- **Triggers**: Push of version tags (v*), manual dispatch
- **Purpose**: Publish packages to NuGet.org
- **Actions**:
  - Build and test
  - Pack all projects
  - Upload artifacts
  - Publish to NuGet.org (requires NUGET_API_KEY secret)

### Create Release (`release.yml`)
- **Triggers**: Push of version tags (v*)
- **Purpose**: Create GitHub releases with packages
- **Actions**:
  - Build and pack all projects
  - Generate SBOM (Software Bill of Materials)
  - Create GitHub release with artifacts

## Required Secrets

- `NUGET_API_KEY`: API key for publishing to NuGet.org
  - Get from: <https://www.nuget.org/account/apikeys>
  - Set in: Settings → Secrets and variables → Actions

## Versioning

Tag format: `v1.0.0`

To create a new release:
```bash
git tag v1.0.0
git push origin v1.0.0
```

This will trigger both the publish and release workflows.