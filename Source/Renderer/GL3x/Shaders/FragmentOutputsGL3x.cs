﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL3x
{
    internal class FragmentOutputsGL3x : FragmentOutputs
    {
        public FragmentOutputsGL3x(int program)
        {
            _program = program;
        }

        #region FragmentOutputs Members

        public override int this[string index]
        {
            get 
            {
                int i = GL.GetFragDataLocation(_program, index);

                if (i == -1)
                {
                    throw new KeyNotFoundException(index);
                }

                return i;
            }
        }

        #endregion

        private int _program;
    }
}
