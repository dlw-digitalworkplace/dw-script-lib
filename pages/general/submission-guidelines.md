# Submission guidelines
Do you have a possible interesting and reusable script, then this page is for you. There are two options to submit:
 1. Using a Pull Request (technical)
 2. Using a GitHub Issue

## Submitting a Pull Request
1. The first step when submitting a new script, is to pull the code to your local machine (`git clone https://github.com/dlw-digitalworkplace/dw-script-lib.git`).
2. Create a new branch that starts from the `main` branch and use the following naming convention: `submission/[name-of-script]`
3. Create a new folder in the `PowerShell` or `ConsoleApps` folder (depending on what you are submitting). Give the folder a clear name (e.g. Add-UserToGroup, Update-SharePointListColumn, ...)
4. Copy the content `Example.md` file from the `/Templates` to your new folder. (The templates folder contains an example structure of how your scripts folder should look like)
5. Add the PowerShell script or Console app code to the same folder. Make sure the PS filename is the same as the `.md` file
6. Update the `.md` file with all the needed details
7. Update the `toc.yml` file in the `PowerShell` or `ConsoleApps` folder and include your new script. 
8. Navigate to GitHub and create a new pull request into the main branch. Select either the 'New PowerShell Script' or 'New Console Application' template and fill in all the details
9. Wait for approval 🎉

## Creating a GitHub Issue
1. Navigate to the [GitHub repo](https://github.com/dlw-digitalworkplace/dw-script-lib)
2. Go to "Issues" and create a new one
3. Select the 'Script Submission Request' template
4. Fill in all the template details. 
5. Someone from the `dw-script-lib` team will contact you 🎉

![Issue-Template](../images/issue-template.png)





