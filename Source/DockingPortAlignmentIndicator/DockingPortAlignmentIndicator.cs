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
using KSP.UI.Screens;

using static NavyFish.LogWrapper;
using System.Linq;

namespace NavyFish
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DockingPortAlignmentIndicator : MonoBehaviour
    {
        private static PluginConfiguration config;
        private static bool hasInitializedStyles = false;
        private static GUIStyle windowStyle, labelStyle, settingsButtonStyle;
        private static Rect windowPosition = new Rect();
        private static Rect lastPosition = new Rect();
        private static Rect debugWindowPosition = new Rect(50,200,350,200);
        
        static Rect selectedPortHUDRect = new Rect(0, 0, targetHUDiconSize, targetHUDiconSize);
        
        public static float gaugeScale = .86f;
        private static int backgroundTextureWidth = 317;
        private static int backgroundTextureHeight = 317;
        private static Rect backgroundRect = new Rect(0, 0f, backgroundTextureWidth * gaugeScale, backgroundTextureHeight * gaugeScale);

        private static int foregroundTextureWidth = 400;
        private static int foregroundTextureHeight = 457;
        private static Rect foregroundRect = new Rect(0, 0f, foregroundTextureWidth * gaugeScale, foregroundTextureHeight * gaugeScale);

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

        private static float velocityVectorIconSize = 31;
        private static float transverseVelocityRange = 3.5f;
        private static float velocityVectorExponent = .75f;

        private static float alignmentGaugeRange = 60f;
        private static float alignmentExponent = .8f;

        private static float CDIExponent = .75f;
        private static float CDIexponentDecreaseBeginRange = 15f;
        private static float CDIexponentDecreaseDoneRange = 5f;

        private static float markerSize = 111;
        private static float targetHUDiconSize = 22;
        private static float pulsePeriod = 1.42f;
        private static float pulseDurationRatio = .4f;

        private static Color colorCDINormal = new Color(.064f, .642f, 0f);
        private static Color colorCDIReverse = new Color(.747f, 0f, .05f);
        private static Color colorsettingsButtonActivated = new Color(.11f, .66f, .11f, 1f);
        private static Color colorsettingsButtonDeactivated = new Color(.22f, .26f, .29f, 1f);
        private static Color colorTargetPortHUDicon = new Color(.989f, .329f, .953f);
        private static Color colorGaugeLabels = new Color(.41f, .41f, .41f, 1f);

        public static Texture2D gaugeForegroundTex = null;
        public static Texture2D gaugeBackgroundTex = null;
        public static Texture2D rpmBackgroundTex = null;
        public static Texture2D alignmentTex = null;
        public static Texture2D directionArrowTex = null;
        public static Texture2D prograde = null;
        public static Texture2D retrograde = null;
        public static Texture2D roll = null;
        public static Texture2D targetPort = null;
        public static Texture2D fontTexture = null;
        public static Texture2D appLauncherIcon = null;
        public static Texture2D customToolbarIcon;

        public static BitmapFont bitmapFont;
        private static float textNumberScale = .7f;
        private static float textLabelScale = .52f;
        private static float textTargetRefNameScale = .77f;

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
        private static bool restrictDockingPorts = true; // Restrict target ports based on size etc
        private static bool drawHudIcon = true;
        private static bool resetTarget = false;
        private static bool blizzyToolbarAvailable = false;
        private static bool forceStockAppLauncher = true;

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
        static bool wasLastMap = false;

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

        /// <summary>
        /// Adds the toolbar button from the Stock toolbar (AppLauncher)
        /// </summary>
        /// Called directly and also as a GameEvent callback.
        private void addToolBarButtonToStockAppLauncher ()
        {
            Log($"addToolBarButtonToStockAppLauncher (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={appLauncherButton})");
            if (HighLogic.LoadedSceneIsFlight && appLauncherButton == null) {
                //print("DPAI: adding stock appLauncher button");
                //RUIToggleButton.OnTrue onTrueDelegate = new RUIToggleButton.OnTrue(onShowGUI);
                //RUIToggleButton.OnFalse onFalseDelegate = new RUIToggleButton.OnFalse(onHideGUI);
                Callback onTrueCallback = new Callback(onShowGUI);
                Callback onFalseCallback = new Callback(onHideGUI);
                appLauncherButton = ApplicationLauncher.Instance.AddModApplication(
                    onTrueCallback,
                    onFalseCallback,
                    null, null, null, null,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    appLauncherIcon);
            }
        }

        /// <summary>
        /// Removes the toolbar button from the Stock toolbar (AppLauncher)
        /// </summary>
        /// Called directly and also as a GameEvent callback.
        private void removeToolBarButtonFromAppLauncher()
        {
            Log("removeToolBarButtonFromAppLauncher (appLauncherButton=={appLauncherButton})");
            if (appLauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
                appLauncherButton = null;
            }
        }

        /// <summary>
        /// GameEvent callback whenever the AppLauncher becomes ready.
        /// </summary>
        private void OnAppLauncherReady()
        {
            Log($"OnAppLauncherReady (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={appLauncherButton})");
            if (HighLogic.LoadedSceneIsFlight)
            {
                addToolBarButtonToStockAppLauncher();
            }
            else
            {
                removeToolBarButtonFromAppLauncher();
            }
        }

        private void onShowGUI()
        {
            //print("DPAI_DEBUG onShowGUI()");
            gaugeVisiblityToggledOn = true;
        }

        private void onHideGUI()
        {
            //print("DPAI_DEBUG onHideGUI()");
            gaugeVisiblityToggledOn = false;
        }

        /// <summary>
        /// Called once per object. Effectively the Constructor.
        /// </summary>
        public void Awake()
        {
            Log($"Awake (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={appLauncherButton})");
            loadTextures();
        }
        
        /// <summary>
        /// Called once only per object just before the first frame.
        /// </summary>
        public void Start()
        {
            Log($"Start (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={appLauncherButton})");
            LoadPrefs();

            updateToolBarButton();

            if ( !hasInitializedStyles ) initStyles();

            settingsWindowPosition = new Rect(0, windowPosition.yMax, 0, 0);
        }

        /// <summary>
        /// Called whenever the object is destroyed. Effectively the Destructor.
        /// </summary>
        private void OnDestroy()
        {
            Log($"OnDestroy (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={appLauncherButton})");

            if (forceStockAppLauncher || !blizzyToolbarAvailable)
            {
                destroyAppLauncherButton();
            }
            else
            {
                destroyBlizzyButton();
            }
        }

        /// <summary>
        /// Creates the ToolbarButton on the Stock Toolbar (AppLauncher)
        /// </summary>
        private void createAppLauncherButton()
        {
            // Various "GameEvents" exist for the ApplicationLauncher, but it seems only one is
            // actually called in KSP1.8.x - onGUIApplicationLauncherReady
            GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);
            if (ApplicationLauncher.Ready && HighLogic.LoadedSceneIsFlight)
            {
                addToolBarButtonToStockAppLauncher();
            }
        }

        /// <summary>
        /// Destroys the ToolbarButton on the Stock Toolbar (AppLauncher)
        /// </summary>
        private void destroyAppLauncherButton()
        {
            removeToolBarButtonFromAppLauncher();
            GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
        }

        /// <summary>
        /// Creates the ToolbarButton on the Blizzy Toolbar
        /// </summary>
        private void createBlizzyButton()
        {
            if (toolbarButton == null)
            {
                toolbarButton = ToolbarManager.Instance.add("DockingAlignment", "dockalign");
                toolbarButton.TexturePath = "NavyFish/Plugins/ToolbarIcons/DPAI";
                toolbarButton.ToolTip = "Show/Hide Docking Port Alignment Indicator";
                toolbarButton.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                toolbarButton.Visible = true;
                toolbarButton.Enabled = true;
                toolbarButton.OnClick += (e) => {
                    gaugeVisiblityToggledOn = !gaugeVisiblityToggledOn;
                };
            }
        }

        /// <summary>
        /// Destroys the ToolbarButton on the Blizzy Toolbar
        /// </summary>
        private void destroyBlizzyButton()
        {
            if (toolbarButton != null)
            {
                toolbarButton.Destroy();
                toolbarButton = null;
            }
        }

        /// <summary>
        /// Dynamically switches the ToolbarButton between the Stock and Blizzy tool bars.
        /// </summary>
        private void updateToolBarButton()
        {
            Log($"updateToolBarButton (GameScene=={HighLogic.LoadedScene}, appLauncherButton=={appLauncherButton})");
            blizzyToolbarAvailable = ToolbarManager.ToolbarAvailable;

            //Debug.Log("DPAI START");

            if (forceStockAppLauncher || !blizzyToolbarAvailable)
            {
                // Destroy blizzy button
                destroyBlizzyButton();
                createAppLauncherButton();
            }
            else
            {
                // Destroy stock launcher button
                destroyAppLauncherButton();
                createBlizzyButton();
            }
        }

        private void OnGUI()
        {
            onGaugeDraw();
            if (shouldDebug) OnDrawDebug();
        }

        public void Update()
        {
            //print("DPAI_DEBUG Update()");

            if ( !HighLogic.LoadedSceneIsFlight ) {
                //print("DPAI_DEBUG update: returning, lodaed scene is not flight");
                return;
            } else {
                //print("DPAI_DEBUG loadedsceneisflight: " + HighLogic.LoadedSceneIsFlight);
                //print("DPAI_DEBUG !FlightGlobals.ActiveVessel.isEVA: " + !FlightGlobals.ActiveVessel.isEVA);
                //print("DPAI_DEBUG !MapView.MapIsEnabled: " + !MapView.MapIsEnabled);
            }
            

            //if (Input.GetKeyDown(KeyCode.B)){
            //        cycledModuleIndex = dockingModulesListIndex + 1;
            //        cycledModuleIndex %= dockingModulesList.Count;
            //        portWasCycled = true;
            //}

            //determineTargetPort();

            bool sceneElligibleForIndicator = (HighLogic.LoadedSceneIsFlight && !FlightGlobals.ActiveVessel.isEVA && !MapView.MapIsEnabled);

            //print("DPAI_DEBUG sceneElligble:" + sceneElligibleForIndicator);
            //print("DPAI_DEBUG guageVisibility" + gaugeVisiblityToggledOn);

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
                determineTargetPort();
                if (targetedDockingModule != null) calculateGaugeData();
                drawIndicatorContentsToTexture();
            }
        }

        private static bool isIVA()
        {
            if (InternalCamera.Instance != null)
            {
                return InternalCamera.Instance.isActive;
            }
            else return false;
        }

        /// <summary>
        /// Returns true if the targetPort is compatible with the current vessel.
        /// </summary>
        /// If the controlling part of the current vessel is a docking port, then it
        /// is compared against the target port. Otherwise, all docking ports on the
        /// current vessel are checked.
        /// <param name="targetPort"></param>
        /// <returns></returns>
        private static bool isCompatiblePort(ModuleDockingNode targetPort)
        {
            bool compatible = false;

            // Get the controlling docking port, or all of them
            var dockingPorts  = referencePart.FindModulesImplementing<ModuleDockingNode>();
            if (dockingPorts.Count == 0) {
                dockingPorts = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleDockingNode>();
            }

            // See if one of the source ports is compatible with the target port.
            using (IEnumerator<ModuleDockingNode> dnEnumerator = dockingPorts.GetEnumerator())
            {
                while (!compatible && dnEnumerator.MoveNext())
                {
                    var sourcePort = dnEnumerator.Current;

                    // Can't dock using a disabled port
                    if (sourcePort.IsDisabled) {
                        continue;
                    }
                    if (!sourcePort.state.StartsWith("Ready")) {
                        continue;
                    }
                    // If one port is gendered, they both have to be
                    // TODO: verify with mods; stock ports are ungendered
                    if (sourcePort.gendered != targetPort.gendered) {
                        continue;
                    }
                    // If the ports are gendered, they have to be opposite gender
                    // TODO: Possibly if one port is gendered, but the other isn't, ignore?
                    if (sourcePort.gendered && (sourcePort.genderFemale == targetPort.genderFemale)) {
                        continue;
                    }
                    // Verify the ports are the same size
                    // NB: Since v1.0.5 of KSP, docking ports can be "multiport" in which case the nodeType is a comma-delimited string
                    //if (sourcePort.nodeType != targetPort.nodeType) {
                    char [] separator = { ',' };
                    var spNodes = sourcePort.nodeType.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    var dpNodes = targetPort.nodeType.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    if (spNodes.Intersect(dpNodes).Count() == 0) {
                        continue;
                    }

                    // Gender-compatible, same size, so, yayy?
                    compatible = true;
                }
            }

            return compatible;
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
                wasLastMap = MapView.MapIsEnabled;
            }

            determineReferencePoint();
            tickCount++;

            bool isInMap = MapView.MapIsEnabled;
            bool justLeftMap = false;
            if (!isInMap && wasLastMap)
            {
                justLeftMap = true;
            }             
            wasLastMap = isInMap;

            bool isInIVA = isIVA();
            bool justEnteredIVA = false;
            if (isInIVA && !wasLastIVA)
            {
                justEnteredIVA = true;
            }
            wasLastIVA = isInIVA;

            if (lastReferencePart != referencePart)
            {
                //print("DPAI: Reference Part Changed - tick " + tickCount);
                bool isCurrentlyIVA = isIVA();
                if (isCurrentlyIVA){
                    //print("DPAI: Is currently IVA - tick " + tickCount);

                    if(justEnteredIVA || justLeftMap){
                        //print("DPAI: Was not previously IVA - tick " + tickCount);
                        
                        if (FlightGlobals.ActiveVessel.Parts.Contains(lastReferencePart))
                        {
                            FlightGlobals.ActiveVessel.SetReferenceTransform(lastReferencePart);
                            //print("DPAI: Re-setting Reference Part - tick " + tickCount);
                            findReferencePoints();
                        }
                    }
                }
                lastReferencePart = referencePart;
                // Force recalculation of possible target ports if we're restricting them
                if (restrictDockingPorts) {
                    currentTargetVesselWasLastSeenLoaded = false;
                }
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
                                        Log($"Adding Docking Port {port} (state={port.state}, other={port.otherNode}) to list of targets.");
                                        // MKW: if node was attached in the VAB, state is "PreAttached"
                                        if (excludeDockedPorts &&
                                                (port.state.StartsWith("Docked", StringComparison.OrdinalIgnoreCase) || 
                                                port.state.StartsWith("PreAttached", StringComparison.OrdinalIgnoreCase))
                                            )
                                        {
                                            //print("continue");
                                            //do not add to list if module is already docked
                                            continue;
                                        }

                                        if(restrictDockingPorts && !isCompatiblePort(port))
                                        {
                                          // Do not add to list if destination port doesn't match
                                          continue;
                                        }

                                        //print("1stAdd");
                                        dockingModulesList.Add(tgt);
                                    }
                                    else
                                    {
                                        //print("2ndAdd");
                                        dockingModulesList.Add(tgt);
                                    }
                                }

                                if (dockingModulesList.Count > 0)
                                {
                                    // If we already have a valid docking port as our current target, don't change it
                                    int idx = dockingModulesList.IndexOf(targetedDockingModule);
                                    dockingModulesListIndex = -1;
                                    if (idx != -1)
                                    {
                                        dockingModulesListIndex = idx;
                                        lastTarget = targetedDockingModule;
                                    }
                                    //if (currentTarget is ModuleDockingNode && !currentTargetVessel.packed)
                                    else if ((currentTarget is ModuleDockingNode) && isOrientedTarget(currentTarget) && !currentTargetVessel.packed)
                                    {
                                        // Use the currently selected target (if it is a docking port)
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

                                    if(dockingModulesListIndex == -1)
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
            
            if (showIndicator)
            {
                windowPosition.width = foregroundTextureWidth * gaugeScale;
                windowPosition.height = foregroundTextureHeight * gaugeScale;
      
                windowPosition = constrainToScreen(GUI.Window(1773, windowPosition, drawRenderedGaugeTexture, "DPAI", labelStyle));

                leftButtonRect.yMin = (402 * gaugeScale);
                leftButtonRect.yMax = (446 * gaugeScale);
                leftButtonRect.xMin = (21 * gaugeScale);
                leftButtonRect.xMax = (66 * gaugeScale);

                rightButtonRect.yMin = leftButtonRect.yMin;
                rightButtonRect.yMax = leftButtonRect.yMax;
                rightButtonRect.xMin = (334 * gaugeScale);
                rightButtonRect.xMax = (380 * gaugeScale);

                if (showSettings)
                {
                    settingsWindowPosition.x = windowPosition.x;
                    settingsWindowPosition.y = windowPosition.yMax;
                    if (!settingsWindowOverflow) settingsWindowPosition.width = windowPosition.width;
                 
                    settingsWindowPosition = GUILayout.Window(1339, settingsWindowPosition, drawSettingsWindowContents, "DPAI Settings", windowStyle);
                    if (settingsWindowPosition.width > windowPosition.width)
                    {
                        settingsWindowOverflow = true;
                    }
                    else
                    {
                        settingsWindowOverflow = false;
                    }
                    
                }
            }
        }

        public static void drawRPMText(int screenWidth, int screenHeight)
        {
            fullScreenRect.Set(0, 0, screenWidth, screenHeight);
            drawGlyphStringGraphics("TGT:", tgtX, fullScreenRect.yMax - rtLabelY, rtLabelScale, BitmapFont.HorizontalAlignment.LEFT, currentHighlightBox == HighlightBox.LEFT ? Color.yellow : Color.white);
            drawGlyphStringGraphics("REF:", fullScreenRect.center.x + refX, fullScreenRect.yMax - rtLabelY, rtLabelScale, BitmapFont.HorizontalAlignment.LEFT, currentHighlightBox == HighlightBox.RIGHT ? Color.yellow : Color.white);

            String targetDisplayName = determineTargetPortName();

            BitmapFont.StringDimensions stringDimensions = bitmapFont.getStringDimensions(targetDisplayName, 1f);
            float virtualWidth = fullScreenRect.width * .5f - tgtX - rtLabelSpacing;
            float widthScale = virtualWidth / stringDimensions.width;
            float virtualHeight = rpmTgtRefTextHeight;
            float heightScale = virtualHeight / (stringDimensions.height);
            textTargetRefNameScale = Math.Min(widthScale, heightScale);
            
            float x = tgtX + rtLabelSpacing + virtualWidth * .5f - (stringDimensions.width * textTargetRefNameScale / 2f);
            float y = fullScreenRect.yMax - _rpmTextYTop - (stringDimensions.yOffset + .5f * stringDimensions.height) * textTargetRefNameScale;

            drawGlyphStringGraphics(targetDisplayName, x, y, textTargetRefNameScale, BitmapFont.HorizontalAlignment.LEFT, currentHighlightBox == HighlightBox.LEFT ? Color.yellow : Color.white);

            String referenceName = getReferencePortName();

            stringDimensions = bitmapFont.getStringDimensions(referenceName, 1f);
            //virtualWidth = fullScreenRect.width * .5f - refX - rtLabelSpacing;
            widthScale = virtualWidth / stringDimensions.width;
            //virtualHeight = rpmTgtRefTextHeight;
            heightScale = virtualHeight / (stringDimensions.height);
            textTargetRefNameScale = Math.Min(widthScale, heightScale);

            x = fullScreenRect.center.x + refX + rtLabelSpacing + virtualWidth * .5f - (stringDimensions.width * textTargetRefNameScale / 2f);
            y = fullScreenRect.yMax - _rpmTextYTop - (stringDimensions.yOffset + .5f * stringDimensions.height) * textTargetRefNameScale;

            drawGlyphStringGraphics(referenceName, x, y, textTargetRefNameScale, BitmapFont.HorizontalAlignment.LEFT, currentHighlightBox == HighlightBox.RIGHT ? Color.yellow : Color.white);
        }

        private static Rect constrainToScreen(Rect r)
        {
            r.x = Mathf.Clamp(r.x, 150 - r.width, Screen.width - 150);
            r.y = Mathf.Clamp(r.y, 150 - r.height, Screen.height - 150);
            return r;
        }

        private static Vector3 identityScaleV3 = new Vector3(1, 1, 1);
        private static Vector3 glassCenterV3 = new Vector3();
        private static Rect screenRect = new Rect();
        private static Vector2 glassCenter = new Vector2();
        private static Rect fullScreenRect = new Rect();
        static float screenPercentRPM = 1f;//.935f;
        static Rect rpmDrawableRect = new Rect();
        
        static Rect visibleRect = new Rect(40, 44, 319, 319);
        public static RenderTexture guiRenderTexture = null;
        
        public static void drawIndicatorContentsToTexture()
        {
            //var cam = KSP.UI.UIMainCamera.Camera;

            guiRenderTexture.DiscardContents();

            var previousRenderTexture = RenderTexture.active;
            
            RenderTexture.active = guiRenderTexture;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, guiRenderTexture.width, guiRenderTexture.height, 0);

            float screenWidth = guiRenderTexture.width;
            float screenHeight = guiRenderTexture.height;
            screenPercentRPM = 1f;

            fullScreenRect.Set(0, 0, screenWidth, screenHeight);
            float virtualWidth = screenWidth * screenPercentRPM;
            float virtualHeight = screenHeight * screenPercentRPM;
            float xOffset = (screenWidth - virtualWidth) * .5f;
            float yOffset = 0;//(screenHeight - virtualHeight) * .5f;

            screenRect.Set(xOffset, yOffset, virtualWidth, virtualHeight);
            glassCenter = screenRect.center;
            glassCenterV3.Set(glassCenter.x, glassCenter.y, 0);

            vertLineHeaderChop = 0;
            vertLineFooterChop = 0;

            rpmDrawableRect.Set(xOffset, vertLineHeaderChop, virtualWidth, virtualHeight - vertLineFooterChop - vertLineHeaderChop);

            Graphics.DrawTexture(screenRect, gaugeBackgroundTex);

            float baseScale = 1f;

            if (targetedDockingModule != null)
            {
                if (useCDI)
                {
                    float xVal = 0, yVal = 0;
                    float visibleYOffset = vertLineHeaderChop;
                    Color colorCDI = determineCDIcolor();

                    calculateCDIvalues0to1(ref xVal, ref yVal);

                    NavyFish.Drawing.DrawVerticalLineGraphics(glassCenter.x + (xVal - .5f) * screenRect.width, rpmDrawableRect.yMin, rpmDrawableRect.height, 2f, colorCDI);
                    NavyFish.Drawing.DrawHorizontalLineGraphics(rpmDrawableRect.xMin, Math.Max(glassCenter.y + (yVal - .5f) * screenRect.height, visibleRect.yMin), rpmDrawableRect.width, 2f, colorCDI);
                }

                
                if (Math.Abs(orientationDeviation.x) > alignmentGaugeRange || Math.Abs(orientationDeviation.y) > alignmentGaugeRange)
                {
                    Vector2 normDir = new Vector2(orientationDeviation.x, orientationDeviation.y).normalized;
                    float arrowX = (alignmentFlipXAxis ? -1 : 1) * normDir.x;
                    float arrowY = (alignmentFlipYAxis ? -1 : 1) * -normDir.y;
                    float angle = (float)Math.Atan2(arrowX, arrowY) * UnityEngine.Mathf.Rad2Deg;

                    float arrowLength = screenRect.height * arrowLengthMult;
                    float arrowWidth = arrowLength * directionArrowTex.width / directionArrowTex.height;

                    Rect arrowRect = new Rect(-arrowWidth * .5f, -arrowLength * arrowLengthOffsetMult, arrowWidth, arrowLength);

                    GL.PushMatrix();

                    GL.MultMatrix(Matrix4x4.TRS(glassCenterV3, Quaternion.Euler(0, 0, angle), identityScaleV3));

                    Graphics.DrawTexture(arrowRect, directionArrowTex);
                    GL.PopMatrix();

                }
                else
                {
                    float displayX = (alignmentFlipXAxis ? -1 : 1) * scaleExponentially(orientationDeviation.x / alignmentGaugeRange, alignmentExponent);
                    float displayY = (alignmentFlipYAxis ? -1 : 1) * scaleExponentially(orientationDeviation.y / alignmentGaugeRange, alignmentExponent);

                    float scaledMarkerSize = markerSize * gaugeAlignmentMarkerScale;

                    Rect markerRect = new Rect(glassCenter.x * (1 + displayX) - scaledMarkerSize * .5f,
                                            glassCenter.y * (1 + displayY) - scaledMarkerSize * .5f,
                                            scaledMarkerSize,
                                            scaledMarkerSize);

                    Graphics.DrawTexture(markerRect, alignmentTex);


                    float scaledRollWidth = roll.width * baseScale * rollMarkerScale;
                    float scaledRollHeight = roll.height * baseScale * rollMarkerScale;
                    
                    GL.PushMatrix();
                   
                    GL.MultMatrix(Matrix4x4.TRS(glassCenterV3, Quaternion.Euler(0, 0, -orientationDeviation.z * (rollFlipAxis ? -1 : 1)), identityScaleV3));

                    Graphics.DrawTexture(new Rect(-scaledRollWidth / 2f, (scaledRollHeight + rollOffset - screenRect.height) / 2f, scaledRollWidth, scaledRollHeight), roll);
                    GL.PopMatrix();
                }

                if (useCDI)
                {
                    drawVelocityVector(screenRect, gaugeVelocityVectorScale);
                }

                float dstXpos = _dstXpos;
                float dstYpos = _dstYpos;
                float cvelXpos = _cvelXpos;
                float cvelYpos = _cvelYpos;

                Color originalColor = GUI.color;

                GUI.color = colorGaugeLabels;
                drawGlyphStringGrahpics("DST", screenRect.x + _DSTLABEL_x, screenRect.y + _dstYpos + _LABEL_yOFF,gaugeLabelScale, BitmapFont.HorizontalAlignment.LEFT);
                drawGlyphStringGrahpics("CVEL", screenRect.x + _CVELLABEL_x, screenRect.y + _cvelYpos + _LABEL_yOFF, gaugeLabelScale, BitmapFont.HorizontalAlignment.RIGHT);
                drawGlyphStringGrahpics("CDST", screenRect.x + _CDSTLABEL_x, screenRect.y + (_CDSTLABEL_y + _LABEL_yOFF), gaugeLabelScale, BitmapFont.HorizontalAlignment.RIGHT);
                GUI.color = originalColor;

                drawGlyphStringGrahpics(distanceToTarget.ToString("F1"), screenRect.x + dstXpos, screenRect.y + _dstYpos, gaugeDigitScale, BitmapFont.HorizontalAlignment.LEFT);
                drawGlyphStringGrahpics(closureV.ToString("F"), screenRect.x + cvelXpos, screenRect.y + _cvelYpos, gaugeDigitScale, BitmapFont.HorizontalAlignment.RIGHT);
                drawGlyphStringGrahpics(closureD.ToString("F1"), screenRect.x + _CLOSURED_x, screenRect.y + _CLOSURED_y, gaugeDigitScale, BitmapFont.HorizontalAlignment.RIGHT);


                if (drawRollDigits)
                {
                    float rDegXPos = _rDegXPos;
                    float rDegYPos = _rDegYPos;
                    GUI.color = colorGaugeLabels;
                    drawGlyphStringGrahpics("R\u00B0", screenRect.x + _degSign_x, screenRect.y + _degSign_y, gaugeLabelScale, BitmapFont.HorizontalAlignment.RIGHT);
                    GUI.color = originalColor;
                    drawGlyphStringGrahpics(orientationDeviation.z.ToString("F1"), screenRect.x + rDegXPos, screenRect.y + rDegYPos, gaugeDigitScale, BitmapFont.HorizontalAlignment.RIGHT);
                }
            }


            GL.PopMatrix();
            RenderTexture.active = previousRenderTexture;
        }

        public static void drawRenderedGaugeTexture(int windowID)
        {
            Rect gaugeRect = new Rect(0, 0, foregroundTextureWidth * gaugeScale, foregroundTextureHeight* gaugeScale);

            backgroundRect.Set(visibleRect.x * gaugeScale,
                                visibleRect.y * gaugeScale,
                                visibleRect.width * gaugeScale,
                                visibleRect.height * gaugeScale);


            GUI.DrawTexture(backgroundRect, guiRenderTexture);

            GUI.DrawTexture(gaugeRect, gaugeForegroundTex);

            drawTargetPortName(gaugeRect);

            Color lastBackColor = GUI.backgroundColor;
            if (showSettings)
            {
                GUI.backgroundColor = colorsettingsButtonActivated;
            }
            else
            {
                GUI.backgroundColor = colorsettingsButtonDeactivated;
            }

            Rect settingsButtonRect = new Rect(settingsButtonX * gaugeScale, settingsButtonY * gaugeScale, settingsButtonWidth * gaugeScale, settingsButtonHeight * gaugeScale);
            bool settingsButtonClicked = GUI.Button(settingsButtonRect, "", settingsButtonStyle);

            drawGlyphStringGUI("Settings", settingsTextX * gaugeScale, settingsTextY * gaugeScale, settingsTextScale * gaugeScale, BitmapFont.HorizontalAlignment.LEFT);

            if (settingsButtonClicked) showSettings = !showSettings;

            if (allowAutoPortTargeting)
            {
                Event ev = Event.current;
                if (ev.type == EventType.MouseDown && ev.button == 0)
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
            //print("Drawing Settings Window.." + settingsWindowPosition.ToString());
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
            drawHudIcon = GUILayout.Toggle(drawHudIcon, "Display HUD Target Port Icon");
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
                showHUDIconWhileIva = GUILayout.Toggle(showHUDIconWhileIva, "Display when using RPM");
                if (showHUDIconWhileIva != last)
                {
                    saveConfigSettings();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("HUD Target Port Icon Size:");
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
                last = restrictDockingPorts;
                restrictDockingPorts = GUILayout.Toggle(restrictDockingPorts, "Restrict Docking Ports");
                if (restrictDockingPorts != last)
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

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            last = alignmentFlipXAxis;
            alignmentFlipXAxis = GUILayout.Toggle(alignmentFlipXAxis, "Invert Alignment X");
            if (alignmentFlipXAxis != last) saveConfigSettings();

            GUILayout.FlexibleSpace();
            //GUILayout.EndHorizontal();
            //GUILayout.BeginHorizontal();
            last = translationFlipXAxis;
            translationFlipXAxis = GUILayout.Toggle(translationFlipXAxis, "Invert Translation X");
            if (translationFlipXAxis != last) saveConfigSettings();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            last = alignmentFlipYAxis;
            alignmentFlipYAxis = GUILayout.Toggle(alignmentFlipYAxis, "Invert Alignment Y");
            if (alignmentFlipYAxis != last) saveConfigSettings();

            GUILayout.FlexibleSpace();
            //GUILayout.EndHorizontal();
            //GUILayout.BeginHorizontal();
            last = translationFlipYAxis;
            translationFlipYAxis = GUILayout.Toggle(translationFlipYAxis, "Invert Translation Y");
            if (translationFlipYAxis != last) saveConfigSettings();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            last = rollFlipAxis;
            rollFlipAxis = GUILayout.Toggle(rollFlipAxis, "Invert Roll Direction");
            if (rollFlipAxis != last) saveConfigSettings();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            last = forceStockAppLauncher;
            forceStockAppLauncher = GUILayout.Toggle(forceStockAppLauncher, "Always use Stock Toolbar");
            if (forceStockAppLauncher != last)
            {
                saveConfigSettings();
                updateToolBarButton();
            }
            GUILayout.EndHorizontal();
        }
        Rect centeredToggleRect = new Rect(0,0,0,0);
    
        private static void drawTargetPortName(Rect positionRect)
        {
            String targetDisplayName = determineTargetPortName();
            BitmapFont.StringDimensions stringDimensions = bitmapFont.getStringDimensions(targetDisplayName, 1f);
            float widthScale = targetNameBoxWidth * gaugeScale / stringDimensions.width;
            float heightScale = targetNameBoxHeight * gaugeScale / (stringDimensions.height);
            textTargetRefNameScale = Math.Min(widthScale, heightScale);
            float x = positionRect.center.x - stringDimensions.width * textTargetRefNameScale / 2f;
            float y = positionRect.yMax - (targetNameBoxYOffset * gaugeScale) - (stringDimensions.yOffset + .5f * stringDimensions.height) * textTargetRefNameScale;
            
            drawGlyphStringGUI(targetDisplayName, x, y, textTargetRefNameScale, BitmapFont.HorizontalAlignment.LEFT);
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

        private static void drawGlyphStringGUI(String valueString, float x, float y, float customScale, BitmapFont.HorizontalAlignment hAlign)
        {
            bitmapFont.drawStringGUI(valueString, x, y, customScale, hAlign, Color.white);
        }

        private static void drawGlyphStringGUI(String valueString, float x, float y, float customScale, BitmapFont.HorizontalAlignment hAlign, Color color)
        {
            bitmapFont.drawStringGUI(valueString, x, y, customScale, hAlign, color);
        }

        private static void drawGlyphStringGrahpics(String valueString, float x, float y, float customScale, BitmapFont.HorizontalAlignment hAlign)
        {
            bitmapFont.drawStringGraphics(valueString, x, y, customScale, hAlign, Color.white);
        }

        private static void drawGlyphStringGraphics(String valueString, float x, float y, float customScale, BitmapFont.HorizontalAlignment hAlign, Color color)
        {
            bitmapFont.drawStringGraphics(valueString, x, y, customScale, hAlign, color);
        }

           private static void drawVelocityVector(Rect gaugeRect, float baseScale)
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

            gaugeX = (translationFlipXAxis ? -1 : 1) * scaleExponentially(gaugeX, velocityVectorExponent);
            gaugeY = (translationFlipYAxis ? -1 : 1) * scaleExponentially(gaugeY, velocityVectorExponent);

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

            // When we exit a scene with the DPAI window showing, the underlying GameObject
            // has already been destroyed but targetedDockingModule is not null and, being
            // an interface variable, does not use the Unity operator == overload.  So we
            // need to do a bit more to avoid a Null-reference exception.
            var tdmTransform = targetedDockingModule?.GetTransform();
            // tdmTransform is an actual Unity object variable, so it uses the overloaded
            // Unity operator == and correctly detects a destroyed GameObject.
            if (tdmTransform == null) {
                return;
            }

            Camera cam = FlightCamera.fetch.mainCamera;
            //Vector3 portToCamera = targetedDockingModule.transform.position - cam.transform.position;
            Vector3 portToCamera = tdmTransform.position - cam.transform.position;

            if (Vector3.Dot(cam.transform.forward, portToCamera) < 0)
            {
                //Port is behind the camera
                return;
            }

            Vector3 screenSpacePortLocation = cam.WorldToScreenPoint(tdmTransform.position);
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

        public static void clearRenameHighlightBoxRPM()
        {
            currentHighlightBox = HighlightBox.NONE;
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
            Log($"saveWindowPosition");
            config.SetValue("window_position", windowPosition);
            config.save();
        }

        private static void saveConfigSettings()
        {
            Log($"saveConfigSettings");
            //config.SetValue("show_cdi", useCDI);
            //config.SetValue("show_rolldigits", drawRollDigits);
            config.SetValue("drawHudIcon", drawHudIcon);
            config.SetValue("showHUDIconWhileEva", showHUDIconWhileIva);
            config.SetValue("HudIconSize", (double)targetHUDiconSize);
            config.SetValue("allowAutoPortTargeting", allowAutoPortTargeting);
            config.SetValue("excludeDockedPorts", excludeDockedPorts);
            config.SetValue("restrictDockingPorts", restrictDockingPorts);
            config.SetValue("gui_scale", (double)gaugeScale);
            config.SetValue("alignmentFlipXAxis", alignmentFlipXAxis);
            config.SetValue("alignmentFlipYAxis", alignmentFlipYAxis);
            config.SetValue("translationFlipXAxis", translationFlipXAxis);
            config.SetValue("translationFlipYAxis", translationFlipYAxis);
            config.SetValue("rollFlipAxis", rollFlipAxis);
            config.SetValue("forceStockAppLauncher", forceStockAppLauncher);
            config.save();
        }

        public static void LoadPrefs()
        {
            Log($"LoadPrefs");
            //print("Load Prefs");
            config = PluginConfiguration.CreateForType<DockingPortAlignmentIndicator>(null);
            config.load();

            gaugeScale = (float)config.GetValue<double>("gui_scale", 0.86);

            Rect defaultWindow = new Rect(Screen.width * .75f - (backgroundTextureWidth * gaugeScale / 2f), Screen.height * .5f - (backgroundTextureHeight * gaugeScale / 2f), backgroundTextureWidth * gaugeScale, backgroundTextureHeight * gaugeScale);
            windowPosition = config.GetValue<Rect>("window_position", defaultWindow);

            windowPosition = constrainToScreen(windowPosition);

            //useCDI = config.GetValue<bool>("show_cdi", true);
            //drawRollDigits = config.GetValue("show_rolldigits", true);
            drawHudIcon = config.GetValue<bool>("drawHudIcon", true);
            targetHUDiconSize = (float)config.GetValue<double>("HudIconSize", 22f);
            allowAutoPortTargeting = config.GetValue<bool>("allowAutoPortTargeting", true);
            excludeDockedPorts = config.GetValue<bool>("excludeDockedPorts", true);
            restrictDockingPorts = config.GetValue<bool>("restrictDockingPorts", true);
            showHUDIconWhileIva = config.GetValue<bool>("showHUDIconWhileEva", false);
            alignmentFlipXAxis = config.GetValue<bool>("alignmentFlipXAxis", false);
            alignmentFlipYAxis = config.GetValue<bool>("alignmentFlipYAxis", false);
            translationFlipXAxis = config.GetValue<bool>("translationFlipXAxis", false);
            translationFlipYAxis = config.GetValue<bool>("translationFlipYAxis", false);
            rollFlipAxis = config.GetValue<bool>("rollFlipAxis", false);
            forceStockAppLauncher = config.GetValue<bool>("forceStockAppLauncher", true);
            saveWindowPosition();
            saveConfigSettings();
            //print("End Load Prefs");
        }
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
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("gaugeBackground.png", null);
            gaugeBackgroundTex = new Texture2D(backgroundTextureWidth, backgroundTextureHeight, TextureFormat.ARGB32, false);
            gaugeBackgroundTex.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("RPM_background.png", null);
            rpmBackgroundTex = new Texture2D(317, 317, TextureFormat.ARGB32, false);
            rpmBackgroundTex.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("gaugeForeground.png", null);
            gaugeForegroundTex = new Texture2D(foregroundTextureWidth, foregroundTextureHeight, TextureFormat.ARGB32, false);
            gaugeForegroundTex.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("alignment.png", null);
            alignmentTex = new Texture2D(207, 207, TextureFormat.ARGB32, false);
            alignmentTex.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("directionArrow.png", null);
            directionArrowTex = new Texture2D(70, 150, TextureFormat.ARGB32, false);
            directionArrowTex.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("prograde.png", null);
            prograde = new Texture2D(96, 96, TextureFormat.ARGB32, false);
            prograde.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("retrograde.png", null);
            retrograde = new Texture2D(96, 96, TextureFormat.ARGB32, false);
            retrograde.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("roll.png", null);
            roll = new Texture2D(64, 64, TextureFormat.ARGB32, false);
            roll.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("DialogPlain.png", null);
            fontTexture = new Texture2D(256, 256, TextureFormat.ARGB32, false);
            fontTexture.LoadImage(arrBytes);
            fontTexture.filterMode = FilterMode.Bilinear;
            fontTexture.wrapMode = TextureWrapMode.Clamp;

            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("targetPort.png", null);
            targetPort = new Texture2D(40, 40, TextureFormat.ARGB32, false);
            targetPort.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignmentIndicator>("appLauncherIcon.png", null);
            appLauncherIcon = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            appLauncherIcon.LoadImage(arrBytes);
            TextReader tr = KSP.IO.TextReader.CreateForType<DockingPortAlignmentIndicator>("DialogPlain.fnt", null);
            List<string> textStrings = new List<string>();
            while (!tr.EndOfStream)
            {
                textStrings.Add(tr.ReadLine());
            }
            tr.Close();
            tr.Dispose();

            bitmapFont = new BitmapFont(fontTexture, textStrings.ToArray());
            guiRenderTexture = new RenderTexture((int)visibleRect.width, (int)visibleRect.height, 0, RenderTextureFormat.ARGB32);
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
        #endregion

        #region Debugging

        private static bool shouldDebug = false;
        
        private void OnDrawDebug()
        {
            debugWindowPosition = GUILayout.Window(1338, debugWindowPosition, drawDebugWindowContents, "Debug", GUILayout.MinWidth(400), GUILayout.MaxWidth(800));
        }

        
        static float rollOffsetRPM = -57;
        
        static int vertLineFooterChop = 50;

        private static float settingsButtonX = 150;
        private static float settingsButtonY = 376;
        private static float settingsButtonWidth = 100;
        private static float settingsButtonHeight = 14;

        static float settingsTextX = 178;
        static float settingsTextY = 374;
        static float settingsTextScale = .47f;

        static float numLabelXOffset = -45;
        static float numLabelYOffset = 5f;
        static float DSTLabelXOffset = -37;

        private static int _rpmTextYTop = 19;
        private static int rpmTgtRefTextHeight = 19;

        private static int tgtX = 6, refX = 6, rtLabelY = 48, rtLabelSpacing = 6;
        private static float rtLabelScale = .6f;

        private static float arrowLengthOffsetMultRPM = 2f; //2.8f;

        private static int vertLineHeaderChop = 15;

        static float dstXpos = 83;
        static float cvelXpos = 298;
        static float textNumberYPos = 339;
        static float closureDXPos = 298;
        static float closureDYPos = 317;
        static float rDegXPos = 315;
        static float rDegYPos = 43;
        static float degreeSignXPos = 292;
        static float degreeSignYPos = 48;

        static float alignmentMarkerVisibleRange = .81f;

        static float gaugeDigitScale = .8f;
        static float gaugeLabelScale = .55f;

        static int _LABEL_yOFF = 6;

        static int _dstXpos = 47;
        static int _dstYpos = 292;
                
        static int _DSTLABEL_x = 7;

        static int _cvelXpos = 304;
        static int _cvelYpos = 292;
        static int _CVELLABEL_x = 227;

        static int _CLOSURED_x = 304;
        static int _CLOSURED_y = 267;

        static int _CDSTLABEL_x = 226;
        static int _CDSTLABEL_y = 267;     
        
        static int _rDegXPos = 304;
        static int _rDegYPos = -1;

        static int _degSign_x = 248;
        static int _degSign_y = 3;

        static float gaugeVelocityVectorScale = 1.05f;

        static float rollMarkerScale = .65f;
        static float rollOffset = -53;

        static float gaugeAlignmentMarkerScale = .9f;
        private static float arrowLengthOffsetMult = 1.6f, arrowLengthMult = .2f;

        static float targetNameBoxWidth = 205;
        static float targetNameBoxHeight = 40;
        static float targetNameBoxYOffset = 37;

        public static int RPMbottomGutter = 30;

        public static bool alignmentFlipXAxis = false;
        public static bool alignmentFlipYAxis = false;
        public static bool translationFlipXAxis = false;
        public static bool translationFlipYAxis = false;
        public static bool rollFlipAxis = false;

        private void drawDebugWindowContents(int windowID)
        {
            //stuff here

            //intTextField(ref tgtX, "tgtX");
            //intTextField(ref refX, "refX");
            bool sceneElligibleForIndicator = (HighLogic.LoadedSceneIsFlight && !FlightGlobals.ActiveVessel.isEVA && !MapView.MapIsEnabled);
            label<Boolean>(sceneElligibleForIndicator, "sceneElligibleForIndicator");
            label<Boolean>(gaugeVisiblityToggledOn, "gaugeVisiblityToggledOn");


            label<Boolean>(RPMPageActive, "RPMPageActive");
            label<Boolean>(isIVA(), "isIVA()");
            label<Boolean>(showIndicator, "showIndicator");
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
            label<Boolean>(showIndicator || (RPMPageActive && isIVA()), "(showIndicator || (RPMPageActive && isIVA()))");

            GUI.DragWindow();
        }

        private static void checkBoxField(ref bool value, string label)
        {
            GUILayout.BeginHorizontal();
            value = GUILayout.Toggle(value, label);
            GUILayout.EndHorizontal();
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
            if (value != null)
            {
                GUILayout.Label(value.ToString());
            } else
            {
                GUILayout.Label("Null");
            }
            GUILayout.EndHorizontal();
        }

        #endregion
    }
}
