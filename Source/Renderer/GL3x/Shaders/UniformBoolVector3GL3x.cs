﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using OpenTK.Graphics.OpenGL;
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL3x
{
    internal class UniformBoolVector3GL3x : Uniform<Vector3b>, ICleanable
    {
        internal UniformBoolVector3GL3x(string name, int location, ICleanableObserver observer)
            : base(name, location, UniformType.BoolVector3)
        {
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        private void Set(Vector3b value)
        {
            if (!_dirty && (_value != value))
            {
                _dirty = true;
                _observer.NotifyDirty(this);
            }

            _value = value;
        }

        #region Uniform<> Members

        public override Vector3b Value
        {
            set { Set(value); }
            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
        {
            GL.Uniform3(Location, _value.X ? 1 : 0, _value.Y ? 1 : 0, _value.Z ? 1 : 0);
            _dirty = false;
        }

        #endregion

        private Vector3b _value;
        private bool _dirty;
        private ICleanableObserver _observer;
    }
}
