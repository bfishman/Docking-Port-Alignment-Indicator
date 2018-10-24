/*
 *    BitmapFont.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DockingPortAlignment
{
    public class BitmapFont
    {
        private Texture2D fontTexture;
        private string[] fontData;
        private int baseline;
        private float glyphTextureWidth;
        private float glyphTextureHeight;
        private GlyphData[] glyphs;
        private Rect destRect = new Rect();

        private static char[] spaceSeparator = new char[] { ' ' };
        private static Encoding defaultEncoding = Encoding.Unicode;

        public BitmapFont(Texture2D bmFontTexture, string[] bmFontData)
        {
            this.fontTexture = bmFontTexture;
            this.fontData = bmFontData;
            parseHeader();
            buildGlyphs();
        }

        private void parseHeader()
        {
            string[] parameters = fontData[1].Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
            baseline = int.Parse(parameters[2].Substring(5));
            glyphTextureWidth = int.Parse(parameters[3].Substring(7));
            glyphTextureHeight = int.Parse(parameters[4].Substring(7));    
        }

        private void buildGlyphs()
        {
            glyphs = new GlyphData[256];
            for (int i = 4; i < fontData.Length; i++ )
            {
                string[] data = fontData[i].Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
                
                int id = int.Parse(data[1].Substring(3));
                int x = int.Parse(data[2].Substring(2));
                int y = int.Parse(data[3].Substring(2));
                int width = int.Parse(data[4].Substring(6));
                int height = int.Parse(data[5].Substring(7));
                int xOff = int.Parse(data[6].Substring(8));
                int yOff = int.Parse(data[7].Substring(8));
                int xAdv = int.Parse(data[8].Substring(9));

                //Unity's texRect's origin is bottom left
                Rect textureCoordinates = new Rect(x / glyphTextureWidth, ((glyphTextureHeight - y - height) / glyphTextureHeight), width / glyphTextureWidth, height / glyphTextureHeight);

                glyphs[id] = new GlyphData(
                    textureCoordinates,
                    width,
                    height,
                    xOff, 
                    yOff, 
                    xAdv);
            }
        }

        public void drawString(String text, float x, float y, float scale)
        {
            //byte[] ascii = defaultEncoding.GetBytes(text);
            float cursorX = x;
            foreach (int id in text)
            {
                GlyphData glyph = getGlyphFromID(id);
                destRect.x = cursorX + glyph.xOffset * scale;
                destRect.y = y + glyph.yOffset * scale;
                destRect.width = glyph.width * scale;
                destRect.height = glyph.height * scale;
                GUI.DrawTextureWithTexCoords(destRect, fontTexture, glyph.srcRect);
                cursorX += glyph.xAdvance * scale;
            }
        }

        public void drawStringGraphics(String text, float x, float y, float scale, Color color)
        {
            //byte[] ascii = defaultEncoding.GetBytes(text);
            float cursorX = x;
            foreach (int id in text)
            {
                GlyphData glyph = getGlyphFromID(id);
                destRect.x = cursorX + glyph.xOffset * scale;
                destRect.y = y + glyph.yOffset * scale;
                destRect.width = glyph.width * scale;
                destRect.height = glyph.height * scale;

                Graphics.DrawTexture(destRect, fontTexture, glyph.srcRect, 0,0,0,0,color);
                cursorX += glyph.xAdvance * scale;
            }
        }

        public StringDimensions getStringDimensions(String text, float scale)
        {
            StringDimensions dim = new StringDimensions();
            dim.yOffset = baseline;
            dim.height = 0;
            if (text.Length == 0) return dim;
            GlyphData glyph;
            for(int i=0; i<(text.Length-1); i++)
            {
                glyph = getGlyphFromID(text[i]);
                dim.yOffset = Math.Min(glyph.yOffset, dim.yOffset);
                //dim.height = Math.Max(glyph.yOffset + glyph.height, dim.height);
                dim.width += glyph.xAdvance;
                dim.height = Math.Max(dim.height, glyph.height);
                //dim.yOffset = Math.Max(dim.yOffset, glyph.yOffset);
            }
            glyph = getGlyphFromID(text[text.Length - 1]);
            dim.yOffset = Math.Min(glyph.yOffset, dim.yOffset);
            //dim.height = Math.Max(glyph.yOffset + glyph.height, dim.height);
            dim.height = Math.Max(dim.height, glyph.height);
            dim.width += glyph.width;

            dim.width *= scale;
            dim.height *= scale;
            dim.yOffset *= scale;
            return dim;
        }

        public int getTextBaselineHeight()
        {
            return baseline;
        }

        private GlyphData getGlyphFromID(int id)
        {
            if (id > 255 || id < 0 || glyphs[id] == null)
            {
                //replace with "#"
                return glyphs[35];
            }
            else
            {
                return glyphs[id];
            }
        }

        public void debug(String str)
        {
            foreach (int id in str)
            {
                System.Console.Write("char= "+ (char)id+ "\tid= "+ id + "\t");

                GlyphData g = getGlyphFromID(id);
                if (g != null)
                {
                    System.Console.WriteLine(g.ToString());
                }
                else
                {
                    System.Console.WriteLine("No Glyph Found");
                }
            }
        }

        public struct StringDimensions
        {
            public float width, height, yOffset;
            public override string ToString()
            {
                return string.Format("W: {0} || H: {1} || yOff: {2}", width, height, yOffset);
            }
        }
    }
}
