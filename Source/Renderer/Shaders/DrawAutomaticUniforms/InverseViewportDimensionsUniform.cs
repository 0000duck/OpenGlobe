﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class InverseViewportDimensionsUniform : DrawAutomaticUniform
    {
        public InverseViewportDimensionsUniform(Uniform uniform)
        {
            _uniform = (Uniform<Vector2F>)uniform;
        }

        #region DrawAutomaticUniform Members

        public override void Set(Context context, DrawState drawState, SceneState sceneState)
        {
            //
            // viewport.Bottom should really be used but Rectangle goes top to botom, not bottom to top.
            //
            Rectangle viewport = context.Viewport;
            _uniform.Value = new Vector2F(1.0f / (float)viewport.Width, 1.0f / (float)viewport.Height);
        }

        #endregion

        private Uniform<Vector2F> _uniform;
    }
}
