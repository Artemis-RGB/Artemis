To generate the [Artemis API docs](https://artemis-rgb.com/docs/) we use [DocFX](https://dotnet.github.io/docfx/).

To build locally run the following commands from this folder.

#### Want to build? Follow these instructions (Windows)
1. Ensure you can build the Artemis solution as per the [main build instructions](https://github.com/Artemis-RGB/Artemis#want-to-build-follow-these-instructions)
2. Install DocFX (with Chocolatey: `choco install docfx`)
3. Open PowerShell in  `<repo>\docfx` (the same folder as this readme file)
4. Run `docfx .\docfx_project\docfx.json` to build static files  
   Run `docfx .\docfx_project\docfx.json --serve` to serve locally
