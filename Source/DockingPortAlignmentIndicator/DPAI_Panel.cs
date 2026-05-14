#region License
/*
 *    This file is part of Docking Port Alignment Indicator by NavyFish.
 *
 *    DPAI_Panel - class interfacing between the GUI and the main logic
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

using System.Diagnostics;
using UnityEngine;
using KSP.UI;

using NavyFish.DPAI.Unity.Interface;

using static NavyFish.DPAI.LogWrapper;

namespace NavyFish.DPAI
{

public static class RectTransformExtensions
{
    public static Rect GetScreenCoordinates(this RectTransform rt)
    {
        var worldCorners = new Vector3[4];
        rt.GetWorldCorners(worldCorners);
        return new Rect(
            Screen.width * rt.pivot.x + worldCorners[0].x,
            Screen.height * rt.pivot.y + worldCorners[0].y,
            worldCorners[2].x - worldCorners[2].x,
            worldCorners[2].y - worldCorners[0].y
        );
    }


    public static Vector2 GetScreenPosition(this RectTransform rt)
    {
        var worldCorners = new Vector3[4];
        rt.GetWorldCorners(worldCorners);
        return new Vector2(worldCorners[0].x, worldCorners[0].y);
    }
#if false
    public static Canvas GetCanvas(this RectTransform rt)
    {
        return rt.gameObject.GetComponentInParent<Canvas>();
    }
    public static float GetWidth(this RectTransform rt)
    {
        return (rt.anchorMax.x - rt.anchorMin.x) * Screen.width + rt.sizeDelta.x * rt.GetCanvas().scaleFactor;
    }

    public static float GetHeight(this RectTransform rt)
    {
        return (rt.anchorMax.y - rt.anchorMin.y) * Screen.height + rt.sizeDelta.y * rt.GetCanvas().scaleFactor;
    }

    public static Vector2 GetScreenPosition(this RectTransform rt)
    {
        Vector2 pos;
        pos.x =
    }

    public Vector2 Size
    {
        get
        {
            Vector2 size;
            size.x =
        }
    }
#endif
}
[KSPAddon(KSPAddon.Startup.Instantly, true)]
public class DPAI_Panel_Loader : MonoBehaviour
{
    private static GameObject m_panelPrefab;

    public static GameObject PanelPrefab
    {
        get { return m_panelPrefab; }
    }

    private void Awake()
    {
        string path = KSPUtil.ApplicationRootPath+ "GameData/NavyFish/AssetBundles/";
        AssetBundle prefabs = AssetBundle.LoadFromFile(path + "dpai");
        m_panelPrefab = prefabs.LoadAsset("DPAI_MainWindow") as GameObject;
    }
}

// The DPAI KSP Assembly class interfacing with the DPAI Unity UI by implemnting the interface
[KSPAddon(KSPAddon.Startup.Flight, false)]
public class DPAI_Panel : MonoBehaviour, IDockingPortAlignmentIndicatorPanel
{
    private string m_version;
    private static DPAI_Panel m_instance = null;
    private static Unity.DockingPortAlignmentIndicator_MainWindow m_window = null;
    private static Settings.SettingsWindow m_settingsWindow = null;

    public static DPAI_Panel Instance
    {
        get { return m_instance; }
    }

    #region MonoBehaviour Lifecycle
    private void Awake()
    {
        LogD("DPAI_Panel.Awake()");
        m_instance = this;
        m_version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        // Load the Unity window from the prefab
        if (m_window == null && DPAI_Panel_Loader.PanelPrefab != null) {
            GameObject obj = Instantiate(DPAI_Panel_Loader.PanelPrefab) as GameObject;
            if (obj == null) {
                LogE("ERROR - could not instantiate the DPAI Panel prefab.");
            } else {
                obj.transform.SetParent(MainCanvasUtil.MainCanvas.transform);
                m_window = obj.GetComponent<Unity.DockingPortAlignmentIndicator_MainWindow>();
                if (m_window == null) {
                    LogE("ERROR - could not access the script object on the panel object");
                }
                m_window?.Initialize(Instance);
            }
        }
        m_window?.gameObject.SetActive(false);
        m_settingsWindow?.Close();
    }

    private void Update()
    {
    }

    private void OnGUI()
    {
        m_settingsWindow?.OnGUI();
    }

    private void OnDestroy()
    {
        m_window.Close();
        m_settingsWindow?.Close();
        m_settingsWindow = null;
    }
    #endregion MonoBehaviour Lifecycle

    public void OnShowGUI()
    {
        LogD("DPAI_Panel.OnShowGUI()");
        m_window?.Open();
        if (Settings.Configuration.Instance.ShowSettingsWindow) {
            m_settingsWindow?.Open(m_window?.RectTransform);
        }
    }

    public void OnHideGUI()
    {
        LogD("DPAI_Panel.OnHideGUI()");
        m_window?.Close();
        m_settingsWindow?.Close();
    }

    public void OnScaleChanged(float scale)
    {
        Scale = scale;
    }

    #region IDockingPortAlignmentIndicatorPanel
    public Vector2 Position {
        get { return Settings.Configuration.Instance.WindowPosition; }
        set { Settings.Configuration.Instance.WindowPosition = value; }
    }

    public float Scale
    {
        get => Settings.Configuration.Instance.GaugeScale;
        set {
            Settings.Configuration.Instance.GaugeScale = value;
            m_window?.SetScale(value);
        }
    }

    public void ClampToScreen(RectTransform rect)
    {
        UIMasterController.ClampToScreen(rect, Vector2.zero);
    }

    public string Version
    {
        get { return m_version; }
    }

    public string PortName
    {
        get { return DockingPortAlignmentIndicator.determineTargetPortName(); }
    }

    // Callback when the "Prev" button is clicked
    public void onPrevClicked()
    {
        LogD("DPAI_Panel.onPrevClicked()");
        DockingPortAlignmentIndicator.cyclePortLeft();
    }

    // Callback when the "Next" button is clicked
    public void onNextClicked()
    {
        LogD("DPAI_Panel.onNextClicked()");
        DockingPortAlignmentIndicator.cyclePortRight();
    }

    // Callback when the "Next" button is clicked
    public void onSettingsClicked()
    {
        LogD("DPAI_Panel.onSettingsClicked()");
        if (m_settingsWindow == null) {
            m_settingsWindow = new Settings.SettingsWindow();
        }
        if (m_settingsWindow.IsOpen()) {
            m_settingsWindow.Close();
            Settings.Configuration.Instance.ShowSettingsWindow = false;
        } else {
            m_settingsWindow.Open(m_window?.RectTransform);
            Settings.Configuration.Instance.ShowSettingsWindow = true;
        }
    }

    // Called during the main window Update() function
    public Texture GaugeMarkers
    {
        get { return DockingPortAlignmentIndicator.guiRenderTexture; }
    }
    #endregion

    public void OnTargetUpdated()
    {
        m_window?.setDockingPortName(PortName);
    }
    public void OnTargetPortRenamed(string portName) {
        m_window?.setDockingPortName(portName);
    }
}

} // End namespace NavyFish.DPAI
