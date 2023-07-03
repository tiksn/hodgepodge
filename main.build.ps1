<#
.Synopsis
	Build script invoked by Invoke-Build.

.Description

#>

param(
)

$data = Import-PowerShellDataFile -Path .\main.build.psd1

task Test Build, {
    $project = Resolve-Path -Path $data.TestProject
    $project = $project.Path
    Exec { dotnet test $project }
    Exec { dotnet run --project $project }
}

task Build Format, {
    $solution = Resolve-Path -Path $data.Solution
    $solution = $solution.Path
    Exec { dotnet restore $solution }
    Exec { dotnet build $solution }
}

task Format Clean, {
    Exec { dotnet tool restore }
    Exec { dotnet fantomas . }
}
task Clean Init, {
    $solution = Resolve-Path -Path $data.Solution
    Exec { dotnet clean $solution }
}

task Init {
    # $trashSubFolder = Get-Date -Format 'yyyyMMHHmmss'
    # $script:trashFolder = Join-Path -Path . -ChildPath '.trash'
    # $script:trashFolder = Join-Path -Path $script:trashFolder -ChildPath $trashSubFolder
    # New-Item -Path $script:trashFolder -ItemType Directory | Out-Null
    # $script:trashFolder = Resolve-Path -Path $script:trashFolder
}
