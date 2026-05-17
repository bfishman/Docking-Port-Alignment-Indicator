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
using UnityEngine;

namespace NavyFish.DPAI.Settings
{


/// <summary>
/// This is a wrapper class around PluginConfiguration to enhance safety and correctness.
/// </summary>
/// The PluginConfiguration class is buggy and some types are not written out, such as "float".
/// Furthermore, the following has been determined:
/// - PluginConfiguration.GetValue<T>(string key, T _default) - this function will create the key if it doesn't exist
/// - PluginConfiguration.GetValue<T>(string key) - this function will return a default T if the key doesn't exist
/// - PluginConfiguration.SetValue(string key, object value) - this will add or update the value
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
        try
        {
            // NOTE: This function throws if the XML configuration file is malformed. Since we provide sensible
            //       defaults, we can simply log this as an error and proceed.
            config.load();
        }
        catch (System.Exception e)
        {
            LogWrapper.LogE($"Configuration.Load() - error loading: {e}");
        }
        dirty = false;
    }

    public void Save()
    {
        if (!dirty)
        {
            return;
        }

        try
        {
            // TODO: save in a background task
            // NOTE: This function throws if an error occurs during saving, such as an illegal value in one of the
            //       configuration entries or an IO error.
            config.save();
            dirty = false;
        }
        catch (System.Exception e)
        {
            LogWrapper.LogE($"Configuration.Save() - error saving: {e}");
        }
    }

    /// <summary>
    /// Returns the configuration value stored with key "key".
    /// </summary>
    /// If the configuration key is not stored, a default value for the type is returned.
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetValue<T>(string key)
    {
        return config.GetValue<T>(key);
    }

    /// <summary>
    /// Returns the configuration value stored with key "key" if it exists and is of the expected type, otherwise it
    /// returns the defaultValue.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetValue<T>(string key, T defaultValue)
    {
        return (config[key] != null && config[key] is T) ? (T)config[key] : defaultValue;
    }

    /// <summary>
    /// Return the configuration value stored with key "key" if it exists, otherwise the value stored with "legacyKey",
    /// or, if neither exist, the "defaultValue".
    /// </summary>
    /// This function ensures that the legacy key is not added to the configuration if it doesn't already exist
    /// <param name="legacyKey"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T GetValue<T>(string legacyKey, string key, T defaultValue)
    {
        return (config[key] != null)? GetValue<T>(key, defaultValue) : GetValue<T>(legacyKey, defaultValue);
    }

    /// <summary>
    /// Sets a configuration item.
    /// </summary>
    /// WARNING: setting a configuration item to "null" will cause writing the configuration to fail.
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue(string key, object value)
    {
        // Setting a null key or value causes an error when saving the configuration
        if (value == null || string.IsNullOrEmpty(key))
        {
            return;
        }
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
            return (float)GetValue<double>("gui_scale", "GaugeScale", 0.86f);
        }
        set { SetValue("GaugeScale", (double)value); }
    }

    public bool DrawHudIcon
    {
        get
        {
            return GetValue<bool>("drawHudIcon", "DrawHudIcon", true);
        }
        set { SetValue("DrawHudIcon", value); }
    }

    public bool ShowHudIconWhileIva
    {
        get
        {
            return GetValue<bool>("showHUDIconWhileIva", "ShowHUDIconWhileIva", true);
        }
        set { SetValue("ShowHudIconWhileIva", value); }
    }

    public float HudIconSize
    {
        get { return GetValue<float>("HudIconSize", 22f); }
        set { SetValue("HudIconSize", value); }
    }

    public bool AllowAutoPortTargeting
    {
        get
        {
            return GetValue<bool>("allowAutoPortTargeting", "AllowAutoPortTargeting", true);
        }
        set { SetValue("AllowAutoPortTargeting", value); }
    }

    public bool ExcludeDockedPorts
    {
        get
        {
            return GetValue<bool>("excludeDockedPorts", "ExcludeDockedPorts", true);
        }
        set { SetValue("ExcludeDockedPorts", value); }
    }

    public bool RestrictDockingPorts
    {
        get
        {
            return GetValue<bool>("restrictDockingPorts", "RestrictDockingPorts", true);
        }
        set { SetValue("RestrictDockingPorts", value); }
    }

    public bool AlignmentFlipXAxis
    {
        get
        {
            return GetValue<bool>("alignmentFlipXAxis", "AlignmentFlipXAxis", false);
        }
        set { SetValue("AlignmentFlipXAxis", value); }
    }

    public bool AlignmentFlipYAxis
    {
        get
        {
            return GetValue<bool>("alignmentFlipYAxis", "AlignmentFlipYAxis", false);
        }
        set { SetValue("AlignmentFlipYAxis", value); }
    }

    public bool TranslationFlipXAxis
    {
        get
        {
            return GetValue<bool>("translationFlipXAxis", "TranslationFlipXAxis", false);
        }
        set { SetValue("TranslationFlipXAxis", value); }
    }

    public bool TranslationFlipYAxis
    {
        get
        {
            return GetValue<bool>("translationFlipYAxis", "TranslationFlipYAxis", false);
        }
        set { SetValue("TranslationFlipYAxis", value); }
    }

    public bool RollFlipAxis
    {
        get
        {
            return GetValue<bool>("rollFlipAxis", "RollFlipAxis", false);
        }
        set { SetValue("RollFlipAxis", value); }
    }

    public bool UseStockToolbar
    {
        get
        {
            return GetValue<bool>("forceStockAppLauncher", "UseStockToolbar", true);
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
            return GetValue<Vector2>("windowPosition", "WindowPosition", new Vector2(0, 0));
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

    [System.Diagnostics.Conditional("DEBUG")]
    private void drawSettingsWindowDebugContents()
    {
        var c = Configuration.Instance;

        GUILayout.BeginHorizontal();
        c.ShowDebugWindow = GUILayout.Toggle(c.ShowDebugWindow, "Show Debug Window");
        GUILayout.EndHorizontal();
    }
}

} // End namespace NavyFish.DPAI.Settings
