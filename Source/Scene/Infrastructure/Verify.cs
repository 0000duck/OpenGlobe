﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using MiniGlobe.Renderer;

namespace MiniGlobe.Scene
{
    internal static class Verify
    {
        public static void ThrowInvalidOperationIfNull(Texture2D texture, string memberName)
        {
            if (texture == null)
            {
                throw new InvalidOperationException(memberName);
            }
        }

        public static void ThrowIfNull(Context context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
        }

        public static void ThrowIfNull(SceneState sceneState)
        {
            if (sceneState == null)
            {
                throw new ArgumentNullException("sceneState");
            }
        }
    }
}