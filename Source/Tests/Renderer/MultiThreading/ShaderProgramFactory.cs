﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class ShaderProgramFactory : Disposable
    {
        public ShaderProgramFactory(GraphicsWindow window, string vertexShader, string fragmentShader)
        {
            _window = window;
            _vs = vertexShader;
            _fs = fragmentShader;
        }

        public void Create()
        {
            _window.MakeCurrent();

            _sp = Device.CreateShaderProgram(_vs, _fs);
            _fence = Device.CreateFence();
        }

        public ShaderProgram ShaderProgram
        {
            get { return _sp; }
        }

        public Fence Fence
        {
            get { return _fence; }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fence.Dispose();
                _sp.Dispose();
                _window.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private readonly GraphicsWindow _window;
        private readonly string _vs;
        private readonly string _fs;
        private ShaderProgram _sp;
        private Fence _fence;
    }
}
