﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Renderer
{
    internal class PerspectiveNearPlaneDistanceUniform : DrawAutomaticUniform
    {
        public PerspectiveNearPlaneDistanceUniform(Uniform uniform)
        {
            _uniform = (Uniform<float>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            _uniform.Value = (float)sceneState.Camera.PerspectiveNearPlaneDistance;
        }

        #endregion

        private Uniform<float> _uniform;
    }
}
