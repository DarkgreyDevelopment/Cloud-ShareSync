# PreRequisites.
Because Cloud-ShareSync is a dotnet 6 application, the following example requires a minimum of [PowerShell v7.2](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.2).

# Config Validation:
> [!TIP]
> You can use `Cloud-ShareSync.dll` in PowerShell to validate your application config file prior to first use.
```powershell
Add-Type -Path '/Path/To/Cloud-ShareSync.dll'
$Config = [Cloud_ShareSync.Core.Configuration.ConfigManager]::GetConfiguration('/path/to/appsettings.json')
$Config
# If your configuration file is valid; you should see your config here.
# However if your configuration file is invalid then you will receive an error message from GetConfiguration instead.
# Use the following command to view the most recent error:
$Error[-1]
```
