<#
Backs up the release signing private key to 1Password, or restores it from there into a new Windows profile.

The key never goes to the screen, a log, shell history, a command line or an unencrypted file:
  - Backup decrypts the DPAPI file into a byte array and writes those bytes to the stdin of `op document create -`.
  - Restore reads the stdout of `op document get` into a byte array and gives it straight to DPAPI Protect.
  - The script prints only status text, the item ID and the public key SHA-256 fingerprint.
  - Plaintext byte arrays are zeroed in finally blocks. No private key material is turned into a .NET string.
1Password asks for approval (Windows Hello or the desktop app) on the first `op` call. The script never asks for a password.

The 1Password item (default vault "Private", title "VRTRAKILL release signing key"):
  - Document file vrtrakill_release_signing_key.xml: the RSA-3072 private key XML (exact bytes sign-release.ps1 encrypts).
  - Section "Public key": field "XML" (the public key XML) and "SHA-256 fingerprint"
    (lowercase hex SHA-256 of the DER SubjectPublicKeyInfo).

Usage:
  pwsh -NoProfile -File tools/backup-signing-key.ps1                    back up; refuse if the item already exists
  pwsh -NoProfile -File tools/backup-signing-key.ps1 -Force             back up; replace the file of the existing document item
  pwsh -NoProfile -File tools/backup-signing-key.ps1 -Vault Personal    use another vault
  pwsh -NoProfile -File tools/backup-signing-key.ps1 -Restore           new Windows profile: write the .dpapi file (and the .pub.xml if missing)
  pwsh -NoProfile -File tools/backup-signing-key.ps1 -Restore -Force    as above; first rename an existing .dpapi file to .dpapi.bak-<time>

Both modes check that the key matches the public key the mod embeds (Shared/Updates/ReleaseKey.cs),
then compare SHA-256 hashes in memory and print only "match" or "mismatch".

Exit codes:
  0  done, hashes match
  1  bad input or missing file (op, key file, ReleaseKey.cs)
  2  refused: the item (backup) or the .dpapi file (restore) already exists and -Force is not set, or the item is ambiguous
  3  an op command failed
  4  hash mismatch after upload or after restore
  5  the key does not match the embedded public key
  6  backup stored and verified, but adding the public key fields to the item failed
#>
param(
    [string]$Vault = 'Private',
    [switch]$Restore,
    [switch]$Force,
    [string]$KeyPath = (Join-Path $env:USERPROFILE '.vrtrakill\release_signing_key.xml.dpapi'),
    [string]$PublicKeyPath = (Join-Path $env:USERPROFILE '.vrtrakill\release_signing_key.pub.xml')
)
$ErrorActionPreference = 'Stop'
$VerbosePreference = 'SilentlyContinue'
$DebugPreference = 'SilentlyContinue'
Set-StrictMode -Version 3
if (-not ('System.Security.Cryptography.ProtectedData' -as [type])) { Add-Type -AssemblyName System.Security }

# same entropy and scope as tools/sign-release.ps1
$Entropy = [Text.Encoding]::UTF8.GetBytes('VRTRAKILL release signing key')
$Scope = [System.Security.Cryptography.DataProtectionScope]::CurrentUser
$Title = 'VRTRAKILL release signing key'
$FileName = 'vrtrakill_release_signing_key.xml'
$Tags = 'vrtrakill,signing-key'
$ReleaseKeyCs = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\Shared\Updates\ReleaseKey.cs'))
$MaxKeyBytes = 65536
$script:OpExe = $null
$script:NullStdin = $null

class BackupFailure : System.Exception {
    [int]$Code
    BackupFailure([int]$code, [string]$message) : base($message) { $this.Code = $code }
}

# Throws a BackupFailure that the main block turns into an error line and an exit code.
function Fail([int]$Code, [string]$Message) { throw [BackupFailure]::new($Code, $Message) }

