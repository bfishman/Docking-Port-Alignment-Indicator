**--------------------------------**
||Docking Port Alignment Indicator||
||--------------------------------||
||---------Version 6.8.2(.2-mkw)--||
||-------Author: NavyFish---------||
**-------- Bryan Fishman----------**

*** NOTE ************************************************************
This is an unofficial release by Micha.
Please do NOT raise bug-reports from this version to NavyFish.
*********************************************************************


Installation Instructions:
  
  *************************************IMPORTANT!!********************************************
  ********************************************************************************************
  **                                                                                        **
  **      Prior to installation, make sure to delete all previous versions of this mod!     **
  **                                                                                        **
  ********************************************************************************************
  *************************************IMPORTANT!!********************************************


  -Simply Extract the included GameData folder into your KSP directory and merge the contents.
   (The included Source directory does not need to be extracted.)

   
Installation Directory Structure should look like this:


KSP Install Directory
 |
 +GameData
 |  |
 |  |...
 |  |
 |  +NavyFish
 |  |  |...
 |  | 
 |  +ModuleManager.X.Y.Z.dll

 
Changelog:

Version 6.8.2(.2-mkw):
[updated] Recompiled for KSP 1.5.1
[added]   Added switching between the Toolbar (Stock vs Blizzy) in the settings menu.
[fixed]   Fixed incorrect usage of Unity APIs in field initializers.

Version 6.8.2:
[updated] Recompiled for KSP 1.4.2

Version 6.8.1:
[updated] Recompiled for KSP 1.4.1

Version 6.8:
[updated] Recompiled for KSP 1.4.0
[fixed] Application Launcher Icon no longer hangs around the main menu... for reals this time

Version 6.7.1:
[fixed] Application Launcher Icon no longer hangs around the main menu.

Version 6.7.0:
[fixed] Incremented version and published new .version file for CKAN/AVC compatibility

Version 6.6.0:
[updated] Recompiled for KSP 1.3.0

Version 6.5.2:
[updated] Updated release package for KSP 1.2.2 (No code changes)

Version 6.5.1:
[fixed] Fix for Blizzy Toolbar's bug exposed by Contract Configurator
[fixed] Fixed Reference Port text on RPM gauge (and incremented version text)

Version 6.5:
[updated] Recompiled to work with KSP 1.2

Version 6.4:
[added]   New gauge foreground artwork by community member Devric. Sweet!
[added]   You can now invert each alignment, translation, and roll axis via the settings menu.
[change]  Unified GUI and RPM rendering paths: Cleaner, faster mod.
[updated] Re-added support for Blizzy's Toolbar mod. If present, icon will default to Blizzy's toolbar.
            The icon can be forced to use the stock app launcher by editing 'DPAI Settings.cfg'
			
			
Version 6.3:
[update]  Rebuilt for KSP v1.1
[change]  Incorporated new gauge foreground artwork by community member Devric.


Version 6.2.1 (BETA):
[update]  Works with KSP 1.1 Pre-Release build v1.1.0.1183
[fixed]   IVA reference part is now properly maintained when switching between IVA/EVA/MapView
[change]  ModuleDockingNodeNamed now no longer relies upon DockingPortAlignmentIndicator.dll, 
          allowing mod authors to use its functionality without depending upon DPAI. Simply 
		  check if a part contains a ModuleDockingNodeNamed partmodule, and if so call module.getCustomName().


Version 6.2:
[fixed]   Stock toolbar button no longer multiplies like a rabbit!


Version 6.1:
[fixed]   DPAI *now* preserves the 'Control From Here' reference part when going IVA  


Version 6.0:
[added]   Raster Prop Monitor (RPM) Integration! (See Below)
[RPM]	  DPAI now has an RPM Page on the stock RPM MFD. More MFD patches to follow.
[RPM]     Full DPAI functionality and display while IVA! (RPM Required)
[RPM]     Display and cycle Target Ports from MDF
[RPM]     Display and cycle the Docking Reference part from MFD
[RPM]     Rename target and reference ports from MFD while IVA
[RPM]     Toggleable info page which displays MFD controls for DPAI
[added]   DPAI now preserves the 'Control From Here' reference part when going IVA    
[added]   Option to show/hide target port HUD indicator while IVA
[added]   Parts with multiple docking ports can now have unique names for each port
[change]  Appearance of 'point towards' arrow (smaller, higher-resolution)
[fixed]   Improved CDI line transition to back-hemisphere (no more unintentional flipping)


Version 5.1:
[changed] Re-enabled support for Blizzy's Toolbar. If present, DPAI will place its button on Blizzy's toolbar.
          DPAI can be forced to place its button on the Stock App Launcher via a config file (see bundled Readme.txt)

		  
