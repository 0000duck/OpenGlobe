﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    internal class ModelViewPerspectiveProjectionMatrixUniform : DrawAutomaticUniform
    {
        public ModelViewPerspectiveProjectionMatrixUniform(Uniform uniform)
        {
            _uniform = uniform as Uniform<Matrix4>;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, SceneState sceneState)
        {
            _uniform.Value = Conversion.ToMatrix4(sceneState.ModelViewPerspectiveProjectionMatrix);
        }

        #endregion

        private Uniform<Matrix4> _uniform;
    }
}
