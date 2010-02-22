﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace MiniGlobe.Renderer
{
    internal class HighResolutionSnapScaleUniformFactory : DrawAutomaticUniformFactory
    {
        #region HighResolutionSnapScaleUniformFactory Members

        public override string Name
        {
            get { return "mg_highResolutionSnapScale"; }
        }

        public override DrawAutomaticUniform Create(Uniform uniform)
        {
            return new HighResolutionSnapScaleUniform(uniform);
        }

        #endregion
    }
}
