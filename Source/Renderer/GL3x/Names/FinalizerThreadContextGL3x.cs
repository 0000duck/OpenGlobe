﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using OpenTK.Graphics;
using System;

namespace OpenGlobe.Renderer.GL3x
{
    internal static class FinalizerThreadContextGL3x
    {
        static FinalizerThreadContextGL3x()
        {
            _window = new NativeWindow();
            _context = new GraphicsContext(new GraphicsMode(32, 24, 8), _window.WindowInfo, 3, 2, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug);
        }

        public static void Initialize()
        {
        }

        public delegate void DisposeCallback(bool disposing);

        public static void RunFinalizer(DisposeCallback callback)
        {
            try
            {
                if (!_context.IsDisposed)
                {
                    _context.MakeCurrent(_window.WindowInfo);
                    try
                    {
                        callback(false);
                    }
                    finally
                    {
                        _context.MakeCurrent(null);
                    }
                }
            }
            catch
            {
            }
        }

        private static NativeWindow _window;
        private static GraphicsContext _context;
    }
}
