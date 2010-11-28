﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class Framebuffer : Disposable
    {
        public abstract ColorAttachments ColorAttachments { get; }
        public abstract Texture2D DepthAttachment { get; set; }
        public abstract Texture2D DepthStencilAttachment { get; set; }
    }
}