# Points this process's stdin at NUL, so op children see no pipe and never wait for piped input.
function Set-NullStdin {
    Add-Type -Namespace VrtkBackup -Name Native -MemberDefinition @'
[DllImport("kernel32.dll", SetLastError = true)] public static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);
[DllImport("kernel32.dll", SetLastError = true)] public static extern bool SetHandleInformation(IntPtr hObject, uint dwMask, uint dwFlags);
'@
    $script:NullStdin = [IO.FileStream]::new('\\.\NUL', [IO.FileMode]::Open, [IO.FileAccess]::Read)
    $h = $script:NullStdin.SafeFileHandle.DangerousGetHandle()
    if (-not [VrtkBackup.Native]::SetHandleInformation($h, 1, 1)) { Fail 1 'Could not make the NUL handle inheritable.' }
    if (-not [VrtkBackup.Native]::SetStdHandle(-10, $h)) { Fail 1 'Could not set stdin to NUL.' }
}

# Runs op; writes $InputBytes to its stdin if given; with -Secret reads stdout into a fixed buffer that the caller must zero.
function Invoke-Op([string[]]$OpArgs, [byte[]]$InputBytes = $null, [switch]$Secret) {
    $psi = [Diagnostics.ProcessStartInfo]::new($script:OpExe)
    foreach ($a in $OpArgs) { $psi.ArgumentList.Add($a) }
    $psi.UseShellExecute = $false
    $psi.RedirectStandardInput = $null -ne $InputBytes
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    # debug logging could echo request details to stderr
    [void]$psi.Environment.Remove('OP_DEBUG')
    $p = [Diagnostics.Process]::Start($psi)
    try {
        $errTask = $p.StandardError.ReadToEndAsync()
        if ($null -ne $InputBytes) {
            $in = $p.StandardInput.BaseStream
            $in.Write($InputBytes, 0, $InputBytes.Length)
            $in.Flush()
            $p.StandardInput.Close()
        }
        $out = $p.StandardOutput.BaseStream
        if ($Secret) {
            $buf = [byte[]]::new($MaxKeyBytes)
            $n = 0
            try {
                while ($true) {
                    if ($n -ge $buf.Length) { $p.Kill(); Fail 3 'op returned more data than a key file holds.' }
                    $r = $out.Read($buf, $n, $buf.Length - $n)
                    if ($r -le 0) { break }
                    $n += $r
                }
            } catch { [Array]::Clear($buf, 0, $buf.Length); throw }
            $text = $null
        } else {
            $text = [IO.StreamReader]::new($out, [Text.Encoding]::UTF8).ReadToEnd()
            $buf = $null; $n = 0
        }
        $p.WaitForExit()
        return @{ Code = $p.ExitCode; Buffer = $buf; Length = $n; Text = $text; Err = $errTask.Result.Trim() }
    } finally { $p.Dispose() }
}

# Runs a non-secret op command and returns its stdout text; fails with exit code 3 and op's error text on a non-zero exit.
function Invoke-OpText([string[]]$OpArgs, [string]$What) {
    $r = Invoke-Op $OpArgs
    if ($r.Code -ne 0) { Fail 3 "op failed to $What (exit $($r.Code)): $($r.Err)" }
    return $r.Text
}

# Returns the items in the vault whose title is the backup title.
function Find-BackupItems {
    $json = Invoke-OpText @('item', 'list', '--vault', $Vault, '--format', 'json') "list the items in vault '$Vault' (pass -Vault <name> if the vault has another name)"
    if ([string]::IsNullOrWhiteSpace($json)) { return @() }
    return @(($json | ConvertFrom-Json) | Where-Object { $_.title -eq $Title })
}

# Returns the start index of $Needle in $Hay at or after $Start, or -1.
function Find-Bytes([byte[]]$Hay, [byte[]]$Needle, [int]$Start = 0) {
    for ($i = $Start; $i -le $Hay.Length - $Needle.Length; $i++) {
        $hit = $true
        for ($j = 0; $j -lt $Needle.Length; $j++) { if ($Hay[$i + $j] -ne $Needle[$j]) { $hit = $false; break } }
        if ($hit) { return $i }
    }
    return -1
}

