#region License
/*
 *    This file is part of Docking Port Alignment Indicator by NavyFish.
 *
 *    IDockingPortAlignmentIndicatorPanel - interface for transferring information
 *        between the DPAI panel UI and the DPAI KSP assembly.
 *
 *    Copyright (C) 2025, Michael Werle
 *
 *    Permission is hereby granted, free of charge, to any person obtaining a copy
 *    of this software and associated documentation files (the "Software"), to deal
 *    in the Software without restriction, including without limitation the rights
 *    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *    copies of the Software, and to permit persons to whom the Software is
 *    furnished to do so, subject to the following conditions:
 *
 *    The above copyright notice and this permission notice shall be included in
 *    all copies or substantial portions of the Software.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *    THE SOFTWARE.
 *
 *    Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 *    project is in no way associated with nor endorsed by Squad.
 */

#endregion License

using UnityEngine;

namespace NavyFish.DPAI.Unity.Interface
{

// This interface should be implemented by the KSP Assembly to transmit and receive information from the Unity UI.
public interface IDockingPortAlignmentIndicatorPanel
{
    // For updating the version string in the title bar
    string Version { get; }

    // For updating the currently targetted port name
    string PortName { get; }

    // Callback when the Prev button is clicked
    void onPrevClicked();

    // Callback when the Next button is clicked
    void onNextClicked();

    // Callback when the Settings button is clicked
    void onSettingsClicked();

    // For saving/restoring the screen position
    Vector2 Position { get; set; }

    // For accessing the window scale
    float Scale { get; }

    // For updating the gauge markers
    Texture GaugeMarkers { get; }

    // Clamps the rectangle to the screen
    void ClampToScreen(RectTransform rect);
}

} // End namespace DockingPortAlignmentIndicator.Unity.Interface
