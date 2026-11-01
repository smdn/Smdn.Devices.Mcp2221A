#!/usr/bin/env pwsh
# SPDX-FileCopyrightText: 2022 smdn <smdn@smdn.jp>
# SPDX-License-Identifier: MIT

$RepositoryRootDirectory = [System.IO.Path]::GetFullPath(
  [System.IO.Path]::Join($PSScriptRoot, "../")
)

Set-Location $RepositoryRootDirectory

$SolutionFile = Get-ChildItem -Path . -Filter '*.slnx' -File | Select-Object -First 1 -ExpandProperty FullName
$SolutionFileCreated = $false

if (!$SolutionFile) {
  # create a temporary solution for the build target projects
  $SolutionFile = [System.IO.Path]::GetFileName(
    [System.IO.Path]::GetDirectoryName($RepositoryRootDirectory)
  ) + ".temp.slnx"

  dotnet new sln --force --format slnx --name $([System.IO.Path]::GetFileNameWithoutExtension($SolutionFile))

  $SolutionFileCreated = $true

  # add build target projects to the solution
  $ProjectFiles = Get-ChildItem -Path $([System.IO.Path]::Join($RepositoryRootDirectory, 'src', 'Smdn.*', 'Smdn.*.csproj')) -File

  foreach ($ProjectFile in $ProjectFiles) {
    dotnet sln $SolutionFile add $ProjectFile
  }
}

# restore dependencies
dotnet restore $SolutionFile

# then build all projects
dotnet build --no-restore $SolutionFile

if ($SolutionFileCreated) {
  # delete the temporary solution
  Remove-Item $SolutionFile
}
