# Repo root from the script's location
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# Path to your license header file
$licenseHeaderFile = Join-Path $repoRoot "LICENSE_HEADER.txt"
$licenseHeader = Get-Content $licenseHeaderFile -Raw

# Ensure the header ends with a newline
if (-not $licenseHeader.EndsWith("`r`n")) {
    $licenseHeader += "`r`n"
}

function Apply-LicenseHeader($filePath, $header) {
    $content = Get-Content $filePath -Raw

    # Skip empty files
    if ([string]::IsNullOrWhiteSpace($content)) { return }

    # If the file already starts with the header, skip
    if ($content.StartsWith($header)) { return }

    # Detect if the file has any existing block comment at the top
    $pattern = '^(?s)\s*/\*.*?\*/\s*'
    if ($content -match $pattern) {
        # Skip files with any existing header
        Write-Host "Skipping $filePath (existing header detected)"
        return
    }

    # Prepend the header
    $newContent = $header + $content
    Set-Content $filePath $newContent
    Write-Host "Added header to $filePath"
}

# Get all .cs files recursively, excluding Designer, bin, and obj folders
Get-ChildItem -Path $repoRoot -Recurse -Filter *.cs |
    Where-Object {
        $_.FullName -notmatch '\\(bin|obj)\\' -and
        $_.Name -notlike '*.Designer.cs'
    } |
    ForEach-Object { Apply-LicenseHeader $_.FullName $licenseHeader }

Write-Host "License header update complete."
