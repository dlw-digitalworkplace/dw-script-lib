# Copy-FolderToDifferentSiteDifferentTenant

## SYNOPSIS
This script will copy the contents of a folder from one site to a specific folder in the target. This all based on the values in the input csv.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Copy-FolderToDifferentSiteDifferentTenant

## AUTHOR
 - Name: Pieter Vandendriessche
 - Email: pieter.vandendriessche@delaware.pro

## Prerequisites
 - A user account with sufficient privileges on source site (first prompt)
 - A user account with sufficient privileges on destination site (second prompt)

## Parameters
| Variable name  | Variable Description                                                    | Example                                 |
|----------------|-------------------------------------------------------------------------|-----------------------------------------|
| $inputCsv      | The absolute path of the input csv                                      | /										 |

## Input file
| Column Name	 | Column Description														| Example                                |
|----------------|--------------------------------------------------------------------------|----------------------------------------|
| srcSiteURL     | The url of the source site												| https://x.sharepoint.com/sites/PJ-050	 |							 |
| dstSiteUrl     | the url of the destination site											| https://x.sharepoint.com/sites/PJ-051	 |		
| srcFolder	     | The name of the folder where the input needs to be copied from.			| Project Management					 |
| dstFolder      | The name of the folder where the files need to be dropped in				| Project Delivery/01. Opportunity Sketch|									 |
| libraryName    | The name of the library where the files originate from					| Documents								 |

## Example
```PowerShell
.\Copy-FolderToDifferentSiteDifferentTenant.ps1 -inputCsv ""
```