# Returns the text inside <Tag>...</Tag> of the key XML bytes; call it only for public elements (Modulus, Exponent).
function Get-PublicElement([byte[]]$Bytes, [string]$Tag) {
    $open = [Text.Encoding]::ASCII.GetBytes("<$Tag>")
    $s = Find-Bytes $Bytes $open
    if ($s -lt 0) { return $null }
    $s += $open.Length
    $e = Find-Bytes $Bytes ([Text.Encoding]::ASCII.GetBytes("</$Tag>")) $s
    if ($e -lt 0) { return $null }
    return [Text.Encoding]::ASCII.GetString($Bytes, $s, $e - $s)
}

# Returns the modulus and exponent (base64) that the mod embeds in Shared/Updates/ReleaseKey.cs.
function Read-EmbeddedPublicKey {
    if (-not (Test-Path -LiteralPath $ReleaseKeyCs)) { Fail 1 "No public key file at $ReleaseKeyCs. Run the script from the VRTRAKILL repository." }
    $text = Get-Content -LiteralPath $ReleaseKeyCs -Raw
    $mod = [regex]::Match($text, 'ModulusBase64\s*=\s*"([^"]+)"').Groups[1].Value
    $exp = [regex]::Match($text, 'ExponentBase64\s*=\s*"([^"]+)"').Groups[1].Value
    if (-not $mod -or -not $exp) { Fail 1 "No public key in $ReleaseKeyCs" }
    return @{ Modulus = $mod; Exponent = $exp }
}

# Fails with exit code 5 unless the private key XML bytes hold a private exponent and the embedded modulus and exponent.
function Assert-KeyMatches([byte[]]$Plain, $Pub) {
    if ((Find-Bytes $Plain ([Text.Encoding]::ASCII.GetBytes('<D>'))) -lt 0) { Fail 5 'The data is not an RSA private key XML (no <D> element).' }
    $mod = Get-PublicElement $Plain 'Modulus'
    $exp = Get-PublicElement $Plain 'Exponent'
    if ($mod -cne $Pub.Modulus -or $exp -cne $Pub.Exponent) { Fail 5 "The private key does not match the public key in $ReleaseKeyCs." }
}

# Returns the public key XML in the format RSA.ToXmlString($false) writes.
function Get-PublicKeyXml($Pub) { "<RSAKeyValue><Modulus>$($Pub.Modulus)</Modulus><Exponent>$($Pub.Exponent)</Exponent></RSAKeyValue>" }

# Returns the lowercase hex SHA-256 of the DER SubjectPublicKeyInfo of the public key.
function Get-Fingerprint($Pub) {
    $p = [System.Security.Cryptography.RSAParameters]::new()
    $p.Modulus = [Convert]::FromBase64String($Pub.Modulus)
    $p.Exponent = [Convert]::FromBase64String($Pub.Exponent)
    $rsa = [System.Security.Cryptography.RSA]::Create()
    try {
        $rsa.ImportParameters($p)
        return [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($rsa.ExportSubjectPublicKeyInfo())).ToLowerInvariant()
    } finally { $rsa.Dispose() }
}

# Downloads the document into an exact-length byte array (one trailing newline after </RSAKeyValue> is dropped); the caller must zero it.
function Get-DocumentBytes([string]$ItemId) {
    $r = Invoke-Op @('document', 'get', $ItemId, '--vault', $Vault) -Secret
    try {
        if ($r.Code -ne 0) { Fail 3 "op failed to download the document (exit $($r.Code)): $($r.Err)" }
        $n = $r.Length
        # op may end stdout with a newline; the key XML itself never ends with one
        if ($n -gt 0 -and $r.Buffer[$n - 1] -eq 10) { $n-- }
        if ($n -gt 0 -and $r.Buffer[$n - 1] -eq 13) { $n-- }
        $bytes = [byte[]]::new($n)
        [Array]::Copy($r.Buffer, $bytes, $n)
        return , $bytes
    } finally { if ($null -ne $r.Buffer) { [Array]::Clear($r.Buffer, 0, $r.Buffer.Length) } }
}

