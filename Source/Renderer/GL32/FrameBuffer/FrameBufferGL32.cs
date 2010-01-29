﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Diagnostics;
using MiniGlobe.Renderer;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer.GL32
{
    internal class FrameBufferGL32 : FrameBuffer
    {
        public FrameBufferGL32()
        {
            GL.GenFramebuffers(1, out _handle);
            _colorAttachments = new ColorAttachmentsGL32();
        }

        internal void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
        }

        internal static void UnBind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        internal void Clean()
        {
            if (_colorAttachments.Dirty)
            {
                ColorAttachmentGL32[] colorAttachments = _colorAttachments.Attachments;

                for (int i = 0; i < colorAttachments.Length; ++i)
                {
                    if (colorAttachments[i].Dirty)
                    {
                        Attach(FramebufferAttachment.ColorAttachment0 + i, colorAttachments[i].Texture);
                        colorAttachments[i].Dirty = false;
                    }
                }

                _colorAttachments.Dirty = false;
            }

            if (_dirtyFlags != DirtyFlags.None)
            {
                if ((_dirtyFlags & DirtyFlags.DepthAttachment) == DirtyFlags.DepthAttachment)
                {
                    Attach(FramebufferAttachment.DepthAttachment, _depthAttachment);
                }

                if ((_dirtyFlags & DirtyFlags.DepthStencilAttachment) == DirtyFlags.DepthStencilAttachment)
                {
                    Attach(FramebufferAttachment.DepthStencilAttachment, _depthStencilAttachment);
                }

                _dirtyFlags = DirtyFlags.None;
            }
        }

        #region FrameBuffer Members

        public override ColorAttachments ColorAttachments
        {
            get { return _colorAttachments; }
        }

        public override Texture2D DepthAttachment
        {
            get { return _depthAttachment; }

            set
            {
                Debug.Assert(value == null || value.Description.DepthRenderable);

                _depthAttachment = value;
                _dirtyFlags |= DirtyFlags.DepthAttachment;
            }
        }

        public override Texture2D DepthStencilAttachment
        {
            get { return _depthStencilAttachment; }

            set
            {
                Debug.Assert(value == null || value.Description.DepthStencilRenderable);

                _depthStencilAttachment = value;
                _dirtyFlags |= DirtyFlags.DepthStencilAttachment;
            }
        }

        #endregion

        internal static void Attach(FramebufferAttachment attachPoint, Texture2D texture)
        {
            if (texture != null)
            {
                // TODO:  Mipmap level
                Texture2DGL32 textureGL = texture as Texture2DGL32;
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachPoint, textureGL.Handle, 0);
            }
            else
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachPoint, 0, 0);
            }

        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteFramebuffers(1, ref _handle);
            }
            base.Dispose(disposing);
        }

        #endregion

        [Flags]
        private enum DirtyFlags
        {
            None = 0,
            DepthAttachment = 1,
            DepthStencilAttachment = 2
        }

        private int _handle;
        private ColorAttachmentsGL32 _colorAttachments;
        private Texture2D _depthAttachment;
        private Texture2D _depthStencilAttachment;
        private DirtyFlags _dirtyFlags;
    }
}
