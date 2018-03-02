function Create-Directory([string[]] $Path) {
  if (!(Test-Path -Path $Path)) {
    New-Item -Path $Path -Force -ItemType "Directory" | Out-Null
  }
}

function InstallDotNetCli {

    if (!$env:DOTNET_INSTALL_DIR) {
      $env:DOTNET_INSTALL_DIR = Join-Path $RepoRoot ".dotnet\$DotNetCliVersion\"
    }

    $DotNetRoot = $env:DOTNET_INSTALL_DIR
    $DotNetInstallScript = Join-Path $DotNetRoot "dotnet-install.ps1"

    if (!(Test-Path $DotNetInstallScript)) {
      Create-Directory $DotNetRoot
      Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -UseBasicParsing -OutFile $DotNetInstallScript
    }

    # Install a stage 0
    $SdkInstallDir = Join-Path $DotNetRoot "sdk\$DotNetCliVersion"

    if (!(Test-Path $SdkInstallDir)) {
      # Use Invoke-Expression so that $DotNetInstallVerbosity is not positionally bound when empty
      Invoke-Expression -Command "& '$DotNetInstallScript' -Version $DotNetCliVersion"

      if($LASTEXITCODE -ne 0) {
        throw "Failed to install stage0"
      }
    }

  # Put the stage 0 on the path
  $env:PATH = "$DotNetRoot;$env:PATH"

  # Disable first run since we want to control all package sources
  $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

  # Don't resolve runtime, shared framework, or SDK from other locations
  $env:DOTNET_MULTILEVEL_LOOKUP=0
}

function RunBenchmark {
    InstallDotNetCli
    $env:DOTNET_HOST_PATH = Join-Path $env:DOTNET_INSTALL_DIR "dotnet.exe"
    $pathToProject = Join-Path $RepoRoot "src"
    $dotnet = Join-Path $RepoRoot ".dotnet\$DotNetCliVersion\dotnet.exe"
    Set-Location $pathToProject
    & $dotnet run -c release
}

$DotNetCliVersion = "2.1.300-preview2-008046"
$RepoRoot = Join-Path $PSScriptRoot "..\"
$RepoRoot = [System.IO.Path]::GetFullPath($RepoRoot);

if ($hostType -eq '')
{
  $hostType = 'full'
}


try {
    RunBenchmark
    exit $lastExitCode
  }
  catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
  }
  finally {
    Pop-Location
    if ($ci -and $prepareMachine) {
      Stop-Processes
    }
  }
