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
  Set-Variable -Name "BuildPath" -Value (Join-Path -Path $RepositoryPath -ChildPath "build") -Scope script

  Set-Variable -Name "GameExecutable" -Value (Join-Path -Path $InstallPath -ChildPath "Rhythm Doctor.exe") -Scope script

  Set-Variable -Name "BepInExPath" -Value (Join-Path -Path $InstallPath -ChildPath "BepInEx") -Scope script
  Set-Variable -Name "PluginPath" -Value (Join-Path -Path $BepInExPath -ChildPath "plugins") -Scope script
  Set-Variable -Name "ConfigPath" -Value (Join-Path -Path $BepInExPath -ChildPath "config") -Scope script
  Set-Variable -Name "LogFile" -Value (Join-Path -Path $BepInExPath -ChildPath "LogOutput.log") -Scope script
}

function Prompt-Menu
{
  function Prompt-Main
  {
    while ($true)
    {
      Write-Host "===== Main Menu =====" -BackgroundColor Blue
      Write-Host " 1: Test" -ForegroundColor Blue
      Write-Host " 2: Format using CSharpier" -ForegroundColor Blue
      Write-Host ""
      Write-Host " 3: Open log" -ForegroundColor Blue
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
          dotnet publish $ProjectPath --configuration Debug --output $BuildPath

          Write-Host "Cleaning old files" -BackgroundColor Magenta
          Remove-Item -Recurse -Path $PluginPath/World
          Remove-Item -Path $PluginPath/Archipelago.MultiClient.Net.dll
          Remove-Item -Path $PluginPath/RhythmDoctor.Archipelago.dll
          Remove-Item -Path $PluginPath/RhythmDoctor.Archipelago.pdb
          Remove-Item -Path $PluginPath/YamlDotNet.dll

          Write-Host "Copying files" -BackgroundColor Magenta
          Copy-Item -Recurse -Path $BuildPath/World -Destination $PluginPath
          Copy-Item -Path $BuildPath/Archipelago.MultiClient.Net.dll -Destination $PluginPath
          Copy-Item -Path $BuildPath/RhythmDoctor.Archipelago.dll -Destination $PluginPath
          Copy-Item -Path $BuildPath/RhythmDoctor.Archipelago.pdb -Destination $PluginPath
          Copy-Item -Path $BuildPath/YamlDotNet.dll -Destination $PluginPath

          Write-Host "Starting Rhythm Doctor" -BackgroundColor Magenta
          Stop-Process -Name "Rhythm Doctor" -Confirm
          Start-Process -Confirm -FilePath $GameExecutable -WorkingDirectory $InstallPath

          Write-Host "Cleaning up" -BackgroundColor Magenta
          Remove-Item -Recurse $BuildPath
          
          continue
        }
        "2"
        {
          Write-Host "Formatting using csharpier" -BackgroundColor Red
          dotnet csharpier $RepositoryPath
          continue
        }
        "3"
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
          Write-Host "Build Path: $BuildPath" -ForegroundColor Yellow
          Write-Host
          Write-Host "Game Executable: $GameExecutable" -ForegroundColor Yellow
          Write-Host 
          Write-Host "BepInEx Path: $BepInExPath" -ForegroundColor Yellow
          Write-Host "Plugin Path: $PluginPath" -ForegroundColor Yellow
          Write-Host "Config Path: $ConfigPath" -ForegroundColor Yellow
          Write-Host "Log Path: $LogFile" -ForegroundColor Yellow
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
      Write-Host "===== Option Menu =====" -BackgroundColor Green
      Write-Host " 1: Set game directory (Currently $($InstallPath))" -ForegroundColor Green
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
        "e"
        {
          return
        }
      }
    }
  }

  Prompt-Main
}

Update-Paths -Initial $true

Prompt-Menu
