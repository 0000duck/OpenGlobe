﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal sealed class ShaderProgramHandleGL3x : IDisposable
    {
        public ShaderProgramHandleGL3x()
        {
            _value = GL.CreateProgram();
        }

        ~ShaderProgramHandleGL3x()
        {
            FinalizerThreadContextGL3x.RunFinalizer(Dispose);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int Value
        {
            get { return _value; }
        }

        private void Dispose(bool disposing)
        {
            if (_value != 0)
            {
                GL.DeleteProgram(_value);
                _value = 0;
            }
        }

        private int _value;
    }
}
