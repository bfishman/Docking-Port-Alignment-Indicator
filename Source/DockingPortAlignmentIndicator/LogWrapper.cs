/*
 *    LogWrapper.cs
 * 
 *    Copyright (C) 2019, Michael WERLE
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


using UnityEngine;

namespace NavyFish
{
/// <summary>
/// A small static class to wrap the UnityEngine.Debug logger.
/// </summary>
/// The main purpose of this wrapper is to prevent output logs in release
/// builds without the need to edit the code.
static class LogWrapper
{
    /// <summary>
    /// Prefix to each logging message.
    /// </summary>
    const string LogPrefix = "[DPAI.";

    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogD(object message)
    {
        Debug.Log(LogPrefix + "DBG] " + message);
    }

    public static void LogE(object message)
    {
        Debug.LogError(LogPrefix + "ERR] "  + message);
    }

    public static void LogW(object message)
    {
        Debug.LogWarning(LogPrefix + "WRN] "  + message);
    }

}
}
