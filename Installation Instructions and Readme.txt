**--------------------------------**
||Docking Port Alignment Indicator||
||--------------------------------||
||----------Version 5.0-----------||
||-------Author: NavyFish---------||
**-------- Bryan Fishman----------**


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

Version 5.0:
[added]   Integration with stock application launcher (removed support for blizzy's toolbar)
[changed] DPAI can now be displayed without target selected. Use stock toolbar button to show/hide indicator
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
[changed] New toolbar button icon, easier to identify. Button will show/hide indicator.
[changed] Removed hard-dependency on Blizzy's Toolbar
[changed] Added custom 'toolbar' button if Blizzy's toolbar is not present.
[changed] Text now drawn using Bitmap Fonts, allowing for higher resolution text.
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
[changed] Roll degrees now range from 0-359.9 (was -180 to +180 before)
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


--------------------Disclosure-----------------------------------------------------

This mod includes version checking using MiniAVC. If you opt-in, it will use the internet to check 
whether there is a new version available. Data is only read from the internet and no personal information
is sent. For a more comprehensive version checking experience, please download the KSP-AVC Plugin.

--------------------License Information--------------------------------------------
 
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
