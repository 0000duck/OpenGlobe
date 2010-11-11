﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class UniformFloatMatrix24GL3x : Uniform<Matrix24<float>>, ICleanable
    {
        internal UniformFloatMatrix24GL3x(string name, int location, ICleanableObserver observer)
            : base(name, UniformType.FloatMatrix24)
        {
            _location = location;
            _value = new Matrix24<float>();
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        #region Uniform<> Members

        public override Matrix24<float> Value
        {
            set
            {
                if (!_dirty && (_value != value))
                {
                    _dirty = true;
                    _observer.NotifyDirty(this);
                }

                _value = value;
            }

            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
        {
            GL.UniformMatrix2x4(_location, 1, false, _value.ReadOnlyColumnMajorValues);
            _dirty = false;
        }

        #endregion

        private int _location;
        private Matrix24<float> _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}
