﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class EarClippingTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null()
        {
            EarClipping.Triangulate(null);
        }
        
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Empty()
        {
            EarClipping.Triangulate(new List<Vector2D>());
        }

        [Test]
        public void Triangle()
        {
            IList<Vector2D> positions = new List<Vector2D>();
            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(1, 0));
            positions.Add(new Vector2D(1, 1));

            IndicesInt32 indices = EarClipping.Triangulate(positions);

            Assert.AreEqual(3, indices.Values.Count);
            Assert.AreEqual(0, indices.Values[0]);
            Assert.AreEqual(1, indices.Values[1]);
            Assert.AreEqual(2, indices.Values[2]);
        }

        [Test]
        public void Square()
        {
            IList<Vector2D> positions = new List<Vector2D>();
            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(1, 0));
            positions.Add(new Vector2D(1, 1));
            positions.Add(new Vector2D(0, 1));

            IndicesInt32 indices = EarClipping.Triangulate(positions);

            Assert.AreEqual(6, indices.Values.Count);
            Assert.AreEqual(0, indices.Values[0]);
            Assert.AreEqual(1, indices.Values[1]);
            Assert.AreEqual(2, indices.Values[2]);
            Assert.AreEqual(0, indices.Values[3]);
            Assert.AreEqual(2, indices.Values[4]);
            Assert.AreEqual(3, indices.Values[5]);
        }

        [Test]
        public void SimpleConcave()
        {
            IList<Vector2D> positions = new List<Vector2D>();

            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(2, 0));
            positions.Add(new Vector2D(2, 2));
            positions.Add(new Vector2D(1, 0.25));
            positions.Add(new Vector2D(0, 2));

            IndicesInt32 indices = EarClipping.Triangulate(positions);

            Assert.AreEqual(9, indices.Values.Count);
            Assert.AreEqual(1, indices.Values[0]);
            Assert.AreEqual(2, indices.Values[1]);
            Assert.AreEqual(3, indices.Values[2]);

            Assert.AreEqual(3, indices.Values[3]);
            Assert.AreEqual(4, indices.Values[4]);
            Assert.AreEqual(0, indices.Values[5]);

            Assert.AreEqual(0, indices.Values[6]);
            Assert.AreEqual(1, indices.Values[7]);
            Assert.AreEqual(3, indices.Values[8]);
        }
    }
}
