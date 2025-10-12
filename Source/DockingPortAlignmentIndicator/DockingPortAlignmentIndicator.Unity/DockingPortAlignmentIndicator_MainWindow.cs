#region License
/*
 *    DockingPortAlignmentIndicator_MainWindow - script to interface with the Unity UI
 *
 *    This script implements the GUI logic to handle the main window of the Docking
 *    Port Alignment Indicator.
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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NavyFish.DPAI.Unity
{

// This class is attached to the Panel component of the Unity GUI
[RequireComponent(typeof(RectTransform))]
public class DockingPortAlignmentIndicator_MainWindow : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Text m_txtTitle = null;
    [SerializeField]
    private Text m_txtVersion = null;
    [SerializeField]
    private Button m_btnPrev = null;
    [SerializeField]
    private Button m_btnNext = null;
    [SerializeField]
    private Text m_txtPortName = null;
    
    // Window handling support
    private RectTransform rect;
    private Vector2 mouseStart;
    private Vector3 windowStart;

    // The communications interface instance on the KSP side
    private Interface.IDockingPortAlignmentIndicatorPanel m_interface;
    private bool m_initialized = false;
    
    public void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    //This must be called by the KSP side to pass in the communications instance.    
    public void Initialize(Interface.IDockingPortAlignmentIndicatorPanel f_interface)
    {
        if (f_interface == null) {
            return;
        }
        m_interface = f_interface;

        m_initialized = true;
    }

    #region UI_Interface
    public void setVersionString(string f_strVersion)
    {
        Debug.Log("Unity.DockingPortAlignmentIndicator.setVersionString()");
        if (m_txtVersion == null) {
            return;
        }
        m_txtVersion.text = f_strVersion;
    }
    
    public void setDockingPortName(string f_name)
    {
        Debug.Log("Unity.DockingPortAlignmentIndicator.setDockingPortName()");
        if (m_txtPortName == null) {
            return;
        }
        m_txtPortName.text = f_name;
    }
    
    public void onPrevClicked()
    {
        Debug.Log("Unity.DockingPortAlignmentIndicator.onPrevClicked()");
        // Forward the button click to the interface implementation
        m_interface?.onPrevClicked();
    }
    
    public void onNextClicked()
    {
        Debug.Log("Unity.DockingPortAlignmentIndicator.onPrevClicked()");
        // Forward the button click to the interface implementation
        m_interface?.onNextClicked();
    }
    #endregion

    #region DragHandler
    private void updateRect(PointerEventData eventData)
    {
        if (rect == null) return;
        
        rect.position = windowStart + (Vector3)(eventData.position - mouseStart);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rect == null) return;
        
        mouseStart = eventData.position;
        windowStart = rect.position;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        updateRect(eventData);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (rect == null) return;
        
        updateRect(eventData);
        
        m_interface?.OnWindowDragged(rect);
    }
    #endregion

    } // End class DockingPortAlignmentIndicator

} // End namespace DockingPortAlignmentIndicator.Unity
