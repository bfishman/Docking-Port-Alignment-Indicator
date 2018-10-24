/*
 *    DockingPortAlignment.cs
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
using System.Collections.Generic;
using DockingPortAlignment;


namespace DockingPortAlignment
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DockingPortAlignment : MonoBehaviour
    {
        private static PluginConfiguration config;
        private static bool hasInitializedStyles = false;
        private static GUIStyle windowStyle, labelStyle, settingsButtonStyle;
        private static Rect windowPosition = new Rect();
        private static Rect lastPosition = new Rect();
        private static Rect debugWindowPosition = new Rect(50,200,350,200);
        
        static Rect selectedPortHUDRect = new Rect(0, 0, targetHUDiconSize, targetHUDiconSize);
        
        public static float gaugeScale = .86f;
        private static int backgroundTextureWidth = 400;
        private static int backgroundTextureHeight = 407;
        private static Rect backgroundRect = new Rect(0, 0f, backgroundTextureWidth * gaugeScale, backgroundTextureHeight * gaugeScale);

        private static int foregroundTextureWidth = 400;
        private static int foregroundTextureHeight = 457;
        private static Rect foregroundRect = new Rect(0, 0f, foregroundTextureWidth * gaugeScale, foregroundTextureHeight * gaugeScale);
        private static float visiblePortion = .77f;

        private static Rect leftButtonRect = new Rect();
        private static Rect rightButtonRect = new Rect();

        private static Rect settingsWindowPosition;
        //private static int settingsWindowWidth = 268;
        //private static int settingsWindowHeight = 120;

        private static Vector3 orientationDeviation = new Vector3();
        private static Vector2 translationDeviation = new Vector3();
        private static Vector2 transverseVelocity = new Vector2();
        private static float negativeOnBackHemisphere;
        private static float xTranslationNegativeOnBackHemi;
        private static float yTranslationNegativeOnBackHemi;
        private static float closureV;
        private static float distanceToTarget;
        private static float closureD;

        private static float velocityVectorIconSize = 42f;
        private static float transverseVelocityRange = 3.5f;
        private static float velocityVectorExponent = .75f;

        private static float alignmentGaugeRange = 60f;
        private static float alignmentExponent = .8f;

        private static float CDIExponent = .75f;
        private static float CDIexponentDecreaseBeginRange = 15f;
        private static float CDIexponentDecreaseDoneRange = 5f;

        private static float markerSize = 140f;
        private static float targetHUDiconSize = 22;
        private static float pulsePeriod = 1.42f;
        private static float pulseDurationRatio = .4f;

        private static Color colorCDINormal = new Color(.064f, .642f, 0f);
        private static Color colorCDIReverse = new Color(.747f, 0f, .05f);
        private static Color colorsettingsButtonActivated = new Color(.11f, .66f, .11f, 1f);
        private static Color colorsettingsButtonDeactivated = new Color(.22f, .26f, .29f, 1f);
        private static Color colorTargetPortHUDicon = new Color(.989f, .329f, .953f);
        private static Color colorGaugeLabels = new Color(.41f, .41f, .41f, 1f);

        public static Texture2D gaugeForegroundTex = new Texture2D(foregroundTextureWidth, foregroundTextureHeight, TextureFormat.ARGB32, false);
        public static Texture2D gaugeBackgroundTex = new Texture2D(backgroundTextureWidth, backgroundTextureHeight, TextureFormat.ARGB32, false);
        public static Texture2D rpmBackgroundTex = new Texture2D(320, 320, TextureFormat.ARGB32, false);
        public static Texture2D alignmentTex = new Texture2D(207, 207, TextureFormat.ARGB32, false);
        public static Texture2D directionArrowTex = new Texture2D(70, 150, TextureFormat.ARGB32, false);
        public static Texture2D prograde = new Texture2D(96, 96, TextureFormat.ARGB32, false);
        public static Texture2D retrograde = new Texture2D(96, 96, TextureFormat.ARGB32, false);
        public static Texture2D roll = new Texture2D(51, 33, TextureFormat.ARGB32, false);
        public static Texture2D targetPort = new Texture2D(40, 40, TextureFormat.ARGB32, false);
        public static Texture2D fontTexture = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        public static Texture2D appLauncherIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        public static Texture2D customToolbarIcon;

        public static BitmapFont bitmapFont;
        private static float textNumberScale = .8f;
        private static float textLabelScale = .6f;
        private static float textTargetNameScale = .77f;

        private static bool showSettings = false;
        private static bool useCDI = true;
        private static bool drawRollDigits = true;
        private static bool showIndicator;
        private static bool portWasCycled = false;
        private static bool currentTargetVesselWasLastSeenLoaded = false;
        public static bool gaugeVisiblityToggledOn = false;
        private static bool targetOutOfRange = false;
        private static bool allowAutoPortTargeting = true;
        private static bool excludeDockedPorts = true;
        private static bool drawHudIcon = true;
        private static bool resetTarget = false;
        private static bool blizzyToolbarAvailable = false;
        private static bool forceStockAppLauncher = false;

        public static bool RPMPageActive = false;
        
        static IButton toolbarButton;

        private static ApplicationLauncherButton appLauncherButton;

        static List<ITargetable> dockingModulesList = new List<ITargetable>();
        static int dockingModulesListIndex = -1;
        
        static ITargetable currentTarget = null;
        static ITargetable lastTarget = null;
        static Vessel currentTargetVessel = null;
        static Vessel lastTargetVessel = null;
        static Vessel lastActiveVessel = null;
        static int cycledModuleIndex = -1;
        static bool showHUDIconWhileIva = false;
        static bool wasLastIVA = false;

        public static List<PartModule> referencePoints = new List<PartModule>();
        public static Part referencePart;
        public static Part lastReferencePart;
        public static int referencePartIndex;
        public static ModuleDockingNodeNamed referencePartNamed;

        static ITargetable targetedDockingModule = null;

        private static ITargetable lastITargetable = null;
        public static ModuleDockingNodeNamed lastNamedNode = null;

        public static ModuleDockingNodeNamed targetNamedModule
        {
            get
            {
                if (targetedDockingModule == null) return null;
                if (!(targetedDockingModule is PartModule)) return null;

                if(targetedDockingModule == lastITargetable) return lastNamedNode;

                lastITargetable = targetedDockingModule;

                List<ModuleDockingNodeNamed> modules = (targetedDockingModule as PartModule).part.FindModulesImplementing<ModuleDockingNodeNamed>();
                if (modules.Count > 0)
                {
                    if (targetedDockingModule is ModuleDockingNode && modules.Count > 1)
                    {
                        foreach (ModuleDockingNodeNamed namedModule in modules)
                        {
                            if (namedModule.controlTransformName.Equals((targetedDockingModule as ModuleDockingNode).controlTransformName))
                            {
                                lastNamedNode = namedModule;
                                return namedModule;
                            }
                        }
                    }
                    lastNamedNode = modules[0];
                    return modules[0];
                }
                else
                {
                    return null;
                }   
            }
        }

        private static bool isOrientedTarget (ITargetable target)
        {
            return target.GetTargetingMode() == VesselTargetModes.DirectionVelocityAndOrientation;
        }

        private void addToStockAppLauncher()
        {
            if (appLauncherButton == null)
            {
                RUIToggleButton.OnTrue onTrueDelegate = new RUIToggleButton.OnTrue(onShowGUI);
                RUIToggleButton.OnFalse onFalseDelegate = new RUIToggleButton.OnFalse(onHideGUI);
                appLauncherButton = ApplicationLauncher.Instance.AddModApplication(
                    onTrueDelegate,
                    onFalseDelegate,
                    null, null, null, null,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    appLauncherIcon);
            }
        }
        
        private void removeFromStockAppLauncher()
        {
            //appLauncherButton = null;
        }

        private void onShowGUI()
        {
            //print("onShowGUI()");
            gaugeVisiblityToggledOn = true;
        }

        private void onHideGUI()
        {
            //print("onHideGUI");
            gaugeVisiblityToggledOn = false;
        }

        public void Awake()
        {
            loadTextures();

            LoadConfigFile();

            blizzyToolbarAvailable = ToolbarManager.ToolbarAvailable;

            if (forceStockAppLauncher || !blizzyToolbarAvailable)
            {
                if (ApplicationLauncher.Ready)
                {
                    //LauncherReady event has probably already fired, so manually call add function now:
                    addToStockAppLauncher();
                }

                //Hook into app-launcher ready event.
                GameEvents.onGUIApplicationLauncherReady.Add(delegate()
                {
                    addToStockAppLauncher();
                });

                GameEvents.onGUIApplicationLauncherDestroyed.Add(delegate()
                {
                    removeFromStockAppLauncher();
                });

            }
            else
            {
                toolbarButton = ToolbarManager.Instance.add("DockingAlignment", "dockalign");
                toolbarButton.TexturePath = "NavyFish/Plugins/ToolbarIcons/DPAI";
                toolbarButton.ToolTip = "Show/Hide Docking Port Alignment Indicator";
                toolbarButton.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                toolbarButton.Visible = true;
                toolbarButton.Enabled = true;
                toolbarButton.OnClick += (e) =>
                {
                    gaugeVisiblityToggledOn = !gaugeVisiblityToggledOn;
                };
            }
         

            if (!hasInitializedStyles) initStyles();

            RenderingManager.AddToPostDrawQueue(2, onGaugeDraw);

            if (shouldDebug) RenderingManager.AddToPostDrawQueue(2, OnDrawDebug);

            settingsWindowPosition = new Rect(0, 0, 0, 0);
            //settingsWindowPosition = new Rect((Screen.width - settingsWindowWidth) / 2f, (Screen.height - settingsWindowHeight)/ 2f, settingsWindowWidth, settingsWindowHeight);
            //print("end of awake");
        }
        
        public void Start()
        {
            LoadPrefs();
            //settingsWindowPosition.x = windowPosition.x;
            //settingsWindowPosition.width = windowPosition.width;
            settingsWindowPosition.y = windowPosition.yMax;
            //settingsWindowPosition.height = settingsWindowHeight;
            //print("end of start");
        }

        public void Update()
        {
            //print("Update: Start");
           
            //if (Input.GetKeyDown(KeyCode.B)){
            //        cycledModuleIndex = dockingModulesListIndex + 1;
            //        cycledModuleIndex %= dockingModulesList.Count;
            //        portWasCycled = true;
            //}

            determineTargetPort();

            bool sceneElligibleForIndicator = (HighLogic.LoadedSceneIsFlight && !FlightGlobals.ActiveVessel.isEVA && !MapView.MapIsEnabled);
            

            if (sceneElligibleForIndicator && gaugeVisiblityToggledOn)
            {
                showIndicator = true;

                //mousePos.Set(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                //print(mousePos);
                //if (windowPosition.Contains(mousePos))
                //{
                //    //print("Contains Mouse");
                //    containsMouse = true;
                //    //InputLockManager.SetControlLock(ControlTypes.All, "DPAI_LOCK");
                //}
                //else
                //{
                //    containsMouse = false;
                //    //InputLockManager.RemoveControlLock("DPAI_LOCK");
                //}
            }
            else
            {
                showIndicator = false;
            }

            if (showIndicator || (RPMPageActive && isIVA()))
            {
                if (targetedDockingModule != null) calculateGaugeData();
            }
            //print("Update: End");
        }

        private static bool isIVA()
        {
            if (InternalCamera.Instance != null)
            {
                return InternalCamera.Instance.isActive;
            }
            else return false;
        }

        public static int tickCount = 0;

        private static void determineTargetPort()
        {
            if (portWasCycled && dockingModulesList.Count > 1)
            {
                if (cycledModuleIndex < 0 || cycledModuleIndex > (dockingModulesList.Count - 1))
                {
                    //protection from bug when target deselected amidst cycling, resulting in -1 index with portWasCycled == true
                    //debugAlertFlag = true;
                    //debugAlertValue = cycledModuleIndex;
                    cycledModuleIndex = 0;
                }
                dockingModulesListIndex = cycledModuleIndex;
                targetedDockingModule = dockingModulesList[dockingModulesListIndex];

                portWasCycled = false;
                if (!targetedDockingModule.GetVessel().packed)
                {
                    FlightGlobals.fetch.SetVesselTarget(targetedDockingModule);
                }
                return;
            }

            if (lastActiveVessel != FlightGlobals.ActiveVessel || resetTarget)
            {
                //print("resetTarget");
                resetTarget = false;
                lastActiveVessel = FlightGlobals.ActiveVessel;
                lastTarget = null;
                lastTargetVessel = null;
                currentTargetVesselWasLastSeenLoaded = false;
                targetedDockingModule = null;
                dockingModulesListIndex = -1;
                portWasCycled = false;
                dockingModulesList.Clear();
                lastReferencePart = null;
                findReferencePoints();
                wasLastIVA = isIVA();
            }

            determineReferencePoint();
            tickCount++;

            if (lastReferencePart != referencePart)
            {
                print("DPAI: Reference Part Changed - tick " + tickCount);
                bool isCurrentlyIVA = isIVA();
                if (isCurrentlyIVA){
                    print("DPAI: Is currently IVA - tick " + tickCount);

                    if(!wasLastIVA){
                        wasLastIVA = true;
                        print("DPAI: Was not previously IVA - tick " + tickCount);
                        
                        if (FlightGlobals.ActiveVessel.Parts.Contains(lastReferencePart))
                        {
                            FlightGlobals.ActiveVessel.SetReferenceTransform(lastReferencePart);
                            print("DPAI: Re-setting Reference Part - tick " + tickCount);
                            findReferencePoints();
                        }
                    }
                } else {
                    wasLastIVA = false;
                }
                lastReferencePart = referencePart;
            }

            currentTarget = FlightGlobals.fetch.VesselTarget;

            if (currentTarget != null)
            {
                currentTargetVessel = currentTarget.GetVessel();

                if (currentTargetVessel != null)
                {
                    if (currentTargetVessel.loaded)
                    {
                        targetOutOfRange = false;

                        if (currentTargetVessel != lastTargetVessel || !currentTargetVesselWasLastSeenLoaded)
                        {
                            //Target Vessel has either changed or just become loaded.

                            lastTargetVessel = currentTargetVessel;

                            if (allowAutoPortTargeting)
                            {
                                //dockingModulesList = currentTargetVessel.FindPartModulesImplementing<ModuleDockingNode>();
                                //print("list rebuilt");
                                List<ITargetable> ITargetableList = currentTargetVessel.FindPartModulesImplementing<ITargetable>();
                                dockingModulesList.Clear();
                                foreach (ITargetable tgt in ITargetableList)
                                {
                                    if (tgt is ModuleDockingNode)
                                    {

                                        ModuleDockingNode port = tgt as ModuleDockingNode;
                                        if (port.state.StartsWith("Docked", StringComparison.OrdinalIgnoreCase) && excludeDockedPorts)
                                        {
                                            //print("continue");
                                            //do not add to list if module is already docked
                                            continue;
                                        }
                                        else
                                        {
                                            //print("1stAdd");
                                            dockingModulesList.Add(tgt);
                                        }
                                    }
                                    else
                                    {
                                        //print("2ndAdd");
                                        dockingModulesList.Add(tgt);
                                    }
                                }

                                if (dockingModulesList.Count > 0)
                                {
                                    //if (currentTarget is ModuleDockingNode && !currentTargetVessel.packed)
                                    if (isOrientedTarget(currentTarget) && !currentTargetVessel.packed)
                                    {
                                        //targetedDockingModule = currentTarget as ModuleDockingNode;
                                        targetedDockingModule = currentTarget;
                                        dockingModulesListIndex = dockingModulesList.FindIndex(m => m.Equals(targetedDockingModule));
                                        if (dockingModulesListIndex == -1)
                                        {
                                            //Unneccessary?
                                            //dockingModulesList.Add(targetedDockingModule);
                                            //dockingModulesListIndex = dockingModulesList.FindIndex(m => m.Equals(targetedDockingModule));
                                        }
                                        lastTarget = targetedDockingModule;
                                    }
                                    else
                                    {
                                        // Automatically select closest docking port.
                                        float shortestDistance = float.MaxValue;
                                        int shortestDistanceIndex = -1;
                                        for (int i = 0; i < dockingModulesList.Count; i++)
                                        {
                                            //ModuleDockingNode port = dockingModulesList[i];
                                            //float distance = Vector3.Distance(port.transform.position, FlightGlobals.ActiveVessel.ReferenceTransform.position);
                                            ITargetable port = dockingModulesList[i];
                                            float distance = Vector3.Distance(port.GetTransform().position, FlightGlobals.ActiveVessel.ReferenceTransform.position);
                                            if (distance < shortestDistance)
                                            {
                                                shortestDistance = distance;
                                                shortestDistanceIndex = i;
                                            }
                                        }

                                        dockingModulesListIndex = shortestDistanceIndex;
                                        targetedDockingModule = dockingModulesList[dockingModulesListIndex];
                                        lastTarget = targetedDockingModule;
                                    }
                                }
                                else
                                {
                                    // Target does not have any docking ports
                                    targetedDockingModule = null;
                                    dockingModulesListIndex = -1;
                                    dockingModulesList.Clear();
                                }
                            }
                            else
                            {
                                targetedDockingModule = null;
                                dockingModulesListIndex = -1;
                                dockingModulesList.Clear();
                            }
                        }
                        else if (currentTarget != lastTarget)
                        {
                            lastTarget = currentTarget;

                            //if (portWasCycled)
                            //{
                            //    portWasCycled = false;
                            //}
                            //else
                            //{
                                // This will happen either when the user manually selects a new target port by
                                // right-clicking on it, OR when a targetable part is targeted beyond 200m
                                // (because its parent vessel will be automatically re-targeted by KSP)
                                //if (currentTarget is ModuleDockingNode)
                                if (currentTarget is PartModule)
                                {
                                    // Likely caused by user right-click a port and setting as target
                                    //targetedDockingModule = currentTarget as ModuleDockingNode;
                                    targetedDockingModule = currentTarget;
                                    dockingModulesListIndex = dockingModulesList.FindIndex(m => m.Equals(targetedDockingModule));
                                }
                            //}
                        }

                        currentTargetVesselWasLastSeenLoaded = true;

                    }
                    else
                    {
                        if (currentTargetVesselWasLastSeenLoaded)
                        {
                            //Target just became unloaded
                            currentTargetVesselWasLastSeenLoaded = false;
                            FlightGlobals.fetch.SetVesselTarget(currentTargetVessel);
                        }

                        targetedDockingModule = null;
                        dockingModulesListIndex = -1;
                        dockingModulesList.Clear();

                        targetOutOfRange = true;

                    }
                }
                else
                {
                    //Current target does not have an associated vessel
                    targetedDockingModule = null;
                    dockingModulesListIndex = -1;
                    dockingModulesList.Clear();
                }
            }
            else
            {
                // Current Target is null
                currentTargetVessel = null;
                lastTarget = null;
                lastTargetVessel = null;
                targetedDockingModule = null;
                dockingModulesListIndex = -1;
                dockingModulesList.Clear();
            }
        }

        private static void calculateGaugeData()
        {
            Transform selfTransform = FlightGlobals.ActiveVessel.ReferenceTransform;
            //ModuleDockingNode targetPort = targetedDockingModule;
            //Transform targetTransform = targetPort.transform;

            //Vector3 targetPortOutVector;
            //Vector3 targetPortRollReferenceVector;

            //if (targetPort.part.name == "dockingPortLateral")
            //{
            //    targetPortOutVector = -targetTransform.forward.normalized;
            //    targetPortRollReferenceVector = -targetTransform.up;
            //}
            //else
            //{
            //    targetPortOutVector = targetTransform.up.normalized;
            //    targetPortRollReferenceVector = targetTransform.forward;
            //}

            ITargetable targetPort = targetedDockingModule as ITargetable;

            Transform targetTransform = targetPort.GetTransform();

			Vector3 targetPortOutVector = targetTransform.forward.normalized;
			Vector3 targetPortRollReferenceVector = -targetTransform.up;
            
            orientationDeviation.x = AngleAroundNormal(-targetPortOutVector, selfTransform.up, selfTransform.forward);
            orientationDeviation.y = AngleAroundNormal(-targetPortOutVector, selfTransform.up, -selfTransform.right);
            orientationDeviation.z = AngleAroundNormal(targetPortRollReferenceVector, selfTransform.forward, selfTransform.up);
            orientationDeviation.z = (360 - orientationDeviation.z) % 360;

            Vector3 targetToOwnship = selfTransform.position - targetTransform.position;

            translationDeviation.x = AngleAroundNormal(targetToOwnship, targetPortOutVector, selfTransform.forward);
            translationDeviation.y = AngleAroundNormal(targetToOwnship, targetPortOutVector, -selfTransform.right);

            if (Math.Abs(translationDeviation.x) >= 90)
            {
                xTranslationNegativeOnBackHemi = -1;
            }
            else
            {
                xTranslationNegativeOnBackHemi = 1;
            }

            if (Math.Abs(translationDeviation.y) >= 90)
            {
                yTranslationNegativeOnBackHemi = -1;
            }
            else
            {
                yTranslationNegativeOnBackHemi = 1;
            }

            if (xTranslationNegativeOnBackHemi < 0 || yTranslationNegativeOnBackHemi < 0){
                negativeOnBackHemisphere = -1;
            }else{
                negativeOnBackHemisphere = 1;
            }

            float normalVelocity = Vector3.Dot(FlightGlobals.ship_tgtVelocity, targetPortOutVector);
            closureV = -normalVelocity*negativeOnBackHemisphere;
            

            Vector3 globalTransverseVelocity = FlightGlobals.ship_tgtVelocity - normalVelocity * targetPortOutVector;
            transverseVelocity.x = Vector3.Dot(globalTransverseVelocity, selfTransform.right);
            transverseVelocity.y = Vector3.Dot(globalTransverseVelocity, selfTransform.forward);            

            distanceToTarget = targetToOwnship.magnitude;
            closureD = Vector3.Dot(targetToOwnship, targetPortOutVector);
        }

        //return signed angle in relation to normal's 2d plane
        private static float AngleAroundNormal(Vector3 a, Vector3 b, Vector3 up)
        {
            return AngleSigned(Vector3.Cross(up, a), Vector3.Cross(up, b), up);
        }

        //-180 to 180 angle
        private static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 up)
        {
            if (Vector3.Dot(Vector3.Cross(v1, v2), up) < 0) //greater than 90 i.e v1 left of v2
                return -Vector3.Angle(v1, v2);
            return Vector3.Angle(v1, v2);
        }

        private bool settingsWindowOverflow = false;

        private void onGaugeDraw()
        {
            if (drawHudIcon)
            {
                bool isCurrentlyIVA = isIVA();
                if ((showIndicator && !isCurrentlyIVA) || (showHUDIconWhileIva && RPMPageActive && isCurrentlyIVA))
                {
                    drawTargetPortHUDIndicator();
                }
            }

            //print("onGaugeDraw: Start");
            if (showIndicator)
            {
                windowPosition.width = foregroundTextureWidth * gaugeScale;
                windowPosition.height = foregroundTextureHeight * gaugeScale;
                //windowPosition.yMin = settingsWindowPosition.yMin - windowPosition.height;
                windowPosition = constrainToScreen(GUI.Window(1337, windowPosition, drawIndicatorContents, "DPAI", labelStyle));

                leftButtonRect.yMin = foregroundRect.height - (57 * gaugeScale);
                leftButtonRect.yMax = foregroundRect.height - (10 * gaugeScale);
                leftButtonRect.xMin = (18 * gaugeScale);
                leftButtonRect.xMax = (69 * gaugeScale);

                rightButtonRect.yMin = leftButtonRect.yMin;
                rightButtonRect.yMax = leftButtonRect.yMax;
                rightButtonRect.xMin = foregroundRect.width - (69 * gaugeScale);
                rightButtonRect.xMax = foregroundRect.width - (18 * gaugeScale);

                if (showSettings)
                {
                    //settingsWindowPosition = constrainToScreen(GUILayout.Window(1339, settingsWindowPosition, drawSettingsWindowContents, "Docking Alignment Indicator Settings", windowStyle));
                    settingsWindowPosition.x = windowPosition.x;
                    settingsWindowPosition.y = windowPosition.yMax;
                    if (!settingsWindowOverflow) settingsWindowPosition.width = windowPosition.width;
                    //settingsWindowPosition.height = settingsWindowHeight;
                    settingsWindowPosition = GUILayout.Window(1339, settingsWindowPosition, drawSettingsWindowContents, "DPAI Settings", windowStyle);
                    if (settingsWindowPosition.width > windowPosition.width)
                    {
                        settingsWindowOverflow = true;
                    }
                    else
                    {
                        settingsWindowOverflow = false;
                    }
                    
                    //GUILayout.Window(1339, settingsWindowPosition, drawSettingsWindowContents, "DPAI Settings", windowStyle);
                }
            }
            //print("onGaugeDraw: End");
        }

        private static Rect constrainToScreen(Rect r)
        {
            r.x = Mathf.Clamp(r.x, 75 - r.width, Screen.width - 75);
            r.y = Mathf.Clamp(r.y, 75 - r.height, Screen.height - 75);
            return r;
        }

        private static Vector3 identityScaleV3 = new Vector3(1, 1, 1);
        private static Vector3 glassCenterV3 = new Vector3();
        private static Rect screenRect = new Rect();
        private static Vector2 glassCenter = new Vector2();
        private static Rect fullScreenRect = new Rect();
        static float screenPercentRPM = 1f;//.935f;
        static Rect rpmDrawableRect = new Rect();
        public static void drawIndicatorContentsRPM(int screenWidth, int screenHeight)
        {
            fullScreenRect.Set(0, 0, screenWidth, screenHeight);
            float virtualWidth = screenWidth*screenPercentRPM;
            float virtualHeight = screenHeight*screenPercentRPM;
            float xOffset = (screenWidth - virtualWidth) * .5f;
            float yOffset = 0;//(screenHeight - virtualHeight) * .5f;

            screenRect.Set(xOffset, yOffset, virtualWidth, virtualHeight);
            glassCenter = screenRect.center;
            glassCenterV3.Set(glassCenter.x, glassCenter.y, 0);

            rpmDrawableRect.Set(xOffset, vertLineHeaderChop, virtualWidth, virtualHeight - vertLineFooterChop - vertLineHeaderChop);

            //if (preRotation == null)
            //{
            //    preRotation = Matrix4x4.TRS(new Vector3(roll.width / 2f, roll.height / 2f), Quaternion.Euler(0f, 0f, 90f), identityScaleV3);
            //}

            Graphics.DrawTexture(screenRect, rpmBackgroundTex);

            float baseScale = screenRect.width / ((float)backgroundTextureWidth);

            if (targetedDockingModule != null)
            {
                if (useCDI)
                {
                    float xVal = 0, yVal = 0;
                    float visibleYOffset = vertLineHeaderChop;
                    Color colorCDI = determineCDIcolor();
                 
                    calculateCDIvalues0to1(ref xVal, ref yVal);  
                    
                    //Drawing.DrawVerticalLineGraphics(xVal, screenRect.y + vertLineHeaderChop, screenRect.height - _vertCDILineChop - vertLineHeaderChop, 4f, colorCDI);
                    //Drawing.DrawHorizontalLineGraphics(screenRect.x, yVal, screenRect.width, 4f, colorCDI);

                    Drawing.DrawVerticalLineGraphics(glassCenter.x + (xVal - .5f) * screenRect.width, rpmDrawableRect.yMin, rpmDrawableRect.height, 4f, colorCDI);
                    Drawing.DrawHorizontalLineGraphics(rpmDrawableRect.xMin, Math.Max(glassCenter.y + (yVal-.5f) * screenRect.height, visibleRect.yMin), rpmDrawableRect.width, 4f, colorCDI);
                }   

                GL.PushMatrix();
                if (Math.Abs(orientationDeviation.x) > alignmentGaugeRange || Math.Abs(orientationDeviation.y) > alignmentGaugeRange)
                {
                    Vector2 normDir = new Vector2(orientationDeviation.x, orientationDeviation.y).normalized;
                    float angle = (float)Math.Atan2(normDir.x, -normDir.y) * UnityEngine.Mathf.Rad2Deg;

                    //float arrowLength = visiblePortion * glassCenter.y;
                    float arrowLength = screenRect.height * arrowLengthMult;
                    float arrowWidth = arrowLength * directionArrowTex.width / directionArrowTex.height;
                                        
                    //Rect arrowRect = new Rect(0.5f * (backgroundRect.width - arrowWidth), glassCenter.y - arrowLength, arrowWidth, arrowLength);
                    //Testing this..
                    Rect arrowRect = new Rect(-arrowWidth*.5f, -arrowLength*arrowLengthOffsetMult, arrowWidth, arrowLength);

                    //GUIUtility.RotateAroundPivot(angle, glassCenter);
                    GL.PushMatrix();
                    //TODO: extract these from method (optimize)
                    
                    //GL.MultMatrix(Matrix4x4.TRS(new Vector3(0,0,0), Quaternion.Euler(0, 0, angle) ,new Vector3(1,1,1)));
                    //Trying this..
                    GL.MultMatrix(Matrix4x4.TRS(glassCenterV3, Quaternion.Euler(0, 0, angle), identityScaleV3));

                    Graphics.DrawTexture(arrowRect, directionArrowTex);
                    GL.PopMatrix();
                    
                }
                else
                {
                    float displayX = scaleExponentially(orientationDeviation.x / alignmentGaugeRange, alignmentExponent);
                    float displayY = scaleExponentially(orientationDeviation.y / alignmentGaugeRange, alignmentExponent);

                    float scaledMarkerSize = markerSize * baseScale;

                    Rect markerRect = new Rect(glassCenter.x * (1 + displayX) - scaledMarkerSize*.5f,
                                            glassCenter.y * (1 + displayY) - scaledMarkerSize * .5f,
                                            scaledMarkerSize,
                                            scaledMarkerSize);

                    Graphics.DrawTexture(markerRect, alignmentTex);


                    float scaledRollWidth = roll.width * baseScale;
                    float scaledRollHeight = roll.height * baseScale;
                    //GUIUtility.RotateAroundPivot(-orientationDeviation.z, glassCenter);
                    GL.PushMatrix();
                    //TODO: extract these from method (optimize)
                    
                    GL.MultMatrix(Matrix4x4.TRS(glassCenterV3, Quaternion.Euler(0, 0, -orientationDeviation.z), identityScaleV3));
                    //GL.MultMatrix(preRotation);

                    //Graphics.DrawTexture(new Rect(glassCenter.x - .5f * scaledRollWidth, (roll.height + 20) * baseScale, scaledRollWidth, scaledRollHeight), roll);
                    Graphics.DrawTexture(new Rect(-scaledRollWidth/2f, (scaledRollHeight + rollOffset - screenRect.height)/2f, scaledRollWidth, scaledRollHeight), roll);
                    GL.PopMatrix();
                }

                GL.PopMatrix();

                if (useCDI)
                {
                    drawVelocityVectorGraphics(screenRect, baseScale);
                }

                drawGaugeValuesRPM(screenRect, baseScale);
            }

            drawRefandTgtLabelsRPM(fullScreenRect);
            drawTargetPortNameRPM(fullScreenRect);
            drawReferencePortNameRPM(fullScreenRect);
            //drawHighlightBox(fullScreenRect);
        }

        //private static void drawHighlightBox(Rect screenRect)
        //{
        //    switch (currentHighlightBox)
        //    {
        //        case HighlightBox.NONE:
        //            return;
        //        case HighlightBox.LEFT:

        //            break;
        //        case HighlightBox.RIGHT:
        //            break;
        //    }
        //}

        //public static void drawRPMHelpScreen(int screenWidth, int screenHeight)
        //{
        //    fullScreenRect.Set(0, 0, screenWidth, screenHeight);
        //    //float virtualWidth = screenWidth * screenPercentRPM;
        //    //float virtualHeight = screenHeight * screenPercentRPM;
        //    //float xOffset = (screenWidth - virtualWidth) * .5f;
        //    //float yOffset = 0;//(screenHeight - virtualHeight) * .5f;

        //    //screenRect.Set(xOffset, yOffset, virtualWidth, virtualHeight);
        //    //glassCenter.Set(xOffset + (screenRect.width / 2f), yOffset + (screenRect.height / 2f));
        //    //glassCenterV3.Set(glassCenter.x, glassCenter.y, 0);

        //    //Graphics.DrawTexture(screenRect, rpmBackgroundTex);

        //    drawRefandTgtLabelsRPM(fullScreenRect);
        //}

        static Rect visibleRect = new Rect();
        public static void drawIndicatorContents(int windowID)
        {
            //print("drawIndicatorContents: Start");

            backgroundRect.width = backgroundTextureWidth * gaugeScale;
            backgroundRect.height = backgroundTextureHeight * gaugeScale;
            
            foregroundRect.width = foregroundTextureWidth * gaugeScale;
            foregroundRect.height = foregroundTextureHeight * gaugeScale;

            //from foreground texture's transparent portion
            visibleRect.Set(40 * gaugeScale, 43 * gaugeScale, 318 * gaugeScale, 319 * gaugeScale);
            Vector2 glassCenter = visibleRect.center;


            GUI.DrawTexture(backgroundRect, gaugeBackgroundTex);

            if (targetedDockingModule != null)
            {


                if (useCDI)
                {
                    float xVal = 0 , yVal = 0;

                    Color colorCDI = determineCDIcolor();
                    calculateCDIvalues0to1(ref xVal, ref yVal);

                    Drawing.DrawVerticalLine(visibleRect.xMin + xVal*visibleRect.width, visibleRect.yMin, visibleRect.height, 2f, colorCDI);
                    Drawing.DrawHorizontalLine(visibleRect.xMin, visibleRect.yMin + yVal*visibleRect.height, visibleRect.width, 2f, colorCDI);
                }

                Matrix4x4 matrixBackup = GUI.matrix;
                if (Math.Abs(orientationDeviation.x) > alignmentGaugeRange || Math.Abs(orientationDeviation.y) > alignmentGaugeRange)
                {
                    Vector2 normDir = new Vector2(orientationDeviation.x, orientationDeviation.y).normalized;
                    float angle = (float)Math.Atan2(normDir.x, -normDir.y) * UnityEngine.Mathf.Rad2Deg;

                    //rpm
                    //float arrowLength = screenRect.height * arrowLengthMult;
                    //float arrowWidth = arrowLength * directionArrowTex.width / directionArrowTex.height;
                    //Rect arrowRect = new Rect(-arrowWidth * .5f, -arrowLength * arrowLengthOffsetMult, arrowWidth, arrowLength);

                    //new
                    float arrowLength = visiblePortion * backgroundRect.height * arrowLengthMult;
                    float arrowWidth = arrowLength * directionArrowTex.width / directionArrowTex.height;
                    Rect arrowRect = new Rect(0.5f * (backgroundRect.width - arrowWidth), glassCenter.y - arrowLength * arrowLengthOffsetMult, arrowWidth, arrowLength);

                    //old
                    //float arrowLength = visiblePortion * glassCenter.y * .5f;  // .5f is fudge factor w/ the new texture
                    //float arrowWidth = arrowLength * directionArrowTex.width / directionArrowTex.height;
                    //Rect arrowRect = new Rect(0.5f * (backgroundRect.width - arrowWidth), glassCenter.y - arrowLength, arrowWidth, arrowLength);

                    GUIUtility.RotateAroundPivot(angle, glassCenter);

                    GUI.DrawTexture(arrowRect, directionArrowTex);
                    GUI.matrix = matrixBackup;
                }
                else
                {
                    float displayX = scaleExponentially(orientationDeviation.x / alignmentGaugeRange, alignmentExponent);
                    float displayY = scaleExponentially(orientationDeviation.y / alignmentGaugeRange, alignmentExponent);

                    float scaledMarkerSize = markerSize * gaugeScale;

                    Rect markerRect = new Rect(glassCenter.x * (1 + displayX) - .5f * scaledMarkerSize,
                                            glassCenter.y * (1 + displayY) - .5f * scaledMarkerSize,
                                            scaledMarkerSize,
                                            scaledMarkerSize);

                    GUI.DrawTexture(markerRect, alignmentTex);

                    //GUIUtility.RotateAroundPivot(orientationDeviation.z, glassCenter);
                    GUIUtility.RotateAroundPivot(-orientationDeviation.z, glassCenter);

                    float scaledRollWidth = roll.width * gaugeScale;
                    float scaledRollHeight = roll.height * gaugeScale;

                    GUI.DrawTexture(new Rect(glassCenter.x - .5f * scaledRollWidth, (roll.height + 20) * gaugeScale, scaledRollWidth, scaledRollHeight), roll);
                }

                GUI.matrix = matrixBackup;

                if (useCDI)
                {
                    drawVelocityVector(visibleRect);
                }

                drawGaugeValues();

            }

            GUI.DrawTexture(foregroundRect, gaugeForegroundTex);

            drawTargetPortName();

            Color lastBackColor = GUI.backgroundColor;
            if (showSettings)
            {
                GUI.backgroundColor = colorsettingsButtonActivated;
            }
            else
            {
                GUI.backgroundColor = colorsettingsButtonDeactivated;
            }

            bool settingsButtonClicked = GUI.Button(new Rect(146 * gaugeScale, 367 * gaugeScale, 110 * gaugeScale, 18 * gaugeScale), "", settingsButtonStyle);
            drawGlyphString("Settings", 173 * gaugeScale, 364 * gaugeScale, gaugeScale * .62f);

            if (settingsButtonClicked) showSettings = !showSettings;

            if (allowAutoPortTargeting)
            {
                Event ev = Event.current;
                if (ev.type == EventType.mouseDown && ev.button == 0)
                {
                    if (rightButtonRect.Contains(ev.mousePosition))
                    {
                        cyclePortRight();

                    }
                    else if (leftButtonRect.Contains(ev.mousePosition))
                    {
                        cyclePortLeft();

                    }
                }
            }

            GUI.DragWindow();

            if (windowPosition.x != lastPosition.x || windowPosition.y != lastPosition.y)
            {
                lastPosition.x = windowPosition.x;
                lastPosition.y = windowPosition.y;
                saveWindowPosition();
            }
            
            //print("drawIndicatorContents: End");
        }

        public static void cyclePortLeft()
        {
            cycledModuleIndex = dockingModulesListIndex - 1;
            if (cycledModuleIndex < 0) cycledModuleIndex = (dockingModulesList.Count - 1);
            portWasCycled = true;
        }

        public static void cyclePortRight()
        {
            if (dockingModulesList.Count > 0)
            {
                cycledModuleIndex = dockingModulesListIndex + 1;
                cycledModuleIndex %= dockingModulesList.Count;
                portWasCycled = true;
            }
        }

        private void drawSettingsWindowContents(int id)
        {
            bool last;
            float lastFloat;
            //print("drawSettingsWindowContents: Start");
            //GUILayout.BeginHorizontal();
            //last = useCDI;
            //useCDI = GUILayout.Toggle(useCDI, "Display CDI Lines");
            //if (useCDI != last) saveConfigSettings();
            //GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            //last = drawRollDigits;
            //drawRollDigits = GUILayout.Toggle(drawRollDigits, "Display Roll Degrees");
            //if (drawRollDigits != last) saveConfigSettings();
            //GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            last = drawHudIcon;
            drawHudIcon = GUILayout.Toggle(drawHudIcon, "Display Target Port HUD Icon");
            if (drawHudIcon != last)
            {
                saveConfigSettings();
                settingsWindowPosition.height = 0;
            }
            GUILayout.EndHorizontal();

            if (drawHudIcon)
            {
                GUILayout.BeginHorizontal();
                last = showHUDIconWhileIva;
                GUILayout.Space(14f);
                showHUDIconWhileIva = GUILayout.Toggle(showHUDIconWhileIva, "Display while IVA (and using RPM)");
                if (showHUDIconWhileIva != last)
                {
                    saveConfigSettings();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("HUD Icon Size:");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                lastFloat = targetHUDiconSize;
                targetHUDiconSize = GUILayout.HorizontalSlider(targetHUDiconSize, 10f, 60f);
                GUILayout.EndHorizontal();
                if (targetHUDiconSize != lastFloat)
                {
                    saveConfigSettings();
                }
            }

            GUILayout.BeginHorizontal();
            last = allowAutoPortTargeting;
            allowAutoPortTargeting = GUILayout.Toggle(allowAutoPortTargeting, "Enable Auto Targeting (and Cycling)");
            if (allowAutoPortTargeting != last)
            {
                saveConfigSettings();
                settingsWindowPosition.height = 0;
                resetTarget = true;
            }
            GUILayout.EndHorizontal();

            if (allowAutoPortTargeting)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(14f);
                last = excludeDockedPorts;
                excludeDockedPorts = GUILayout.Toggle(excludeDockedPorts, "Exclude Docked Ports");
                if (excludeDockedPorts != last)
                {
                    saveConfigSettings();
                    resetTarget = true;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("GUI Scale:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float lastScale = gaugeScale;
            gaugeScale = GUILayout.HorizontalSlider(gaugeScale, 0.4f, 3.0f);
            GUILayout.EndHorizontal();
            if (gaugeScale != lastScale)
            {
                windowPosition.width = foregroundTextureWidth * gaugeScale;
                windowPosition.height = foregroundTextureHeight * gaugeScale;
                windowPosition.y = settingsWindowPosition.y - windowPosition.height;
                saveConfigSettings();
            }
            
            //GUI.DragWindow();

            //print("drawSettingsWindowContents: Start");
        }

        private static void drawGaugeValues()
        {
            float dstXpos = 89 * gaugeScale;
            float cvelXpos = 289 * gaugeScale;
            float yPos = 335 * gaugeScale;

            Color originalColor = GUI.color;
            
            GUI.color = colorGaugeLabels;
            drawGlyphString("DST", 45 * gaugeScale, yPos + 6 * gaugeScale, gaugeScale * textLabelScale);
            drawGlyphString("CVEL", 238 * gaugeScale, yPos + 6 * gaugeScale, gaugeScale * textLabelScale);
            drawGlyphString("CDST", 236 * gaugeScale, (310 + 6) * gaugeScale, gaugeScale * textLabelScale);
            GUI.color = originalColor;

            drawGlyphString(distanceToTarget.ToString("F1"), dstXpos, yPos);
            drawGlyphString(closureV.ToString("F"), cvelXpos, yPos);
            drawGlyphString(closureD.ToString("F1"), 289 * gaugeScale, 310 * gaugeScale);


            if (drawRollDigits)
            {
                float rDegXPos = 306 * gaugeScale;
                float rDegYPos = 41 * gaugeScale;
                GUI.color = colorGaugeLabels;
                drawGlyphString("R\u00B0", 280 * gaugeScale, (38 + 6) * gaugeScale, gaugeScale * textLabelScale);
                GUI.color = originalColor;
                drawGlyphString(orientationDeviation.z.ToString("F1"), rDegXPos, rDegYPos);
            }
        }

        static int _dstXpos = 59;
        static int _cvelXpos = 300;
        static int _yPos = 335;
        static int _LABEL_yOFF = 10;
        static int _DSTLABEL_x = 14;
        static int _CVELLABEL_x = 238;
        static int _CDSTLABEL_x = 236;
        static int _CDSTLABEL_y = 305;

        static int _CLOSURED_x = 300;
        static int _CLOSURED_y = 305;

        private static void drawGaugeValuesRPM(Rect screenRect, float scale)
        {
            float dstXpos = _dstXpos * scale;
            float cvelXpos = _cvelXpos * scale;
            float yPos = _yPos * scale;

            Color originalColor = GUI.color;

            GUI.color = colorGaugeLabels;
            drawGlyphStringRPM("DST", screenRect.x + _DSTLABEL_x * scale, screenRect.y + yPos + _LABEL_yOFF * scale, scale * textLabelScale);
            drawGlyphStringRPM("CVEL", screenRect.x + _CVELLABEL_x * scale, screenRect.y + yPos + _LABEL_yOFF * scale, scale * textLabelScale);
            drawGlyphStringRPM("CDST", screenRect.x + _CDSTLABEL_x * scale, screenRect.y + (_CDSTLABEL_y + _LABEL_yOFF) * scale, scale * textLabelScale);
            GUI.color = originalColor;

            drawGlyphStringRPM(distanceToTarget.ToString("F1"), screenRect.x + dstXpos, screenRect.y + yPos, scale);
            drawGlyphStringRPM(closureV.ToString("F"), screenRect.x + cvelXpos, screenRect.y + yPos, scale);
            drawGlyphStringRPM(closureD.ToString("F1"), screenRect.x + _CLOSURED_x * scale, screenRect.y + _CLOSURED_y * scale, scale);


            if (drawRollDigits)
            {
                float rDegXPos = _rDegXPos * scale;
                float rDegYPos = _rDegYPos * scale;
                GUI.color = colorGaugeLabels;
                drawGlyphStringRPM("R\u00B0", screenRect.x + _degSign_x * scale, screenRect.y + _degSign_y * scale, scale * textLabelScale);
                GUI.color = originalColor;
                drawGlyphStringRPM(orientationDeviation.z.ToString("F1"), screenRect.x + rDegXPos, screenRect.y + rDegYPos, scale);
            }
        }

        private static void drawTargetPortName()
        {
            String targetDisplayName = determineTargetPortName();
            BitmapFont.StringDimensions stringDimensions = bitmapFont.getStringDimensions(targetDisplayName, 1f);
            float widthScale = 190 * gaugeScale / stringDimensions.width;
            float heightScale = 20 * gaugeScale / (stringDimensions.height);
            textTargetNameScale = Math.Min(widthScale, heightScale);
            float x = foregroundRect.center.x - stringDimensions.width * textTargetNameScale / 2f;
            float y = foregroundRect.yMax - (34 * gaugeScale) - (stringDimensions.yOffset + .5f * stringDimensions.height) * textTargetNameScale;
            
            drawGlyphString(targetDisplayName, x, y, textTargetNameScale);
        }

        private static void drawRefandTgtLabelsRPM(Rect screenRect)
        {
            drawGlyphStringRPM("TGT:", tgtX, screenRect.yMax - rtLabelY, rtLabelScale, currentHighlightBox == HighlightBox.LEFT ? Color.yellow : Color.white);
            drawGlyphStringRPM("REF:", screenRect.center.x + refX, screenRect.yMax - rtLabelY, rtLabelScale, currentHighlightBox == HighlightBox.RIGHT ? Color.yellow : Color.white);
        }

        private static void drawTargetPortNameRPM(Rect screenRect)
        {
            //String targetDisplayName = "Tgt: ";
            //targetDisplayName += determineTargetPortName();

            String targetDisplayName = determineTargetPortName();
            
            BitmapFont.StringDimensions stringDimensions = bitmapFont.getStringDimensions(targetDisplayName, 1f);
            float virtualWidth = screenRect.width * .5f - tgtX - rtLabelSpacing;
            float widthScale = virtualWidth / stringDimensions.width;
            float virtualHeight = rpmTgtRefTextHeight;
            float heightScale = virtualHeight / (stringDimensions.height);
            textTargetNameScale = Math.Min(widthScale, heightScale);
            //float x = screenRect.center.x - stringDimensions.width * textTargetNameScale / 2f;
            //float x = 0f;
            float x = tgtX + rtLabelSpacing + virtualWidth*.5f - (stringDimensions.width * textTargetNameScale / 2f);
            float y = screenRect.yMax - _rpmTextYTop - (stringDimensions.yOffset + .5f * stringDimensions.height) * textTargetNameScale;

            drawGlyphStringRPM(targetDisplayName, x, y, textTargetNameScale, currentHighlightBox == HighlightBox.LEFT ? Color.yellow : Color.white);
        }

        //private static List<ModuleDockingNodeNamed> refNamedModules = new List<ModuleDockingNodeNamed>();

        public static string getReferencePortName()
        {
            if (referencePartNamed != null)
            {
                //referenceName += referencePartNamed.portName;
                return referencePartNamed.portName;
            }
            else if (referencePart != null)
            {
                //referenceName += referencePart.name;
                return referencePart.name;
            }
            else
            {
                //referenceName += "None";
                return "None";
            }
        }

        private static void drawReferencePortNameRPM(Rect screenRect){
            //String referenceName = "Ref: ";
            String referenceName = getReferencePortName();

            BitmapFont.StringDimensions stringDimensions = bitmapFont.getStringDimensions(referenceName, 1f);
            float virtualWidth = screenRect.width * .5f - refX - rtLabelSpacing;
            float widthScale = virtualWidth / stringDimensions.width;
            float virtualHeight = rpmTgtRefTextHeight;
            float heightScale = virtualHeight / (stringDimensions.height);
            textTargetNameScale = Math.Min(widthScale, heightScale);
            //float x = screenRect.xMax - stringDimensions.width * textTargetNameScale;
            float x = screenRect.center.x + refX + rtLabelSpacing + virtualWidth*.5f - (stringDimensions.width * textTargetNameScale / 2f);
            float y = screenRect.yMax - _rpmTextYTop - (stringDimensions.yOffset + .5f * stringDimensions.height) * textTargetNameScale;

            drawGlyphStringRPM(referenceName, x, y, textTargetNameScale, currentHighlightBox == HighlightBox.RIGHT ? Color.yellow : Color.white);
        }

        private static String determineTargetPortName()
        {
            String targetDisplayName;

            if (currentTargetVessel == null)
            {
                targetDisplayName = "No Vessel Targeted";
            }
            else if (targetedDockingModule == null)
            {
                if (targetOutOfRange)
                {
                    targetDisplayName = "Target Out of Range";
                }
                else
                {
                    targetDisplayName = "No Port Targeted";
                }
            }
            else if (targetNamedModule == null)
            {
                targetDisplayName = (targetedDockingModule as PartModule).part.partInfo.title;
            }
            else
            {
                targetDisplayName = targetNamedModule.portName;
            }
            return targetDisplayName;
        }

        private static void drawGlyphString(String valueString, float x, float y)
        {
            bitmapFont.drawString(valueString, x, y, gaugeScale * textNumberScale);
        }

        private static void drawGlyphString(String valueString, float x, float y, float customScale)
        {
            bitmapFont.drawString(valueString, x, y, customScale);
        }

        private static void drawGlyphStringRPM(String valueString, float x, float y, float customScale)
        {
            bitmapFont.drawStringGraphics(valueString, x, y, customScale, Color.white);
        }

        private static void drawGlyphStringRPM(String valueString, float x, float y, float customScale, Color color)
        {
            bitmapFont.drawStringGraphics(valueString, x, y, customScale, color);
        }

        private static void drawVelocityVector(Rect gaugeRect)
        {
            float gaugeX, gaugeY;
            
            gaugeX = UnityEngine.Mathf.Clamp(transverseVelocity.x, -transverseVelocityRange, transverseVelocityRange) / transverseVelocityRange;
            gaugeY = UnityEngine.Mathf.Clamp(transverseVelocity.y, -transverseVelocityRange, transverseVelocityRange) / transverseVelocityRange;

            Texture2D velocityVectorTexture = null;
            if (closureV > 0)
            {
                velocityVectorTexture = prograde;
            }
            else
            {
                velocityVectorTexture = retrograde;
            }

            if (Math.Abs(orientationDeviation.x) > 90f){
                gaugeX *= -1;
                gaugeY *= -1;
            }
            
            gaugeX = scaleExponentially(gaugeX, velocityVectorExponent);
            gaugeY = scaleExponentially(gaugeY, velocityVectorExponent);

            float scaledVelocityVectorSize = velocityVectorIconSize * gaugeScale;
            float scaledVelocityVectorHalfSize = scaledVelocityVectorSize * .5f;

            GUI.DrawTexture(new Rect(gaugeRect.xMin + .5f * gaugeRect.width * (1 + gaugeX) - scaledVelocityVectorHalfSize,
                                        gaugeRect.yMin + .5f * gaugeRect.height * (1 + gaugeY) - scaledVelocityVectorHalfSize,
                                        scaledVelocityVectorSize,
                                        scaledVelocityVectorSize),
                                        velocityVectorTexture);
        }

        private static void drawVelocityVectorGraphics(Rect gaugeRect, float baseScale)
        {
            float gaugeX, gaugeY;

            gaugeX = UnityEngine.Mathf.Clamp(transverseVelocity.x, -transverseVelocityRange, transverseVelocityRange) / transverseVelocityRange;
            gaugeY = UnityEngine.Mathf.Clamp(transverseVelocity.y, -transverseVelocityRange, transverseVelocityRange) / transverseVelocityRange;

            Texture2D velocityVectorTexture = null;
            if (closureV > 0)
            {
                velocityVectorTexture = prograde;
            }
            else
            {
                velocityVectorTexture = retrograde;
            }

            if (Math.Abs(orientationDeviation.x) > 90f)
            {
                gaugeX *= -1;
                gaugeY *= -1;
            }

            gaugeX = scaleExponentially(gaugeX, velocityVectorExponent);
            gaugeY = scaleExponentially(gaugeY, velocityVectorExponent);

            float scaledVelocityVectorSize = velocityVectorIconSize * baseScale;
            float scaledVelocityVectorHalfSize = scaledVelocityVectorSize * .5f;

            Graphics.DrawTexture(new Rect(gaugeRect.xMin + .5f * gaugeRect.width * (1 + gaugeX) - scaledVelocityVectorHalfSize,
                                        gaugeRect.yMin + .5f * gaugeRect.height * (1 + gaugeY) - scaledVelocityVectorHalfSize,
                                        scaledVelocityVectorSize,
                                        scaledVelocityVectorSize),
                                        velocityVectorTexture);
        }

        private static float scaleExponentially(float value, float exponent)
        {
            return (float)Math.Pow(Math.Abs(value), exponent) * Math.Sign(value);
        }

        private static Color determineCDIcolor()
        {
            if (negativeOnBackHemisphere < 0) return colorCDIReverse;
            return colorCDINormal;
        }
        
        private static void calculateCDIvalues0to1(ref float xVal, ref float yVal)
        {
            float gaugeX = xTranslationNegativeOnBackHemi * wrapRange(translationDeviation.x / 90f);
            float gaugeY = yTranslationNegativeOnBackHemi * wrapRange(translationDeviation.y / 90f);

            float exponent = CDIExponent;

            if (distanceToTarget <= CDIexponentDecreaseDoneRange) exponent = 1f;
            else if (distanceToTarget < CDIexponentDecreaseBeginRange)
            {
                float toGo = distanceToTarget - CDIexponentDecreaseDoneRange;
                float range = CDIexponentDecreaseBeginRange - CDIexponentDecreaseDoneRange;
                float lerp = toGo / range;

                float exponentReduction = 1f - CDIExponent;

                //this gradually eliminates the exponential scaling, avoiding over-sensitive CDI lines
                exponent = 1 - (exponentReduction) * lerp;
            }

            xVal = (scaleExponentially(gaugeX, exponent) + 1)/2f;
            yVal = (scaleExponentially(gaugeY, exponent) + 1)/2f;
        }
      
        private static float wrapRange(float a)
        {
            return ((((a + 1f) % 2) + 2) % 2) - 1f;
        }

        static Vector2 centerVec2 = new Vector2();
        static Color iconColor = new Color(1f, 1f, 1f, 1f);

        private static void drawTargetPortHUDIndicator()
        {
            //print("drawTargetPortIndicator: Start");

            if (targetedDockingModule != null)
            {
                Camera cam = FlightCamera.fetch.mainCamera;
                //Vector3 portToCamera = targetedDockingModule.transform.position - cam.transform.position;
                Vector3 portToCamera = targetedDockingModule.GetTransform().position - cam.transform.position;

                if (Vector3.Dot(cam.transform.forward, portToCamera) < 0)
                {
                    //Port is behind the camera
                    return;
                }

                Vector3 screenSpacePortLocation = cam.WorldToScreenPoint(targetedDockingModule.GetTransform().position);
                centerVec2.x = screenSpacePortLocation.x;
                centerVec2.y = cam.pixelHeight - screenSpacePortLocation.y;
                selectedPortHUDRect.center = centerVec2;

                selectedPortHUDRect.width = targetHUDiconSize;
                selectedPortHUDRect.height = targetHUDiconSize;

                float pulsePercent = (UnityEngine.Time.fixedTime % pulsePeriod) / pulsePeriod;

                if (pulsePercent < pulseDurationRatio)
                {
                    float pulseAbsence = (pulsePercent / pulseDurationRatio); //0 to 1
                    float pulsePresence = 1 - pulseAbsence; //1 to 0

                    iconColor.r = UnityEngine.Mathf.Clamp01(pulsePresence + (pulseAbsence) * colorTargetPortHUDicon.r);
                    iconColor.g = UnityEngine.Mathf.Clamp01(pulsePresence + (pulseAbsence) * colorTargetPortHUDicon.g);
                    iconColor.b = UnityEngine.Mathf.Clamp01(pulsePresence + (pulseAbsence) * colorTargetPortHUDicon.b);
                }
                else
                {
                    iconColor.r = colorTargetPortHUDicon.r;
                    iconColor.g = colorTargetPortHUDicon.g;
                    iconColor.b = colorTargetPortHUDicon.b;
                }

                Color originalColor = GUI.color;
                GUI.color = iconColor;
                GUI.DrawTexture(selectedPortHUDRect, targetPort, ScaleMode.ScaleToFit, true);
                GUI.color = originalColor;
            }
            //print("drawTargetPortIndicator: End");
        }

        public static void cycleReferencePoint(int direction)
        {
            if (referencePartIndex == -1)
            {
                findReferencePoints();
            }

            if (referencePartIndex == -1) return;

            int newIndex = referencePartIndex + direction;
            if (newIndex < 0)
            {
                newIndex = referencePoints.Count - 1;
            }
            else if (newIndex >= referencePoints.Count)
            {
                newIndex = 0;
            }

            if (newIndex != referencePartIndex)
            {
                PartModule module = referencePoints[newIndex];

                var node = module as ModuleDockingNode;
                var pod = module as ModuleCommand;
                var claw = module as ModuleGrappleNode;

                //Thanks Mihara!
                if (node != null)
                {
                    node.MakeReferenceTransform();
                } else if (pod != null)
                {
                    pod.MakeReference();
                } else if (claw != null)
                {
                    claw.MakeReferenceTransform();
                }


                determineReferencePoint();
                determineReferencePointIndex();
            }
        }

        private static void findReferencePoints()
        {
            determineReferencePoint();

            referencePoints.Clear();

            foreach (Part thatPart in FlightGlobals.ActiveVessel.Parts)
            {
                foreach (PartModule thatModule in thatPart.Modules)
                {
                    var thatNode = thatModule as ModuleDockingNode;
                    var thatCommand = thatModule as ModuleCommand;
                    var thatClaw = thatModule as ModuleGrappleNode;
                    if (thatNode != null || thatCommand != null || thatClaw != null)
                    {
                        referencePoints.Add(thatModule);
                    }
                }
            }

            determineReferencePointIndex();
        }

        private static void determineReferencePoint()
        {
            Part refPart = FlightGlobals.ActiveVessel.GetReferenceTransformPart();
            if (refPart != null && refPart != referencePart)
            {
                referencePart = refPart;
                referencePartNamed = null;
                List<ModuleDockingNodeNamed> namedPorts = referencePart.FindModulesImplementing<ModuleDockingNodeNamed>();

                if (namedPorts.Count > 0)
                {
                    referencePartNamed = namedPorts[0];
                }
            }
        }

        private static void determineReferencePointIndex()
        {
            referencePartIndex = -1;
            int i=0;
            foreach (PartModule m in referencePoints)
            {
                if (m.part.Equals(referencePart))
                {
                    referencePartIndex = i;
                    break;
                }
                i++;
            }
        }

        public static void setRenameHighlightBoxRPM(HighlightBox box)
        {
            currentHighlightBox = box;
        }

        private static HighlightBox currentHighlightBox = HighlightBox.NONE;

        public enum HighlightBox
        {
            LEFT,
            RIGHT,
            NONE
        }

        #region Preferences
        private static void saveWindowPosition()
        {
            config.SetValue("window_position", windowPosition);
            config.save();
        }

        private static void saveConfigSettings()
        {
            //config.SetValue("show_cdi", useCDI);
            //config.SetValue("show_rolldigits", drawRollDigits);
            config.SetValue("drawHudIcon", drawHudIcon);
            config.SetValue("showHUDIconWhileEva", showHUDIconWhileIva);
            config.SetValue("HudIconSize", targetHUDiconSize);
            config.SetValue("allowAutoPortTargeting", allowAutoPortTargeting);
            config.SetValue("excludeDockedPorts", excludeDockedPorts);
            config.SetValue("gui_scale", (double)gaugeScale);
            config.save();
        }

        public static void LoadPrefs()
        {
            //print("Load Prefs");
            config = PluginConfiguration.CreateForType<DockingPortAlignment>(null);
            config.load();

            gaugeScale = (float)config.GetValue<double>("gui_scale", 0.86);

            Rect defaultWindow = new Rect(Screen.width * .75f - (backgroundTextureWidth * gaugeScale / 2f), Screen.height * .5f - (backgroundTextureHeight * gaugeScale / 2f), backgroundTextureWidth * gaugeScale, backgroundTextureHeight * gaugeScale);
            windowPosition = config.GetValue<Rect>("window_position", defaultWindow);

            windowPosition = constrainToScreen(windowPosition);

            //useCDI = config.GetValue<bool>("show_cdi", true);
            //drawRollDigits = config.GetValue("show_rolldigits", true);
            drawHudIcon = config.GetValue<bool>("drawHudIcon", true);
            targetHUDiconSize = config.GetValue("HudIconSize", 22f);
            allowAutoPortTargeting = config.GetValue<bool>("allowAutoPortTargeting", true);
            excludeDockedPorts = config.GetValue<bool>("excludeDockedPorts", true);
            showHUDIconWhileIva = config.GetValue<bool>("showHUDIconWhileEva", false);

            saveWindowPosition();
            saveConfigSettings();
            //print("End Load Prefs");
        }

        private void LoadConfigFile()
        {

            ConfigNode cfgFile = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/NavyFish/Plugins/Docking Port Alignment Indicator/DPAI Settings.cfg");

            if (cfgFile == null)
            {
                print("DPAI: Failed to load 'DPAI Settings.cfg'");
            }
            else
            {
                ConfigNode rootNode = cfgFile.GetNode("DockingPortAlignment");
                if (rootNode != null)
                {
                    forceStockAppLauncher = rootNode.GetValue("forceStockAppLauncher") == "true";
                    //print("DPAI: forceStockAppLauncher = " + forceStockAppLauncher.ToString());
                }
                else
                {
                    print("DPAI: Config Root Node is null");
                }
            }
        }
        #endregion

        #region RPM

        #endregion

        #region Resources

        //public static Material mat_background;

        //public static Material generateUnlitMaterial(Texture2D tex2D)
        //{
        //    Shader unlit = Shader.Find("KSP/Alpha/Unlit Transparent");
        //    Material mat = new Material(unlit);
        //    mat.color = new Color(1, 1, 1, 1);
        //    mat.mainTexture = tex2D;
        //    return mat;
        //}



        private static void loadTextures()
        {
            Byte[] arrBytes;
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("gaugeBackground.png", null);
            gaugeBackgroundTex.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("RPM_background.png", null);
            rpmBackgroundTex.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("gaugeForeground.png", null);
            gaugeForegroundTex.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("alignment.png", null);
            alignmentTex.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("directionArrow.png", null);
            directionArrowTex.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("prograde.png", null);
            prograde.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("retrograde.png", null);
            retrograde.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("roll.png", null);
            roll.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("MS33558.png", null);
            fontTexture.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("targetPort.png", null);
            targetPort.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("appLauncherIcon.png", null);
            appLauncherIcon.LoadImage(arrBytes);
            TextReader tr = KSP.IO.TextReader.CreateForType<DockingPortAlignment>("MS33558.fnt", null);
            List<string> textStrings = new List<string>();
            while (!tr.EndOfStream)
            {
                textStrings.Add(tr.ReadLine());
            }
            tr.Close();
            tr.Dispose();

            bitmapFont = new BitmapFont(fontTexture, textStrings.ToArray());
        }

        private void initStyles()
        {
            Color lightGrey = new Color(.8f, .8f, .85f);

            windowStyle = new GUIStyle(HighLogic.Skin.window);
            windowStyle.stretchWidth = true;
            windowStyle.stretchHeight = true;

            labelStyle = new GUIStyle(HighLogic.Skin.label);
            labelStyle.stretchWidth = true;
            labelStyle.stretchHeight = true;
            labelStyle.normal.textColor = lightGrey;

            settingsButtonStyle = new GUIStyle(HighLogic.Skin.button);
            settingsButtonStyle.padding = new RectOffset(1, 1, 1, 1);
            settingsButtonStyle.stretchHeight = true;
            settingsButtonStyle.stretchWidth = false;
            settingsButtonStyle.fontSize = 11;
            settingsButtonStyle.normal.textColor = lightGrey;

            hasInitializedStyles = true;
        }

        private static void OnDestroy()
        {
            if (toolbarButton != null) toolbarButton.Destroy();
        }
        #endregion

        #region Debugging

        private static bool shouldDebug = false;
        
        private void OnDrawDebug()
        {
            debugWindowPosition = GUILayout.Window(1338, debugWindowPosition, drawDebugWindowContents, "Debug", GUILayout.MinWidth(400), GUILayout.MaxWidth(800));
        }

        static int rollOffset = 7;
        static int _rDegXPos = 320;
        static int _rDegYPos = 5;
        static int _degSign_x = 295;
        static int _degSign_y = 9;
        static int vertLineFooterChop = 50;

        private static int _rpmTextYTop = 19;
        private static int rpmTgtRefTextHeight = 19;

        private static int tgtX = 6, refX = 6, rtLabelY = 48, rtLabelSpacing = 6;
        private static float rtLabelScale = .6f;

        private static float arrowLengthOffsetMult = 1.6f, arrowLengthMult = .25f;

        private static int vertLineHeaderChop = 15;
        
        private void drawDebugWindowContents(int windowID)
        {
            floatTextField(ref visiblePortion, "visiblePortion");
            label<float>(wrapRange(translationDeviation.x / 90f), "transXwrap");
            label<float>(wrapRange(translationDeviation.y / 90f), "transYwrap");
            //intTextField(ref vertLineHeaderChop, "vertLineHeaderChop");
            //intTextField(ref _rpmTextYTop, "_rpmTextYTop");
            //intTextField(ref rpmTgtRefTextHeight, "rpmTgtRefTextHeight");

            //intTextField(ref tgtX, "tgtX");
            //intTextField(ref refX, "refX");
            //intTextField(ref rtLabelY, "rtLabelY");
            //intTextField(ref rtLabelSpacing, "rtLabelSpacing");
            //floatTextField(ref rtLabelScale, "rtLabelScale");

            ////sliderField(ref screenPercentRPM, "screenPercent", .1f, 1f);

            //sliderField(ref arrowLengthMult, "arrowLengthMult", .01f, 5f);
            //sliderField(ref arrowLengthOffsetMult, "arrowLengthOffsetMult", .01f, 5f);

            //label<Part>(referencePart, "Reference Part: ");

            //if (referencePartNamed != null)
            //{
            //    label<String>(referencePartNamed.portName, "Reference Name: ");
            //}
            
            //if (referencePoints.Count > 0){
            //    int i=0;
            //    foreach(PartModule pm in referencePoints){
            //        if (pm is ModuleDockingNodeNamed)
            //        {
            //            label<String>(((ModuleDockingNodeNamed)pm).portName, "\trefPoints[" + i + "]: ");
            //        }
            //        else
            //        {
            //            label<String>(pm.part.name, "\trefPoints[" + i + "]: ");
            //        }
            //        i++;
            //    }
            //}

            //label<int>(referencePartIndex, "RefPartIndex: ");

            label<bool>(isIVA(), "Is Iva: ");

            GUI.DragWindow();
        }

        private static void intTextField(ref int value, string label)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle);
            value = int.Parse(GUILayout.TextField(value.ToString()));
            GUILayout.EndHorizontal();
        }

        private static void floatTextField(ref float value, string label)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle);
            value = float.Parse(GUILayout.TextField(value.ToString()));
            GUILayout.EndHorizontal();
        }


        private static void sliderField(ref float value, string label)
        {
            sliderField(ref value, label, 0f, 1f);
        }

        private static void sliderField(ref float value, string label, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            value = GUILayout.HorizontalSlider(value, min, max);
            GUILayout.Label(value.ToString(), GUILayout.Width(60));
            GUILayout.EndHorizontal();
        }

        private static void label<T>(T value, string label)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            GUILayout.Label(value.ToString());
            GUILayout.EndHorizontal();
        }

        #endregion
    }
}