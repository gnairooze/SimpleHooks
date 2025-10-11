#!/usr/bin/env python3
"""
SimpleHooks Release Automation Script

This script automates the release process for SimpleHooks project including:
- Version management
- .NET project publishing
- GitHub release creation
- Docker image building and pushing
"""

import os
import sys
import subprocess
import json
import re
import argparse
import zipfile
from pathlib import Path
from typing import List, Dict, Optional


class ReleaseAutomation:
    def __init__(self, new_version: str, github_token: Optional[str] = None, docker_registry: str = "gnairooze"):
        self.new_version = new_version
        self.github_token = github_token
        self.docker_registry = docker_registry
        self.root_path = Path.cwd()
        self.code_path = self.root_path / "code"
        
        # Project configurations
        self.projects = {
            "SimpleHooks.Web": {
                "path": self.code_path / "SimpleHooks.Web",
                "csproj": "SimpleHooks.Web.csproj",
                "docker_tag": "web"
            },
            "SimpleHooks.AuthApi": {
                "path": self.code_path / "SimpleHooks.AuthApi", 
                "csproj": "SimpleHooks.AuthApi.csproj",
                "docker_tag": "authapi"
            },
            "SimpleHooks.Server": {
                "path": self.code_path / "SimpleHooks.Server",
                "csproj": "SimpleHooks.Server.csproj", 
                "docker_tag": "proc"
            },
            "SimpleHooks.Assist": {
                "path": self.code_path / "SimpleHooks.Assist",
                "csproj": "SimpleHooks.Assist.csproj",
                "docker_tag": None  # No Docker image for this project
            }
        }
        
        self.publish_path = self.root_path / "publish"
        self.release_path = self.root_path / "release"

    def run_command(self, command: List[str], cwd: Optional[Path] = None, check: bool = True) -> subprocess.CompletedProcess:
        """Execute a command and return the result"""
        print(f"Running: {' '.join(command)}")
        if cwd:
            print(f"Working directory: {cwd}")
        
        try:
            result = subprocess.run(
                command, 
                cwd=cwd, 
                check=check, 
                capture_output=True, 
                text=True
            )
            if result.stdout:
                print(f"Output: {result.stdout}")
            return result
        except subprocess.CalledProcessError as e:
            print(f"Error running command: {e}")
            if e.stderr:
                print(f"Error output: {e.stderr}")
            if check:
                raise
            return e

    def update_version_in_file(self, file_path: Path, new_version: str):
        """Update version in a specific file"""
        print(f"Updating version in {file_path}")
        
        if not file_path.exists():
            print(f"Warning: File {file_path} does not exist")
            return
            
        content = file_path.read_text(encoding='utf-8')
        
        if file_path.suffix == '.csproj':
            # Update AssemblyVersion and FileVersion in .csproj files
            content = re.sub(
                r'<AssemblyVersion>[\d\.]+</AssemblyVersion>',
                f'<AssemblyVersion>{new_version}</AssemblyVersion>',
                content
            )
            content = re.sub(
                r'<FileVersion>[\d\.]+</FileVersion>',
                f'<FileVersion>{new_version}</FileVersion>',
                content
            )
        elif file_path.name == 'README.md':
            # Update version in README.md
            content = re.sub(
                r'version: [\d\.]+',
                f'version: {new_version}',
                content
            )
        
        file_path.write_text(content, encoding='utf-8')
        print(f"Updated {file_path}")

    def step_1_update_readme(self):
        """Step 1: Update readme file with the new version"""
        print(f"\n=== Step 1: Setting new version to {self.new_version} ===")
        
        # Update README.md
        readme_path = self.root_path / "README.md"
        self.update_version_in_file(readme_path, self.new_version)

    def step_2_to_5_update_assemblies(self):
        """Steps 2-5: Update assembly versions for all projects"""
        print(f"\n=== Steps 2-5: Updating assembly versions ===")
        
        for project_name, config in self.projects.items():
            csproj_path = config["path"] / config["csproj"]
            self.update_version_in_file(csproj_path, self.new_version)

    def step_6_commit_changes_and_push(self):
        """Step 6: Commit and push version changes"""
        print(f"\n=== Step 6: Commit and push version changes ===")
        
        try:
            # Add all modified files to git
            add_cmd = ["git", "add", "."]
            self.run_command(add_cmd)
            
            # Commit changes
            commit_cmd = ["git", "commit", "-m", f"Update version to {self.new_version}"]
            self.run_command(commit_cmd)
            
            # Push to main branch
            push_cmd = ["git", "push", "origin", "main"]
            self.run_command(push_cmd)
            
            print(f"Successfully committed and pushed version {self.new_version} to main branch")
            
        except subprocess.CalledProcessError as e:
            print(f"Warning: Git operations failed: {e}")
            print("You may need to commit and push the changes manually")

    def step_7_to_10_publish_projects(self):
        """Steps 7-10: Publish all projects for win-x64"""
        print(f"\n=== Steps 7-10: Publishing projects for win-x64 ===")
        
        # Create publish directory
        self.publish_path.mkdir(exist_ok=True)
        
        for project_name, config in self.projects.items():
            print(f"\nPublishing {project_name}...")
            
            project_publish_path = self.publish_path / project_name
            project_publish_path.mkdir(exist_ok=True)
            
            # Publish command
            publish_cmd = [
                "dotnet", "publish",
                str(config["path"] / config["csproj"]),
                "-c", "Release",
                "-r", "win-x64",
                "--self-contained", "true",
                "-o", str(project_publish_path)
            ]
            
            self.run_command(publish_cmd)

    def step_11_to_18_create_github_release(self):
        """Steps 11-18: Create GitHub release and upload compressed files"""
        print(f"\n=== Steps 11-18: Creating GitHub release ===")
        
        if not self.github_token:
            print("Warning: No GitHub token provided. Skipping GitHub release creation.")
            print("Please create the release manually and upload the compressed files.")
            self._create_compressed_files()
            return
        
        # Create compressed files
        self._create_compressed_files()
        
        # Create GitHub release using gh CLI
        try:
            # Create release
            create_release_cmd = [
                "gh", "release", "create", self.new_version,
                "--title", f"Release {self.new_version}",
                "--notes", f"Release version {self.new_version}"
            ]
            
            # Add compressed files to release
            for project_name in self.projects.keys():
                zip_file = self.release_path / f"{project_name}-{self.new_version}.zip"
                if zip_file.exists():
                    create_release_cmd.append(str(zip_file))
            
            self.run_command(create_release_cmd)
            
        except subprocess.CalledProcessError:
            print("Error: Failed to create GitHub release. Make sure 'gh' CLI is installed and authenticated.")
            print("You can install it from: https://cli.github.com/")

    def _create_compressed_files(self):
        """Create compressed files for each published project"""
        print("Creating compressed files...")
        
        # Create release directory
        self.release_path.mkdir(exist_ok=True)
        
        for project_name, config in self.projects.items():
            project_publish_path = self.publish_path / project_name
            
            if not project_publish_path.exists():
                print(f"Warning: Published files for {project_name} not found")
                continue
                
            zip_file_path = self.release_path / f"{project_name}-{self.new_version}.zip"
            
            print(f"Creating {zip_file_path}")
            
            with zipfile.ZipFile(zip_file_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
                for file_path in project_publish_path.rglob('*'):
                    if file_path.is_file():
                        arcname = file_path.relative_to(project_publish_path)
                        zipf.write(file_path, arcname)

    def step_19_to_30_docker_operations(self):
        """Steps 19-30: Build and push Docker images"""
        print(f"\n=== Steps 19-30: Docker operations ===")
        
        for project_name, config in self.projects.items():
            docker_tag = config.get("docker_tag")
            if not docker_tag:
                print(f"Skipping Docker operations for {project_name} (no Docker configuration)")
                continue
                
            print(f"\nProcessing Docker operations for {project_name}...")
            
            project_path = config["path"]
            dockerfile_path = project_path / "Dockerfile"
            
            if not dockerfile_path.exists():
                print(f"Warning: Dockerfile not found for {project_name}")
                continue
            
            # Build Docker image with version tag
            version_tag = f"{self.docker_registry}/simple-hooks:{docker_tag}-{self.new_version}"
            latest_tag = f"{self.docker_registry}/simple-hooks:{docker_tag}-latest"
            
            # Build image
            build_cmd = [
                "docker", "build",
                "-t", version_tag,
                "-t", latest_tag,
                str(project_path)
            ]
            
            try:
                self.run_command(build_cmd)
                
                # Push version tag
                push_version_cmd = ["docker", "push", version_tag]
                self.run_command(push_version_cmd)
                
                # Push latest tag
                push_latest_cmd = ["docker", "push", latest_tag]
                self.run_command(push_latest_cmd)
                
            except subprocess.CalledProcessError as e:
                print(f"Error with Docker operations for {project_name}: {e}")
                print("Make sure Docker is running and you're logged in to the registry")

    def run_full_release(self):
        """Execute the complete release process"""
        print(f"Starting full release process for version {self.new_version}")
        
        try:
            self.step_1_update_readme()
            self.step_2_to_5_update_assemblies()
            self.step_6_commit_changes_and_push()
            self.step_7_to_10_publish_projects()
            self.step_11_to_18_create_github_release()
            self.step_19_to_30_docker_operations()
            
            print(f"\n Release {self.new_version} completed successfully!")
            print("\nNext steps:")
            print("1. Verify the GitHub release was created correctly")
            print("2. Test the Docker images")
            print("3. Update any deployment configurations")
            
        except Exception as e:
            print(f"\n Release process failed: {e}")
            sys.exit(1)

    def run_partial_release(self, steps: List[str]):
        """Execute specific steps of the release process"""
        step_mapping = {
            "readme": self.step_1_update_readme,
            "assemblies": self.step_2_to_5_update_assemblies,
            "commit": self.step_6_commit_changes_and_push,
            "publish": self.step_7_to_10_publish_projects,
            "github": self.step_11_to_18_create_github_release,
            "docker": self.step_19_to_30_docker_operations
        }
        
        for step in steps:
            if step in step_mapping:
                step_mapping[step]()
            else:
                print(f"Unknown step: {step}")
                print(f"Available steps: {list(step_mapping.keys())}")


def main():
    parser = argparse.ArgumentParser(description="SimpleHooks Release Automation")
    parser.add_argument("version", help="New version number (e.g., 2.8.3)")
    parser.add_argument("--github-token", help="GitHub token for release creation")
    parser.add_argument("--docker-registry", default="gnairooze", help="Docker registry username")
    parser.add_argument("--steps", nargs="+", 
                       choices=["readme", "assemblies", "commit", "publish", "github", "docker"],
                       help="Specific steps to run (default: all)")
    parser.add_argument("--dry-run", action="store_true", help="Show what would be done without executing")
    
    args = parser.parse_args()
    
    # Validate version format
    if not re.match(r'^\d+\.\d+\.\d+$', args.version):
        print("Error: Version must be in format X.Y.Z (e.g., 2.8.3)")
        sys.exit(1)
    
    if args.dry_run:
        print(f"DRY RUN: Would execute release process for version {args.version}")
        print(f"Steps: {args.steps if args.steps else 'all'}")
        return
    
    # Create release automation instance
    automation = ReleaseAutomation(
        new_version=args.version,
        github_token=args.github_token,
        docker_registry=args.docker_registry
    )
    
    # Run release process
    if args.steps:
        automation.run_partial_release(args.steps)
    else:
        automation.run_full_release()


if __name__ == "__main__":
    main()
