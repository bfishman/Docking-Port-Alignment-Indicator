#region License
/*
 *    This file is part of Docking Port Alignment Indicator by NavyFish.
 *
 *    Toolbar - class wrapping up handling the toolbar integration.
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

using System;
using KSP.UI.Screens;
using UnityEngine;

using static NavyFish.DPAI.LogWrapper;

namespace NavyFish.DPAI.Toolbar
{

public class Toolbar
{
    public void Dispose() {
        SetToolbarButtons(false, false);
        m_instance = null;
    }

    private void OnButtonClicked() {
        onToolbarButtonClicked?.Invoke();
    }

    #region Singleton
    private static Toolbar m_instance = null;

    private Toolbar() {
        m_instance = this;
    }

    public static Toolbar Instance {
        get {
            if (m_instance == null) {
                m_instance = new Toolbar();
            }
            return m_instance;
        }
    }
    #endregion

    #region Public_Interface
    /// <summary>
    /// Event when the toolbar button is clicked.
    /// </summary>
    public delegate void OnToolbarButtonClicked();
    public OnToolbarButtonClicked onToolbarButtonClicked;

    /// <summary>
    /// Set visibility of DPAI toolbar icons on the stock and Blizzy toolbars.
    /// </summary>
    /// Note that the availability of the Blizzy toolbar will determine what actually happens - if it is not
    /// available, then the stock toolbar will be used instead.
    /// <param name="stock">Show the DPAI icon on the Stock toolbar if set to <c>true</c>.</param>
    /// <param name="blizzy">Show the DPAI icon on the Blizzy toolbar if set to <c>true</c>.</param>
    public void SetToolbarButtons(bool stock, bool blizzy) {
        stock = stock || !IsBlizzyAvailable;
        blizzy = blizzy && IsBlizzyAvailable;

        if (stock) {
            CreateStockButton();
        } else {
            DestroyStockButton();
        }
        if (blizzy) {
            CreateBlizzyButton();
        } else {
            DestroyBlizzyButton();
        }
    }
    #endregion

    #region StockToolbar
    private ApplicationLauncherButton m_stbButton = null;
    private Texture2D m_stbButtonIcon = null;

    private void LoadStockToolbarIcon () {
        Byte[] buf;
        buf = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("appLauncherIcon.png", null);
        m_stbButtonIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        m_stbButtonIcon.LoadImage(buf);
    }

    private void AddButtonToStockToolbar () {
        LogD($"AddButtonToStockToolbar (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={m_stbButton})");
        if (!ApplicationLauncher.Ready || m_stbButton != null) {
            return;
        }
        // Note: the stock toolbar manages internal state and alternately calls the onTrue and onFalse callbacks every
        // time the button is clicked.
        var onButtonClickCallback = new Callback(OnButtonClicked);
        m_stbButton = ApplicationLauncher.Instance.AddModApplication(
            onButtonClickCallback,
            onButtonClickCallback,
            null, null, null, null,
            ApplicationLauncher.AppScenes.FLIGHT|ApplicationLauncher.AppScenes.MAPVIEW,
            m_stbButtonIcon);
    }

    /// <summary>
    /// Called when the ApplicationLauncher is ready. Add the button to the toolbar if we haven't already.
    /// </summary>
    private void OnAppLauncherReady () {
        LogD($"OnAppLauncherReady (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={m_stbButton})");
        AddButtonToStockToolbar();
    }

    private void CreateStockButton ()
    {
        LogD($"CreateStockButton (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={m_stbButton})");
        if (m_stbButtonIcon == null) {
            LoadStockToolbarIcon();
        }
        // Note: we may have to wait for the stock ApplicationLauncher to be ready before we can add the button to
        // it, so add an event callback which does the actual work if it is not ready
        if (ApplicationLauncher.Ready) {
            AddButtonToStockToolbar();
        } else {
            GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);
        }
    }

    private void DestroyStockButton ()
    {
        LogD($"DestroyStockButton (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={m_stbButton})");
        GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
        if (m_stbButton == null) {
            return;
        }
        ApplicationLauncher.Instance?.RemoveModApplication(m_stbButton);
        m_stbButton = null;
    }
    #endregion

    #region BlizzyToolbar
    private IButton m_btbButton = null;

    private bool IsBlizzyAvailable {
        get { return ToolbarManager.ToolbarAvailable; }
    }

    public void CreateBlizzyButton ()
    {
        LogD($"CreateBlizzyButton (GameScene=={HighLogic.LoadedScene}, blizzyButton=={m_btbButton})");
        if (m_btbButton != null) {
            return;
        }
        m_btbButton = ToolbarManager.Instance.add("DockingAlignment", "dockalign");
        m_btbButton.TexturePath = "NavyFish/Plugins/ToolbarIcons/DPAI";
        m_btbButton.ToolTip = "Show/Hide Docking Port Alignment Indicator";
        m_btbButton.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
        m_btbButton.Visible = true;
        m_btbButton.Enabled = true;
        m_btbButton.OnClick += (e) => { OnButtonClicked(); };
    }

    public void DestroyBlizzyButton ()
    {
        LogD($"DestroyBlizzyButton (GameScene=={HighLogic.LoadedScene}, blizzyButton=={m_btbButton})");
        if (m_btbButton == null) {
            return;
        }
        m_btbButton.Destroy();
        m_btbButton = null;
    }
    #endregion
}

} // End namespace NavyFish.DPAI.Toolbar
