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

namespace NavyFish.DPAI
{

public class Toolbar
{
    private bool _stock = false;
    private bool _blizzy = false;

    public void Dispose() {
        LogD($"Toolbar.Dispose(); GameScene={HighLogic.LoadedScene}, appLauncherButton={m_stbButton}, blizzyButto={m_btbButton}");
        GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
        GameEvents.onGUIApplicationLauncherUnreadifying.Remove(OnAppLauncherUnreadifying);
        SetToolbarButtons(false, false);
        m_instance = null;
    }

    private void OnButtonClicked() {
        LogD($"Toolbar.OnButtonClicked(); GameScene={HighLogic.LoadedScene}, appLauncherButton={m_stbButton}, blizzyButto={m_btbButton}");
        onToolbarButtonClicked?.Invoke();
    }

    #region Singleton
    private static Toolbar m_instance = null;

    private Toolbar() {
        LogD($"Toolbar(); GameScene={HighLogic.LoadedScene}");
        m_instance = this;
        GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);
        GameEvents.onGUIApplicationLauncherUnreadifying.Add(OnAppLauncherUnreadifying);
    }

    public static Toolbar Instance {
        get {
            m_instance = m_instance ?? new Toolbar();
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
        _blizzy = blizzy && IsBlizzyAvailable;
        _stock = stock || !_blizzy;

        if (_stock) {
            CreateStockButton();
        } else {
            DestroyStockButton();
        }
        if (_blizzy) {
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
        LogD($"Toolbar.AddButtonToStockToolbar (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={m_stbButton})");
        if (!_stock || !ApplicationLauncher.Ready || !HighLogic.LoadedSceneIsFlight || m_stbButton != null) {
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

    private void RemoveButtonFromStockToolbar() {
        LogD($"Toolbar.RemoveButtonFromStockToolbar (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={m_stbButton})");
        if (m_stbButton == null) {
            return;
        }
        ApplicationLauncher.Instance?.RemoveModApplication(m_stbButton);
        m_stbButton = null;
    }

    /// <summary>
    /// Called when the ApplicationLauncher is ready. Add the button to the toolbar if we haven't already.
    /// </summary>
    private void OnAppLauncherReady () {
        LogD($"Toolbar.OnAppLauncherReady (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={m_stbButton})");
        AddButtonToStockToolbar();
    }

    /// <summary>
    /// Called when the ApplicationLauncher is about to be destroyed.
    /// </summary>
    /// This is supposed to happen when the game transitions back to the main menu. If we don't remove the button from
    /// the stock toolbar it is rendered in the main menu despite there not being a stock toolbar ready! Or perhaps it
    /// is but it's empty by default and thus invisible.
    /// <param name="nextGameScene"></param>
    private void OnAppLauncherUnreadifying(GameScenes nextGameScene) {
        LogD($"Toolbar.OnAppLauncherUnreadifying (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={m_stbButton})");
        RemoveButtonFromStockToolbar();
    }

    private void CreateStockButton ()
    {
        LogD($"Toolbar.CreateStockButton (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={m_stbButton})");
        if (m_stbButtonIcon == null) {
            LoadStockToolbarIcon();
        }
        // Note: we may have to wait for the stock ApplicationLauncher to be ready before we can add the button to
        // it.
        if (ApplicationLauncher.Ready) {
            AddButtonToStockToolbar();
        }
    }

    private void DestroyStockButton ()
    {
        LogD($"Toolbar.DestroyStockButton (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={m_stbButton})");
        GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
        RemoveButtonFromStockToolbar();
    }
    #endregion

    #region BlizzyToolbar
    private IButton m_btbButton = null;

    public static bool IsBlizzyAvailable {
        get { return ToolbarManager.ToolbarAvailable; }
    }

    public void CreateBlizzyButton ()
    {
        LogD($"Toolbar.CreateBlizzyButton (GameScene=={HighLogic.LoadedScene}, blizzyButton=={m_btbButton})");
        if (!_blizzy || m_btbButton != null) {
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
        LogD($"Toolbar.DestroyBlizzyButton (GameScene=={HighLogic.LoadedScene}, blizzyButton=={m_btbButton})");
        if (m_btbButton == null) {
            return;
        }
        m_btbButton.Destroy();
        m_btbButton = null;
    }
    #endregion
}

} // End namespace NavyFish.DPAI.Toolbar
