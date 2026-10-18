# Builds Release and writes the Thunderstore package VRTRAKILL-<ver>.zip (r2modman / Gale).
# Zip root: manifest.json, icon.png, README.md, CHANGELOG.md, plugins/, patchers/.
# The mod manager puts plugins/ in BepInEx/plugins/<team>-VRTRAKILL and patchers/ in BepInEx/patchers/<team>-VRTRAKILL.
# patchers/GameData/ULTRAKILL_Data holds the files VRPatcher copies into the game's ULTRAKILL_Data at start-up.
# Usage: pwsh tools/package-thunderstore.ps1 [-ULTRAKILLDir <game folder>] [-OutDir <dir>]
param(
    [string]$ULTRAKILLDir = $(if ($env:ULTRAKILLDir) { $env:ULTRAKILLDir } else { 'C:\Program Files (x86)\Steam\steamapps\common\ULTRAKILL' }),
    [string]$OutDir = (Join-Path $PSScriptRoot '..\artifacts\thunderstore')
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

$Repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$Work = Join-Path $Repo 'artifacts\work\thunderstore'
$Payload = Join-Path $Repo 'ULTRAKILL_COPYTOROOT'
$Meta = Join-Path $Repo 'thunderstore'

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

# Thunderstore upload checks (thunderstore-io/Thunderstore repository/validation and package_upload.py)
function Test-Package($ZipPath) {
    $errors = [System.Collections.Generic.List[string]]::new()
    $zip = [System.IO.Compression.ZipFile]::OpenRead($ZipPath)
    try {
        $names = $zip.Entries | ForEach-Object FullName
        foreach ($n in $names) {
            if ($n.StartsWith('..') -or $n.StartsWith('/') -or $n.StartsWith('\') -or $n.Contains('\') -or $n.Contains('/..') -or $n.Contains('//') -or $n.Contains('./')) { $errors.Add("unsafe path: $n") }
        }
        $dupes = $names | Group-Object { $_.ToLowerInvariant() } | Where-Object Count -gt 1
        foreach ($d in $dupes) { $errors.Add("duplicate name: $($d.Name)") }
        if ($names.Count -gt 10000) { $errors.Add("too many files: $($names.Count)") }

        $read = { param($n) $e = $zip.GetEntry($n); if (-not $e) { return $null }; $ms = [System.IO.MemoryStream]::new(); $s = $e.Open(); try { $s.CopyTo($ms) } finally { $s.Dispose() }; , $ms.ToArray() }
        $utf8 = [System.Text.UTF8Encoding]::new($false, $true)

        $manifestBytes = & $read 'manifest.json'
        if (-not $manifestBytes) { $errors.Add('missing manifest.json') }
        else {
            $m = $utf8.GetString($manifestBytes) | ConvertFrom-Json
            foreach ($k in 'name', 'version_number', 'website_url', 'description', 'dependencies') { if ($null -eq $m.$k) { $errors.Add("manifest.json: missing $k") } }
            if ($m.name -notmatch '^[a-zA-Z0-9_]{1,128}$') { $errors.Add("manifest.json: bad name '$($m.name)'") }
            if ($m.version_number -notmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$') { $errors.Add("manifest.json: bad version_number '$($m.version_number)'") }
            if ($m.description.Length -gt 250) { $errors.Add("manifest.json: description is $($m.description.Length) characters, max 250") }
            foreach ($dep in $m.dependencies) { if ($dep -notmatch '^[a-zA-Z0-9_]+-[a-zA-Z0-9_]+-\d+\.\d+\.\d+$') { $errors.Add("manifest.json: bad dependency '$dep'") } }
        }

        $icon = & $read 'icon.png'
        if (-not $icon) { $errors.Add('missing icon.png') }
        else {
            $png = [byte[]](0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)
            # IHDR width and height, big-endian
            $w = ([int]$icon[16] -shl 24) -bor ([int]$icon[17] -shl 16) -bor ([int]$icon[18] -shl 8) -bor [int]$icon[19]
            $h = ([int]$icon[20] -shl 24) -bor ([int]$icon[21] -shl 16) -bor ([int]$icon[22] -shl 8) -bor [int]$icon[23]
            if (-not [System.Linq.Enumerable]::SequenceEqual([byte[]]$icon[0..7], $png)) { $errors.Add('icon.png is not a PNG') }
            elseif ($w -ne 256 -or $h -ne 256) { $errors.Add("icon.png is ${w}x${h}, must be 256x256") }
        }

        foreach ($md in 'README.md', 'CHANGELOG.md') {
            $b = & $read $md
            if (-not $b) { if ($md -eq 'README.md') { $errors.Add('missing README.md') }; continue }
            if ($b.Length -ge 3 -and $b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF) { $errors.Add("$md starts with a UTF-8 BOM") }
            try { $text = $utf8.GetString($b) } catch { $errors.Add("$md is not UTF-8"); continue }
            if ($text.Length -gt 100000) { $errors.Add("$md is longer than 100000 characters") }
        }
    } finally { $zip.Dispose() }
    if ((Get-Item $ZipPath).Length -gt 500MB) { $errors.Add('zip is larger than 500 MB') }
    return $errors
}

Write-Host "Building against $ULTRAKILLDir"
& dotnet build (Join-Path $Repo 'VRTRAKILL.sln') -c Release -nologo -v m --no-incremental "-p:ULTRAKILLDir=$ULTRAKILLDir"
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }

if (Test-Path $Work) { Remove-Item $Work -Recurse -Force }
$Plugins = Join-Path $Work 'plugins'
$Patchers = Join-Path $Work 'patchers'
New-Item -ItemType Directory -Force $Work | Out-Null

foreach ($f in 'manifest.json', 'README.md', 'CHANGELOG.md') { Copy-Item (Join-Path $Meta $f) $Work -ErrorAction Stop }
Copy-Item (Join-Path $Repo 'icon.png') $Work

# plugins/: the files the zip install puts in BepInEx/plugins/VRTRAKILL
Copy-Tree (Join-Path $Payload 'BepInEx\plugins\VRTRAKILL') $Plugins
Copy-Item (Join-Path $Repo 'VRTRAKILL.Plugin\bin\Release\VRTRAKILL.dll') $Plugins
Copy-Item (Join-Path $Repo 'bin\VRTRAKILL.Utilities.dll') $Plugins
Copy-Item (Join-Path $Repo 'LICENSE.md') $Plugins
Copy-Item (Join-Path $Repo 'THIRD_PARTY_NOTICES.md') $Plugins

# patchers/: VRPatcher, its dependencies, and the game files it copies into ULTRAKILL_Data
Copy-Tree (Join-Path $Payload 'BepInEx\patchers') $Patchers
Copy-Item (Join-Path $Repo 'bin\Release\CopyTo\ULTRAKILL\BepInEx\patchers\VRPatcher.dll') $Patchers
Copy-Tree (Join-Path $Payload 'ULTRAKILL_Data') (Join-Path $Patchers 'GameData\ULTRAKILL_Data')

$bad = Get-ChildItem $Work -Recurse -File -Force | Where-Object { $_.Name -like 'globalgamemanagers*' -or $_.Extension -eq '.pdb' }
if ($bad) { throw "Unexpected files in $Work`: $($bad.FullName -join ', ')" }

$manifest = Get-Content (Join-Path $Meta 'manifest.json') -Raw | ConvertFrom-Json
New-Item -ItemType Directory -Force $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path
$ZipPath = Join-Path $OutDir "$($manifest.name)-$($manifest.version_number).zip"
New-Zip $Work $ZipPath

$problems = Test-Package $ZipPath
if ($problems.Count -gt 0) { throw "Package failed Thunderstore checks:`n$($problems -join "`n")" }

$hash = (Get-FileHash $ZipPath -Algorithm SHA256).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText("$ZipPath.sha256", "$hash  $(Split-Path $ZipPath -Leaf)`n")
Write-Host "Wrote $ZipPath ($((Get-Item $ZipPath).Length) bytes), sha256 $hash"
