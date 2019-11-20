PACKAGEUPDATER
==============

Updates nugets packages version in the .csproj files in the solution directory.



Install
-------
0. Download [latest release.](https://github.com/PawKanarek/PackageUpdater/releases)
1. Unpack the .zip that contains PackageUpdater.exe
2. Manually add folder that contains PackageUpdater.exe to the PATH Environment Variable
or
2. Execute this in comand line tool. 
```sh
> packageupdater -p //Adds containing folder to PATH automatically.  
```


Usage
-----
Execute packageupdater.exe in command line tool
```sh
packageupdater [options]
```
where options are:

| Option | Description |
| ------ | ----------- | 
| -h | Displays help |
| --- | ---|
| -p | Add Current location to environment variable PATH |
| --- | --- |
| -u [package_name] [new_version] | Updates nuget packages to given version in current folder. e.g. '-u Xamarin.Forms 4.3.0.991211'. Where [packagename] is nuget package name reference (Program will use functions .ToLower() and .Cointains([packagename])) [new_version] new version to replace current.
