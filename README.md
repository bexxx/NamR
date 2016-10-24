# NamR
[![Build status](https://ci.appveyor.com/api/projects/status/gpp0yxuh543tawpd?svg=true)](https://ci.appveyor.com/project/bexxx/namr)
Download this extension from the [VS Gallery](https://visualstudiogallery.msdn.microsoft.com/3dfe2dca-9a74-49b5-ac11-fd9b7af57e59)
or get the [CI build](http://vsixgallery.com/extension/A98A9358-9F24-4407-AAB7-5871243606AA/).

---------------------------------------

This extension will provide the IntelliSense completion for naming things in C#. That's right, no more typing cancellationToken in e.g. a parameter declaration.
Next steps are variable declarations and other places as well as quick fix support for already named things.

See the [changelog](CHANGELOG.md) for changes and roadmap.

## Features

- IntelliSense for parameter names
 
![Param Intellisense](Images/param_intellisense.png)

- IntelliSense name proposals for properties

![Propertyname](Images/propertyname.png)

- IntelliSense parameter name proposals for constructor arguments

![Ctorparams](Images/ctorparams.png)


## Contribute
Check out the [contribution guidelines](CONTRIBUTING.md)
if you want to contribute to this project.

For cloning and building this project yourself, make sure
to install the
[Extensibility Tools 2015](https://visualstudiogallery.msdn.microsoft.com/ab39a092-1343-46e2-b0f1-6a3f91155aa6)
extension for Visual Studio which enables some features
used by this project.

## License
[MIT](LICENSE)