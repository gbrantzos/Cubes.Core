# Host Configuration
Cubes host configuration controls loading of applications and assemblies during startup. 
Configuration is done using command line arguments or environment variables. 


#### Startup options

Currently the following options are available as command line parameters or environment variables:

Command Line Parameter | Environment Variable | Value
--- |--- | ---
--root | CUBES_ROOT | Points to the base folder where Cubes creates all needed folders.
--application | CUBES_APPLICATION | A path to an application manifest file. 
--admin | CUBES_ADMIN | Path to CubesManagement.zip file (web management UI).


#### Common Libraries
Assemblies under the folder root\Libraries are considered common libraries and loaded before all applications. 
Sub-folders named `win` and `unix` under common folder should contain platform specific assemblies.


#### Application Manifest
An application is defined by a manifest file:
```
Name: Pharmex
Active: true
Assemblies:
    -   Cubes.Pharmex.dll
```
Notes:
- An application marked as not `Active` will not be loaded. 
- A manifest file can define multiple assemblies to be loaded.
- The path of a manifest file is stored in `ManifestPath` property and is used for creating relative paths.
- Paths are treated first as absolute and if not found, as relative to the application's `ManifestPath`.


#### Library filtering
In application manifest file, we can define filtering for the assemblies that will be loaded, based on platform.

For example an assembly named `{os:win}` will only be loaded when running on Windows platform.