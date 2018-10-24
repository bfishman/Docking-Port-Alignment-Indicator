using System;
using UnityEngine;
using KSP.IO;
using System.Collections.Generic;

namespace DockingPortAlignment
{
    class CustomButton
    {
        public Boolean Enabled;
        private Texture2D iconTexture;
        private Rect positionRect;
        private Rect tooltipRect = new Rect(0,0,300,30);
        private GUIStyle iconStyle;
        private GUIStyle tooltipStyle;
        private GUIStyle windowStyle;
        private GUIContent buttonContent;
        private GUIContent windowContent;
        private string tooltipText = "Show/Hide Docking Alignment Indicator";
        public CustomButton()
        {
            DockingPortAlignment.print("CustomButton Constructor");
            this.iconTexture = DockingPortAlignment.customToolbarIcon;
            positionRect = new Rect(200, 200, 0, 0);
            iconStyle = new GUIStyle();
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            windowStyle.padding = new RectOffset(6, 6, 6, 6);
            windowStyle.margin = new RectOffset(0, 0, 0, 0);
            windowStyle.stretchHeight = true;
            windowStyle.stretchWidth = true;
            windowStyle.fixedWidth = 0;
            windowStyle.fixedHeight = 0;
            buttonContent = new GUIContent(iconTexture, "customIconButton");
            windowContent = new GUIContent("", "customIconButton");

            tooltipStyle = new GUIStyle();
            tooltipStyle.normal.textColor = Color.white;
            tooltipStyle.alignment = TextAnchor.MiddleCenter;
            tooltipStyle.wordWrap = true;

            RenderingManager.AddToPostDrawQueue(3, OnDrawIconWindow);
            
        }

        private void OnDrawIconWindow()
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

            if (positionRect.Contains(mousePos))
            {
                tooltipRect.x = positionRect.center.x - .5f * tooltipRect.width;
                tooltipRect.y = positionRect.yMax + 1;
                GUI.Label(tooltipRect, tooltipText, tooltipStyle);
            }

            positionRect = GUILayout.Window(3, positionRect, drawIconWindowContents, windowContent, windowStyle);

        }

        private void drawIconWindowContents(int windowID)
        {
            Boolean previousState = GUI.enabled;
            if (!Enabled) GUI.enabled = false;

            if (GUILayout.Button(buttonContent, iconStyle, GUILayout.Width(24), GUILayout.Height(24)))
            {
                DockingPortAlignment.indicatorIsHidden = !DockingPortAlignment.indicatorIsHidden;
            }
            GUI.enabled = previousState;

            GUI.DragWindow();
        }
    }
}
