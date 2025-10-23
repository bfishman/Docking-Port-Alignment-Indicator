#region License
/*
 *    This file is part of Docking Port Alignment Indicator by NavyFish.
 *
 *    SettingsWindow - class wrapping up the settings.
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


// We want the settings window to open/close
// We want the settings window to "attach" to the parent window

using KSP.IO;
using KSP.Localization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NavyFish.DPAI.Settings
{

public sealed class Configuration
{
    #region Singleton
    private static readonly Configuration instance = new Configuration();
    static Configuration() { }
    private Configuration() { }
    public static Configuration Instance {
        get { return instance; }
    }
    #endregion Singleton

    #region Events
    public delegate void OnPropertyChanged(string propertyName);
    public static OnPropertyChanged onPropertyChanged;
    private void NotifyPropertyChanged(string propertyName)
    {
        onPropertyChanged?.Invoke(propertyName);
    }
    #endregion Events

    #region PluginConfigurationWrapper
    private PluginConfiguration config = PluginConfiguration.CreateForType<DockingPortAlignmentIndicator>(null);
    private bool dirty = false;

    public void Load()
    {
        config.load();
        dirty = false;
    }

    public void Save()
    {
        if (dirty) {
            // TODO: save in a background task
            config.save();
            dirty = false;
        }
    }

    public object this [string key] {
        get { return config[key]; }
        set { SetValue(key, value); }
    }

    public T GetValue<T>(string key)
    {
        return config.GetValue<T>(key);
    }
    public T GetValue<T>(string key, T _default)
    {
        return config.GetValue<T>(key, _default);
    }
    public void SetValue(string key, object value)
    {
        if (value != config[key]) {
            config.SetValue(key, value);
            dirty = true;
        }
    }
    #endregion PluginConfigurationWrapper

    #region GettersSetters
    public float GaugeScale {
        get { return config.GetValue<float>("gui_scale", 0.86f); }
        set {
            if (GaugeScale != value) {
                config.SetValue("gui_scale", value);
                dirty = true;
                NotifyPropertyChanged("GaugeScale");
            }
        }
    }

    public bool DrawHudIcon {
        get { return config.GetValue<bool>("drawHudIcon", true); }
        set {
            if (DrawHudIcon != value) {
                config.SetValue("drawHudIcon", value);
                dirty = true;
                NotifyPropertyChanged("DrawHudIcon");
            }
        }
    }

    public bool ShowHudIconWhileIva {
        get { return config.GetValue<bool>("showHUDIconWhileEva", true); }
        set {
            if (ShowHudIconWhileIva != value) {
                config.SetValue("showHUDIconWhileEva", value);
                dirty = true;
                NotifyPropertyChanged("ShowHudIconWhileIva");
            }
        }
    }

    public float HudIconSize {
        get { return config.GetValue<float>("HudIconSize", 22f); }
        set {
            if (HudIconSize != value) {
                config.SetValue("HudIconSize", value);
                dirty = true;
                NotifyPropertyChanged("HudIconSize");
            }
        }
    }

    public bool AllowAutoPortTargeting {
        get { return config.GetValue<bool>("allowAutoPortTargeting", true); }
        set {
            if (AllowAutoPortTargeting != value) {
                config.SetValue("allowAutoPortTargeting", value);
                dirty = true;
                NotifyPropertyChanged("AllowAutoPortTargeting");
            }
        }
    }

    public bool ExcludeDockedPorts {
        get { return config.GetValue<bool>("excludeDockedPorts", true); }
        set {
            if (ExcludeDockedPorts != value) {
                config.SetValue("excludeDockedPorts", value);
                dirty = true;
                NotifyPropertyChanged("ExcludeDockedPorts");
            }
        }
    }

    public bool RestrictDockingPorts {
        get { return config.GetValue<bool>("restrictDockingPorts", true); }
        set {
            if (RestrictDockingPorts != value) {
                config.SetValue("restrictDockingPorts", value);
                dirty = true;
                NotifyPropertyChanged("RestrictDockingPorts");
            }
        }
    }

    public bool AlignmentFlipXAxis {
        get { return config.GetValue<bool>("alignmentFlipXAxis", false); }
        set {
            if (AlignmentFlipXAxis != value) {
                config.SetValue("alignmentFlipXAxis", value);
                dirty = true;
                NotifyPropertyChanged("AlignmentFlipXAxis");
            }
        }
    }

    public bool AlignmentFlipYAxis {
        get { return config.GetValue<bool>("alignmentFlipYAxis", false); }
        set {
            if (AlignmentFlipYAxis != value) {
                config.SetValue("alignmentFlipYAxis", value);
                dirty = true;
                NotifyPropertyChanged("AlignmentFlipYAxis");
            }
        }
    }

    public bool TranslationFlipXAxis {
        get { return config.GetValue<bool>("translationFlipXAxis", false); }
        set {
            if (TranslationFlipXAxis != value) {
                config.SetValue("translationFlipXAxis", value);
                dirty = true;
                NotifyPropertyChanged("TranslationFlipXAxis");
            }
        }
    }

    public bool TranslationFlipYAxis {
        get { return config.GetValue<bool>("translationFlipYAxis", false); }
        set {
            if (TranslationFlipYAxis != value) {
                config.SetValue("translationFlipYAxis", value);
                dirty = true;
                NotifyPropertyChanged("TranslationFlipYAxis");
            }
        }
    }

    public bool RollFlipAxis {
        get { return config.GetValue<bool>("rollFlipAxis", false); }
        set {
            if (RollFlipAxis != value) {
                config.SetValue("rollFlipAxis", value);
                dirty = true;
                NotifyPropertyChanged("RollFlipAxis");
            }
        }
    }

    public bool ForceStockAppLauncher {
        get { return config.GetValue<bool>("forceStockAppLauncher", false); }
        set {
            if (ForceStockAppLauncher != value) {
                config.SetValue("forceStockAppLauncher", value);
                dirty = true;
                NotifyPropertyChanged("ForceStockAppLauncher");
            }
        }
    }

    public Vector2 WindowPosition {
        get { return config.GetValue<Vector2>("windowPosition", new Vector2(0,0)); }
        set {
            if (WindowPosition != value) {
                config.SetValue("windowPosition", value);
                dirty = true;
                NotifyPropertyChanged("WindowPosition");
            }
        }
    }
    #endregion

}

public class SettingsWindow
{
    private RectTransform m_parent = null;
    private GUIStyle m_windowStyle = null;
    private Rect m_pos;

    public SettingsWindow()
    {
        m_windowStyle  = new GUIStyle(HighLogic.Skin.window);
        m_windowStyle.stretchWidth = true;
        m_windowStyle.stretchHeight = true;

        m_pos = new Rect(Screen.width/2 - 50, Screen.height/2 - 50, 100, 100);
    }

    public void Open(RectTransform parent)
    {
        m_parent = parent;
    }

    public void Close()
    {
        m_parent = null;
    }

    public bool IsOpen()
    {
        return m_parent != null;
    }

    public void OnGUI()
    {
        if (IsOpen())
        {
            //var pos = m_parent.rect;
            //pos.y -= m_parent.rect.height;
            //KSP.UI.UIMasterController.ClampToScreen(pos, Vector2.zero);
            // TODO: clamp to main window
            m_pos = GUILayout.Window(1339, m_pos, drawSettingsWindowContents, Localizer.GetStringByTag("#dpai_settings"), m_windowStyle);
        }
    }

    private void drawSettingsWindowContents(int id)
    {
        var c = Configuration.Instance;

        GUILayout.BeginHorizontal();
        c.DrawHudIcon = GUILayout.Toggle(c.DrawHudIcon, Localizer.GetStringByTag("#display_hud_target_port_icon"));
        GUILayout.EndHorizontal();

        if (c.DrawHudIcon)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(14f);
            c.ShowHudIconWhileIva = GUILayout.Toggle(c.ShowHudIconWhileIva, Localizer.GetStringByTag("#display_when_using_rpm"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.GetStringByTag("#hud_target_port_icon_size"));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            c.HudIconSize = GUILayout.HorizontalSlider(c.HudIconSize, 10f, 60f);
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        c.AllowAutoPortTargeting = GUILayout.Toggle(c.AllowAutoPortTargeting, Localizer.GetStringByTag("#enable_auto_targeting_and_cycling"));
        GUILayout.EndHorizontal();

        if (c.AllowAutoPortTargeting)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(14f);
            c.ExcludeDockedPorts = GUILayout.Toggle(c.ExcludeDockedPorts, Localizer.GetStringByTag("#exlude_docked_ports"));
            c.RestrictDockingPorts = GUILayout.Toggle(c.RestrictDockingPorts, Localizer.GetStringByTag("#restrict_docking_ports"));
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(Localizer.GetStringByTag("#gui_scale") + $" {c.GaugeScale,4:#0%}");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        c.GaugeScale = GUILayout.HorizontalSlider(c.GaugeScale, 0.4f, 3.0f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        c.AlignmentFlipXAxis = GUILayout.Toggle(c.AlignmentFlipXAxis, Localizer.GetStringByTag("#invert_alignment_x"));
        GUILayout.FlexibleSpace();
        c.TranslationFlipXAxis = GUILayout.Toggle(c.TranslationFlipXAxis, Localizer.GetStringByTag("#invert_translation_x"));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        c.AlignmentFlipYAxis = GUILayout.Toggle(c.AlignmentFlipYAxis, Localizer.GetStringByTag("#invert_alignment_y"));
        GUILayout.FlexibleSpace();
        c.TranslationFlipYAxis = GUILayout.Toggle(c.TranslationFlipYAxis, Localizer.GetStringByTag("#invert_translation_y"));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        c.RollFlipAxis = GUILayout.Toggle(c.RollFlipAxis, Localizer.GetStringByTag("#invert_roll_direction"));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        c.ForceStockAppLauncher = GUILayout.Toggle(c.ForceStockAppLauncher, Localizer.GetStringByTag("#always_use_stock_toolbar"));
        GUILayout.EndHorizontal();

        GUI.DragWindow();
    } // End drawSettingsWindowContents
}

} // End namespace NavyFish.DPAI.Settings
