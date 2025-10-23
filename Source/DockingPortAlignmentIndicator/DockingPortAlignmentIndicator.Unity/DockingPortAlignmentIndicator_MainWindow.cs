#region License
/*
 *    This file is part of Docking Port Alignment Indicator by NavyFish.
 *
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
#endregion License

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NavyFish.DPAI.Unity
{

// Attach this script to the rawimage whose texture we want to draw
public class RawImageTexture : MonoBehaviour
{
    private RawImage m_rawImage;
    private Texture m_texture;

    void Start()
    {
        m_rawImage = GetComponent<RawImage>();
        m_rawImage.texture = m_texture;
    }

    public Texture Texture {
        get { return m_texture; }
        set { m_texture = value; }
    }
}

// This class is attached to the Panel component of the Unity GUI
[RequireComponent(typeof(RectTransform))]
public class DockingPortAlignmentIndicator_MainWindow : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region Lifetime
    // The communications interface instance on the KSP side
    private Interface.IDockingPortAlignmentIndicatorPanel m_interface;

    public void Awake()
    {
        m_rect = GetComponent<RectTransform>();

        // Set up a reference to the gauge markers texture and clear it
        // m_imgGaugeMarkers = GetComponentInChildren<RawImage>();
        clearGaugeMarkers();
    }

    //This must be called by the KSP side to pass in the communications instance.
    public void Initialize(Interface.IDockingPortAlignmentIndicatorPanel f_interface)
    {
        if (f_interface == null) {
            return;
        }
        m_interface = f_interface;

        setVersionString(m_interface.Version);
        setDockingPortName(m_interface.PortName);
        setPosition(m_interface.Position);
        SetScale(m_interface.Scale);
    }

    public void Update()
    {
        if (m_interface != null) {
            setGaugeMarkers(m_interface.GaugeMarkers);
        }
    }
    #endregion

    #region UI_Interface
    // These methods can be used to control the window
    private void setPosition(Vector2 pos)
    {
        if (m_rect == null) {
            return;
        }
        m_rect.anchoredPosition = new Vector3(pos.x, pos.y, 0);
    }

    public void SetScale(float scale)
    {
        Scale = scale;
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    #endregion UI_Interface

    #region Unity_Interface
    // These fields and methods are applied directly in the Unity UI
    // Some of them are only for setting up links to the UI elements but are otherwise unused, so suppress the
    // warnings in this part of the code.
    #pragma warning disable 0414
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
    [SerializeField]
    private RawImage m_imgGaugeMarkers = null;
    [SerializeField]
    private Button m_btnSettings = null;
    #pragma warning restore 0414

    public void setVersionString(string f_strVersion)
    {
        Debug.Log("[DPAI] Unity.DockingPortAlignmentIndicator.setVersionString()");
        if (m_txtVersion == null) {
            return;
        }
        m_txtVersion.text = f_strVersion;
    }

    public void setDockingPortName(string f_name)
    {
        Debug.Log("[DPAI] Unity.DockingPortAlignmentIndicator.setDockingPortName()");
        if (m_txtPortName == null) {
            return;
        }
        m_txtPortName.text = f_name;
    }

    public void onPrevClicked()
    {
        Debug.Log("[DPAI] Unity.DockingPortAlignmentIndicator.onPrevClicked()");
        // Forward the button click to the interface implementation
        m_interface?.onPrevClicked();
    }

    public void onNextClicked()
    {
        Debug.Log("[DPAI] Unity.DockingPortAlignmentIndicator.onPrevClicked()");
        // Forward the button click to the interface implementation
        m_interface?.onNextClicked();
    }

    public void onSettingsClicked()
    {
        Debug.Log("[DPAI] Unity.DockingPortAlignmentIndicator.onSettingsClicked()");
        // Forward the button click to the interface implementation
        m_interface?.onSettingsClicked();
    }

    public void setGaugeMarkers(Texture t)
    {
        if (m_imgGaugeMarkers != null) {
            m_imgGaugeMarkers.texture = t;
        }
    }
    #endregion

    #region GettersSetters
    public RectTransform RectTransform { get { return m_rect; } }

    public float Scale
    {
        get { return (m_rect != null) ? m_rect.localScale.x : 1.0f; }
        set {
            if (m_rect) {
                m_rect.localScale = new Vector3( value, value, 1.0f );
            }
        }
    }
    #endregion GettersSetters

    #region DragHandler
    // Window handling support
    private RectTransform m_rect;
    private Vector2 m_mouseStart;
    private Vector3 m_windowStart;

    private void updateRect(PointerEventData eventData)
    {
        if (m_rect == null) return;

        m_rect.position = m_windowStart + (Vector3)(eventData.position - m_mouseStart);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (m_rect == null) return;

        m_mouseStart = eventData.position;
        m_windowStart = m_rect.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        updateRect(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_rect == null) return;

        updateRect(eventData);

        if (m_interface == null) return;
        m_interface.ClampToScreen(m_rect);
        m_interface.Position = new Vector2(m_rect.anchoredPosition.x, m_rect.anchoredPosition.y);
    }
    #endregion

    #region Utily Functions
    public void clearGaugeMarkers()
    {
        // Defaults
        int w = 318;
        int h = 318;

        if (m_imgGaugeMarkers == null) {
            Debug.LogError("[DPAI.MainWindow] clearGauageMarkers() error - gauge markers image not set");
            return;
        }
        if (m_imgGaugeMarkers.texture == null) {
            Debug.LogWarning("[DPAI.MainWindow] clearGauageMarkers() error - gauge markers image has no texture");
        } else {
            w = m_imgGaugeMarkers.texture.width;
            h = m_imgGaugeMarkers.texture.height;
        }
        RenderTexture prevTex = UnityEngine.RenderTexture.active;
        RenderTexture target = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
        GL.Viewport(new Rect(0f, 0f, w, h));
        GL.Clear(true, true, Color.clear);
        UnityEngine.RenderTexture.active = prevTex;
        m_imgGaugeMarkers.texture = target;
        UnityEngine.RenderTexture.ReleaseTemporary(target);
    }

    #endregion

    } // End class DockingPortAlignmentIndicator

} // End namespace DockingPortAlignmentIndicator.Unity
