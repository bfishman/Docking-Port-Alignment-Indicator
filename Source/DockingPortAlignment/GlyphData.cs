/*
 *    GlyphData.cs
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
using System.Text;
using UnityEngine;

namespace DockingPortAlignment
{
    public class GlyphData
    {

        public readonly int width, height, xOffset, yOffset, xAdvance;
        public readonly Rect srcRect;

        public GlyphData(Rect textureCoordinates, int width, int height, int xOffset, int yOffset, int xAdvance)
        {
            this.srcRect = textureCoordinates;

            this.width = width;
            this.height = height;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.xAdvance = xAdvance;
        }

        public override string ToString()
        {
            return String.Format("SrcRect: {0}\tWidth={1}\tHeight={2}\txOff={3}\tyOff={4}\txAdv={5}", srcRect.ToString(), width, height, xOffset, yOffset, xAdvance);
        }
    }
}