<!-- Readme.md v1.1.1.3
Biomatic (BIO)
created: 11 Aug 2018
updated: 03 Feb 2020 -->

<!-- Download on SpaceDock or Github or Curseforge. Also available on CKAN. -->

# Biomatic
## Simple biome identification, notification, tracking, and warp stopping. <br>
*adopted for curation by zer0Kerbal - originally by @Biff_Space*  
![Mod Version][shield:mod:latest] 
![KSP version][shield:ksp] ![KSP-AVC][shield:kspavc] ![License GPLv3][shield:license] ![][LOGO:gplv3]   
![SpaceDock][shield:spacedock] ![CKAN][shield:ckan] ![GitHub][shield:github] ![Curseforge][shield:curseforge]  
![Validate AVC .version files][shield:avcvalid]  
***
![Biomatic][IMG:hero:0]
[![.NET](https://github.com/zer0Kerbal/Biomatic/actions/workflows/dotnet.yml/badge.svg)](https://github.com/zer0Kerbal/Biomatic/actions/workflows/dotnet.yml)
The Biomatic sensor identifies the biome that the ship is in / above, and shows the information in a small text window. Optionally the ship's situation (high / low space, high / low flight, landed, splashed) can be shown as well. Biomes where science has been done can be added to a list, which is used together with a kill warp option to de-warp the ship when entering an un-listed biome. The list of biomes can be per-vessel, or global.

It is integrated with the stock toolbar by default, but can be configured to use blizzy's using [toolbar controller]

## The Biomatic Part

The Biomatic part is found under the science tab. In career or science mode, it is found in the 'Space Exploration' node of the tech tree - this must be researched before Biomatic will work, either as a part or as a Module Manager add-on to command pods.

## Module Manager Patch(es)
- adds Biomatic to any command module or probe core
- [OPTIONAL] makes Start the techRequired for the Biomatic part 

## Dependencies
- [x] [Kerbal Space Program][KSP:website] [![][shield:ksp]][KSP:website] ***may*** work on other versions (YMMV)
- [x] ***NEW*** [Toolbar Controller][thread:toolbarcontroller]
- [x] ***NEW*** [Click Through Blocker][thread:clickthroughblocker]

### Recommends
- [x] [Module Manager][thread:mm]
- [x] ***NEW*** [Toolbar][thread:toolbar]

### Installation 

The Biomatic folder needs to be unzipped and put in your KSP\GameData folder:

### Feedback

Any comments and suggestions for improvements are welcome, as are reports of bugs / problems - please let me know what you think.

### Code

- Window now disappears with the rest of the UI on F2
- If the part config file is edited to move the part to a different tech tree node, it will now behave sensibly rather than requiring 'spaceExploration' to be unlocked

### Licence
- ![GPLv3](https://www.gnu.org/graphics/gplv3-or-later-sm.png) [Software and Sourcecode Only License. This only covers the software and sourcecode. Original license.](https://www.gnu.org/licenses/gpl-3.0.html)
Biomatic, its associated files and the source code are all released under the GPL 3 licence, text here: http://www.gnu.org/licenses/gpl.txt.

![Screenshot](https://i.imgur.com/TrdnPPU.jpg)

[![Biomatic YouTube](http://img.youtube.com/vi/D3lBi38pTjU/0.jpg)](http://www.youtube.com/watch?v=D3lBi38pTjU "Biomatic in Action")

<a href="http://www.youtube.com/watch?feature=player_embedded&v=D3lBi38pTjU
" target="_blank"><img src="http://img.youtube.com/vi/D3lBi38pTjU/0.jpg" 
alt="Biomatic in Action" width="640" height="480" border="10" /></a>

![YouTube](https://youtu.be/D3lBi38pTjU)

<iframe width="935" height="527" src="https://www.youtube.com/embed/D3lBi38pTjU" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
### Original:
- ![Biomatic biome identifier](https://forum.kerbalspaceprogram.com/index.php?/topic/80379-*)
- ![SpaceDock](http://spacedock.info/mod/634)
- none known
***  
*red box below is a link to forum post on how to get support*  
[![How to get support][image:get-support]][thread:getsupport]

### License
#### aka Legal Mumbo Jumbo
Source: [GitHub][MOD:github:repo]  
License: ![License MIT][shield:license]  
> *** All bundled mods are distributed under their own licenses***<br>
> *** All art assets (textures, models, animations) are distributed under their own licenses*** 
### Original
[Thread][MOD:original:thread]  
[Download][MOD:original:download]  
Source: [GitHub][MOD:original:source]  
License: ![License MIT][shield:license]  
<!-- graphical links to downloads -->
[![][image:rel-github]][MOD:rel-github] [![][image:rel-spacedock]][MOD:rel-spacedock] [![][image:rel-curseforge]][MOD:rel-curseforge]  

*Be Kind: Lithobrake, not jakebrake! Keep your Module Manager up to date*

###### v1.3.3.7 original: 01 Oct 2019 zed'K | updated: 17 Mar 2020 zed'K

[MOD:license]:      https://github.com/zer0Kerbal/Biomatic/blob/master/LICENSE
[MOD:contributing]: https://github.com/zer0Kerbal/Biomatic/blob/master/.github/CONTRIBUTING.md
[MOD:issues]:       https://github.com/zer0Kerbal/Biomatic/issues
[MOD:wiki]:         https://github.com/zer0Kerbal/Biomatic/
[MOD:known]:        https://github.com/zer0Kerbal/Biomatic/wiki/Known-Issues
[MOD:forum]:        https://forum.kerbalspaceprogram.com/index.php?/topic/191426-*
[MOD:github:repo]:  https://github.com/zer0Kerbal/Biomatic/
[MOD:changelog]:    https://github.com/zer0Kerbal/Biomatic/Changelog.cfg
<!--- original mod stuff -->
[MOD:original:source]: http://spacedock.info/mod/634/Biomatic
[MOD:original:thread]: https://forum.kerbalspaceprogram.com/index.php?/topic/80379-*
[MOD:original:download]: http://spacedock.info/mod/634/Biomatic

[KSP:website]: http://kerbalspaceprogram.com/
[LOGO:gplv3]: https://i.postimg.cc/90kCDs7K/gplv3-48x17.png

[MOD:rel-github]: https://github.com/zer0Kerbal/Biomatic/releases/latest "GitHub"
[MOD:rel-spacedock]: http://spacedock.info/mod/634
[MOD:rel-curseforge]: https://www.curseforge.com/kerbal/ksp-mods/biomatic
[MOD:rel-ckan]: http://forum.kerbalspaceprogram.com/index.php?/topic/90246-*

[image:rel-github]:     https://i.imgur.com/RE4Ppr9.png
[image:rel-spacedock]:  https://i.imgur.com/m0a7tn2.png
[image:rel-curseforge]: https://i.postimg.cc/RZNyB5vP/Download-On-Curse.png
[image:get-support]:    https://i.postimg.cc/vHP6zmrw/image.png

[image:rel-ckan]:  https://i.postimg.cc/x8XSVg4R/sj507JC.png
[image:changelog]: https://i.postimg.cc/qM9p4V0C/changelog.png
[image:source]:    https://i.postimg.cc/tJ8GqW0H/source.png

[image:rel-github-sm]:     https://i.postimg.cc/1XXy5yfD/github.png
[image:rel-spacedock-sm]:  https://i.postimg.cc/DZ22Hrhj/spacedock.png
[image:rel-curseforge-sm]: https://i.postimg.cc/ZRVTSWKT/UVVt0OP.png
  
[shield:mod:latest]: https://img.shields.io/github/v/release/zer0Kerbal/Biomatic?include_prereleases?style=plastic
[shield:mod]: https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/zer0Kerbal/Biomatic/master/json/mod.json
[shield:ksp]: https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/zer0Kerbal/Biomatic/master/json/ksp.json
[shield:license]: https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/zer0Kerbal/Biomatic/master/json/license.json&logo=gnu
[shield:kspavc]:     https://img.shields.io/badge/KSP-AVC--supported-brightgreen.svg?style=plastic
[shield:spacedock]:  https://img.shields.io/badge/SpaceDock-listed-blue.svg?style=plastic
[shield:ckan]:       https://img.shields.io/badge/CKAN-Indexed-blue.svg?style=plastic
[shield:github]:     https://img.shields.io/badge/Github-Indexed-blue.svg?style=plastic&logo=github
[shield:curseforge]: https://img.shields.io/badge/CurseForge-listed-blue.svg?style=plastic
[shield:avcvalid]:    https://github.com/zer0Kerbal/Biomatic/workflows/Validate%20AVC%20.version%20files/badge.svg

[thread:mm]: http://forum.kerbalspaceprogram.com/index.php?/topic/50533-*
[thread:mc]: https://forum.kerbalspaceprogram.com/index.php?/topic/178484-*
[thread:kcl]: https://forum.kerbalspaceprogram.com/index.php?/topic/179207-*
[thread:sr]: https://forum.kerbalspaceprogram.com/index.php?/topic/179306-*
[thread:kct]: https://forum.kerbalspaceprogram.com/index.php?/topic/182877-*

[thread:toolbarcontroller]:   https://forum.kerbalspaceprogram.com/index.php?/topic/169509-*
[thread:clickthroughblocker]: https://forum.kerbalspaceprogram.com/index.php?/topic/170747-*
[thread:toolbar]:             https://forum.kerbalspaceprogram.com/index.php?/topic/161857-*

[thread:tweakscale]:           https://forum.kerbalspaceprogram.com/index.php?/topic/179030-*
[thread:Community Resource Pack]: http://forum.kerbalspaceprogram.com/index.php?/topic/83007-*
[thread:getsupport]: https://forum.kerbalspaceprogram.com/index.php?/topic/83212-*

[LINK:magico13]:       https://forum.kerbalspaceprogram.com/index.php?/profile/73338-magico13/
[LINK:severedsolo]:    https://forum.kerbalspaceprogram.com/index.php?/profile/80345-severedsolo/
[LINK:linuxgurugamer]: https://forum.kerbalspaceprogram.com/index.php?/profile/129964-linuxgurugamer/
[LINK:siriussame]:     https://forum.kerbalspaceprogram.com/index.php?/profile/116426-siriussam/
[LINK:enneract]:       https://forum.kerbalspaceprogram.com/index.php?/profile/56759-enneract/
[LINK:pehvbot]:               https://forum.kerbalspaceprogram.com/index.php?/profile/182810-pehvbot/
[LINK:zer0Kerbal]:     https://forum.kerbalspaceprogram.com/index.php?/profile/190933-zer0kerbal/

[IMG:hero:0]: https://spacedock.info/content/BiffSpace_2144/Biomatic/Biomatic-1462190469.105298.jpg "Biomatic"
[IMG:hero:1]: https://

<!--
this file: GPLv2 BY
zer0Kerbal-->
