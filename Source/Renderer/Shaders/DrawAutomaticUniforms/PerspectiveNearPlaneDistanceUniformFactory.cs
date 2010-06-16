﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Renderer
{
    internal class PerspectiveNearPlaneDistanceUniformFactory : DrawAutomaticUniformFactory
    {
        #region PerspectiveNearPlaneDistanceUniformFactory Members

        public override string Name
        {
            get { return "og_perspectiveNearPlaneDistance"; }
        }

        public override DrawAutomaticUniform Create(Uniform uniform)
        {
            return new PerspectiveNearPlaneDistanceUniform(uniform);
        }

        #endregion
    }
}
