/*
 *    RenameWindow.cs
 *
 *    Copyright (C) 2014, Bryan Fishman
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

using System;
using KSP.Localization;
using UnityEngine;

namespace NavyFish.DPAI
{
    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    public class RenameWindow : MonoBehaviour
    {
        public static RenameWindow instance;

        private Rect renameWindowRect = new Rect(0, 0, 250, 80);
        private ModuleDockingNodeNamed portModuleToRename = null;
        private bool windowOpen = false;
        private Vessel lastActiveVessel;
        private string windowTitle = getLocalisationString("#rename_docking_port", "Rename Docking Port");
        private static string sName = getLocalisationString("#name", "Name");
        private static string sOk = getLocalisationString("#ok", "Ok");
        private static string sCancel = getLocalisationString("#cancel", "Cancel");
        private Type DPAI;
        private string m_newPortName = null;
        public void Start()
        {
            //Debug.Log("RenameWindow: Start");
            DontDestroyOnLoad(this);
            RenameWindow.instance = this;

            DPAI = Type.GetType("NavyFish.DPAI.DockingPortAlignmentIndicator, DockingPortAlignmentIndicator");
        }

        public void DisplayForNode(ModuleDockingNodeNamed namedNode)
        {
            DisplayForNode(namedNode, windowTitle);
        }

        public void DisplayForNode(ModuleDockingNodeNamed namedNode, string windowTitle)
        {
            this.windowTitle = windowTitle;
            //Debug.Log("Rename Window: Display for Node");
            //if (RenderingManager.fetch != null)
            //{
            //    if (!windowOpen) RenderingManager.AddToPostDrawQueue(0, DrawRenameDialog);
            //    lastActiveVessel = FlightGlobals.ActiveVessel;
            //}

            if (!windowOpen)
            {
                if (FlightGlobals.ActiveVessel != null)
                {
                    lastActiveVessel = FlightGlobals.ActiveVessel;
                }
            }

            windowOpen = true;
            portModuleToRename = namedNode;
            m_newPortName = portModuleToRename.portName;

        }

        private void OnGUI()
        {
            if (windowOpen)
            {
                DrawRenameDialog();
            }
        }

        private void DrawRenameDialog()
        {
            //if (!windowOpen) return;
            renameWindowRect.x = .5f * (Screen.width - renameWindowRect.width);
            renameWindowRect.y = .5f * Screen.height - 200;
            renameWindowRect = GUILayout.Window(1340, renameWindowRect, onRenameDialogDraw, windowTitle, HighLogic.Skin.window);
        }

        private void onRenameDialogDraw(int id)
        {
            //Debug.Log("RenameWindow: onRenameDialogDraw");
            Vessel currentActiveVessel = FlightGlobals.ActiveVessel;
            bool activeVesselChanged = false;
            if (lastActiveVessel != currentActiveVessel)
            {
                activeVesselChanged = true;
                lastActiveVessel = currentActiveVessel;
            }

            if (portModuleToRename != null && (HighLogic.LoadedSceneIsEditor || portModuleToRename.vessel.loaded) && !activeVesselChanged)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(sName, GUILayout.Width(50));
                m_newPortName = GUILayout.TextField(m_newPortName, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                bool isDone = GUILayout.Button(sOk);
                bool isCancel = GUILayout.Button(sCancel);
                GUILayout.EndHorizontal();

                if (isDone || isCancel) {
                    closeWindow(isDone);
                }
            }
            else
            {
                closeWindow();
            }
        }

        public void closeWindow(bool ok = false)
        {
            if (portModuleToRename != null)
            {
                if (ok) {
                    portModuleToRename.renameModule(m_newPortName);
                }

                if (DPAI != null)
                {
                    Debug.Log("RenameWindow: attempting to invoke clearRenameHighlightBoxRPM");
                    DPAI.GetMethod("clearRenameHighlightBoxRPM").Invoke(null, null);
                }
                else {
                    Debug.Log("RenameWindow: DPAI is Null");
                }
                portModuleToRename = null;
            }
            renameWindowRect.Set(0, 0, 250, 80);
            windowOpen = false;

            //RenderingManager.RemoveFromPostDrawQueue(0, DrawRenameDialog);
        }

        // Return the localised string from tag f_tag.
        // If there is no localisation for f_tag, return f_default. If f_default is invalid,
        // return f_tag.
        public static string getLocalisationString(string f_tag, string f_default = "")
        {
            string s = f_default;

            Debug.Assert(!string.IsNullOrWhiteSpace(f_tag));

            bool ok = Localizer.TryGetStringByTag(f_tag, out s);
            if (!ok) {
                Debug.LogWarning("[DPAI.RenameWindow] Warning: localisation string missing - " + f_tag);
                s = string.IsNullOrWhiteSpace(f_default) ? f_tag : f_default;
            }

            return s;
        }
    }
}
