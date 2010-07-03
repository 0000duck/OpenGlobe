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
    internal class CameraEyeUniform : DrawAutomaticUniform
    {
        public CameraEyeUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<Vector3S>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = sceneState.Camera.Eye.ToVector3S();
        }

        #endregion

        private Uniform<Vector3S> _uniform;
    }
}
