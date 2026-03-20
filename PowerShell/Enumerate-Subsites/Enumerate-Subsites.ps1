#Requires -Modules PnP.PowerShell
<#
.SYNOPSIS
    Enumerates all subsites for a list of SharePoint root sites and writes results to CSV.

.DESCRIPTION
    Reads root site URLs from sites_input.txt, recursively enumerates all subsites using
    PnP.PowerShell (PnP.Framework under the hood), and appends results to sites_output.csv.

    Supports pause/resume: progress is saved to progress_state.json after every site.
    Stopping the script (Ctrl+C or any termination) preserves state so the next run resumes
    where it left off.

    If a subsite branch reaches depth 100, a warning row is written to the CSV and that
    branch stops recursing.

.PARAMETER ClientId
    The Entra ID (Azure AD) application (client) ID.

.PARAMETER TenantId
    Your tenant ID or domain, e.g. contoso.onmicrosoft.com or the GUID.

.PARAMETER Thumbprint
    Certificate thumbprint from the local Windows certificate store (CurrentUser or LocalMachine).

.PARAMETER InputFile
    Path to the file containing root site URLs (one per line). Defaults to sites_input.txt
    in the same directory as this script.

.PARAMETER OutputFile
    Path to the output CSV file. Defaults to sites_output.csv in the same directory.

.PARAMETER StateFile
    Path to the resume state JSON file. Defaults to progress_state.json in the same directory.

.PARAMETER Force
    If specified, ignores any existing state file and starts a fresh enumeration.
    WARNING: this does NOT clear the output CSV — existing rows are kept as-is.

.EXAMPLE
    .\Enumerate-Subsites.ps1 -ClientId "xxxxxxxx-..." -TenantId "contoso.onmicrosoft.com" -Thumbprint "ABCDEF1234..."

.EXAMPLE
    # Force fresh start (ignore saved progress)
    .\Enumerate-Subsites.ps1 -ClientId "..." -TenantId "..." -Thumbprint "..." -Force
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ClientId,

    [Parameter(Mandatory)]
    [string] $TenantId,

    [Parameter(Mandatory)]
    [string] $Thumbprint,

    [Parameter()]
    [string] $InputFile = (Join-Path $PSScriptRoot 'sites_input.txt'),

    [Parameter()]
    [string] $OutputFile = (Join-Path $PSScriptRoot 'sites_output.csv'),

    [Parameter()]
    [string] $StateFile = (Join-Path $PSScriptRoot 'progress_state.json'),

    [switch] $Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Constants ────────────────────────────────────────────────────────────────
$DEPTH_LIMIT      = 100
$WARNING_TITLE    = '*** DEPTH LIMIT REACHED ***'
$CSV_HEADER       = 'siteUrl,title,siteId,parentSiteUrl,depth'

# ── Helpers ──────────────────────────────────────────────────────────────────

function Write-Log {
    param([string]$Message, [string]$Level = 'INFO')
    $ts = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
    $color = switch ($Level) {
        'WARN'  { 'Yellow' }
        'ERROR' { 'Red' }
        default { 'Cyan' }
    }
    Write-Host "[$ts] [$Level] $Message" -ForegroundColor $color
}

function Save-State {
    param($State)
    $State | ConvertTo-Json -Depth 10 | Set-Content -Path $StateFile -Encoding UTF8
}

function Load-State {
    if (Test-Path $StateFile) {
        $raw = Get-Content -Path $StateFile -Raw -Encoding UTF8 | ConvertFrom-Json
        # ConvertFrom-Json returns PSCustomObject; convert lists back to typed collections
        $state = @{
            ProcessedSiteIds = [System.Collections.Generic.HashSet[string]]::new(
                [string[]]($raw.ProcessedSiteIds),
                [System.StringComparer]::OrdinalIgnoreCase
            )
            Queue            = [System.Collections.Generic.Stack[hashtable]]::new()
            TotalProcessed   = [int]$raw.TotalProcessed
        }
        # Stack serialises top-to-bottom; push in reverse to restore correct order
        $savedItems = @($raw.Queue | ForEach-Object {
            @{ Url = $_.Url; ParentUrl = $_.ParentUrl; Depth = [int]$_.Depth }
        })
        for ($i = $savedItems.Count - 1; $i -ge 0; $i--) {
            $state.Queue.Push($savedItems[$i])
        }
        return $state
    }
    return $null
}