Version 5.0:
[added]   Integration with stock application launcher (removed support for blizzy's toolbar)
[change]  DPAI can now be displayed without target selected. Use stock toolbar button to show/hide indicator
[added]   Setting to enable/disable cycling and automatic targeting of ports on target vessel (default: enabled)
[added]   Setting to exclude 'already docked' ports when cycling through available targets (default: enabled)
[added]   Setting to show/hide Target Port HUD Icon and change its size
[added]   KSP AVC integration (Mini-AVC bundled)


Version 4.0:
[added] Ability to cycle through all unoccupied ports on your target. No more need to right-click a port!
[added] Ports can now be targeted up to 2.25 Km away!
[added] When a vessel is first targeted, the nearest port is automatically targeted.
[added] Magenta HUD indicator floats on top of currently targeted port to allow for easy identification.
[added] Ability to rename all docking ports using the right-click GUI (in both VAB/SPH and flight). 
[added] Text readout on indicator which displays the name of the targeted port.
[added] Closure Distance (CDST) readout, displays range to target port along the approach axis only.
[added] Prograde icon on velocity vector changes to Retrograde icon when CVEL is negative.
[added] Support for parts with multiple docking ports (Thanks taniwha!)
[added] Support for Extraplanetary Launchpads' recycle bins (Thanks taniwha!)
[change] New toolbar button icon, easier to identify. Button will show/hide indicator.
[change] Removed hard-dependency on Blizzy's Toolbar
[change] Added custom 'toolbar' button if Blizzy's toolbar is not present.
[change] Text now drawn using Bitmap Fonts, allowing for higher resolution text.
[fixed] Indicator no longer displayed in Map View or while on EVA
[fixed] Generalized port orientation logic - non-stock lateral ports should now work (Thanks taniwha!)


Version 3.0:
[update] Verified compatibility with KSP v0.23
[added]  Indicator's size can now be scaled freely (from settings window)
[added]  Toolbar-Plugin miniature icon for toggling the settings window
[added]  Glyph Font rendering to support indicator scaling
[fixed]  Indicator will now display at full resolution regardless of KSP's Texture Settings


Version 2.2:
[update] Verified compatibility with KSP v0.22
[change] Roll degrees now range from 0-359.9 (was -180 to +180 before)
[fixed] Stock lateral docking ports (Inline Clampotron) now orient correctly!
[fixed] Settings window toggle-buttons now much more visible


Version 2.1:
[fixed] A bug that caused the velocity vector to behave incorrectly in certain circumstances [thanks Mr Shifty].
[fixed] CVEL now measures velocity along the target port's normal vector (intended behavior), not true velocity.
[fixed] Negated CVEL value when passing into back-hemisphere (red CDI region). Closure is now always a positive value.
[change] Velocity Indicator now draws on top of of Alignment Vector.
[change] Reduced sensitivity of alignment indicator near center of the gauge slightly.
[change] Removed forward velocity component of the velocity vector. Now full deflection always indicates 3.5 m/s of lateral velocity.
[change] Added slight exponential scaling to velocity vector near center of gauge.
[change] Velocity Indicator will display as a 'retrograde' vector when your alignment is greater than 90 degrees off-axis, indicating that translation controls have a reversed impact on the movement of the visual indicator. Moving the indicator towards the green cross will still bring you towards centerline.


Version 2.0:
[added] Course Deviation Indicators (CDI)
[added] Translational Velocity vector
[added] Closure velocity and distance readouts
[added] settings menu
[added] Increased precision of alignment indicator with exponential scaling
[change] Enhanced precision of roll indicator
[change] Retouched all graphics

Version 1.0: First Release

-------------------Contributions--------------------------------------------------------------

Guage foreground artwork contributed by community member Devric as of version 6.4. Huge props!

--------------------Disclosures---------------------------------------------------------------

This mod uses MiniAVC (https://github.com/CYBUTEK/KSPAddonVersionChecker) to provide automatic
version checking. If you opt-in, it will use the internet to check whether there is a new version 
available. Data is only read from the internet and no personal information is sent. For a more 
comprehensive version checking experience, please download the full KSP-AVC Plugin.

This mod is packed with and requires the use of Module Manager in order to provide renamable 
docking ports as well as RPM integration. (https://github.com/sarbian/ModuleManager)

-------------------License Information---------------------------------------------------------
 
    Copyright (C) 2014, Bryan Fishman
    
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
   
    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
 
    Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
    project is in no way associated with nor endorsed by Squad.
