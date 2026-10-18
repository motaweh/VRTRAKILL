# Builds Release and writes the release zips:
#   VRTRAKILL-<ver>-full.zip     mod + BepInEx 5.4.23.5 x64
#   VRTRAKILL-<ver>-modonly.zip  mod only
#   SHA256SUMS.txt, SHA256SUMS.txt.sig (tools/sign-release.ps1; -NoSign skips it for local test builds)
# The zip root is the ULTRAKILL game folder.
# The version is VRTRAKILLVersion in Directory.Build.props.
# Usage: pwsh tools/package.ps1 [-ULTRAKILLDir <game folder>] [-OutDir <dir>] [-NoSign]
param(
    [string]$ULTRAKILLDir = $(if ($env:ULTRAKILLDir) { $env:ULTRAKILLDir } else { 'C:\Program Files (x86)\Steam\steamapps\common\ULTRAKILL' }),
    [string]$OutDir = (Join-Path $PSScriptRoot '..\artifacts'),
    [switch]$NoSign
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

$Repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$Work = Join-Path $Repo 'artifacts\work'
$Payload = Join-Path $Repo 'ULTRAKILL_COPYTOROOT'
$Version = ([xml](Get-Content (Join-Path $Repo 'Directory.Build.props') -Raw)).Project.PropertyGroup.VRTRAKILLVersion | Where-Object { $_ } | Select-Object -First 1
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw "VRTRAKILLVersion '$Version' in Directory.Build.props is not major.minor.patch" }

# official release asset; the hash is the digest GitHub lists for it
$BepInExUrl = 'https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip'
$BepInExSha256 = '82f9878551030f54657792c0740d9d51a09500eeae1fba21106b0c441e6732c4'

# fixed entry time so the same inputs give the same zip
$EntryTime = [DateTimeOffset]::new(2026, 1, 1, 0, 0, 0, [TimeSpan]::Zero)

function Copy-Tree($From, $To) {
    New-Item -ItemType Directory -Force $To | Out-Null
    Copy-Item (Join-Path $From '*') $To -Recurse -Force
}

function New-Zip($Dir, $ZipPath) {
    $root = (Resolve-Path $Dir).Path.TrimEnd('\') + '\'
    $files = [string[]](Get-ChildItem $Dir -Recurse -File -Force | ForEach-Object { $_.FullName.Substring($root.Length).Replace('\', '/') })
    [Array]::Sort($files, [StringComparer]::Ordinal)
    if (Test-Path $ZipPath) { Remove-Item $ZipPath }
    $zip = [System.IO.Compression.ZipFile]::Open($ZipPath, 'Create')
    try {
        foreach ($rel in $files) {
            $entry = $zip.CreateEntry($rel, [System.IO.Compression.CompressionLevel]::Optimal)
            $entry.LastWriteTime = $EntryTime
            $out = $entry.Open()
            $in = [System.IO.File]::OpenRead((Join-Path $root $rel))
            try { $in.CopyTo($out) } finally { $in.Dispose(); $out.Dispose() }
        }
    } finally { $zip.Dispose() }
}

Write-Host "Building against $ULTRAKILLDir"
& dotnet build (Join-Path $Repo 'VRTRAKILL.sln') -c Release -nologo -v m --no-incremental "-p:ULTRAKILLDir=$ULTRAKILLDir"
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }

if (Test-Path $Work) { Remove-Item $Work -Recurse -Force }
$Mod = Join-Path $Work 'modonly'
$Full = Join-Path $Work 'full'

# mod-only: the payload, the three built DLLs, license files. BepInEx.cfg is left out so an existing BepInEx config is kept.
Copy-Tree $Payload $Mod
Remove-Item (Join-Path $Mod 'BepInEx\config') -Recurse -Force
$PluginDir = Join-Path $Mod 'BepInEx\plugins\VRTRAKILL'
Copy-Item (Join-Path $Repo 'VRTRAKILL.Plugin\bin\Release\VRTRAKILL.dll') $PluginDir
Copy-Item (Join-Path $Repo 'bin\VRTRAKILL.Utilities.dll') $PluginDir
Copy-Item (Join-Path $Repo 'bin\Release\CopyTo\ULTRAKILL\BepInEx\patchers\VRPatcher.dll') (Join-Path $Mod 'BepInEx\patchers')
Copy-Item (Join-Path $Repo 'LICENSE.md') $PluginDir
Copy-Item (Join-Path $Repo 'THIRD_PARTY_NOTICES.md') $PluginDir

# full: the BepInEx release zip, then the mod and BepInEx.cfg on top
$Cache = Join-Path $Repo 'artifacts\cache'
New-Item -ItemType Directory -Force $Cache | Out-Null
$BepInExZip = Join-Path $Cache 'BepInEx_win_x64_5.4.23.5.zip'
if (-not (Test-Path $BepInExZip)) {
    Write-Host "Downloading $BepInExUrl"
    Invoke-WebRequest $BepInExUrl -OutFile $BepInExZip -UseBasicParsing
}
$hash = (Get-FileHash $BepInExZip -Algorithm SHA256).Hash.ToLowerInvariant()
if ($hash -ne $BepInExSha256) { throw "BepInEx zip SHA256 is $hash, expected $BepInExSha256" }
Expand-Archive $BepInExZip $Full
Copy-Tree $Mod $Full
Copy-Tree (Join-Path $Payload 'BepInEx\config') (Join-Path $Full 'BepInEx\config')

foreach ($dir in $Mod, $Full) {
    $bad = Get-ChildItem $dir -Recurse -File -Force | Where-Object { $_.Name -like 'globalgamemanagers*' -or $_.Extension -eq '.pdb' }
    if ($bad) { throw "Unexpected files in $dir`: $($bad.FullName -join ', ')" }
}

New-Item -ItemType Directory -Force $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path
$names = "VRTRAKILL-$Version-full.zip", "VRTRAKILL-$Version-modonly.zip"
New-Zip $Full (Join-Path $OutDir $names[0])
New-Zip $Mod (Join-Path $OutDir $names[1])

$sums = foreach ($n in $names) { '{0}  {1}' -f (Get-FileHash (Join-Path $OutDir $n) -Algorithm SHA256).Hash.ToLowerInvariant(), $n }
[System.IO.File]::WriteAllText((Join-Path $OutDir 'SHA256SUMS.txt'), (($sums -join "`n") + "`n"))
Get-Content (Join-Path $OutDir 'SHA256SUMS.txt')
if (-not $NoSign) { & (Join-Path $PSScriptRoot 'sign-release.ps1') -SumsPath (Join-Path $OutDir 'SHA256SUMS.txt') }
