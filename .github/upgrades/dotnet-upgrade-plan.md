# .NET 8.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 8.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 8.0 upgrade.
3. Convert Ognmp\Ognmp.csproj to SDK-style project format.
4. Upgrade Ognmp\Ognmp.csproj to .NET 8.0.

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

No projects are excluded from this upgrade.

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### Ognmp\Ognmp.csproj modifications

Project properties changes:
  - Target framework should be changed from `net48` to `net8.0-windows`
  - Project file needs to be converted from legacy format to SDK-style format

Other changes:
  - This is a Windows Forms or WPF application targeting .NET Framework 4.8. The project will need to be converted to SDK-style and retargeted to net8.0-windows to maintain Windows-specific functionality.
  - The project uses Windows Services (System.ServiceProcess), which is fully supported in .NET 8.0 with the System.ServiceProcess.ServiceController package.
  - All existing code including ServiceController usage should continue to work after conversion and retargeting.
