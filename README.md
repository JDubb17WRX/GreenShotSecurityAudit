Greenshot - restricted destination build
========================================

This repository contains a Windows-focused Greenshot fork that has been trimmed to a smaller and safer destination surface.

What this build supports
------------------------

This build only allows the following capture destinations:

* Save As
* Open in image editor
* Copy to clipboard
* Microsoft Outlook

All other built-in and plugin-backed export destinations are hidden, blocked, or excluded from the installer for this fork.

Why this fork is different
--------------------------

Upstream Greenshot supports many more export targets and plugins. This fork intentionally narrows that surface area to reduce complexity, lower risk, and keep the installed application focused on the approved workflows above.

The reduced build currently does the following:

* Enforces a central destination allowlist at runtime.
* Sanitizes saved destination configuration so removed exports cannot be re-enabled by stale settings.
* Limits the destination picker to the approved destinations only.
* Keeps the Office plugin, but exposes Outlook only.
* Restricts runtime plugin activation to the Office plugin.
* Installs the Office plugin as the only bundled plugin, without exposing extra plugin choices in the installer.

Supported user workflows
------------------------

Greenshot in this repository is intended for users who need to:

* capture screenshots quickly,
* annotate or redact them in the editor,
* save them through a Save As dialog,
* copy them to the clipboard, or
* attach them to Microsoft Outlook.

Build requirements
------------------

* Windows
* Visual Studio 2022 or newer
* .NET Framework 4.8.x targeting support
* .NET SDK 9.0.x for `dotnet msbuild` builds

Build instructions
------------------

Use either Visual Studio or the command line:

* Open `src/Greenshot.sln` and build the `Release` configuration.
* Or run:

```powershell
dotnet msbuild src/Greenshot.sln /p:Configuration=Release /restore /t:Rebuild
```

Build output
------------

The release installer is written to the `installer` directory at the repository root.

Notes for developers
--------------------

* This fork is intentionally more restrictive than upstream. If you add a new destination or plugin, update the allowlist, UI, and installer behavior together.
* Some upstream projects may still build as part of the full solution even when their destinations are not exposed at runtime. Runtime behavior and shipped installer contents are the source of truth for this fork.

About upstream Greenshot
------------------------

The upstream project is available at:

* https://github.com/greenshot/greenshot
* https://getgreenshot.org/

If you are looking for the full upstream feature set, use the official Greenshot project instead of this restricted fork.