function New-State {
    param([string[]]$RootUrls)
    $state = @{
        ProcessedSiteIds = [System.Collections.Generic.HashSet[string]]::new(
            [System.StringComparer]::OrdinalIgnoreCase
        )
        Queue            = [System.Collections.Generic.Stack[hashtable]]::new()
        TotalProcessed   = 0
    }
    # Push in reverse so the first URL in the input file is processed first
    for ($i = $RootUrls.Count - 1; $i -ge 0; $i--) {
        $state.Queue.Push(@{ Url = $RootUrls[$i].TrimEnd('/'); ParentUrl = ''; Depth = 0 })
    }
    return $state
}

function Append-CsvRow {
    param(
        [string] $SiteUrl,
        [string] $Title,
        [string] $SiteId,
        [string] $ParentSiteUrl,
        [int]    $Depth
    )
    # Escape any double-quotes in string fields per RFC 4180
    function Escape-CsvField ([string]$value) {
        if ($value -match '[,"\r\n]') {
            return '"' + $value.Replace('"', '""') + '"'
        }
        return $value
    }

    $row = "{0},{1},{2},{3},{4}" -f `
        (Escape-CsvField $SiteUrl),
        (Escape-CsvField $Title),
        (Escape-CsvField $SiteId),
        (Escape-CsvField $ParentSiteUrl),
        $Depth

    [System.IO.File]::AppendAllText($OutputFile, $row + "`r`n", [System.Text.Encoding]::UTF8)
}

function Connect-ToSite {
    param([string]$Url)
    Connect-PnPOnline -Url $Url -ClientId $ClientId -Tenant $TenantId -Thumbprint $Thumbprint -ErrorAction Stop
}

# ── Ensure CSV header exists ──────────────────────────────────────────────────

if (-not (Test-Path $OutputFile)) {
    [System.IO.File]::WriteAllText($OutputFile, $CSV_HEADER + "`r`n", [System.Text.Encoding]::UTF8)
    Write-Log "Created output file: $OutputFile"
} else {
    Write-Log "Appending to existing output file: $OutputFile"
}

# ── Load or initialise state ─────────────────────────────────────────────────

$state = $null

if (-not $Force -and (Test-Path $StateFile)) {
    Write-Log "Resuming from saved state: $StateFile"
    $state = Load-State
    Write-Log ("Resuming with {0} items in queue, {1} already processed." -f `
        $state.Queue.Count, $state.TotalProcessed)
} else {
    if ($Force -and (Test-Path $StateFile)) {
        Write-Log 'Force flag set — ignoring saved state, starting fresh.' 'WARN'
        Remove-Item $StateFile -Force
    }

    Write-Log "Reading root sites from: $InputFile"
    if (-not (Test-Path $InputFile)) {
        Write-Error "Input file not found: $InputFile"
        exit 1
    }

    $rootUrls = @(Get-Content $InputFile |
        Where-Object { $_ -notmatch '^\s*#' -and $_ -match '\S' } |
        ForEach-Object { $_.Trim() })

    if ($rootUrls.Count -eq 0) {
        Write-Error "No site URLs found in $InputFile. Add URLs (one per line) and re-run."
        exit 1
    }

    Write-Log "Found $($rootUrls.Count) root site(s) to process."
    $state = New-State -RootUrls $rootUrls

    Save-State $state
}

# ── Graceful shutdown on Ctrl+C ───────────────────────────────────────────────

$script:ShuttingDown = $false

# Register cleanup for both normal exit and Ctrl+C
$null = Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action {
    if (-not $script:ShuttingDown) {
        $script:ShuttingDown = $true
        Write-Host "`n[$(Get-Date -f 'yyyy-MM-dd HH:mm:ss')] [WARN] Script interrupted — saving state..." -ForegroundColor Yellow
        Save-State $state
        Write-Host "[$(Get-Date -f 'yyyy-MM-dd HH:mm:ss')] [WARN] State saved. Re-run the script to resume." -ForegroundColor Yellow
    }
}

