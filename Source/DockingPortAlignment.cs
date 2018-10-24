using System;
using UnityEngine;
using KSP.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Toolbar;

namespace DockingPortAlignment
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DockingPortAlignment : MonoBehaviour
    {
        private static PluginConfiguration config;
        private static bool hasInitializedStyles = false;
        private static GUIStyle windowStyle, labelStyle, gaugeStyle, settingsButtonStyle, rightAlignedStyle, settingsAreaStyle, settingsToggleStyle;
        private static Rect windowPosition = new Rect();
        private static Rect lastPosition = new Rect();

        public static float scale = .65f;
        private static int gaugeWidth = 400;
        private static int gaugeHeight = 407;
        private static Rect gaugeRect = new Rect(0, 0f, gaugeWidth * scale, gaugeHeight * scale);
        private static float visiblePortion = .76f;

        private static LineRenderer ownshipLine = null;
        private static LineRenderer targetLine = null;

        private static Vector3 orientationDeviation = new Vector3();
        private static Vector2 translationDeviation = new Vector3();
        private static Vector2 transverseVelocity = new Vector2();
        private static float negativeOnBackHemisphere;
        private static float closureV;
        private static float distanceToTarget;

        private static Color colorCDINormal = new Color(.064f, .642f, 0f);
        private static Color colorCDIReverse = new Color(.747f, 0f, .05f);
        private static Color color_settingsButtonActivated = new Color(.11f, .66f, .11f, 1f);
        private static Color color_settingsButtonDeactivated = new Color(.22f, .26f, .29f, 1f);
        private static Color color_settingsWindow = new Color(.19f, .21f, .24f);

        public static Texture2D frontGlass = new Texture2D(gaugeWidth, gaugeHeight, TextureFormat.ARGB32, false);
        public static Texture2D background = new Texture2D(gaugeWidth, gaugeHeight, TextureFormat.ARGB32, false);
        public static Texture2D markerTexture = new Texture2D(207, 207, TextureFormat.ARGB32, false);
        public static Texture2D arrowTexture = new Texture2D(21, 87, TextureFormat.ARGB32, false);
        public static Texture2D prograde = new Texture2D(96, 96, TextureFormat.ARGB32, false);
        public static Texture2D retrograde = new Texture2D(96, 96, TextureFormat.ARGB32, false);
        public static Texture2D roll = new Texture2D(51, 33, TextureFormat.ARGB32, false);
        public static Texture2D roll_label = new Texture2D(17, 17, TextureFormat.ARGB32, false);

        public static int glyphTextureWidth = 256;
        public static int glyphTextureHeight = 32;
        public static Texture2D digits = new Texture2D(glyphTextureWidth, glyphTextureHeight, TextureFormat.ARGB32, false);
        public static float glyphTexOffsetX = 1 / (float)glyphTextureWidth;
        public static float glyphTexOffsetY = 1 / (float)glyphTextureHeight;

        public static GlyphData[] glyphData;

        // TODO Adjust size of source textures instead of down-scaling
        private static float velocityVectorIconSize = 42f;
        private static float markerSize = 140f;

        private static bool showSettings = false;
        private static bool useCDI = true;
        private static bool drawRollDigits = false;

        private static float transverseVelocityRange = 3.5f;
        private static float velocityVectorExponent = .75f;

        private static float alignmentGaugeRange = 60f;
        private static float alignmentExponent = .8f;

        private static float CDIExponent = .75f;
        private static float CDIexponentDecreaseBeginRange = 15f;
        private static float CDIexponentDecreaseDoneRange = 5f;

        private static IButton dockingButton;
        private static Boolean showIndicator;
        private static Rect settingsWindowPosition;
        private static int settingsWindowWidth = 268;
        private static int settingsWindowHeight = 120;


        public void Awake()
        {
            dockingButton = ToolbarManager.Instance.add("DockingAlignment", "dockalign");
            dockingButton.TexturePath = "NavyFish/Plugins/ToolbarIcons/settingsicon";
            dockingButton.ToolTip = "Docking Alignment Indicator Settings";
            dockingButton.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
            dockingButton.Visible = true;
            dockingButton.Enabled = false;
            dockingButton.OnClick += (e) =>
            {
                showSettings = !showSettings;
            };

            if (!hasInitializedStyles) initStyles();

            RenderingManager.AddToPostDrawQueue(0, OnDraw);

            loadTextures();

            settingsWindowPosition = new Rect((Screen.width - settingsWindowWidth) / 2f, (Screen.height - settingsWindowHeight) / 2f, settingsWindowWidth, settingsWindowHeight);

            print("Loaded Docking Port Alignment Gauge!");
        }

        private static void loadTextures()
        {
            Byte[] arrBytes;
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("background.png", null);
            background.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("frontglass.png", null);
            frontGlass.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("marker.png", null);
            markerTexture.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("arrow.png", null);
            arrowTexture.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("prograde.png", null);
            prograde.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("retrograde.png", null);
            retrograde.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("roll.png", null);
            roll.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("roll_label.png", null);
            roll_label.LoadImage(arrBytes);
            arrBytes = KSP.IO.File.ReadAllBytes<DockingPortAlignment>("digits.png", null);
            digits.LoadImage(arrBytes);

            glyphData = new GlyphData[12];
            glyphData[0] = new GlyphData(52, 11, 17, 1, 5, 11, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[1] = new GlyphData(45, 7, 17, 2, 5, 11, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[2] = new GlyphData(95, 11, 16, 1, 6, 11, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[3] = new GlyphData(85, 10, 16, 1, 6, 11, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[4] = new GlyphData(33, 12, 17, 0, 5, 11, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[5] = new GlyphData(22, 11, 17, 1, 5, 11, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[6] = new GlyphData(74, 11, 16, 1, 6, 11, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[7] = new GlyphData(11, 11, 17, 1, 5, 11, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[8] = new GlyphData(0, 11, 17, 1, 5, 11, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[9] = new GlyphData(63, 11, 16, 1, 6, 11, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[10] = new GlyphData(106, 8, 4, 0, 14, 7, glyphTexOffsetX, glyphTexOffsetY, 33);
            glyphData[11] = new GlyphData(114, 4, 4, 2, 18, 6, glyphTexOffsetX, glyphTexOffsetY, 33);
        }

        public void Start()
        {
            LoadPrefs();
        }

        private void OnDraw()
        {
            showIndicator = FlightGlobals.fetch.VesselTarget is ModuleDockingNode;

            if (showIndicator)
            {
                dockingButton.Enabled = true;

                calculateGaugeData();

                windowPosition.width = gaugeWidth * scale;
                windowPosition.height = gaugeHeight * scale;

                windowPosition = constrainToScreen(GUI.Window(1337, windowPosition, OnWindow, "Enchanced Nav Ball", labelStyle));


                if (showSettings)
                {
                    settingsWindowPosition = constrainToScreen(GUILayout.Window(1339, settingsWindowPosition, onSettingsWindow, "Docking Alignment Indicator Settings", windowStyle));
                }
            }
            else
            {
                dockingButton.Enabled = false;
            }
        }

        private static Rect constrainToScreen(Rect r)
        {
            print(r.x + ", " + r.y + ", " + r.width + ", " + r.height);
            r.x = Mathf.Clamp(r.x, 75 - r.width, Screen.width - 75);
            r.y = Mathf.Clamp(r.y, 75 - r.height, Screen.height - 75);
            return r;
        }

        private void calculateGaugeData()
        {
            Transform selfTransform = FlightGlobals.ActiveVessel.ReferenceTransform;
            ModuleDockingNode targetPort = FlightGlobals.fetch.VesselTarget as ModuleDockingNode;
            Transform targetTransform = targetPort.transform;
            Vector3 targetPortOutVector;
            Vector3 targetPortRollReferenceVector;

            if (targetPort.part.name == "dockingPortLateral")
            {
                targetPortOutVector = -targetTransform.forward.normalized;
                targetPortRollReferenceVector = -targetTransform.up;
            }
            else
            {
                targetPortOutVector = targetTransform.up.normalized;
                targetPortRollReferenceVector = targetTransform.forward;
            }

            orientationDeviation.x = AngleAroundNormal(-targetPortOutVector, selfTransform.up, selfTransform.forward);
            orientationDeviation.y = AngleAroundNormal(-targetPortOutVector, selfTransform.up, -selfTransform.right);
            orientationDeviation.z = AngleAroundNormal(targetPortRollReferenceVector, selfTransform.forward, selfTransform.up);
            orientationDeviation.z = (orientationDeviation.z + 360) % 360;

            Vector3 targetToOwnship = selfTransform.position - targetTransform.position;

            translationDeviation.x = AngleAroundNormal(targetToOwnship, targetPortOutVector, selfTransform.forward);
            translationDeviation.y = AngleAroundNormal(targetToOwnship, targetPortOutVector, -selfTransform.right);

            if (Math.Abs(translationDeviation.x) >= 90)
            {
                negativeOnBackHemisphere = -1;
            }
            else
            {
                negativeOnBackHemisphere = 1;
            }

            float normalVelocity = Vector3.Dot(FlightGlobals.ship_tgtVelocity, targetPortOutVector);
            closureV = -normalVelocity * negativeOnBackHemisphere;

            //Old behavior where velocity vector incorporated forward velocity
            //relativeVelocity.x = AngleAroundNormal(FlightGlobals.ship_tgtVelocity, targetPortOutVector, selfTransform.forward);
            //relativeVelocity.y = AngleAroundNormal(FlightGlobals.ship_tgtVelocity, targetPortOutVector, -selfTransform.right);

            Vector3 globalTransverseVelocity = FlightGlobals.ship_tgtVelocity - normalVelocity * targetPortOutVector;
            transverseVelocity.x = Vector3.Dot(globalTransverseVelocity, selfTransform.right);
            transverseVelocity.y = Vector3.Dot(globalTransverseVelocity, selfTransform.forward);

            //Prograde/Retrograde Vector
            //Vector3 localVelocity = selfTransform.InverseTransformDirection(FlightGlobals.ship_tgtVelocity);
            //relativeVelocity.x = (float)Math.Atan2(localVelocity.x, localVelocity.y);
            //relativeVelocity.y = (float)Math.Atan2(localVelocity.z, localVelocity.y);
            //relativeVelocity *= (float)(2f / Math.PI);

            distanceToTarget = Vector3.Distance(targetTransform.position, selfTransform.position);
        }

        private void OnWindow(int windowID)
        {
            gaugeRect.width = gaugeWidth * scale;
            gaugeRect.height = gaugeHeight * scale;
            Vector2 gaugeCenter = new Vector2(gaugeRect.width / 2f, gaugeRect.height / 2f);

            GUI.DrawTexture(gaugeRect, background);

            if (useCDI)
            {
                drawCDI(gaugeRect);
            }

            Matrix4x4 matrixBackup = GUI.matrix;
            if (Math.Abs(orientationDeviation.x) > alignmentGaugeRange || Math.Abs(orientationDeviation.y) > alignmentGaugeRange)
            {
                Vector2 normDir = new Vector2(orientationDeviation.x, orientationDeviation.y).normalized;
                float angle = (float)Math.Atan2(normDir.x, -normDir.y) * UnityEngine.Mathf.Rad2Deg;

                float arrowLength = visiblePortion * gaugeCenter.y;
                float arrowWidth = arrowLength * arrowTexture.width / arrowTexture.height;

                Rect arrowRect = new Rect(0.5f * (gaugeRect.width - arrowWidth), gaugeCenter.y - arrowLength, arrowWidth, arrowLength);

                GUIUtility.RotateAroundPivot(angle, gaugeCenter);

                GUI.DrawTexture(arrowRect, arrowTexture);
                GUI.matrix = matrixBackup;
            }
            else
            {
                float displayX = scaleExponentially(orientationDeviation.x / alignmentGaugeRange, alignmentExponent);
                float displayY = scaleExponentially(orientationDeviation.y / alignmentGaugeRange, alignmentExponent);

                float scaledMarkerSize = markerSize * scale;

                Rect markerRect = new Rect(gaugeCenter.x * (1 + displayX * visiblePortion),
                                        gaugeCenter.y * (1 + displayY * visiblePortion),
                                        scaledMarkerSize,
                                        scaledMarkerSize);

                GUI.DrawTexture(new Rect(markerRect.x - .5f * markerRect.width, markerRect.y - .5f * markerRect.height, markerRect.width, markerRect.height), markerTexture);

                GUIUtility.RotateAroundPivot(orientationDeviation.z, gaugeCenter);

                float scaledRollWidth = roll.width * scale;
                float scaledRollHeight = roll.height * scale;


                GUI.DrawTexture(new Rect(gaugeCenter.x - .5f * scaledRollWidth, (roll.height + 20) * scale, scaledRollWidth, scaledRollHeight), roll);
            }

            GUI.matrix = matrixBackup;

            if (useCDI)
            {
                drawVelocityVector(gaugeRect);
            }

            drawGaugeText(gaugeRect);

            GUI.DrawTexture(gaugeRect, frontGlass);

            Color lastBackColor = GUI.backgroundColor;
            if (showSettings)
            {
                GUI.backgroundColor = color_settingsButtonActivated;
            }
            else
            {
                GUI.backgroundColor = color_settingsButtonDeactivated;
            }

            bool settingsButtonClicked = GUI.Button(new Rect(gaugeCenter.x - 52 * scale, gaugeRect.height - 18 * scale, 104 * scale, 15 * scale), "Settings", settingsButtonStyle);

            if (settingsButtonClicked) showSettings = !showSettings;

            GUI.DragWindow();

            if (windowPosition.x != lastPosition.x || windowPosition.y != lastPosition.y)
            {
                lastPosition.x = windowPosition.x;
                lastPosition.y = windowPosition.y;
                saveWindowPosition();
            }
        }

        private float scaleExponentially(float value, float exponent)
        {
            return (float)Math.Pow(Math.Abs(value), exponent) * Math.Sign(value);
        }

        private void onSettingsWindow(int id)
        {
            GUILayout.BeginHorizontal();
            bool last = useCDI;
            useCDI = GUILayout.Toggle(useCDI, "Display CDI Lines");
            if (useCDI != last) saveConfigSettings();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            last = drawRollDigits;
            drawRollDigits = GUILayout.Toggle(drawRollDigits, "Display Roll Degrees");
            if (drawRollDigits != last) saveConfigSettings();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("GUI Scale:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float lastScale = scale;
            scale = GUILayout.HorizontalSlider(scale, 0.4f, 3.0f);
            if (scale != lastScale) saveConfigSettings();
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        private void drawGaugeText(Rect gaugeRect)
        {
            float dstXpos = 96 * scale;
            float cvelXpos = 300 * scale;
            float yPos = 335 * scale;

            drawGlyphString(distanceToTarget.ToString("F1"), dstXpos, yPos);
            drawGlyphString(closureV.ToString("F"), cvelXpos, yPos);

            //GUI.Label(new Rect(dstXpos, yPos, 100f, 50f), distanceToTarget.ToString("F1"), labelStyle);
            //GUI.Label(new Rect(cvelXpos, yPos, 100f, 30f), closureV.ToString("F"), labelStyle);

            if (drawRollDigits)
            {
                GUI.DrawTexture(new Rect(271 * scale, 47 * scale, roll_label.width * scale, roll_label.height * scale), roll_label);

                //GUI.Label(new Rect(gaugeRect.width - (200 * scale), 40 * scale, 100f, 30f), orientationDeviation.z.ToString("F1"), rightAlignedStyle);
                float rDegXPos = 306 * scale;
                float rDegYPos = 41 * scale;
                drawGlyphString(orientationDeviation.z.ToString("F1"), rDegXPos, rDegYPos);
            }
        }

        private void drawGlyphString(String valueString, float x, float y)
        {
            char[] chars = valueString.ToCharArray();
            Rect sourceRect = new Rect();
            Rect destRect = new Rect();
            float cursorX = x;
            float cursorY = y;
            foreach (char c in chars)
            {
                int index = -1;
                if (c == '-') index = 10;
                else if (c == '.') index = 11;
                else
                {
                    index = int.Parse(c.ToString());
                }
                GlyphData glyph = glyphData[index];
                sourceRect.x = glyph.textureX;
                sourceRect.y = glyph.textureY;
                sourceRect.width = glyph.textureWidth;
                sourceRect.height = glyph.textureHeight;
                destRect.x = cursorX + glyph.xOffset * scale;
                destRect.y = cursorY + glyph.yOffset * scale;
                destRect.width = glyph.width * scale;
                destRect.height = glyph.height * scale;
                GUI.DrawTextureWithTexCoords(destRect, digits, sourceRect);
                cursorX += glyph.xAdvance * scale;
            }
        }

        private void drawVelocityVector(Rect gaugeRect)
        {
            float gaugeX, gaugeY;

            //Range-wrapping from old indicator behavior (which incorporated closure velocity component)
            //float mirror = 1f;
            //if (Math.Abs(relativeVelocity.x) <= 90) mirror = -1f;
            //gaugeX = mirror * wrapRange(relativeVelocity.x / 90f);
            //gaugeY = mirror * wrapRange(relativeVelocity.y / 90f);

            gaugeX = UnityEngine.Mathf.Clamp(transverseVelocity.x, -transverseVelocityRange, transverseVelocityRange) / transverseVelocityRange;
            gaugeY = UnityEngine.Mathf.Clamp(transverseVelocity.y, -transverseVelocityRange, transverseVelocityRange) / transverseVelocityRange;

            Texture2D velocityVectorTexture = prograde;
            if (Math.Abs(orientationDeviation.x) > 90f)
            {
                gaugeX *= -1;
                gaugeY *= -1;
                velocityVectorTexture = retrograde;
            }

            gaugeX = scaleExponentially(gaugeX, velocityVectorExponent);
            gaugeY = scaleExponentially(gaugeY, velocityVectorExponent);

            float scaledVelocityVectorSize = velocityVectorIconSize * scale;
            float scaledVelocityVectorHalfSize = scaledVelocityVectorSize * .5f;

            GUI.DrawTexture(new Rect(.5f * gaugeRect.width * (1 + gaugeX * visiblePortion) - scaledVelocityVectorHalfSize,
                                        .5f * gaugeRect.height * (1 + gaugeY * visiblePortion) - scaledVelocityVectorHalfSize,
                                        scaledVelocityVectorSize,
                                        scaledVelocityVectorSize),
                                        velocityVectorTexture);
        }

        private float wrapRange(float a)
        {
            return ((((a + 1f) % 2) + 2) % 2) - 1f;
        }

        private void drawCDI(Rect gaugeRect)
        {

            Color colorCDI = colorCDINormal;
            if (negativeOnBackHemisphere < 0) colorCDI = colorCDIReverse;

            float gaugeX = negativeOnBackHemisphere * wrapRange(translationDeviation.x / 90f);
            float gaugeY = negativeOnBackHemisphere * wrapRange(translationDeviation.y / 90f);

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

            gaugeX = scaleExponentially(gaugeX, exponent);
            gaugeY = scaleExponentially(gaugeY, exponent);

            float xVal, yVal;

            xVal = .5f * gaugeRect.width * (gaugeX * visiblePortion + 1);
            yVal = .5f * gaugeRect.height * (gaugeY * visiblePortion + 1);

            Drawing.DrawVerticalLine(xVal, 0, gaugeRect.height, 2f, colorCDI);
            Drawing.DrawHorizontalLine(0, yVal, gaugeRect.width, 2f, colorCDI);
        }

        //return signed angle in relation to normal's 2d plane
        private float AngleAroundNormal(Vector3 a, Vector3 b, Vector3 up)
        {
            return AngleSigned(Vector3.Cross(up, a), Vector3.Cross(up, b), up);
        }

        //-180 to 180 angle
        private float AngleSigned(Vector3 v1, Vector3 v2, Vector3 up)
        {
            if (Vector3.Dot(Vector3.Cross(v1, v2), up) < 0) //greater than 90 i.e v1 left of v2
                return -Vector3.Angle(v1, v2);
            return Vector3.Angle(v1, v2);
        }

        private static void saveWindowPosition()
        {
            config.SetValue("window_position", windowPosition);
            config.save();
        }

        private static void saveConfigSettings()
        {
            config.SetValue("show_cdi", useCDI);
            config.SetValue("show_rolldigits", drawRollDigits);
            config.SetValue("gui_scale", (double)scale);
            config.save();
        }

        public static void LoadPrefs()
        {
            config = PluginConfiguration.CreateForType<DockingPortAlignment>(null);
            config.load();

            scale = (float)config.GetValue<double>("gui_scale", 0.65);

            print("Scale equals " + scale);

            Rect defaultWindow = new Rect(Screen.width * .75f - (gaugeWidth * scale / 2f), Screen.height * .5f - (gaugeHeight * scale / 2f), gaugeWidth * scale, gaugeHeight * scale);
            windowPosition = config.GetValue<Rect>("window_position", defaultWindow);

            windowPosition = constrainToScreen(windowPosition);

            useCDI = config.GetValue<bool>("show_cdi", true);
            drawRollDigits = config.GetValue("show_rolldigits", true);

            saveWindowPosition();
            saveConfigSettings();
        }

        private static void OnDestroy()
        {
            dockingButton.Destroy();
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

            gaugeStyle = new GUIStyle(HighLogic.Skin.label);
            gaugeStyle.stretchWidth = true;
            gaugeStyle.normal.textColor = lightGrey;

            rightAlignedStyle = new GUIStyle(HighLogic.Skin.label);
            rightAlignedStyle.stretchWidth = true;
            rightAlignedStyle.normal.textColor = lightGrey;
            rightAlignedStyle.alignment = TextAnchor.UpperRight;

            settingsButtonStyle = new GUIStyle(HighLogic.Skin.button);
            settingsButtonStyle.padding = new RectOffset(1, 1, 1, 1);
            settingsButtonStyle.stretchHeight = true;
            settingsButtonStyle.stretchWidth = true;
            settingsButtonStyle.fontSize = 12;
            settingsButtonStyle.normal.textColor = lightGrey;

            settingsAreaStyle = new GUIStyle(HighLogic.Skin.window);
            settingsAreaStyle.padding = new RectOffset(5, 5, 5, 5);

            settingsToggleStyle = new GUIStyle(HighLogic.Skin.toggle);
            settingsToggleStyle.normal.textColor = lightGrey;

            hasInitializedStyles = true;
        }
    }
}