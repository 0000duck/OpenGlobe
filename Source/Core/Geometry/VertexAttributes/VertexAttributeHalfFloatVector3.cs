﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;

namespace MiniGlobe.Core.Geometry
{
    public class VertexAttributeHalfFloatVector3 : VertexAttribute<Vector3h>
    {
        public VertexAttributeHalfFloatVector3(string name)
            : base(name, VertexAttributeType.HalfFloatVector3)
        {
        }

        public VertexAttributeHalfFloatVector3(string name, int capacity)
            : base(name, VertexAttributeType.HalfFloatVector3, capacity)
        {
        }
    }
}
