# Enumerate-Subsites

## SYNOPSIS

Recursively enumerates all subsites for a list of SharePoint root sites and appends results to a CSV file. Supports pause and resume — stopping the script at any point saves progress, and the next run continues where it left off.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Enumerate-Subsites

## AUTHOR
 - Name: Pieter Heemeryck
 - Email: pieter.heemeryck@delaware.pro

## SYNTAX

Add your root SharePoint site URLs to `sites_input.txt`, one per line. Lines starting with `#` are treated as comments and ignored.

```
# This is a comment
https://contoso.sharepoint.com/sites/HR
https://contoso.sharepoint.com/sites/Finance
```


### Basic

```powershell
.\Enumerate-Subsites.ps1 `
    -ClientId   "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" `
    -TenantId   "contoso.onmicrosoft.com" `
    -Thumbprint "ABCDEF1234..."
```

## Prerequisites
- PowerShell 7+
- [PnP.PowerShell](https://pnp.github.io/powershell/) module installed
- An Entra ID (Azure AD) app registration with **SharePoint API** application permission: `Sites.Read.All`
- A certificate installed in the local Windows certificate store

## Description

### Files

| File | Description |
|---|---|
| `Enumerate-Subsites.ps1` | Main script |
| `sites_input.txt` | Root site URLs to enumerate (one per line) |
| `sites_output.csv` | Output — appended incrementally, never overwritten |
| `progress_state.json` | Auto-managed resume checkpoint — deleted on clean completion |

---

### Pause & Resume

- **To pause:** press `Ctrl+C` — state is saved automatically before the script exits.
- **To resume:** run the same command again. The script detects `progress_state.json` and continues from where it left off, skipping already-processed sites.
- **On clean completion:** `progress_state.json` is automatically deleted.

---

### Output format

`sites_output.csv` contains the following columns:

| Column | Description |
|---|---|
| `siteUrl` | Full URL of the site or subsite |
| `title` | Display name |
| `siteId` | GUID identifier |
| `parentSiteUrl` | URL of the parent site (`""` for root sites from the input list) |
| `depth` | Nesting level — `0` for root sites, `1` for their direct children, etc. |

---

### Depth limit

Recursion stops at depth **100**. If this limit is ever reached, a warning row is written to `sites_output.csv`:

| siteUrl | title | siteId | parentSiteUrl | depth |
|---|---|---|---|---|
| `https://.../deeply/nested` | `*** DEPTH LIMIT REACHED ***` | _(empty)_ | `https://.../parent` | `100` |

The branch is not recursed further.

---

### Error handling

If a site fails to process (e.g. due to missing permissions or the site being unavailable), the error is logged to the console and the site is skipped. It will not be retried on resume. All other sites in the queue continue to be processed normally.

---

### Parameters

| Parameter | Required | Description |
|---|---|---|
| `-ClientId` | Yes | Entra ID application (client) ID |
| `-TenantId` | Yes | Tenant ID or domain (e.g. `contoso.onmicrosoft.com`) |
| `-Thumbprint` | Yes | Certificate thumbprint from local Windows cert store |
| `-InputFile` | No | Path to input file. Defaults to `sites_input.txt` next to the script |
| `-OutputFile` | No | Path to output CSV. Defaults to `sites_output.csv` next to the script |
| `-StateFile` | No | Path to state file. Defaults to `progress_state.json` next to the script |
| `-Force` | No | Ignore saved state and start a fresh enumeration |


## EXAMPLES

### Custom input/output paths

```powershell
.\Enumerate-Subsites.ps1 `
    -ClientId   "..." `
    -TenantId   "..." `
    -Thumbprint "..." `
    -InputFile  "C:\path\to\my_sites.txt" `
    -OutputFile "C:\path\to\results.csv"
```

### Force a fresh start (ignore saved progress)

```powershell
.\Enumerate-Subsites.ps1 `
    -ClientId   "..." `
    -TenantId   "..." `
    -Thumbprint "..." `
    -Force
```

> **Note:** `-Force` discards `progress_state.json` but does **not** clear `sites_output.csv`. Existing rows are preserved.

## Tags
 * M365
 * SPO
 * Subsites