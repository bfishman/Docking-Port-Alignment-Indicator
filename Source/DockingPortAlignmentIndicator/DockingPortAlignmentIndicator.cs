#region License
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
#endregion

using System;
using UnityEngine;
using KSP.IO;
using System.Collections.Generic;
using System.Diagnostics;
using static NavyFish.DPAI.LogWrapper;
using System.Linq;

namespace NavyFish.DPAI
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DockingPortAlignmentIndicator : MonoBehaviour
    {
        private static Settings.Configuration c = null;
        private static bool hasInitializedStyles = false;
        private static Rect debugWindowPosition = new Rect(50,200,350,200);

        static Rect selectedPortHUDRect;

        private static int backgroundTextureWidth = 317;
        private static int backgroundTextureHeight = 317;

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
        private static float pulsePeriod = 1.42f;
        private static float pulseDurationRatio = .4f;

        private static Color colorCDINormal = new Color(.064f, .642f, 0f);
        private static Color colorCDIReverse = new Color(.747f, 0f, .05f);
        private static Color colorTargetPortHUDicon = new Color(.989f, .329f, .953f);
        private static Color colorGaugeLabels = new Color(.41f, .41f, .41f, 1f);

        public static Texture2D gaugeBackgroundTex = null;
        public static Texture2D rpmBackgroundTex = null;
        public static Texture2D alignmentTex = null;
        public static Texture2D directionArrowTex = null;
        public static Texture2D prograde = null;
        public static Texture2D retrograde = null;
        public static Texture2D roll = null;
        public static Texture2D targetPort = null;
        public static Texture2D fontTexture = null;

        public static BitmapFont bitmapFont;
        private static float textTargetRefNameScale = .77f;

        private static bool useCDI = true;
        private static bool drawRollDigits = true;
        private static bool showIndicator;
        private static bool portWasCycled = false;
        private static bool currentTargetVesselWasLastSeenLoaded = false;
        public static bool gaugeVisiblityToggledOn = false;
        private static bool targetOutOfRange = false;
        private static bool resetTarget = false;

        public static bool RPMPageActive = false;

        static List<ITargetable> dockingModulesList = new List<ITargetable>();
        static int dockingModulesListIndex = -1;

        static ITargetable currentTarget = null;
        static ITargetable lastTarget = null;
        static Vessel currentTargetVessel = null;
        static Vessel lastTargetVessel = null;
        static Vessel lastActiveVessel = null;
        static int cycledModuleIndex = -1;
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

        // Callback for toolbar button click
        private void onShowGUI()
        {
            LogD("onShowGUI()");
            gaugeVisiblityToggledOn = true;
            MainWindow?.OnShowGUI();

            ModuleDockingNodeNamed.onPortRenamed += OnPortRenamed;
        }

        // Callback for toolbar button click
        private void onHideGUI()
        {
            LogD("onHideGUI()");
            gaugeVisiblityToggledOn = false;

            ModuleDockingNodeNamed.onPortRenamed -= OnPortRenamed;
            MainWindow?.OnHideGUI();
        }

        private void onToggleGUI()
        {
            if (gaugeVisiblityToggledOn) {
                onHideGUI();
            } else {
                onShowGUI();
            }
        }

        private bool IsSceneEligibleForIndicator {
            get {
                //return HighLogic.LoadedSceneIsFlight && !FlightGlobals.ActiveVessel.isEVA && !MapView.MapIsEnabled;
                return HighLogic.LoadedSceneIsFlight && !FlightGlobals.ActiveVessel.isEVA;
            }
        }


        public DPAI_Panel MainWindow {
            get { return DPAI_Panel.Instance; }
        }

        private void OnPortRenamed(ModuleDockingNodeNamed renamedNode)
        {
            if (renamedNode == targetNamedModule) {
                MainWindow?.OnTargetPortRenamed(determineTargetPortName());
            };
        }

        /// <summary>
        /// Handle additional functionality on setting changes
        /// </summary>
        /// <param name="setting">Name of the setting that was changed.</param>
        void OnSettingChanged(string setting)
        {
            switch(setting) {
                case "UseStockToolbar":
                case "UseBlizzyToolbar":
                    updateToolBarButton();
                    break;
                case "AllowAutoPortTargeting":
                case "ExcludeDockedPorts":
                case "RestrictDockingPorts":
                    resetTarget = true;
                    break;
                case "GaugeScale":
                    MainWindow?.OnScaleChanged(c.GaugeScale);
                    break;
            }
        }

        #region Toolbar
        /// <summary>
        /// Dynamically turns the Stock and Blizzy toolbarbuttons on or off.
        /// </summary>
        private void updateToolBarButton()
        {
            LogD($"updateToolBarButton (GameScene=={HighLogic.LoadedScene}");
            Toolbar.Instance.SetToolbarButtons(c.UseStockToolbar, c.UseBlizzyToolbar);
        }

        private void OnToolbarButtonClicked()
        {
            onToggleGUI();
            c.IsWindowVisible = gaugeVisiblityToggledOn;
        }
        #endregion // Toolbar

        #region GameEvents
        // GameEvents.onKSPediaSpawn
        // Called when the KSPedia is shown
        private void OnKSPediaSpawn ()
        {
            LogD($"GameEvents.OnKSPediaSpawn()");
            if (gaugeVisiblityToggledOn) {
                onHideGUI();
            }
        }

        // GameEvents.onKSPediaDespawn
        // Note: this event seems to get fired twice when the KSPedia is closed,
        //       so ensure this function only performs its actions once.
        private void OnKSPediaDespawn ()
        {
            LogD($"GameEvents.OnKSPediaDespawn()");
            if (c.IsWindowVisible) {
                onShowGUI();
            }
        }

        /// <summary>
        /// Called once per object. Effectively the Constructor.
        /// </summary>
        public void Awake()
        {
            LogD($"Awake (GameScene=={HighLogic.LoadedScene})");

            // Initialize all the things that rely on an initialized Unity environment
            c = Settings.Configuration.Instance;
            c.Load();
            selectedPortHUDRect = new Rect(0, 0, c.HudIconSize, c.HudIconSize);

            loadTextures();
        }

        /// <summary>
        /// Called once only per object just before the first frame.
        /// </summary>
        public void Start()
        {
            LogD($"Start (GameScene=={HighLogic.LoadedScene})");

            Toolbar.Instance.onToolbarButtonClicked += OnToolbarButtonClicked;
            updateToolBarButton();

            Settings.Configuration.onPropertyChanged += OnSettingChanged;
            GameEvents.onGUIKSPediaSpawn.Add(OnKSPediaSpawn);
            GameEvents.onGUIKSPediaDespawn.Add(OnKSPediaDespawn);

            if (c.IsWindowVisible) {
                onShowGUI();
            }
        }

        /// <summary>
        /// Called whenever the object is destroyed. Effectively the Destructor.
        /// </summary>
        private void OnDestroy()
        {
            LogD($"OnDestroy (GameScene=={HighLogic.LoadedScene}, ForceStockAppLauncher=={c.UseStockToolbar})");

            onHideGUI();
            c.Save();

            // TODO: destroy toolbar buttons?

            Toolbar.Instance.onToolbarButtonClicked -= OnToolbarButtonClicked;
            Settings.Configuration.onPropertyChanged -= OnSettingChanged;
            GameEvents.onGUIKSPediaSpawn.Remove(OnKSPediaSpawn);
            GameEvents.onGUIKSPediaDespawn.Remove(OnKSPediaDespawn);
        }

        private void OnGUI()
        {
            onGaugeDraw();
            if (shouldDebug) OnDrawDebug();
        }

        public void Update()
        {
            if ( !HighLogic.LoadedSceneIsFlight ) {
                return;
            }

            //if (Input.GetKeyDown(KeyCode.B)){
            //        cycledModuleIndex = dockingModulesListIndex + 1;
            //        cycledModuleIndex %= dockingModulesList.Count;
            //        portWasCycled = true;
            //}

            showIndicator = IsSceneEligibleForIndicator && gaugeVisiblityToggledOn;

            if (showIndicator || (RPMPageActive && isIVA()))
            {
                var lastTargetedDockingModule = targetedDockingModule;
                determineTargetPort();
                if (targetedDockingModule != lastTargetedDockingModule) {
                    // TODO: Make event
                    MainWindow?.OnTargetUpdated();
                }
                if (targetedDockingModule != null) calculateGaugeData();
                drawIndicatorContentsToTexture();
            }
        }
        #endregion GameEvents

        private static bool isIVA()
        {
            return InternalCamera.Instance?.isActive ?? false;
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
            var dockingPorts  = referencePart?.FindModulesImplementing<ModuleDockingNode>();
            if ((dockingPorts?.Count ?? 0) == 0) {
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

            var justLeftMap = !MapView.MapIsEnabled && wasLastMap;
            wasLastMap = MapView.MapIsEnabled;

            var justEnteredIVA = isIVA() && !wasLastIVA;
            wasLastIVA = isIVA();

            if (lastReferencePart != referencePart)
            {
                if (isIVA() && (justEnteredIVA || justLeftMap)) {
                    if (FlightGlobals.ActiveVessel.Parts.Contains(lastReferencePart))
                    {
                        FlightGlobals.ActiveVessel.SetReferenceTransform(lastReferencePart);
                        findReferencePoints();
                    }
                }
                lastReferencePart = referencePart;
                // Force recalculation of possible target ports if we're restricting them
                if (c.RestrictDockingPorts) {
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

                            if (c.AllowAutoPortTargeting)
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
                                        LogD($"Adding Docking Port {port} (state={port.state}, other={port.otherNode}) to list of targets.");
                                        // MKW: if node was attached in the VAB, state is "PreAttached"
                                        if (c.ExcludeDockedPorts &&
                                            (port.state.StartsWith("Docked", StringComparison.OrdinalIgnoreCase) ||
                                             port.state.StartsWith("PreAttached", StringComparison.OrdinalIgnoreCase))
                                           )
                                        {
                                            //print("continue");
                                            //do not add to list if module is already docked
                                            continue;
                                        }

                                        if(c.RestrictDockingPorts && !isCompatiblePort(port))
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

                            // This will happen either when the user manually selects a new target port by
                            // right-clicking on it, OR when a targetable part is targeted beyond 200m
                            // (because its parent vessel will be automatically re-targeted by KSP)
                            if (currentTarget is PartModule)
                            {
                                // Likely caused by user right-click a port and setting as target
                                targetedDockingModule = currentTarget;
                                dockingModulesListIndex = dockingModulesList.FindIndex(m => m.Equals(targetedDockingModule));
                            }
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

        private void onGaugeDraw()
        {
            if (c.DrawHudIcon)
            {
                if ((showIndicator && !isIVA()) || (c.ShowHudIconWhileIva && RPMPageActive && isIVA()))
                {
                    drawTargetPortHUDIndicator();
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

            // MW: Are we rendering the background _and_ the gauges, or just the gauges here?
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

                    Drawing.DrawVerticalLineGraphics(glassCenter.x + (xVal - .5f) * screenRect.width, rpmDrawableRect.yMin, rpmDrawableRect.height, 2f, colorCDI);
                    Drawing.DrawHorizontalLineGraphics(rpmDrawableRect.xMin, Math.Max(glassCenter.y + (yVal - .5f) * screenRect.height, visibleRect.yMin), rpmDrawableRect.width, 2f, colorCDI);
                }


                if (Math.Abs(orientationDeviation.x) > alignmentGaugeRange || Math.Abs(orientationDeviation.y) > alignmentGaugeRange)
                {
                    Vector2 normDir = new Vector2(orientationDeviation.x, orientationDeviation.y).normalized;
                    float arrowX = (c.AlignmentFlipXAxis ? -1 : 1) * normDir.x;
                    float arrowY = (c.AlignmentFlipYAxis ? -1 : 1) * -normDir.y;
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
                    float displayX = (c.AlignmentFlipXAxis ? -1 : 1) * scaleExponentially(orientationDeviation.x / alignmentGaugeRange, alignmentExponent);
                    float displayY = (c.AlignmentFlipYAxis ? -1 : 1) * scaleExponentially(orientationDeviation.y / alignmentGaugeRange, alignmentExponent);

                    float scaledMarkerSize = markerSize * gaugeAlignmentMarkerScale;

                    Rect markerRect = new Rect(glassCenter.x * (1 + displayX) - scaledMarkerSize * .5f,
                        glassCenter.y * (1 + displayY) - scaledMarkerSize * .5f,
                        scaledMarkerSize,
                        scaledMarkerSize);

                    Graphics.DrawTexture(markerRect, alignmentTex);


                    float scaledRollWidth = roll.width * baseScale * rollMarkerScale;
                    float scaledRollHeight = roll.height * baseScale * rollMarkerScale;

                    GL.PushMatrix();

                    GL.MultMatrix(Matrix4x4.TRS(glassCenterV3, Quaternion.Euler(0, 0, -orientationDeviation.z * (c.RollFlipAxis ? -1 : 1)), identityScaleV3));

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

        public static void cyclePortLeft()
        {
            if (!c.AllowAutoPortTargeting || dockingModulesList.Count < 2) {
                return;
            }
            cycledModuleIndex = dockingModulesListIndex - 1;
            if (cycledModuleIndex < 0) {
                cycledModuleIndex = (dockingModulesList.Count - 1);
            }
            portWasCycled = true;
        }

        public static void cyclePortRight()
        {
            if (!c.AllowAutoPortTargeting || dockingModulesList.Count < 2) {
                return;
            }
            cycledModuleIndex = dockingModulesListIndex + 1;
            cycledModuleIndex %= dockingModulesList.Count;
            portWasCycled = true;
        }

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
                return Utils.GetStringByTag("#none");
            }
        }

        public static String determineTargetPortName()
        {
            String targetDisplayName;

            if (currentTargetVessel == null)
            {
                targetDisplayName = Utils.GetStringByTag("#no_vessel_targeted");
            }
            else if (targetedDockingModule == null)
            {
                if (targetOutOfRange)
                {
                    targetDisplayName = Utils.GetStringByTag("#target_out_of_range");
                }
                else
                {
                    targetDisplayName = Utils.GetStringByTag("#no_port_targeted");
                }
            }
            else if (targetNamedModule == null)
            {
                targetDisplayName = targetedDockingModule.GetDisplayName();
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

            gaugeX = (c.TranslationFlipXAxis ? -1 : 1) * scaleExponentially(gaugeX, velocityVectorExponent);
            gaugeY = (c.TranslationFlipYAxis ? -1 : 1) * scaleExponentially(gaugeY, velocityVectorExponent);

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

            selectedPortHUDRect.width = c.HudIconSize;
            selectedPortHUDRect.height = c.HudIconSize;

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
            TextReader tr = KSP.IO.TextReader.CreateForType<DockingPortAlignmentIndicator>("DialogPlain.dat", null);
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

        #endregion

        #region Debugging

        private static bool shouldDebug = true;
        private static GUIStyle labelStyle = null;

        private void initStyles()
        {
            Color lightGrey = new Color(.8f, .8f, .85f);

            labelStyle = new GUIStyle(HighLogic.Skin.label);
            labelStyle.stretchWidth = true;
            labelStyle.stretchHeight = true;
            labelStyle.normal.textColor = lightGrey;

            hasInitializedStyles = true;
        }

        [Conditional("DEBUG")]
        private void OnDrawDebug()
        {
            if (!hasInitializedStyles) {
                initStyles();
            }
            debugWindowPosition = GUILayout.Window(1338, debugWindowPosition, drawDebugWindowContents, "DPAI Debug", GUILayout.MinWidth(400), GUILayout.MaxWidth(800));
        }

        static int vertLineFooterChop = 50;

        private static int _rpmTextYTop = 19;
        private static int rpmTgtRefTextHeight = 19;

        private static int tgtX = 6, refX = 6, rtLabelY = 48, rtLabelSpacing = 6;
        private static float rtLabelScale = .6f;

        private static int vertLineHeaderChop = 15;

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

        public static int RPMbottomGutter = 30;

        private void drawDebugWindowContents(int windowID)
        {
            //stuff here

            //intTextField(ref tgtX, "tgtX");
            //intTextField(ref refX, "refX");
            label<Boolean>(IsSceneEligibleForIndicator, "sceneEligibleForIndicator");
            label<Boolean>(gaugeVisiblityToggledOn, "gaugeVisiblityToggledOn");


            label<Boolean>(RPMPageActive, "RPMPageActive");
            label<Boolean>(isIVA(), "isIVA()");
            label<Boolean>(showIndicator, "showIndicator");
            GUILayout.BeginHorizontal();
            label<Boolean>(showIndicator || (RPMPageActive && isIVA()), "(showIndicator || (RPMPageActive && isIVA()))");
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            label<bool>(c.DrawHudIcon, "Draw HUD Icon");
            label<bool>(c.ShowHudIconWhileIva, "Show HUD Icon in IVA");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            label<bool>(c.AllowAutoPortTargeting, "Allow Port Targetting");
            label<bool>(c.RestrictDockingPorts, "Restrict Docking Ports");
            label<bool>(c.ExcludeDockedPorts, "Exclude Docked Ports");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            label<bool>(c.AlignmentFlipXAxis, "Alignment Flip X Axis");
            label<bool>(c.AlignmentFlipYAxis, "Alignment Flip Y Axis");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            label<bool>(c.TranslationFlipXAxis, "Translation Flip X Axis");
            label<bool>(c.TranslationFlipYAxis, "Translation Flip Y Axis");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            label<bool>(c.RollFlipAxis,"Roll Flip Axis");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            label<bool>(c.UseStockToolbar,"Force Stock App Launcher");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

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

} // End namespace NavyFish.DPAI
