---
permalink: /Changelog.html
title: The Change Log
description: The Opening Credits, and the closing credits, plus the first of two (or is three) end credit scenes
# layout: bare
tags: changes,changelog,change-log,page,kerbal,ksp,zer0Kerbal,zedK
---
<!-- hdr-changelog.md v1.0.0.1
MOD-NAME (ABBV)
created: 13 May 2022
updated: 05 Nov 2022
CC BY-ND 4.0 by zer0Kerbal -->  
# Changelog  
  
| modName    | Biomatic (BIO) by Biff Space                                      |
| ---------- | ----------------------------------------------------------------- |
| license    | GPL-3.0+ARR                                                       |
| author     | BiffSpace and zer0Kerbal                                          |
| forum      | (https://forum.kerbalspaceprogram.com/index.php?/topic/191426-*/) |
| github     | (https://github.com/zer0Kerbal/Biomatic)                          |
| curseforge | (https://www.curseforge.com/kerbal/ksp-mods/Biomatic)             |
| spacedock  | (https://spacedock.info/mod/634)                                  |
| ckan       | Biomatic                                                          |

## Version 1.8.0.0-release `<future>` edition

* Recompile for KSP 1.12.1

---

## Version 1.7.0.0-release `<future>` edition

* Recompile for KSP 1.11.1

---

## Version 1.6.0.0-release `<future>` edition

* Recompile for KSP 1.10.1

---

## Version 1.5.0.0-release `<future>` edition

* Recompile for KSP 1.9.1
* Updated to .NET 4.8
* Unity 2019

---

## Version 1.4.0.0-release `<future *Beep*>` edition

* Recompile for KSP 1.8.1
* Updated to .NET 4.8
* Unity 2019

---

## Version 1.3.3.7-release `<The Last 1.7.x version (stable)>` edition

* Updated
  * KSP 1.7.3
  * .NET 3.5
  * Unity 2017 1f1

* NO FURTHER planned 1.7.x versions will be provided
* updated to automated build process (deploy.bat/buildRelease.bat/AssemblyVersion.tt)
* fix buildRelease.batch file
* added to CurseForge
* updated
  * SpaceDock
  * CKAN
    * and modernized backend processes
    * to latest version of Kerbal Changelog
    * Biomatic.cfg patch, it might work now. :D

### Code 1.3.3.7

* removed <bool _3D> parameter from <AudioClip.Create("beep"> in <MakeBeep()>
* Added Toolbarcontroller and Clickthroughblocker code and as dependencies (thank you linuxgurugamer)
* Added ClickThroughBlocker (thank you linuxgurugamer)
* Added ToolbarController (thank you linuxgurugamer)
* added localization to InstallChecker.cs
* Commented out all stock toolbar code (thank you linuxgurugamer)
* Commented out all Blizzy toolbar code (thank you linuxgurugamer)
* Commented out entire ToolbarWrapper.cs file (thank you linuxgurugamer)
* deleted all commented out toolbar code
* added GameSettings.cs
  * added Global setting: useAlternateSkin (working)
  * added Global setting: hideWhenPaused (working)
  * added Global setting: colorPAW = true (not fully implemented yet)
  * added Global setting: UsesEC = true (not yet)
  * honors CheatOptions.InfiniteElectricity (unknown)
  * *should* work properly in TimeWarp (unknown)
* Editor Part Info
  * added in Part Module information that shows in editor
  * localized
  * should accurately reflect resource consumption

---

## Version 1.3.2.0-release `<Of course we speak English for KSP 1.7.3>` edition

* Recompile
  * for KSP 1.7.3
  * with .NET 3.5

### Code 1.3.2.0

* code localization
  * English (en-us.cfg)
  * provided by [tinygrox](https://github.com/tinygrox). Thank you.
* updated projects/issues on GitHub
* code clean, update pass
* updated ToolbarWrapper.cs
* added InstallChecker.cs
* added AssemblyVersion.tt
* added Log.cs
* added Utility.cs
* added issue templates to github

---

## Version 1.3.1.0-release `<Land Ho!>` edition

### Adopted by [zer0Kerbal](https://github.com/zer0Kerbal)

* updated
  * KSP 1.8.1
  * .NET 4.8
  * Unity 2019x

* License: GNU(GPL v3)
* reformatted changelog into KerbalChangeLog formating
* moved Changelog into separate file
* added .zip .rar and .7z (and various other things) to .gitignore
* update patch to include ":NEEDS"
* added optional patch to change Biomatic techRequired to start
* converted .tga -> png -> dds (DT5)
* created new Forum thread

--

## Version 1.3.0.0-release `<Released on 2016-10-14>` edition

* UPDATED (2016-10-14), v1.3.0.0
* for Kerbal Space Program 1.2.2
* Recompiled with KSP 1.2 binaries

* Added option for per-vessel or global biome list

---

## Version 1.2.0.0-release `<Released on 2016-06-23>` edition

* for Kerbal Space Program 1.1.3
* UPDATED (2016-06-23), v1.2.0.0
* Recompiled with KSP 1.1.3 binaries
* Available in map mode

---

## Version 1.1.0.8-release `<Released on 2016-06-09>` edition

* for Kerbal Space Program 1.1.2
* UPDATED (2016-06-09), v1.1.0.8
* Added sounds (on entering unticked biome, on entering any biome, or no sound options)
* Removed log file debug info

---

## Version 1.1.0.6-release `<Released on 2016-05-18` edition

* for Kerbal Space Program 1.1.2
* UPDATED (2016-05-18), v1.1.0.6

* Window now disappears with the rest of the UI on F2
* If the part config file is edited to move the part to a different tech tree node, it will now behave sensibly rather than requiring 'spaceExploration' to be unlocked

---

## Version 1.1.0.1-release `<Released on 2016-05-02>` edition

* for Kerbal Space Program 1.1.2

---