# Returns $true when two byte arrays have the same SHA-256.
function Test-SameHash([byte[]]$A, [byte[]]$B) {
    $ha = [System.Security.Cryptography.SHA256]::HashData($A)
    $hb = [System.Security.Cryptography.SHA256]::HashData($B)
    return [System.Security.Cryptography.CryptographicOperations]::FixedTimeEquals($ha, $hb)
}

# Uploads the DPAPI key to 1Password, verifies the stored copy and adds the public key fields.
function Invoke-Backup {
    $pub = Read-EmbeddedPublicKey
    if (-not (Test-Path -LiteralPath $KeyPath)) { Fail 1 "No signing key at $KeyPath." }
    if (Test-Path -LiteralPath $PublicKeyPath) {
        $pubXml = (Get-Content -LiteralPath $PublicKeyPath -Raw).Trim()
        if ($pubXml -cne (Get-PublicKeyXml $pub)) { Fail 5 "$PublicKeyPath does not match the public key in $ReleaseKeyCs." }
    } else { $pubXml = Get-PublicKeyXml $pub }
    $fingerprint = Get-Fingerprint $pub

    $existing = @(Find-BackupItems)
    if ($existing.Count -gt 0 -and -not $Force) { Fail 2 "Vault '$Vault' already has an item titled '$Title'. Pass -Force to replace its file." }
    if ($existing.Count -gt 1) { Fail 2 "Vault '$Vault' has $($existing.Count) items titled '$Title'. Remove the extra ones in 1Password first." }
    if ($existing.Count -eq 1 -and $existing[0].category -ne 'DOCUMENT') { Fail 2 "The item '$Title' in vault '$Vault' is a $($existing[0].category), not a document. Rename or remove it first." }

    $plain = [System.Security.Cryptography.ProtectedData]::Unprotect([IO.File]::ReadAllBytes($KeyPath), $Entropy, $Scope)
    $stored = $null
    try {
        Assert-KeyMatches $plain $pub
        if ($existing.Count -eq 1) {
            $id = $existing[0].id
            $r = Invoke-Op @('document', 'edit', $id, '-', '--vault', $Vault, '--file-name', $FileName) -InputBytes $plain
            if ($r.Code -ne 0) { Fail 3 "op failed to replace the document file (exit $($r.Code)): $($r.Err)" }
            Write-Host "Replaced the file of item $id in vault '$Vault'."
        } else {
            $r = Invoke-Op @('document', 'create', '-', '--title', $Title, '--file-name', $FileName, '--vault', $Vault, '--tags', $Tags, '--format', 'json') -InputBytes $plain
            if ($r.Code -ne 0) { Fail 3 "op failed to create the document (exit $($r.Code)): $($r.Err)" }
            $id = $null
            try { $meta = $r.Text | ConvertFrom-Json } catch { $meta = $null }
            if ($meta) { foreach ($k in 'id', 'uuid') { if ($meta.PSObject.Properties[$k]) { $id = $meta.$k; break } } }
            if (-not $id) {
                $found = @(Find-BackupItems)
                if ($found.Count -ne 1) { Fail 3 "The document was created, but its item ID could not be found. Check vault '$Vault' in 1Password." }
                $id = $found[0].id
            }
            Write-Host "Created item $id in vault '$Vault'."
        }

        $stored = Get-DocumentBytes $id
        $same = Test-SameHash $plain $stored
    } finally {
        [Array]::Clear($plain, 0, $plain.Length)
        if ($null -ne $stored) { [Array]::Clear($stored, 0, $stored.Length) }
    }
    if (-not $same) { Write-Host 'mismatch'; Fail 4 "The stored document is not the same as the local key. Check item $id in 1Password before you trust it." }
    Write-Host 'match'

    $r = Invoke-Op @('item', 'edit', $id, '--vault', $Vault, "Public key.XML[text]=$pubXml", "Public key.SHA-256 fingerprint[text]=$fingerprint", '--format', 'json')
    if ($r.Code -ne 0) { Fail 6 "The key is stored and verified, but op failed to add the public key fields (exit $($r.Code)): $($r.Err)" }
    Write-Host "Public key SHA-256 fingerprint: $fingerprint"
    Write-Host "Backup done: '$Title' in vault '$Vault'."
}

