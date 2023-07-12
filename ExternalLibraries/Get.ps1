$ProgressPreference = 'SilentlyContinue'

Invoke-WebRequest -Uri "https://assets.neos.com/install/Pro/Data/2022.1.28.1310_YTDLP.7z" -OutFile Neos.7z
7z x -y Neos.7z -oNeos
del Neos.7z

# Required for program to run.

mv Neos/Neos_Data/Managed/BaseX.dll ./ -Force
mv Neos/Neos_Data/Managed/CloudX.Shared.dll ./ -Force
mv Neos/Neos_Data/Managed/CodeX.dll ./ -Force

# Required for Unit Tests.

mv Neos/Neos_Data/Managed/Octokit.dll ./ -Force
mv Neos/Neos_Data/Managed/Ben.Demystifier.dll ./ -Force
rm -r ./Neos
