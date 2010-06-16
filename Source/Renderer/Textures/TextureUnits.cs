﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections;

namespace OpenGlobe.Renderer
{
    public abstract class TextureUnits
    {
        public abstract TextureUnit this[int index] { get; }
        public abstract int Count { get; }
        public abstract IEnumerator GetEnumerator();
    }
}
