﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;

namespace OpenGlobe.Core.Geometry
{
    public class VertexAttributeRGBA : VertexAttribute<byte>
    {
        public VertexAttributeRGBA(string name)
            : base(name, VertexAttributeType.UnsignedByte)
        {
        }

        public VertexAttributeRGBA(string name, int capacity)
            : base(name, VertexAttributeType.UnsignedByte, capacity * 4)
        {
            if (capacity > int.MaxValue / 4)
            {
                throw new ArgumentOutOfRangeException("capacity", "capacity causes int overflow.");
            }
        }

        public void AddColor(Color color)
        {
            Values.Add(color.R);
            Values.Add(color.G);
            Values.Add(color.B);
            Values.Add(color.A);
        }
    }
}
