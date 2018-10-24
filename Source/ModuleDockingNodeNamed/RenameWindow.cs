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
using UnityEngine;
using KSP.IO;

namespace DockingPortAlignment
{
    class RenameWindow
    {
        private Rect renameWindowRect = new Rect(0, 0, 250, 80);
        private ModuleDockingNodeNamed portModuleToRename = null;

        internal void DisplayForNode(ModuleDockingNodeNamed namedNode)
        {
            portModuleToRename = namedNode;
            RenderingManager.AddToPostDrawQueue(0, DrawRenameDialog);
        }

        private void DrawRenameDialog()
        {
            renameWindowRect.x = .5f * (Screen.width - renameWindowRect.width);
            renameWindowRect.y = .5f * Screen.height - 200;
            renameWindowRect = GUILayout.Window(1340, renameWindowRect, onRenameDialogDraw, "Rename Docking Port", HighLogic.Skin.window);
        }

        private void onRenameDialogDraw(int id)
        {
            if (portModuleToRename != null && (HighLogic.LoadedSceneIsEditor || portModuleToRename.vessel.loaded))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name:", GUILayout.Width(50));
                string newName = GUILayout.TextField(portModuleToRename.portName, GUILayout.ExpandWidth(true));
                portModuleToRename.renameModule(newName);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                bool isDone = GUILayout.Button("Done");
                GUILayout.EndHorizontal();
                if (isDone)
                {
                    portModuleToRename = null;
                    RenderingManager.RemoveFromPostDrawQueue(0, DrawRenameDialog);
                    renameWindowRect.Set(0, 0, 250, 80);
                }
            }
            else
            {
                RenderingManager.RemoveFromPostDrawQueue(0, DrawRenameDialog);
                renameWindowRect.Set(0, 0, 250, 80);
            }
        }
    }
}
