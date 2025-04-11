function Update-Paths
{
  param (
    $Initial
  )

  if ($Initial -eq $true)
  {
    Set-Variable -Name "InstallPath" -Value "C:\Program Files (x86)\Steam\steamapps\common\Rhythm Doctor" -Scope script
  }

  Set-Variable -Name "RepositoryPath" -Value (Join-Path -Resolve -Path $PSScriptRoot -ChildPath ..) -Scope script
  Set-Variable -Name "ProjectPath" -Value (Join-Path -Path $RepositoryPath -ChildPath "RhythmDoctor.Archipelago") -Scope script
  Set-Variable -Name "ProjectFilePath" -Value (Join-Path -Path $ProjectPath -ChildPath "RhythmDoctor.Archipelago.csproj") -Scope script
  Set-Variable -Name "BuildPath" -Value (Join-Path -Path $RepositoryPath -ChildPath "build") -Scope script

  Set-Variable -Name "GameAssembly" -Value (Join-Path -Path $InstallPath -ChildPath "Rhythm Doctor_Data\Managed\Assembly-CSharp.dll") -Scope script

  Set-Variable -Name "BepInExPath" -Value (Join-Path -Path $InstallPath -ChildPath "BepInEx") -Scope script
  Set-Variable -Name "PluginPath" -Value (Join-Path -Path $BepInExPath -ChildPath "plugins") -Scope script
  Set-Variable -Name "ConfigPath" -Value (Join-Path -Path $BepInExPath -ChildPath "config") -Scope script
  Set-Variable -Name "DoorstopConfigFile" -Value (Join-Path -Path $InstallPath -ChildPath "doorstop_config.ini") -Scope script
  Set-Variable -Name "LogFile" -Value (Join-Path -Path $BepInExPath -ChildPath "LogOutput.log") -Scope script

  Set-Variable -Name "RenderDocPath" -Value (Join-Path -Path (scoop prefix renderdoc) -ChildPath "qrenderdoc.exe") -Scope script
  Set-Variable -Name "ILSpyPath" -Value (Join-Path -Path (scoop prefix ilspy) -ChildPath "ILSpy.exe") -Scope script
  Set-Variable -Name "dnSpyExPath" -Value (Join-Path -Path (scoop prefix dnspyex) -ChildPath "dnSpy.exe") -Scope script
}

#region Actions
function Launch-RhythmDoctor
{
  param (
    $Stop,
    $Confirm
  )

  if ($Confirm)
  {
    if ($Stop)
    {
      Stop-Process -Confirm -Name "Rhythm Doctor" -ErrorAction SilentlyContinue
      Start-Sleep -Seconds 1
    }
    Start-Process -Confirm "steam://launch/774181"
  }
  else {
    if ($Stop)
    {
      Stop-Process -Name "Rhythm Doctor" -ErrorAction SilentlyContinue
      Start-Sleep -Seconds 1
    }
    Start-Process -Confirm "steam://launch/774181"
  }
}

function Build-Project
{
  return dotnet publish $ProjectPath --configuration Debug --output $BuildPath
}

function Clean-OldPluginFiles
{
  Remove-Item -Recurse -Path $PluginPath/World -ErrorAction SilentlyContinue
  Remove-Item -Path $PluginPath/Archipelago.MultiClient.Net.dll -ErrorAction SilentlyContinue
  Remove-Item -Path $PluginPath/RhythmDoctor.Archipelago.dll -ErrorAction SilentlyContinue
  Remove-Item -Path $PluginPath/RhythmDoctor.Archipelago.pdb -ErrorAction SilentlyContinue
  Remove-Item -Path $PluginPath/YamlDotNet.dll -ErrorAction SilentlyContinue
}

function Copy-Plugin
{
  Copy-Item -Recurse -Path $BuildPath/World -Destination $PluginPath
  Copy-Item -Path $BuildPath/Archipelago.MultiClient.Net.dll -Destination $PluginPath
  Copy-Item -Path $BuildPath/RhythmDoctor.Archipelago.dll -Destination $PluginPath
  Copy-Item -Path $BuildPath/RhythmDoctor.Archipelago.pdb -Destination $PluginPath
  Copy-Item -Path $BuildPath/YamlDotNet.dll -Destination $PluginPath
}

function Clean-BuildFolder
{
  Remove-Item -Recurse $BuildPath -ErrorAction SilentlyContinue
}
#endregion

