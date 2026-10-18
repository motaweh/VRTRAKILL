# Signs SHA256SUMS.txt for the in-game updater: writes SHA256SUMS.txt.sig (base64 RSA-3072 PKCS#1 v1.5 SHA-256).
# The private key lives outside the repo, encrypted with DPAPI for the current Windows user. It is never printed.
# Usage:
#   pwsh tools/sign-release.ps1 -Init                         create the key pair once and write the public key to Shared/Updates/ReleaseKey.cs
#   pwsh tools/sign-release.ps1 -SumsPath artifacts\SHA256SUMS.txt
param(
    [string]$SumsPath,
    [switch]$Init,
    [string]$KeyPath = (Join-Path $env:USERPROFILE '.vrtrakill\release_signing_key.xml.dpapi')
)
$ErrorActionPreference = 'Stop'
if (-not ('System.Security.Cryptography.ProtectedData' -as [type])) { Add-Type -AssemblyName System.Security }

$Repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$KeyFile = Join-Path $Repo 'Shared\Updates\ReleaseKey.cs'
$Entropy = [Text.Encoding]::UTF8.GetBytes('VRTRAKILL release signing key')
$Scope = [System.Security.Cryptography.DataProtectionScope]::CurrentUser

# Returns the RSA key from the DPAPI file.
function Read-PrivateKey {
    if (-not (Test-Path $KeyPath)) { throw "No signing key at $KeyPath. Run tools/sign-release.ps1 -Init once." }
    $plain = [System.Security.Cryptography.ProtectedData]::Unprotect([IO.File]::ReadAllBytes($KeyPath), $Entropy, $Scope)
    try {
        $rsa = [System.Security.Cryptography.RSA]::Create()
        $rsa.FromXmlString([Text.Encoding]::UTF8.GetString($plain))
        return $rsa
    } finally { [Array]::Clear($plain, 0, $plain.Length) }
}

# Returns the RSA public key embedded in Shared/Updates/ReleaseKey.cs.
function Read-EmbeddedPublicKey {
    $text = Get-Content $KeyFile -Raw
    $mod = [regex]::Match($text, 'ModulusBase64\s*=\s*"([^"]+)"').Groups[1].Value
    $exp = [regex]::Match($text, 'ExponentBase64\s*=\s*"([^"]+)"').Groups[1].Value
    if (-not $mod -or -not $exp) { throw "No public key in $KeyFile" }
    $p = [System.Security.Cryptography.RSAParameters]::new()
    $p.Modulus = [Convert]::FromBase64String($mod)
    $p.Exponent = [Convert]::FromBase64String($exp)
    $rsa = [System.Security.Cryptography.RSA]::Create()
    $rsa.ImportParameters($p)
    return $rsa
}

if ($Init) {
    if (Test-Path $KeyPath) { throw "A signing key already exists at $KeyPath. Remove it by hand only if you mean to replace the release key." }
    New-Item -ItemType Directory -Force (Split-Path $KeyPath) | Out-Null
    $rsa = [System.Security.Cryptography.RSA]::Create(3072)
    $plain = [Text.Encoding]::UTF8.GetBytes($rsa.ToXmlString($true))
    try { [IO.File]::WriteAllBytes($KeyPath, [System.Security.Cryptography.ProtectedData]::Protect($plain, $Entropy, $Scope)) }
    finally { [Array]::Clear($plain, 0, $plain.Length) }
    $pub = $rsa.ExportParameters($false)
    [IO.File]::WriteAllText((Join-Path (Split-Path $KeyPath) 'release_signing_key.pub.xml'), $rsa.ToXmlString($false))
    $rsa.Dispose()
    $cs = @"
namespace VRTRAKILL.Updates
{
    // The public half of the release signing key (RSA 3072); tools/sign-release.ps1 -Init writes this file.
    internal static class ReleaseKey
    {
        public const string ModulusBase64 = "$([Convert]::ToBase64String($pub.Modulus))";
        public const string ExponentBase64 = "$([Convert]::ToBase64String($pub.Exponent))";
    }
}
"@
    [IO.File]::WriteAllText($KeyFile, $cs.Replace("`r`n", "`n"))
    Write-Host "Created the signing key at $KeyPath (DPAPI, current user)."
    Write-Host "Wrote the public key to $KeyFile"
    return
}

if (-not $SumsPath) { throw 'Pass -SumsPath <SHA256SUMS.txt> or -Init.' }
$SumsPath = (Resolve-Path $SumsPath).Path
$data = [IO.File]::ReadAllBytes($SumsPath)
$sha = [System.Security.Cryptography.HashAlgorithmName]::SHA256
$pad = [System.Security.Cryptography.RSASignaturePadding]::Pkcs1

$rsa = Read-PrivateKey
try { $sig = $rsa.SignData($data, $sha, $pad) } finally { $rsa.Dispose() }

# the mod checks against the embedded key, so a signature that key rejects must not ship
$pubKey = Read-EmbeddedPublicKey
try { if (-not $pubKey.VerifyData($data, $sig, $sha, $pad)) { throw "The signing key does not match the public key in $KeyFile" } }
finally { $pubKey.Dispose() }

$SigPath = "$SumsPath.sig"
[IO.File]::WriteAllText($SigPath, [Convert]::ToBase64String($sig) + "`n")
Write-Host "Signed $SumsPath -> $SigPath"
