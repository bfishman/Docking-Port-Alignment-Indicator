/*
Copyright (c) 2026, Rudolf Meier
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// This must be set to your namespace
namespace NavyFish.DPAI
{

/**********************************************************\
*          --- DO NOT EDIT BELOW THIS COMMENT ---          *
*                                                          *
* This file contains classes and interfaces to use the     *
* DockingPortEx and DockingFunctions Plugins without       *
* creating a hard dependency on them.                      *
*                                                          *
* There is nothing in this file that needs to be edited    *
* by hand.                                                 *
*                                                          *
*          --- DO NOT EDIT BELOW THIS COMMENT ---          *
\**********************************************************/

class DockingPortExHelper
{
    static bool Check_KSPAssembly(CustomAttributeData atr, String name, int v1, int v2)
    {
        if(atr.ConstructorArguments.Count != 3 && atr.ConstructorArguments.Count != 4)
            return false;
        if((string)atr.ConstructorArguments[0].Value != name)
            return false;
        if((int)atr.ConstructorArguments[1].Value < v1)
            return false;
        if(((int)atr.ConstructorArguments[1].Value == v1)
           && ((int)atr.ConstructorArguments[1].Value < v2))
            return false;
        return true;
    }

    static void FindAssemblies()
    {
        int f = 0;

        foreach(var x in AssemblyLoader.loadedAssemblies)
        {
            foreach(CustomAttributeData customAttribute in x.assembly.CustomAttributes)
            {
                if(customAttribute.AttributeType.Name == "KSPAssembly")
                {
                    if((assemblyModuleDockingPortEx == null)
                       && Check_KSPAssembly(customAttribute, "DockingPortEx", 1, 0))
                    {
                        assemblyModuleDockingPortEx = x.assembly;
                        ++f;
                    }

                    if((assemblyDockingFunctions == null)
                       && Check_KSPAssembly(customAttribute, "DockingFunctions", 1, 1))
                    {
                        assemblyDockingFunctions = x.assembly;
                        ++f;
                    }

                    if(f >= 2)
                        return;
                }
            }
        }

        // only return something if both were found
        assemblyModuleDockingPortEx = null;
        assemblyDockingFunctions = null;
    }

    static bool init = false;

    static Assembly assemblyModuleDockingPortEx;
    public static Type type_ModuleDockingPortEx;
    public static MethodInfo method_GetName;
    public static MethodInfo method_MakeReference;
    public static MethodInfo method_IsDocked;
    public static MethodInfo method_IsReadyFor;

    static Assembly assemblyDockingFunctions;
    public static Type type_IDockable;

    public static void Initialize()
    {
        if(init)
            return;

        FindAssemblies();

        if(assemblyModuleDockingPortEx != null)
        {
            type_ModuleDockingPortEx = assemblyModuleDockingPortEx.GetType("DockingPortNext.Module.ModuleDockingPortEx");
            method_GetName = type_ModuleDockingPortEx.GetMethod("GetName");
            method_MakeReference = type_ModuleDockingPortEx.GetMethod("MakeReferenceTransform");
            method_IsDocked = type_ModuleDockingPortEx.GetMethod("IsDocked");
            method_IsReadyFor = type_ModuleDockingPortEx.GetMethod("IsReadyFor");

            type_IDockable = assemblyDockingFunctions.GetType("DockingFunctions.IDockable");
        }

        init = true;
    }

    public static List<ModuleDockingPortExWrapper> FindModulesImplementing_ModuleDockingPortEx(Part part)
    {
        List<ModuleDockingPortExWrapper> filtered = new List<ModuleDockingPortExWrapper>();

        if(type_ModuleDockingPortEx != null)
        {
            List<ITargetable> modules = part.FindModulesImplementing<ITargetable>();

            foreach(var m in modules)
            {
                if(type_ModuleDockingPortEx.IsInstanceOfType(m))
                    filtered.Add(new ModuleDockingPortExWrapper((PartModule)m));
            }
        }

        return filtered;
    }

    public static List<ModuleDockingPortExWrapper> FindPartModulesImplementing_ModuleDockingPortEx(Vessel vessel)
    {
        List<ModuleDockingPortExWrapper> filtered = new List<ModuleDockingPortExWrapper>();

        if(type_ModuleDockingPortEx != null)
        {
            List<ITargetable> modules = vessel.FindPartModulesImplementing<ITargetable>();

            foreach(var m in modules)
            {
                if(type_ModuleDockingPortEx.IsInstanceOfType(m))
                    filtered.Add(new ModuleDockingPortExWrapper((PartModule)m));
            }
        }

        return filtered;
    }

    public static bool is_ModuleDockingPortEx(PartModule module)
    {
        if (type_ModuleDockingPortEx == null || module == null)
        {
            return false;
        }

        return type_ModuleDockingPortEx.IsInstanceOfType(module);
    }

    public static bool is_ModuleDockingPortEx(ITargetable target)
    {
        return is_ModuleDockingPortEx(target as PartModule);
    }

   public static ModuleDockingPortExWrapper as_ModuleDockingPortEx(PartModule module)
    {
        if (!is_ModuleDockingPortEx(module))
        {
            return null;
        }

        return new ModuleDockingPortExWrapper(module);
    }

    public static ModuleDockingPortExWrapper as_ModuleDockingPortEx(ITargetable target)
    {
        return as_ModuleDockingPortEx(target as PartModule);
    }

    public static bool is_IDockable(PartModule module)
    {
        if(type_IDockable == null || module == null)
            return false;

        return type_IDockable.IsInstanceOfType(module);
    }

    public static bool is_IDockable(ITargetable target)
    {
        return is_IDockable(target as PartModule);
    }
}

public class ModuleDockingPortExWrapper
{
    public ModuleDockingPortExWrapper(PartModule module)
    {
        moduleDockingPortEx = module.GetComponent(DockingPortExHelper.type_ModuleDockingPortEx);
    }

    public object moduleDockingPortEx;

    public String GetName()
    {
        return (String)DockingPortExHelper.method_GetName.Invoke(moduleDockingPortEx, null);
    }

    public void MakeReference()
    {
        DockingPortExHelper.method_MakeReference.Invoke(moduleDockingPortEx, null);
    }

    public bool IsDocked()
    {
        return (bool)DockingPortExHelper.method_IsDocked.Invoke(moduleDockingPortEx, null);
    }

    public bool IsReadyFor(ITargetable port)
    {
        object dockable = (port as PartModule)?.GetComponent(DockingPortExHelper.type_IDockable);

        if(dockable == null)
            return false;

        return (bool)DockingPortExHelper.method_IsReadyFor.Invoke(moduleDockingPortEx, new object[] { dockable });
    }
};
}
