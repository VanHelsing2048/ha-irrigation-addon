param(
    [string]$Version = "0.1.0"
)

$ErrorActionPreference = "Stop"

$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$dist = Join-Path $root "dist"
$zipPath = Join-Path $dist "ha-irrigation-addon-$Version.zip"

New-Item -ItemType Directory -Force -Path $dist | Out-Null

if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Push-Location $root
try {
    git archive --format=zip --worktree-attributes --prefix=ha-irrigation-addon/ --output=$zipPath HEAD
}
finally {
    Pop-Location
}

Write-Host "Created $zipPath"
