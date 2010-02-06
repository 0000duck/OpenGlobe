﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK.Graphics.OpenGL;
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL32
{
    internal class UniformFloatGL32 : Uniform<float>, ICleanable
    {
        internal UniformFloatGL32(int programHandle, string name, int location)
            : base(name, location, UniformType.Float)
        {
            Set(0);
        }

        private void Set(float value)
        {
            _value = value;
            _dirty = true;
        }

        #region Uniform<> Members

        public override float Value
        {
            set
            {
                if (_value != value)
                {
                    Set(value);
                }
            }

            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
        {
            if (_dirty)
            {
                GL.Uniform1(Location, _value);
                _dirty = false;
            }
        }

        #endregion

        private float _value;
        private bool _dirty;
    }
}
