﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using OpenTK;

namespace MiniGlobe.Core.Geometry
{
    [TestFixture]
    public class EllipsoidTests
    {
        [Test]
        public void Construct()
        {
            Ellipsoid ellipsoid = new Ellipsoid(new Vector3D(1, 2, 3));
            Assert.AreEqual(1, ellipsoid.Radii.X);
            Assert.AreEqual(2, ellipsoid.Radii.Y);
            Assert.AreEqual(3, ellipsoid.Radii.Z);

            Ellipsoid ellipsoid2 = new Ellipsoid(4, 5, 6);
            Assert.AreEqual(new Vector3D(4, 5, 6), ellipsoid2.Radii);

            Ellipsoid sphere = Ellipsoid.UnitSphere;
            Assert.IsTrue(sphere.OneOverRadii.Equals((new Vector3D(1, 1, 1))));
            Assert.IsTrue(sphere.OneOverRadiiSquared.Equals((new Vector3D(1, 1, 1))));
        }

        [Test]
        public void DeticSurfaceNormal()
        {
            Assert.IsTrue(new Vector3D(1, 0, 0).Equals(Ellipsoid.UnitSphere.DeticSurfaceNormal(new Vector3D(1, 0, 0))));
            Assert.IsTrue(new Vector3D(0, 0, 1).Equals(Ellipsoid.UnitSphere.DeticSurfaceNormal(new Vector3D(0, 0, 1))));
        }

        [Test]
        public void CentricSurfaceNormal()
        {
            Vector3D v = new Vector3D(1, 2, 3);
            Assert.AreEqual(v.Normalize(), Ellipsoid.CentricSurfaceNormal(v));
        }

        [Test]
        public void SphereIntersectionsTwoFromOutside()
        {
            Ellipsoid unitSphere = Ellipsoid.UnitSphere;
            
            double[] intersections = unitSphere.Intersections(new Vector3D(2.0, 0.0, 0.0), new Vector3D(-1.0, 0.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 2.0, 0.0), new Vector3D(0.0, -1.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.0, 2.0), new Vector3D(0.0, 0.0, -1.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 1.0, 0.0), new Vector3D(-1.0, 0.0, 0.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(-2.0, 0.0, 0.0), new Vector3D(1.0, 0.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, -2.0, 0.0), new Vector3D(0.0, 1.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.0, -2.0), new Vector3D(0.0, 0.0, 1.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);
            Assert.AreEqual(3.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(-1.0, -1.0, 0.0), new Vector3D(1.0, 0.0, 0.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(1.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(-2.0, 0.0, 0.0), new Vector3D(-1.0, 0.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-3.0, intersections[0], 1e-14);
            Assert.AreEqual(-1.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, -2.0, 0.0), new Vector3D(0.0, -1.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-3.0, intersections[0], 1e-14);
            Assert.AreEqual(-1.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.0, -2.0), new Vector3D(0.0, 0.0, -1.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-3.0, intersections[0], 1e-14);
            Assert.AreEqual(-1.0, intersections[1], 1e-14);
        }

        [Test]
        public void SphereIntersectionsTwoFromInside()
        {
            Ellipsoid unitSphere = Ellipsoid.UnitSphere;

            double[] intersections;

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-1.0, intersections[0], 1e-14);
            Assert.AreEqual(1.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.5, 0.0), new Vector3D(0.0, 1.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-1.5, intersections[0], 1e-14);
            Assert.AreEqual(0.5, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(0.0, 0.5, 0.0), new Vector3D(0.0, -1.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-0.5, intersections[0], 1e-14);
            Assert.AreEqual(1.5, intersections[1], 1e-14);
        }

        [Test]
        public void SphereIntersectionsFromEdge()
        {
            Ellipsoid unitSphere = Ellipsoid.UnitSphere;

            double[] intersections;

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(0.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, 1.0, 0.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(0.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(1.0, 0.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(-2.0, intersections[0], 1e-14);
            Assert.AreEqual(0.0, intersections[1], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, 0.0, -1.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(0.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, -1.0, 0.0));
            Assert.AreEqual(1, intersections.Length);
            Assert.AreEqual(0.0, intersections[0], 1e-14);

            intersections = unitSphere.Intersections(new Vector3D(1.0, 0.0, 0.0), new Vector3D(-1.0, 0.0, 0.0));
            Assert.AreEqual(2, intersections.Length);
            Assert.AreEqual(0.0, intersections[0], 1e-14);
            Assert.AreEqual(2.0, intersections[1], 1e-14);
        }

        [Test]
        public void SphereIntersectionsNoIntersection()
        {
            Ellipsoid unitSphere = Ellipsoid.UnitSphere;

            double[] intersections;

            intersections = unitSphere.Intersections(new Vector3D(2.0, 0.0, 0.0), new Vector3D(0.0, 0.0, 1.0));
            Assert.AreEqual(0, intersections.Length);

            intersections = unitSphere.Intersections(new Vector3D(2.0, 0.0, 0.0), new Vector3D(0.0, 0.0, -1.0));
            Assert.AreEqual(0, intersections.Length);

            intersections = unitSphere.Intersections(new Vector3D(2.0, 0.0, 0.0), new Vector3D(0.0, 1.0, 0.0));
            Assert.AreEqual(0, intersections.Length);

            intersections = unitSphere.Intersections(new Vector3D(2.0, 0.0, 0.0), new Vector3D(0.0, -1.0, 0.0));
            Assert.AreEqual(0, intersections.Length);
        }

        [Test]
        public void ToVector3D()
        {
            Ellipsoid ellipsoid = new Ellipsoid(1, 1, 0.7);

            Assert.IsTrue(Vector3D.UnitX.EqualsEpsilon(ellipsoid.ToVector3D(new Geodetic3D(0, 0, 0)), 1e-10));
            Assert.IsTrue(Vector3D.UnitY.EqualsEpsilon(ellipsoid.ToVector3D(new Geodetic3D(Trig.ToRadians(90), 0, 0)), 1e-10));
            Assert.IsTrue(new Vector3D(0, 0, 0.7).EqualsEpsilon(ellipsoid.ToVector3D(new Geodetic3D(0, Trig.ToRadians(90), 0)), 1e-10));
        }
    }
}