# Downloads the key from 1Password into memory, encrypts it with DPAPI for this user and writes the .dpapi file.
function Invoke-Restore {
    $pub = Read-EmbeddedPublicKey
    if ((Test-Path -LiteralPath $KeyPath) -and -not $Force) { Fail 2 "A signing key already exists at $KeyPath. Pass -Force to rename it to a .bak file and restore." }
    $found = @(Find-BackupItems)
    if ($found.Count -eq 0) { Fail 1 "Vault '$Vault' has no item titled '$Title'." }
    if ($found.Count -gt 1) { Fail 2 "Vault '$Vault' has $($found.Count) items titled '$Title'. Remove the extra ones in 1Password first." }
    if ($found[0].category -ne 'DOCUMENT') { Fail 2 "The item '$Title' in vault '$Vault' is a $($found[0].category), not a document." }
    $id = $found[0].id

    $plain = Get-DocumentBytes $id
    $check = $null
    try {
        Assert-KeyMatches $plain $pub
        $cipher = [System.Security.Cryptography.ProtectedData]::Protect($plain, $Entropy, $Scope)
        New-Item -ItemType Directory -Force (Split-Path $KeyPath) | Out-Null
        $bak = $null
        if (Test-Path -LiteralPath $KeyPath) {
            $bak = "$KeyPath.bak-$(Get-Date -Format yyyyMMdd-HHmmss)"
            Move-Item -LiteralPath $KeyPath -Destination $bak
            Write-Host "Renamed the old key file to $bak"
        }
        [IO.File]::WriteAllBytes($KeyPath, $cipher)
        $check = [System.Security.Cryptography.ProtectedData]::Unprotect([IO.File]::ReadAllBytes($KeyPath), $Entropy, $Scope)
        $same = Test-SameHash $plain $check
    } finally {
        [Array]::Clear($plain, 0, $plain.Length)
        if ($null -ne $check) { [Array]::Clear($check, 0, $check.Length) }
    }
    if (-not $same) {
        Write-Host 'mismatch'
        # never leave a bad key in place of a good one
        Remove-Item -LiteralPath $KeyPath
        if ($bak) { Move-Item -LiteralPath $bak -Destination $KeyPath }
        Fail 4 "The key written to $KeyPath did not decrypt to the downloaded document. It was removed; no key was changed."
    }
    Write-Host 'match'
    Write-Host "Wrote the signing key to $KeyPath (DPAPI, current user)."
    if (-not (Test-Path -LiteralPath $PublicKeyPath)) {
        [IO.File]::WriteAllText($PublicKeyPath, (Get-PublicKeyXml $pub))
        Write-Host "Wrote the public key to $PublicKeyPath"
    }
    Write-Host "Public key SHA-256 fingerprint: $(Get-Fingerprint $pub)"
}

$code = 0
try {
    $cmd = Get-Command op -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $cmd) { Fail 1 'The 1Password CLI (op) is not on PATH.' }
    $script:OpExe = $cmd.Source
    Set-NullStdin
    if ($Restore) { Invoke-Restore } else { Invoke-Backup }
} catch [BackupFailure] {
    [Console]::Error.WriteLine("error: $($_.Exception.Message)")
    $code = $_.Exception.Code
} catch {
    # only the exception type and message: no script values are attached to these
    [Console]::Error.WriteLine("error: $($_.Exception.GetType().Name): $($_.Exception.Message)")
    $code = 1
} finally {
    $Error.Clear()
    if ($null -ne $script:NullStdin) { $script:NullStdin.Dispose() }
}
exit $code
