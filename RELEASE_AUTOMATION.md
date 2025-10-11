# SimpleHooks Release Automation

This Python script automates the complete release process for SimpleHooks as outlined in `docs/simple-hooks-release-steps.md`.

## Features

- ✅ **Version Management**: Updates version numbers in all project files and README
- ✅ **.NET Publishing**: Publishes all projects for win-x64 platform
- ✅ **GitHub Releases**: Creates GitHub releases with compressed artifacts
- ✅ **Docker Operations**: Builds and pushes Docker images with proper tagging
- ✅ **Selective Execution**: Run specific steps or the complete process
- ✅ **Error Handling**: Comprehensive error handling and logging

## Prerequisites

1. **Python 3.7+** - The script uses standard library modules only
2. **.NET 8.0 SDK** - Required for publishing projects
3. **Docker** (optional) - For Docker image operations
4. **GitHub CLI** (optional) - For automated GitHub release creation
5. **Git** - For version control operations

## Installation

No additional Python packages required. The script uses only standard library modules.

```bash
# Optional: Install GitHub CLI for automated releases
# Windows: winget install GitHub.cli
# macOS: brew install gh
# Linux: See https://cli.github.com/

# Optional: Install Docker
# See https://docker.com/get-started
```

## Usage

### Full Release Process

Execute all release steps automatically:

```bash
python release_automation.py 2.8.3
```

### With GitHub Token (for automated releases)

```bash
python release_automation.py 2.8.3 --github-token YOUR_GITHUB_TOKEN
```

### Selective Step Execution

Run only specific steps:

```bash
# Update versions only
python release_automation.py 2.8.3 --steps version assemblies

# Publish projects only
python release_automation.py 2.8.3 --steps publish

# Docker operations only
python release_automation.py 2.8.3 --steps docker

# GitHub release only
python release_automation.py 2.8.3 --steps github --github-token YOUR_TOKEN
```

### Available Steps

- `version` - Update version in README.md
- `assemblies` - Update AssemblyVersion and FileVersion in .csproj files
- `readme` - Update README (included in version step)
- `publish` - Publish all projects for win-x64
- `github` - Create GitHub release with compressed artifacts
- `docker` - Build and push Docker images

### Dry Run

See what would be executed without making changes:

```bash
python release_automation.py 2.8.3 --dry-run
```

## Configuration

### Docker Registry

By default, images are pushed to `gnairooze/simple-hooks`. To use a different registry:

```bash
python release_automation.py 2.8.3 --docker-registry your-username
```

### GitHub Token

For automated GitHub releases, you need a GitHub personal access token with `repo` permissions:

1. Go to GitHub Settings > Developer settings > Personal access tokens
2. Generate a new token with `repo` scope
3. Use it with `--github-token` parameter or set as environment variable

## Output Structure

The script creates the following directory structure:

```
project-root/
├── publish/                    # Published .NET applications
│   ├── SimpleHooks.Web/
│   ├── SimpleHooks.AuthApi/
│   ├── SimpleHooks.Server/
│   └── SimpleHooks.Assist/
└── release/                    # Compressed release artifacts
    ├── SimpleHooks.Web-2.8.3.zip
    ├── SimpleHooks.AuthApi-2.8.3.zip
    ├── SimpleHooks.Server-2.8.3.zip
    └── SimpleHooks.Assist-2.8.3.zip
```

## Docker Images

The script builds and pushes Docker images with the following tags:

- `gnairooze/simple-hooks:web-2.8.3` and `gnairooze/simple-hooks:web-latest`
- `gnairooze/simple-hooks:authapi-2.8.3` and `gnairooze/simple-hooks:authapi-latest`
- `gnairooze/simple-hooks:proc-2.8.3` and `gnairooze/simple-hooks:proc-latest`

## Error Handling

The script includes comprehensive error handling:

- **Command failures**: Detailed error messages with stdout/stderr
- **Missing files**: Warnings for missing Dockerfiles or project files
- **Docker issues**: Helpful messages about Docker daemon and authentication
- **GitHub CLI**: Instructions for installation and authentication

## Troubleshooting

### Common Issues

1. **"dotnet command not found"**
   - Install .NET 8.0 SDK from https://dotnet.microsoft.com/

2. **"docker command not found"**
   - Install Docker from https://docker.com/
   - Make sure Docker daemon is running

3. **"gh command not found"**
   - Install GitHub CLI from https://cli.github.com/
   - Authenticate with `gh auth login`

4. **Docker push permission denied**
   - Login to Docker registry: `docker login`
   - Ensure you have push permissions to the registry

5. **GitHub release creation failed**
   - Ensure GitHub CLI is authenticated: `gh auth status`
   - Check if release tag already exists: `gh release list`

### Manual Fallback

If automated GitHub release creation fails, the script still creates compressed files in the `release/` directory. You can manually:

1. Create a new release on GitHub
2. Upload the compressed files from the `release/` directory

## Script Mapping to Original Steps

The script maps to the original 28 release steps as follows:

| Original Steps | Script Function | Description |
|---------------|----------------|-------------|
| 1 | `step_1_set_new_version()` | Set new version |
| 2-5 | `step_2_to_5_update_assemblies()` | Update assembly versions |
| 6 | `step_6_update_readme()` | Update README (done in step 1) |
| 7-10 | `step_7_to_10_publish_projects()` | Publish all projects |
| 11-18 | `step_11_to_18_create_github_release()` | Create release & upload files |
| 19-28 | `step_19_to_30_docker_operations()` | Docker build, push, and tagging |

## Contributing

To extend the script:

1. Add new projects to the `self.projects` dictionary
2. Implement additional steps in new methods
3. Update the `step_mapping` in `run_partial_release()`
4. Add corresponding command-line options if needed
