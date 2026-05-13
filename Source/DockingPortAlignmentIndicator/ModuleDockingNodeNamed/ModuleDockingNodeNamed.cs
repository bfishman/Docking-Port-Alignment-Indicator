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
using System.Collections.Generic;
using KSP;
using KSPAssets;

namespace NavyFish.DPAI
{
    public class ModuleDockingNodeNamed : PartModule
    {
        public static RenameWindow renameWindow = null;
        public delegate void OnPortRenamed(ModuleDockingNodeNamed renamedNode);
        public static OnPortRenamed onPortRenamed;

        public ModuleDockingNodeNamed() {}

        public string getCustomName()
        {
            return portName;
        }

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#port_name", isPersistant = true)]
        public string portName;

        [KSPField(isPersistant = true)]
        public bool initialized;

        [KSPField(isPersistant = true)]
        public string controlTransformName;

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, externalToEVAOnly = false, unfocusedRange = 2000f, guiName = "#rename_port")]
        public void renameDockingPort()
        {
            RenameWindow.instance.DisplayForNode(this);
        }

        public override void OnAwake()
        {
            //Debug.Log("moduleDockingNodeNamed:  OnAwake Begun");
            base.OnAwake();
            //if (renameWindow == null)
            //{
            //    renameWindow = new RenameWindow();
            //}
            //Debug.Log("moduleDockingNodeNamed:  OnAwake Complete");
        }

        public override void OnStart(PartModule.StartState state)
        {
            renameWindow = RenameWindow.instance;
            //Debug.Log("moduleDockingNodeNamed:  OnStart Begun");
            base.OnStart(state);
            if(!initialized){
                initialized = true;
                renameModule(part.partInfo.title);
            }
            if(controlTransformName.Equals("not_initialized")){
                List<ModuleDockingNode> dockingNodes = this.part.FindModulesImplementing<ModuleDockingNode>();
                List<ModuleDockingNodeNamed> namedNodes = this.part.FindModulesImplementing<ModuleDockingNodeNamed>();

                if (dockingNodes.Count != namedNodes.Count)
                {
                    //Debug.Log("Mismatch between number of ModuleDockingNode and ModuleDockingNodeNamed nodes");
                }

                int index = 0;
                foreach (ModuleDockingNodeNamed namedNode in namedNodes)
                {
                    ModuleDockingNode dockingNode = dockingNodes[index];
                    namedNode.controlTransformName = dockingNode.controlTransformName;
                    index++;
                }
            }
            //Debug.Log("moduleDockingNodeNamed:  OnStart Complete");
        }

        internal void renameModule(string newName)
        {
            if (newName != null && newName != portName) {
                portName = newName;
                onPortRenamed?.Invoke(this);
            }
        }
    }
}
