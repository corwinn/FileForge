
 File Forge
............

    A tool for crafting file formats.

    A visual file format editor. Edit a file format description while the file
  whose data is being described is being visualized (loaded or created anew).

    This is version 1.0 of the "File Forge".

    Who shall create the "File Foundry"?



 Compiling
...........

  Requires the "Library of the Wind": https://github.com/corwinn/WindLib
  Requires "NUnit", and the "DockPanel Suite".

  Make sure you have, at the source directory:
    * "3rd_party/testing/bin/net40/nunit.framework.dll"
    * "3rd_party/docking/WeifenLuo.WinFormsUI.Docking.dll"
    * "../Library of the Wind/bin/Debug/Wind.Controls.dll"
    * "../Library of the Wind/bin/Debug/Wind.Debug.dll"
  Make a build directory somewhere.
  Issue cmake at it: `cmake -G "Visual Studio 12 2013"`
  Load the just gnerated at the build directory solution in the "VS".
  You have to create *.settings, *.resx, and all other "special" files.
  *.designer.cs, if ever needed, will be created by a code generator of mine,
  excluding the resources part.
  "AssemblyInfo.cs" is intentionally already there. Also, you'll have to add the
  "AnyCpu" targets ("Debug\" and "Release\" output path), until I find out how
  to make "cmake" do that :)


 Credits
.........

 * "NUnit"
   https://nunit.org

 * "DockPanel Suite"
   https://github.com/dockpanelsuite/dockpanelsuite/releases/tag/Release_2.10



 Disclaimer
............

    All trademarks and/or service marks used in this project are the trademarks
  and/or service marks of their respective owners.
