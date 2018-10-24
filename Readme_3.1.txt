**--------------------------------**
||Docking Port Alignment Indicator||
||--------------------------------||
||----------Version 3.1-----------||
||-------Author: NavyFish---------||
**--------------------------------**

Thanks for trying out this mod! Check out the forum thread if you have any questions 
or would like to make suggestions.

http://forum.kerbalspaceprogram.com/threads/43901-0-23-Docking-Port-Alignment-Indicator-%28Version-3-0-Updated-12-18-13%29

Installation:

  
  *************************************IMPORTANT!!********************************************
  ********************************************************************************************
  **                                                                                        **
  **      Prior to installation, make sure to delete all previous versions of this mod!     **
  **                                                                                        **
  ********************************************************************************************
  *************************************IMPORTANT!!********************************************

  Extract the included GameData folder into your KSP directory and merge the contents.
  The included Source directory does not need to be extracted.


Changelog:

Version 3.1
[update] Repackaged to include latest version of Blizzy's Toolbar. *No code changes made to DPAI*


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