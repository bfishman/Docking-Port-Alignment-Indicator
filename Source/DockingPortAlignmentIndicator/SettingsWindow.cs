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

using System.Diagnostics;
using KSP.IO;
using KSP.Localization;
using UnityEngine;

namespace NavyFish.DPAI.Settings
{

// NOTE: The PluginConfiguration class is buggy and only supports `bool`, `int`, `double`, and `string`, despite what
// the documentation may say. Ensure that any other types are cast to/from these supported types.
public sealed class Configuration
{
    #region Singleton

    private static readonly Configuration instance = new Configuration();

    static Configuration()
    {
    }

    private Configuration()
    {
    }

    public static Configuration Instance
    {
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
        if (dirty)
        {
            // TODO: save in a background task
            config.save();
            dirty = false;
        }
    }

    public object this[string key]
    {
        get { return config[key]; }
        set { SetValue(key, value); }
    }

    public T GetValue<T>(string key)
    {
        return config.GetValue<T>(key);
    }

    public T GetValue<T>(string key, T _default)
    {
        return (config[key] != null) ? config.GetValue<T>(key) : _default;
    }

    public void SetValue(string key, object value)
    {
        var equal = (value.GetType().IsValueType) ? value.Equals(config[key]) : (value == config[key]);
        if (!equal) {
            config.SetValue(key, value);
            dirty = true;
            NotifyPropertyChanged(key);
        }
    }

    #endregion PluginConfigurationWrapper

    #region GettersSetters

    public float GaugeScale
    {
        get
        {
            // NOTE: There seems to be a bug in the settings implementation which ignores saving float values.
            var legacyValue = GetValue<double>("gui_scale", 0.86f);
            return (float)GetValue<double>("GaugeScale", legacyValue);
        }
        set { SetValue("GaugeScale", (double)value); }
    }

    public bool DrawHudIcon
    {
        get
        {
            var legacyValue = GetValue<bool>("drawHudIcon", true);
            return GetValue<bool>("DrawHudIcon", legacyValue);
        }
        set { SetValue("DrawHudIcon", value); }
    }

    public bool ShowHudIconWhileIva
    {
        get
        {
            var legacyValue = GetValue<bool>("showHUDIconWhileIva", true);
            return GetValue<bool>("ShowHUDIconWhileIva", legacyValue);
        }
        set { SetValue("ShowHudIconWhileIva", value); }
    }

    public float HudIconSize
    {
        get { return (float)GetValue<double>("HudIconSize", 22f); }
        set { SetValue("HudIconSize", (double)value); }
    }

    public bool AllowAutoPortTargeting
    {
        get
        {
            var legacyValue = GetValue<bool>("allowAutoPortTargeting", true);
            return GetValue<bool>("AllowAutoPortTargeting", legacyValue);
        }
        set { SetValue("AllowAutoPortTargeting", value); }
    }

    public bool ExcludeDockedPorts
    {
        get
        {
            var legacyValue = GetValue<bool>("excludeDockedPorts", true);
            return GetValue<bool>("ExcludeDockedPorts", legacyValue);
        }
        set { SetValue("ExcludeDockedPorts", value); }
    }

    public bool RestrictDockingPorts
    {
        get
        {
            var legacyValue = GetValue<bool>("restrictDockingPorts", true);
            return GetValue<bool>("RestrictDockingPorts", legacyValue);
        }
        set { SetValue("RestrictDockingPorts", value); }
    }

    public bool AlignmentFlipXAxis
    {
        get
        {
            var legacyValue = GetValue<bool>("alignmentFlipXAxis", false);
            return GetValue<bool>("AlignmentFlipXAxis", legacyValue);
        }
        set { SetValue("AlignmentFlipXAxis", value); }
    }

    public bool AlignmentFlipYAxis
    {
        get
        {
            var legacyValue = GetValue<bool>("alignmentFlipYAxis", false);
            return GetValue<bool>("AlignmentFlipYAxis", legacyValue);
        }
        set { SetValue("AlignmentFlipYAxis", value); }
    }

    public bool TranslationFlipXAxis
    {
        get
        {
            var legacyValue = GetValue<bool>("translationFlipXAxis", false);
            return GetValue<bool>("TranslationFlipXAxis", legacyValue);
        }
        set { SetValue("TranslationFlipXAxis", value); }
    }

    public bool TranslationFlipYAxis
    {
        get
        {
            var legacyValue = config.GetValue<bool>("translationFlipYAxis", false);
            return config.GetValue<bool>("TranslationFlipYAxis", legacyValue);
        }
        set { SetValue("TranslationFlipYAxis", value); }
    }

    public bool RollFlipAxis
    {
        get
        {
            var legacyValue = config.GetValue<bool>("rollFlipAxis", false);
            return config.GetValue<bool>("RollFlipAxis", legacyValue);
        }
        set { SetValue("RollFlipAxis", value); }
    }

    public bool UseStockToolbar
    {
        get
        {
            var legacyValue = config.GetValue<bool>("forceStockAppLauncher", true);
            return config.GetValue<bool>("UseStockToolbar", legacyValue);
        }
        set { SetValue("UseStockToolbar", value); }
    }

