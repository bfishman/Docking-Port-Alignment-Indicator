/*
 *    ModuleDockingNodeNamed.cs
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

namespace DockingPortAlignment
{
    public class ModuleDockingNodeNamed : PartModule
    {
        static RenameWindow renameWindow = null;

        public ModuleDockingNodeNamed() {}

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Port Name", isPersistant = true)]
        public string portName;

        [KSPField(isPersistant = true)]
        public bool initialized;

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, externalToEVAOnly = false, unfocusedRange = 2000f, guiName = "Rename Port")]
        public void renameDockingPort()
        {
            renameWindow.DisplayForNode(this);
        }

        public override void OnAwake()
        {
            base.OnAwake();
            if (renameWindow == null)
            {
                renameWindow = new RenameWindow();
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if(!initialized){
                initialized = true;
                renameModule(part.partInfo.title);
            }
        }

        internal void renameModule(string newName)
        {
            portName = newName;
        }
    }
}
