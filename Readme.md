Kapok Framework
===============
Kapok is a software platform/framework based on .NET Core for building enterprise systems.

&nbsp;

Requirements
------------
* .NET Core 6.0 (LTS) or above

Add NuGet package source
------------------------

1. Create a [Github personal access token (PAT)](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token) with `read:packages` permissions.

2. Run the following command in your shell. This works on both Linux and Windows.

``` bash
dotnet nuget add source https://nuget.pkg.github.com/kapok-fwk/index.json -n kapok-fwk -u <GITHUB_USER_NAME> -p <GITHUB_USER_TOKEN> --store-password-in-clear-text
```

3. Now you are ready to go :tada: Install the kapok packages of your wish.
