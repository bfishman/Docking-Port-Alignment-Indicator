# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [{VERSION}] for KSP {KSPVERSION} - {DATE}

## [6.8.6] - 2020-02-13
### Changed
- Recompiled for KSP 1.9
- Separated Changelog out from Readme.
### Fixed
- Docking ports attached in VAB were not being ignored (#3)


## [6.8.5] - 2019-12-14
### Changed
- Recompiled for KSP 1.8.1
- Defaults to using Stock Toolbar.
### Fixed
- Stock Toolbar icon showing up in different scenes (main menu).


## [6.8.4] - 2019-11-30
### Changed
- Merged all downstream 'mkw' changes into Master Brnach
- Rebuilt bitmap font file to fix kerning. More work needs to be done, however.


## [6.8.3(.4-mkw)]
### Changed
- Merged upstream v6.8.3 into my branchline, keeping my modifications


## [6.8.3] - 2019-03-03
### Changed
- Recompiled for KSP 1.6.1
### Fixed
- RPM MFD Display would not work unless DPAI UI app was toggled visible at least once
- Module Manager warnings


## [6.8.2(.3-mkw)]
### Changed
- Removed "DPAI Settings.cfg"
### Fixed
- Minor code refactor which removed a potential null-reference exception.


## [6.8.2(.2-mkw)]
### Added
- Added switching between the Toolbar (Stock vs Blizzy) in the settings menu.
### Changed
- Recompiled for KSP 1.5.1
### Fixed
- Fixed incorrect usage of Unity APIs in field initializers.


## [6.8.2] - 2018-04-12
### Changed
- Recompiled for KSP 1.4.2


## [6.8.1] - 2018-03-14
### Changed
- Recompiled for KSP 1.4.1


## [6.8.0] - 2018-03-10
### Changed
- Recompiled for KSP 1.4.0
### Fixed
- Application Launcher Icon no longer hangs around the main menu... for reals this time


## [6.7.1]
### Fixed
- Application Launcher Icon no longer hangs around the main menu.


## [6.7.0] - 2017-05-31
### Fixed
- Incremented version and published new .version file for CKAN/AVC compatibility


## [6.6.0]
### Changed
- Recompiled for KSP 1.3.0


## [6.5.2] - 2017-01-04
### Changed
- Updated release package for KSP 1.2.2 (No code changes)


## [6.5.1] - 2016-10-25
### Fixed
- Fix for Blizzy Toolbar's bug exposed by Contract Configurator
- Fixed Reference Port text on RPM gauge (and incremented version text)


## [6.5.0] - 2016-10-14
### Changed
- Recompiled to work with KSP 1.2


## [6.4.0] - 2016-05-12
### Changed
- New gauge foreground artwork by community member Devric. Sweet!
- You can now invert each alignment, translation, and roll axis via the settings menu.
- Unified GUI and RPM rendering paths: Cleaner, faster mod.
- Re-added support for Blizzy's Toolbar mod. If present, icon will default to Blizzy's toolbar.
  The icon can be forced to use the stock app launcher by editing 'DPAI Settings.cfg'


## [6.3.0] - 2016-04-20
### Changed
- Rebuilt for KSP v1.1
- Incorporated new gauge foreground artwork by community member Devric.


## [6.2.1] (BETA)
### Changed
- Works with KSP 1.1 Pre-Release build v1.1.0.1183
- ModuleDockingNodeNamed now no longer relies upon DockingPortAlignmentIndicator.dll, 
  allowing mod authors to use its functionality without depending upon DPAI. Simply 
  check if a part contains a ModuleDockingNodeNamed partmodule, and if so call module.getCustomName().
### Fixed
- IVA reference part is now properly maintained when switching between IVA/EVA/MapView


## [6.2.0] - 2015-05-15
### Fixed
- Stock toolbar button no longer multiplies like a rabbit!


## [6.1.0] - 2015-03-18
### Fixed
- DPAI *now* preserves the 'Control From Here' reference part when going IVA  


## [6.0.0]
### Added
- Raster Prop Monitor (RPM) Integration! (See Below)
  - [RPM] DPAI now has an RPM Page on the stock RPM MFD. More MFD patches to follow.
  - [RPM] Full DPAI functionality and display while IVA! (RPM Required)
  - [RPM] Display and cycle Target Ports from MDF
  - [RPM] Display and cycle the Docking Reference part from MFD
  - [RPM] Rename target and reference ports from MFD while IVA
  - [RPM] Toggleable info page which displays MFD controls for DPAI
- DPAI now preserves the 'Control From Here' reference part when going IVA    
- Option to show/hide target port HUD indicator while IVA
- Parts with multiple docking ports can now have unique names for each port
### Changed
- Appearance of 'point towards' arrow (smaller, higher-resolution)
### Fixed
- Improved CDI line transition to back-hemisphere (no more unintentional flipping)


## [5.1.0] - 2015-01-11
### Changed
- Re-enabled support for Blizzy's Toolbar. If present, DPAI will place its button on Blizzy's toolbar.  DPAI can be forced to place its button on the Stock App Launcher via a config file (see bundled Readme.txt)

		  
## [5.0.0] - 2015-01-10
### Added
- Integration with stock application launcher (removed support for blizzy's toolbar)
- Setting to enable/disable cycling and automatic targeting of ports on target vessel (default: enabled)
- Setting to exclude 'already docked' ports when cycling through available targets (default: enabled)
- Setting to show/hide Target Port HUD Icon and change its size
- KSP AVC integration (Mini-AVC bundled)
### Changed
- DPAI can now be displayed without target selected. Use stock toolbar button to show/hide indicator


## [4.0.0] - 2014-06-30
### Added
- Ability to cycle through all unoccupied ports on your target. No more need to right-click a port!
- Ports can now be targeted up to 2.25 Km away!
- When a vessel is first targeted, the nearest port is automatically targeted.
- Magenta HUD indicator floats on top of currently targeted port to allow for easy identification.
- Ability to rename all docking ports using the right-click GUI (in both VAB/SPH and flight). 
- Text readout on indicator which displays the name of the targeted port.
- Closure Distance (CDST) readout, displays range to target port along the approach axis only.
- Prograde icon on velocity vector changes to Retrograde icon when CVEL is negative.
- Support for parts with multiple docking ports (Thanks taniwha!)
- Support for Extraplanetary Launchpads' recycle bins (Thanks taniwha!)
### Changed
- New toolbar button icon, easier to identify. Button will show/hide indicator.
- Removed hard-dependency on Blizzy's Toolbar
- Added custom 'toolbar' button if Blizzy's toolbar is not present.
- Text now drawn using Bitmap Fonts, allowing for higher resolution text.
### Fixed
- Indicator no longer displayed in Map View or while on EVA
- Generalized port orientation logic - non-stock lateral ports should now work (Thanks taniwha!)


## [3.0]
### Added
- Indicator's size can now be scaled freely (from settings window)
- Toolbar-Plugin miniature icon for toggling the settings window
- Glyph Font rendering to support indicator scaling
### Changed
- Verified compatibility with KSP v0.23
### Fixed
- Indicator will now display at full resolution regardless of KSP's Texture Settings


## [2.2]
### Changed
- Verified compatibility with KSP v0.22
- Roll degrees now range from 0-359.9 (was -180 to +180 before)
### Fixed
- Stock lateral docking ports (Inline Clampotron) now orient correctly!
- Settings window toggle-buttons now much more visible


## [2.1]
### Changed
- Velocity Indicator now draws on top of of Alignment Vector.
- Reduced sensitivity of alignment indicator near center of the gauge slightly.
- Removed forward velocity component of the velocity vector. Now full deflection always indicates 3.5 m/s of lateral velocity.
- Added slight exponential scaling to velocity vector near center of gauge.
- Velocity Indicator will display as a 'retrograde' vector when your alignment is greater than 90 degrees off-axis, indicating that translation controls have a reversed impact on the movement of the visual indicator. Moving the indicator towards the green cross will still bring you towards centerline.
### Fixed
- A bug that caused the velocity vector to behave incorrectly in certain circumstances [thanks Mr Shifty].
- CVEL now measures velocity along the target port's normal vector (intended behavior), not true velocity.
- Negated CVEL value when passing into back-hemisphere (red CDI region). Closure is now always a positive value.


## [2.0]
### Added
- Course Deviation Indicators (CDI)
- Translational Velocity vector
- Closure velocity and distance readouts
- settings menu
- Increased precision of alignment indicator with exponential scaling
### Changed
- Enhanced precision of roll indicator
- Retouched all graphics


## [1.0]
### Added
- First Release
