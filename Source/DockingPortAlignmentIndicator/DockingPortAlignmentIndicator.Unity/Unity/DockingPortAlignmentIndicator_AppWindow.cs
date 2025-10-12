#if false

#region License
/*
 *    DockingPortAlignmentIndicator_AppWindow - script for controlling the app window
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

#endregion

using System;
using NavyFish.DPAI.Unity.Interface;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NavyFish.DPAI.Unity.Unity
{

public class DockingPortAlignmentIndicator_AppWindow : 
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private Interface.IDockingPortAlignmentIndicatorPanel m_interface;
    private bool m_initialized = false;

    public DockingPortAlignmentIndicator_AppWindow ()
    {
        
    }
    
    public void Initialize(Interface.IDockingPortAlignmentIndicatorPanel f_interface)
    {
        if (dpaiPanel == null) {
            return;
        }
        m_interface = f_interface;

        m_initialized = true;
    }
}

}

#endif