# ── Main enumeration loop ─────────────────────────────────────────────────────

Write-Log "Starting enumeration. Press Ctrl+C at any time to pause — progress will be saved."
Write-Log ('-' * 70)

try {
    while ($state.Queue.Count -gt 0) {
        $current = $state.Queue.Pop()
        $url      = $current.Url
        $parentUrl = $current.ParentUrl
        $depth    = $current.Depth

        # Skip if already processed (can happen when resuming)
        if ($state.ProcessedSiteIds.Contains($url)) {
            Write-Log "Skipping already-processed site: $url"
            continue
        }

        # ── Depth limit check ────────────────────────────────────────────────
        if ($depth -ge $DEPTH_LIMIT) {
            Write-Log "DEPTH LIMIT ($DEPTH_LIMIT) reached at: $url" 'WARN'
            Append-CsvRow -SiteUrl $url -Title $WARNING_TITLE -SiteId '' `
                          -ParentSiteUrl $parentUrl -Depth $depth
            # Mark as processed so we don't re-enqueue it on resume
            $state.ProcessedSiteIds.Add($url) | Out-Null
            Save-State $state
            continue
        }

        # ── Connect and fetch metadata ────────────────────────────────────────
        try {
            Write-Log "Processing [depth=$depth]: $url"
            Connect-ToSite -Url $url

            $web = Get-PnPWeb -Includes Id, Title, Url

            $siteId   = $web.Id.ToString()
            $title    = $web.Title
            $siteUrl  = $web.Url.TrimEnd('/')

            # ── Write this site to CSV (guard against duplicates) ────────────
            $alreadyInCsv = (Test-Path $OutputFile) -and (
                Import-Csv -Path $OutputFile |
                Where-Object { $_.siteUrl.TrimEnd('/') -eq $siteUrl } |
                Select-Object -First 1
            )
            if ($alreadyInCsv) {
                Write-Log "  Skipping CSV write — '$siteUrl' already present in output." 'WARN'
            } else {
                Append-CsvRow -SiteUrl $siteUrl -Title $title -SiteId $siteId `
                              -ParentSiteUrl $parentUrl -Depth $depth
            }

            # ── Fetch direct children (one level — no -Recurse) ───────────────
            $subwebs = @(Get-PnPSubWeb -Includes Url)

            # Push in reverse so the first child is processed first (depth-first order)
            for ($i = $subwebs.Count - 1; $i -ge 0; $i--) {
                $subUrl = $subwebs[$i].Url.TrimEnd('/')
                if (-not $state.ProcessedSiteIds.Contains($subUrl)) {
                    $state.Queue.Push(@{
                        Url       = $subUrl
                        ParentUrl = $siteUrl
                        Depth     = $depth + 1
                    })
                }
            }

            Write-Log ("  -> {0} child site(s) enqueued. Total processed so far: {1}" -f `
                $subwebs.Count, ($state.TotalProcessed + 1))

        } catch {
            Write-Log "Failed to process site '$url': $_" 'ERROR'
            Write-Log "  Site will be skipped. Check permissions or availability." 'WARN'
            # Still mark as processed to avoid infinite retry loops on resume
        } finally {
            try { Disconnect-PnPOnline -ErrorAction SilentlyContinue } catch {}
        }

        # ── Update state ──────────────────────────────────────────────────────
        $state.ProcessedSiteIds.Add($url) | Out-Null
        $state.TotalProcessed++
        Save-State $state
    }

    Write-Log ('-' * 70)
    Write-Log "Enumeration complete. Total sites processed: $($state.TotalProcessed)"
    Write-Log "Output written to: $OutputFile"

    # Clean up state file on successful completion
    if (Test-Path $StateFile) {
        Remove-Item $StateFile -Force
        Write-Log "State file removed (clean run)."
    }

} catch {
    Write-Log "Unexpected error: $_" 'ERROR'
    $script:ShuttingDown = $true
    Save-State $state
    Write-Log "State saved. Re-run the script to resume." 'WARN'
    exit 1
}
