﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Renderer
{
    internal class PerspectiveNearPlaneDistanceUniform : DrawAutomaticUniform
    {
        public PerspectiveNearPlaneDistanceUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<float>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, SceneState sceneState)
        {
            _uniform.Value = (float)sceneState.Camera.PerspectiveNearPlaneDistance;
        }

        #endregion

        private Uniform<float> _uniform;
    }
}