#region Check
function Check-PowerShellVersion
{
  if ($PSVersionTable.PSVersion.Major -lt 7)
  {
    Write-Host "You need PowerShell 7 or higher to run this script."
    Write-Host "Please install it using your favourite package manager."
    Write-Host
    Write-Host "    WinGet: winget install -e --id Microsoft.PowerShell"
    Write-Host "     Scoop: scoop install pwsh"
    Write-Host "Chocolatey: choco install pwsh"
    exit
  }
}

function Import-IniModule
{
  if (Get-Module -ListAvailable -Name PsIni)
  {
    if (Get-Module -Name PsIni)
    {
      # Already imported
      return $true
    }

    Import-Module -Name PsIni
    return $true
  }

  Write-Host "You need the PSIni module for this option."
  Write-Host "Please install it, or this option will be unavailable"

  $response = Read-Host "Install PSIni? (y/n)".ToLower()
  if ($response -eq "y")
  {
    Install-Module -Scope CurrentUser -Name PsIni
    Import-IniModule
  }
  else {
    return $false
  }
}

function Get-DoorstopConfig
{
  if (!(Import-IniModule))
  {
    return
  }

  Set-Variable -Name "DoorstopConfig" -Value (Get-IniContent $DoorstopConfigFile) -Scope script
}

function Save-DoorstopConfig
{
  if (!(Import-IniModule) -and !$DoorstopConfig)
  {
    return
  }

  $DoorstopConfig | Out-IniFile -FilePath "$DoorstopConfigFile" -Force
}
#endregion

