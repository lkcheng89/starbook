ASCOM Driver for Vixen Starbook
===============================

This repository provides the ASCOM driver for Vixen Starbook series telescope controllers. This driver is developed and tested with Starbook S, and it should be also supported Starbook & Starbook TEN.

Prerequisites
--------------

In order to use this driver you need to install:

* .NET Framework 4 or higher version (https://dotnet.microsoft.com/download)

    Please ensure this is installed on system or ASCOM platform may not work correctly.

* ASCOM Platform 6.2 or higher version (https://ascom-standards.org/)

  **↓ Users do not have to install items below. These are required only for developers ↓**

* ASCOM Platform Developer Components 6.2 or higher version (https://ascom-standards.org/Developer/DriverImpl.htm)

    Please install this to build and register driver.

* Microsoft Visual Studio 2017 or higher version (https://visualstudio.microsoft.com/)

    Please install this to build project in C# 7.0.

* Inno Setup (https://jrsoftware.org/isinfo.php)

    Please install this to create setup program that registers driver for both 32- and 64-bits platforms.

Installing
----------

Execute **StarbookDriver-v<MajorVersion>.<MinorVersion>.exe**, follow the instruction of setup program and everything will be done automatically. The name of telescope driver will be called **Starbook** in chooser dialog of ASCOM platform.

Building
--------

Compile source code
^^^^^^^^^^^^^^^^^^^
Launch Microsoft Visual Studio and open **Starbook.sln** located at the root directory, build solution from the menu "Build > Build Solution" or accelerator key "F6".
::
    Suggest "Run as administrator" because "Register for COM interop" requires administrator's privileges in Debug mode.

Register ASCOM driver
^^^^^^^^^^^^^^^^^^^^^
$(FrameworkDir)\\regasm.exe /codebase bin\\Release\\ASCOM.Starbook.Telescope.dll
::
    * $(FrameworkDir) is located at C:\Windows\Microsoft.NET\Framework\v4.0.30319 for 32-bits platform;
    * $(FrameworkDir) is located at C:\Windows\Microsoft.NET\Framework64\v4.0.30319 for 64-bits platform.

*This step is ommitted when build in Debug mode, this is required only when build in Release mode.*

Unregister ASCOM driver
^^^^^^^^^^^^^^^^^^^^^^^
$(FrameworkDir)\\regasm.exe -u bin\\Release\\ASCOM.Starbook.Telescope.dll

Create setup program
^^^^^^^^^^^^^^^^^^^^
Launch Inno Setup and open **StarbookSetup.iss** located at the root directory, click **Run** button or accelerator key "F9" to create the setup program named **StarbookDriver-v<MajorVersion>.<MinorVersion>.exe**.

Usage
-----

Report me an issue if you find any problem during your usage. Stay up a night with clear sky.
