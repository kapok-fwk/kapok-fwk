# Publishing Guide

1. Build the source with configuration Release e.g. in Visual Studio
2. Publish the packages via
``` ps1
PS> $VERSION = "0.1.1"
PS> $YOUR_GITHUB_PAT = "<your github public access key>"
PS> dotnet nuget push .\src\Kapok.Acl\bin\Release\Kapok.Acl.$VERSION.nupkg --api-key $YOUR_GITHUB_PAT --source "kapok-fwk"
PS> dotnet nuget push .\src\Kapok.Core\bin\Release\Kapok.Core.$VERSION.nupkg --api-key $YOUR_GITHUB_PAT --source "kapok-fwk"
PS> dotnet nuget push .\src\Kapok.Data.EntityFrameworkCore\bin\Release\Kapok.Data.EntityFrameworkCore.$VERSION.nupkg --api-key $YOUR_GITHUB_PAT --source "kapok-fwk"
PS> dotnet nuget push .\src\Kapok.DataPort\bin\Release\Kapok.DataPort.$VERSION.nupkg --api-key $YOUR_GITHUB_PAT --source "kapok-fwk"
PS> dotnet nuget push .\src\Kapok.Report\bin\Release\Kapok.Report.$VERSION.nupkg --api-key $YOUR_GITHUB_PAT --source "kapok-fwk"
PS> dotnet nuget push .\src\Kapok.Report.Razor\bin\Release\Kapok.Report.Razor.$VERSION.nupkg --api-key $YOUR_GITHUB_PAT --source "kapok-fwk"
PS> dotnet nuget push .\src\Kapok.Report.SqlServer\bin\Release\Kapok.Report.SqlServer.$VERSION.nupkg --api-key $YOUR_GITHUB_PAT --source "kapok-fwk"
PS> dotnet nuget push .\src\Kapok.View\bin\Release\Kapok.View.$VERSION.nupkg --api-key $YOUR_GITHUB_PAT --source "kapok-fwk"
```

See also:
* Github: [Working with the NuGet registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)
* Code Project: [Using Github as Private Nuget Package Server and Share Your Packages](https://www.codeproject.com/Tips/5292364/Using-Github-as-Private-Nuget-Package-Server-and-S)