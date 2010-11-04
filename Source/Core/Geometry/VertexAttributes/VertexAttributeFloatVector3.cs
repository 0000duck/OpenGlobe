﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Core
{
    public class VertexAttributeFloatVector3 : VertexAttribute<Vector3S>
    {
        public VertexAttributeFloatVector3(string name)
            : base(name, VertexAttributeType.FloatVector3)
        {
        }

        public VertexAttributeFloatVector3(string name, int capacity)
            : base(name, VertexAttributeType.FloatVector3, capacity)
        {
        }
    }
}