    public bool UseBlizzyToolbar
    {
        get { return config.GetValue<bool>("UseBlizzyToolbar", false); }
        set { SetValue("UseBlizzyToolbar", value); }
    }

    public Vector2 WindowPosition
    {
        get
        {
            var legacyValue = config.GetValue<Vector2>("windowPosition", new Vector2(0, 0));
            return config.GetValue<Vector2>("WindowPosition", legacyValue);
        }
        set { SetValue("WindowPosition", value); }
    }

    public bool IsWindowVisible
    {
        get { return config.GetValue<bool>("IsWindowVisible", false); }
        set { SetValue("IsWindowVisible", value); }
    }

    public bool ShowSettingsWindow {
        get { return config.GetValue<bool>("ShowSettingsWindow", false); }
        set { SetValue("ShowSettingsWindow", value); }
    }

    public bool ShowDebugWindow {
        get { return config.GetValue<bool>("ShowDebugWindow", false); }
        set { SetValue("ShowDebugWindow", value); }
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

    private bool isLocked = false;

    public void OnGUI()
    {
        if (IsOpen())
        {
            //var pos = m_parent.rect;
            //pos.y -= m_parent.rect.height;
            //KSP.UI.UIMasterController.ClampToScreen(pos, Vector2.zero);
            // TODO: clamp to main window
            m_pos = GUILayout.Window(1339, m_pos, drawSettingsWindowContents, Utils.GetStringByTag("#dpai_settings"), m_windowStyle);
        }
        isLocked = Utils.PreventClickthrough(IsOpen(), m_pos, isLocked);
    }

    private void drawSettingsWindowContents(int id)
    {
        var c = Configuration.Instance;

        // Close button
        var rect = new Rect(m_pos.width - 20, 4, 16, 16);
        if (GUI.Button(rect, ""))
        {
            Close();
            Settings.Configuration.Instance.ShowSettingsWindow = false;
            return;
        }

        GUILayout.BeginHorizontal();
        c.DrawHudIcon = GUILayout.Toggle(c.DrawHudIcon, Utils.GetStringByTag("#display_hud_target_port_icon"));
        GUILayout.EndHorizontal();

        if (c.DrawHudIcon)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(14f);
            c.ShowHudIconWhileIva = GUILayout.Toggle(c.ShowHudIconWhileIva, Utils.GetStringByTag("#display_when_using_rpm"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(Utils.GetStringByTag("#hud_target_port_icon_size"));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            c.HudIconSize = GUILayout.HorizontalSlider(c.HudIconSize, 10f, 60f);
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        c.AllowAutoPortTargeting = GUILayout.Toggle(c.AllowAutoPortTargeting, Utils.GetStringByTag("#enable_auto_targeting_and_cycling"));
        GUILayout.EndHorizontal();

        if (c.AllowAutoPortTargeting)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(14f);
            c.ExcludeDockedPorts = GUILayout.Toggle(c.ExcludeDockedPorts, Utils.GetStringByTag("#exlude_docked_ports"));
            c.RestrictDockingPorts = GUILayout.Toggle(c.RestrictDockingPorts, Utils.GetStringByTag("#restrict_docking_ports"));
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(Utils.GetStringByTag("#gui_scale") + $" {c.GaugeScale,4:#0%}");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        c.GaugeScale = GUILayout.HorizontalSlider(c.GaugeScale, 0.4f, 3.0f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        c.AlignmentFlipXAxis = GUILayout.Toggle(c.AlignmentFlipXAxis, Utils.GetStringByTag("#invert_alignment_x"));
        GUILayout.FlexibleSpace();
        c.TranslationFlipXAxis = GUILayout.Toggle(c.TranslationFlipXAxis, Utils.GetStringByTag("#invert_translation_x"));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        c.AlignmentFlipYAxis = GUILayout.Toggle(c.AlignmentFlipYAxis, Utils.GetStringByTag("#invert_alignment_y"));
        GUILayout.FlexibleSpace();
        c.TranslationFlipYAxis = GUILayout.Toggle(c.TranslationFlipYAxis, Utils.GetStringByTag("#invert_translation_y"));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        c.RollFlipAxis = GUILayout.Toggle(c.RollFlipAxis, Utils.GetStringByTag("#invert_roll_direction"));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        c.UseStockToolbar = GUILayout.Toggle(c.UseStockToolbar, Utils.GetStringByTag("#use_stock_toolbar"));
        if (Toolbar.IsBlizzyAvailable) {
            c.UseBlizzyToolbar = GUILayout.Toggle(c.UseBlizzyToolbar, Utils.GetStringByTag("#use_blizzy_toolbar"));
        }
        GUILayout.EndHorizontal();

        drawSettingsWindowDebugContents();

        GUI.DragWindow();
    } // End drawSettingsWindowContents

    [Conditional("DEBUG")]
    private void drawSettingsWindowDebugContents()
    {
        var c = Configuration.Instance;

        GUILayout.BeginHorizontal();
        c.ShowDebugWindow = GUILayout.Toggle(c.ShowDebugWindow, "Show Debug Window");
        GUILayout.EndHorizontal();
    }
}

} // End namespace NavyFish.DPAI.Settings