#region Menus
function Prompt-Menu
{
  function Prompt-Main
  {
    while ($true)
    {
      Write-Host "===== Main Menu =====" -BackgroundColor Blue
      Write-Host " 1: Test" -ForegroundColor Blue
      Write-Host " 2: Restart Rhythm Doctor" -ForegroundColor Blue
      Write-Host " 3: Format using CSharpier" -ForegroundColor Blue
      Write-Host
      Write-Host " t: Tools" -ForegroundColor Blue
      Write-Host " l: Open log" -ForegroundColor Blue
      Write-Host " v: Print variables" -ForegroundColor Blue
      Write-Host " o: Set options" -ForegroundColor Blue
      Write-Host " e: Exit script" -ForegroundColor Blue
      Write-Host "=====================" -BackgroundColor Blue
      $Selection = (Read-Host " >").ToLower()

      switch ($Selection)
      {
        "1"
        {
          Write-Host "Building" -BackgroundColor Magenta
          Build-Project

          Write-Host "Cleaning old files" -BackgroundColor Magenta
          Clean-OldPluginFiles

          Write-Host "Copying files" -BackgroundColor Magenta
          Copy-Plugin

          Write-Host "Starting Rhythm Doctor" -BackgroundColor Magenta
          Launch-RhythmDoctor -Stop $true -Confirm $false

          Write-Host "Cleaning up" -BackgroundColor Magenta
          Clean-BuildFolder
          continue
        }
        "2"
        {
          Launch-RhythmDoctor -Stop $true -Confirm $false
        }
        "3"
        {
          Write-Host "Formatting using csharpier" -BackgroundColor Red
          dotnet csharpier $RepositoryPath
          continue
        }
        "t"
        {
          Prompt-Tools
          continue
        }
        "l"
        {
          Invoke-Item $LogFile
          continue
        }
        "v"
        {
          Write-Host "===== Variables =====" -BackgroundColor Yellow
          Write-Host "Install Path: $InstallPath" -ForegroundColor Yellow
          Write-Host "Repository Path: $RepositoryPath" -ForegroundColor Yellow
          Write-Host "Project Path: $ProjectPath" -ForegroundColor Yellow
          Write-Host ".csproj Path: $ProjectFilePath" -ForegroundColor Yellow
          Write-Host "Build Path: $BuildPath" -ForegroundColor Yellow
          Write-Host
          Write-Host "Game Assembly: $GameAssembly" -ForegroundColor Yellow
          Write-Host
          Write-Host "BepInEx Path: $BepInExPath" -ForegroundColor Yellow
          Write-Host "Plugin Path: $PluginPath" -ForegroundColor Yellow
          Write-Host "Config Path: $ConfigPath" -ForegroundColor Yellow
          Write-Host "Doorstop Config Path: $DoorstopConfigFile" -ForegroundColor Yellow
          Write-Host "Log Path: $LogFile" -ForegroundColor Yellow
          Write-Host
          Write-Host "RenderDoc Path: $RenderDocPath" -ForegroundColor Yellow
          Write-Host "ILSpy Path: $ILSpyPath" -ForegroundColor Yellow
          Write-Host "dnSpyEx Path: $dnSpyExPath" -ForegroundColor Yellow
          Write-Host "=====================" -BackgroundColor Yellow
          continue
        }
        "o"
        {
          Prompt-Options
          continue
        }
        "e"
        {
          exit
        }
      }
    }
  }

  function Prompt-Options
  {
    while ($true)
    {
      if (!(Import-IniModule))
      {
        return
      }

      Get-DoorstopConfig

      Write-Host "===== Option Menu =====" -BackgroundColor Green
      Write-Host " 1: Set game directory (Currently $($InstallPath))" -ForegroundColor Green
      Write-Host " 2: Toggle BepInEx enabled (Currently $($DoorstopConfig["General"]["enabled"]))" -ForegroundColor Green
      Write-Host " 3: Toggle debugger enabled (Currently $($DoorstopConfig["UnityMono"]["debug_enabled"]))" -ForegroundColor Green
      Write-Host " 4: Toggle debugger suspend enabled (Currently $($DoorstopConfig["UnityMono"]["debug_suspend"]))" -ForegroundColor Green
      Write-Host
      Write-Host " e: Exit submenu" -ForegroundColor Green
      Write-Host "=======================" -BackgroundColor Green
      $Selection = (Read-Host " >").ToUpper();

      switch ($Selection)
      {
        "1"
        {
          $InstallPath = Read-Host "Enter game directory path"
          Update-Paths
          continue
        }
        "2"
        {
          # We need this weird bodge instead of negating using '!'
          # as 'true' will toggle to false,
          # but not the other way around.
          # Very strange behaviour!
          if ($DoorstopConfig["General"]["enabled"] -eq $true)
          {
            $toSetTo = "false"
          }
          else {
            $toSetTo = "true"
          }
          $DoorstopConfig["General"]["enabled"] = $toSetTo
          Save-DoorstopConfig
          continue
        }
        "3"
        {
          if ($DoorstopConfig["UnityMono"]["debug_enabled"] -eq $true)
          {
            $toSetTo = "false"
          }
          else {
            $toSetTo = "true"
          }
          $DoorstopConfig["UnityMono"]["debug_enabled"] = $toSetTo
          Save-DoorstopConfig
          continue
        }
        "4"
        {
          if ($DoorstopConfig["UnityMono"]["debug_suspend"] -eq $true)
          {
            $toSetTo = "false"
          }
          else {
            $toSetTo = "true"
          }
          $DoorstopConfig["UnityMono"]["debug_suspend"] = $toSetTo
          Save-DoorstopConfig
          continue
        }
        "e"
        {
          return
        }
      }
    }
  }

  function Prompt-Tools
  {
    while ($true)
    {
      Write-Host "===== Tools  =====" -BackgroundColor Red
      Write-Host " 1: JetBrains Rider (Toolbox)" -ForegroundColor Red
      Write-Host " 2: Visual Studio Code (Standalone)" -ForegroundColor Red
      Write-Host " 3: RenderDoc (Scoop)" -ForegroundColor Red
      Write-Host " 4: ILSpy (Scoop)" -ForegroundColor Red
      Write-Host " 5: dnSpyEx (Scoop)" -ForegroundColor Red
      Write-Host
      Write-Host " e: Exit submenu" -ForegroundColor Red
      Write-Host "==================" -BackgroundColor Red
      $Selection = (Read-Host " >").ToUpper();

      switch ($Selection)
      {
        "1"
        {
          rider $ProjectFilePath
          continue
        }
        "2"
        {
          code $RepositoryPath
          continue
        }
        "3"
        {
          & $RenderDocPath
          continue
        }
        "4"
        {
          & $ILSpyPath $GameAssembly
          continue
        }
        "5"
        {
          & $dnSpyExPath $GameAssembly
          continue
        }
        "e"
        {
          return
        }
      }
    }
  }

  Prompt-Main
}
#endregion

#region Main
Check-PowerShellVersion

Update-Paths -Initial $true

Prompt-Menu
#endregion
