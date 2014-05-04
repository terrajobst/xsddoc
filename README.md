# Usage

 1. Install [Sandcastle Help File Builder (SHFB)][shfb]

    After the installation, you need to reboot the machine
    to make sure the new environment variables become
    available

 2. Install [XML Schema Documenter (XsdDoc)][xsddoc]

 3. Read the [documentation][docs]

# Development

## Prerequisites

 1. [Microsoft Visual Studio 2013 (Professional, Premium or Ultimate)][vs].

 2. [Sandcastle Help File Builder (SHFB)][shfb]
   
 3. [Windows Installer XML (WiX) toolset Version 3.8][wix]
  
## Building

1. Run `build.cmd`

2. The folder `bin` will now contain the following subfolders:
   - `help`. Contains the  help file.
   - `raw`. Contains the binaries (compiled in `Release` configuration).
   - `release`. Contains the zipped setup, samples, and source code.
   - `samples`. Contains several sample .chm files.
   - `setup`. Contains the setup.   
   - `src`. Contains the complete source code.

[shfb]: http://shfb.codeplex.com/releases
[xsddoc]: http://xsddoc.codeplex.com/releases
[docs]: http://xsddoc.codeplex.com/documentation
[vs]: http://msdn.microsoft.com/en-us/vstudio/default.aspx
[wix]: http://wix.codeplex.com/releases/view/115492

## To be done

- xmlEntityLink: should render bold when the target is the current topic.
- Investigate how we can use shared content for localization
- Consider qualifying names within parents/usages so that they are unqiue.
- Redefinition?
- Substitution groups?