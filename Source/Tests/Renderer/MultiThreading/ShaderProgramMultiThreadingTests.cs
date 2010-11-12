﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Threading;
using System.Drawing;
using NUnit.Framework;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer.Multithreading
{
    [TestFixture]
    public class ShaderProgramMultithreadingTests
    {
        /// <summary>
        /// Creates the rendering context, then creates a shader program on a
        /// different thread.  The shader program is used to render one point.
        /// </summary>
        [Test]
        public void CreateShaderProgram()
        {
            using (var threadWindow = Device.CreateWindow(1, 1))
            using (var window = Device.CreateWindow(1, 1))
            using (var factory = new ShaderProgramFactory(threadWindow, ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader()))
            {
                Thread t = new Thread(factory.Create);
                t.Start();
                t.Join();

                ///////////////////////////////////////////////////////////////////

                using (FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context))
                using (VertexArray va = TestUtility.CreateVertexArray(window.Context, factory.ShaderProgram.VertexAttributes["position"].Location))
                {
                    window.Context.FrameBuffer = frameBuffer;
                    window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), factory.ShaderProgram, va), new SceneState());

                    TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);
                }
            }
        }

        /// <summary>
        /// Creates the rendering context, then creates two shader programs on two 
        /// different threads ran one after the other.  The shader programs are 
        /// used to render one point.
        /// </summary>
        [Test]
        public void CreateShaderProgramSequential()
        {
            using (var thread0Window = Device.CreateWindow(1, 1))
            using (var thread1Window = Device.CreateWindow(1, 1))
            using (var window = Device.CreateWindow(1, 1))
            using (ShaderProgramFactory factory0 = new ShaderProgramFactory(thread0Window, ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader()))
            using (ShaderProgramFactory factory1 = new ShaderProgramFactory(thread1Window, ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader()))
            {
                Thread t0 = new Thread(factory0.Create);
                t0.Start();
                t0.Join();

                Thread t1 = new Thread(factory1.Create);
                t1.Start();
                t1.Join();

                using (FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context))
                using (VertexArray va = TestUtility.CreateVertexArray(window.Context, factory0.ShaderProgram.VertexAttributes["position"].Location))
                {
                    window.Context.FrameBuffer = frameBuffer;
                    window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), factory0.ShaderProgram, va), new SceneState());
                    TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

                    window.Context.Clear(new ClearState());
                    window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), factory1.ShaderProgram, va), new SceneState());
                    TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);
                }
            }
        }

        /// <summary>
        /// Creates the rendering context, then creates two shader programs on two 
        /// different threads ran in parallel.  The shader programs are used to
        /// render one point.
        /// </summary>
        [Test]
        public void CreateShaderProgramParallel()
        {
            using (var thread0Window = Device.CreateWindow(1, 1))
            using (var thread1Window = Device.CreateWindow(1, 1))
            using (var window = Device.CreateWindow(1, 1))
            using (ShaderProgramFactory factory0 = new ShaderProgramFactory(thread0Window, ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader()))
            using (ShaderProgramFactory factory1 = new ShaderProgramFactory(thread1Window, ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader()))
            {
                Thread t0 = new Thread(factory0.Create);
                t0.Start();

                Thread t1 = new Thread(factory1.Create);
                t1.Start();

                t0.Join();
                t1.Join();

                using (FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context))
                using (VertexArray va = TestUtility.CreateVertexArray(window.Context, factory0.ShaderProgram.VertexAttributes["position"].Location))
                {
                    window.Context.FrameBuffer = frameBuffer;
                    window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), factory0.ShaderProgram, va), new SceneState());
                    TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

                    window.Context.Clear(new ClearState());
                    window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), factory1.ShaderProgram, va), new SceneState());
                    TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);
                }
            }
        }
    }
}
