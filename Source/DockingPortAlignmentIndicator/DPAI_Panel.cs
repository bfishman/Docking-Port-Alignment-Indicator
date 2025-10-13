using System;
using UnityEngine;
using KSP.UI;

using NavyFish.DPAI.Unity.Interface;

using static NavyFish.DPAI.LogWrapper;

namespace NavyFish.DPAI
{

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

    public static DPAI_Panel Instance
    {
        get { return m_instance; }
    }

    private void Awake()
    {
        LogD("DPAI_Panel.Awake()");
        m_instance = this;
        // TODO: retrieve version from assembly
        m_version = "1.12.0";
    }

    public void OnShowGUI()
    {
        LogD("DPAI_Panel.OnShowGUI()");
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
        m_window?.gameObject.SetActive(true);
    }

    public void OnHideGUI()
    {
        LogD("DPAI_Panel.OnHideGUI()");
        m_window?.gameObject.SetActive(false);
    }

    #region IDockingPortAlignmentIndicatorPanel
    public void OnWindowDragged(RectTransform rect)
    {
        UIMasterController.ClampToScreen(rect, Vector2.zero);
        // TODO: save window position so it can be applied in future
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
