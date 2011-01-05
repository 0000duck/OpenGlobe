#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class Matrix4DTests
    {
        [Test]
        public void Construct0()
        {
            Matrix4D m = new Matrix4D();

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(0.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct1()
        {
            Matrix4D m = new Matrix4D(1.0);

            for (int i = 0; i < m.NumberOfComponents; ++i)
            {
                Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0], 1e-14);
            }
        }

        [Test]
        public void Construct2()
        {
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(2.0, m.Column1Row0);
            Assert.AreEqual(3.0, m.Column2Row0);
            Assert.AreEqual(4.0, m.Column3Row0);
            Assert.AreEqual(5.0, m.Column0Row1);
            Assert.AreEqual(6.0, m.Column1Row1);
            Assert.AreEqual(7.0, m.Column2Row1);
            Assert.AreEqual(8.0, m.Column3Row1);
            Assert.AreEqual(9.0, m.Column0Row2);
            Assert.AreEqual(10.0, m.Column1Row2);
            Assert.AreEqual(11.0, m.Column2Row2);
            Assert.AreEqual(12.0, m.Column3Row2);
            Assert.AreEqual(13.0, m.Column0Row3);
            Assert.AreEqual(14.0, m.Column1Row3);
            Assert.AreEqual(15.0, m.Column2Row3);
            Assert.AreEqual(16.0, m.Column3Row3);

            Assert.AreEqual(1.0, m.ReadOnlyColumnMajorValues[0]);
            Assert.AreEqual(5.0, m.ReadOnlyColumnMajorValues[1]);
            Assert.AreEqual(9.0, m.ReadOnlyColumnMajorValues[2]);
            Assert.AreEqual(13.0, m.ReadOnlyColumnMajorValues[3]);
            Assert.AreEqual(2.0, m.ReadOnlyColumnMajorValues[4]);
            Assert.AreEqual(6.0, m.ReadOnlyColumnMajorValues[5]);
            Assert.AreEqual(10.0, m.ReadOnlyColumnMajorValues[6]);
            Assert.AreEqual(14.0, m.ReadOnlyColumnMajorValues[7]);
            Assert.AreEqual(3.0, m.ReadOnlyColumnMajorValues[8]);
            Assert.AreEqual(7.0, m.ReadOnlyColumnMajorValues[9]);
            Assert.AreEqual(11.0, m.ReadOnlyColumnMajorValues[10]);
            Assert.AreEqual(15.0, m.ReadOnlyColumnMajorValues[11]);
            Assert.AreEqual(4.0, m.ReadOnlyColumnMajorValues[12]);
            Assert.AreEqual(8.0, m.ReadOnlyColumnMajorValues[13]);
            Assert.AreEqual(12.0, m.ReadOnlyColumnMajorValues[14]);
            Assert.AreEqual(16.0, m.ReadOnlyColumnMajorValues[15]);
        }

        [Test]
        public void Construct3()
        {
            Matrix3D rotation = new Matrix3D(
                1.0, 4.0, 7.0,
                2.0, 5.0, 8.0,
                3.0, 6.0, 9.0);
            Vector3D translation = new Vector3D(10.0, 11.0, 12.0);

            Matrix4D m = new Matrix4D(rotation, translation);

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(4.0, m.Column1Row0);
            Assert.AreEqual(7.0, m.Column2Row0);
            Assert.AreEqual(10.0, m.Column3Row0);
            Assert.AreEqual(2.0, m.Column0Row1);
            Assert.AreEqual(5.0, m.Column1Row1);
            Assert.AreEqual(8.0, m.Column2Row1);
            Assert.AreEqual(11.0, m.Column3Row1);
            Assert.AreEqual(3.0, m.Column0Row2);
            Assert.AreEqual(6.0, m.Column1Row2);
            Assert.AreEqual(9.0, m.Column2Row2);
            Assert.AreEqual(12.0, m.Column3Row2);
            Assert.AreEqual(0.0, m.Column0Row3);
            Assert.AreEqual(0.0, m.Column1Row3);
            Assert.AreEqual(0.0, m.Column2Row3);
            Assert.AreEqual(1.0, m.Column3Row3);
        }

        [Test]
        public void DoubleToFloat()
        {
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);

            Matrix4F mf = m.ToMatrix4F();

            Assert.AreEqual(1.0f, mf.Column0Row0, 1e-7);
            Assert.AreEqual(2.0f, mf.Column1Row0, 1e-7);
            Assert.AreEqual(3.0f, mf.Column2Row0, 1e-7);
            Assert.AreEqual(4.0f, mf.Column3Row0, 1e-7);
            Assert.AreEqual(5.0f, mf.Column0Row1, 1e-7);
            Assert.AreEqual(6.0f, mf.Column1Row1, 1e-7);
            Assert.AreEqual(7.0f, mf.Column2Row1, 1e-7);
            Assert.AreEqual(8.0f, mf.Column3Row1, 1e-7);
            Assert.AreEqual(9.0f, mf.Column0Row2, 1e-7);
            Assert.AreEqual(10.0f, mf.Column1Row2, 1e-7);
            Assert.AreEqual(11.0f, mf.Column2Row2, 1e-7);
            Assert.AreEqual(12.0f, mf.Column3Row2, 1e-7);
            Assert.AreEqual(13.0f, mf.Column0Row3, 1e-7);
            Assert.AreEqual(14.0f, mf.Column1Row3, 1e-7);
            Assert.AreEqual(15.0f, mf.Column2Row3, 1e-7);
            Assert.AreEqual(16.0f, mf.Column3Row3, 1e-7);
        }

        [Test]
        public void Identity()
        {
            Matrix4D m = Matrix4D.Identity;

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(0.0, m.Column1Row0);
            Assert.AreEqual(0.0, m.Column2Row0);
            Assert.AreEqual(0.0, m.Column3Row0);
            Assert.AreEqual(0.0, m.Column0Row1);
            Assert.AreEqual(1.0, m.Column1Row1);
            Assert.AreEqual(0.0, m.Column2Row1);
            Assert.AreEqual(0.0, m.Column3Row1);
            Assert.AreEqual(0.0, m.Column0Row2);
            Assert.AreEqual(0.0, m.Column1Row2);
            Assert.AreEqual(1.0, m.Column2Row2);
            Assert.AreEqual(0.0, m.Column3Row2);
            Assert.AreEqual(0.0, m.Column0Row3);
            Assert.AreEqual(0.0, m.Column1Row3);
            Assert.AreEqual(0.0, m.Column2Row3);
            Assert.AreEqual(1.0, m.Column3Row3);
        }

        [Test]
        public void Transpose()
        {
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0).Transpose();

            Assert.AreEqual(1.0, m.Column0Row0);
            Assert.AreEqual(5.0, m.Column1Row0);
            Assert.AreEqual(9.0, m.Column2Row0);
            Assert.AreEqual(13.0, m.Column3Row0);
            Assert.AreEqual(2.0, m.Column0Row1);
            Assert.AreEqual(6.0, m.Column1Row1);
            Assert.AreEqual(10.0, m.Column2Row1);
            Assert.AreEqual(14.0, m.Column3Row1);
            Assert.AreEqual(3.0, m.Column0Row2);
            Assert.AreEqual(7.0, m.Column1Row2);
            Assert.AreEqual(11.0, m.Column2Row2);
            Assert.AreEqual(15.0, m.Column3Row2);
            Assert.AreEqual(4.0, m.Column0Row3);
            Assert.AreEqual(8.0, m.Column1Row3);
            Assert.AreEqual(12.0, m.Column2Row3);
            Assert.AreEqual(16.0, m.Column3Row3);
        }

        [Test]
        public void Inverse()
        {
            Matrix4D m = new Matrix4D(Matrix3D.Identity, Vector3D.Zero);
            Matrix4D mInverse = m.Inverse();

            Assert.AreEqual(Matrix4D.Identity, mInverse * m);
        }

        [Test]
        public void Inverse2()
        {
            Matrix4D m = new Matrix4D(Matrix3D.Identity, new Vector3D(1.0, 2.0, 3.0));
            Matrix4D mInverse = m.Inverse();

            Assert.AreEqual(Matrix4D.Identity, mInverse * m);
        }

        [Test]
        public void Inverse3()
        {
            Matrix4D m = new Matrix4D(
                0.72,  0.70, 0.00,  0.00, 
               -0.40,  0.41, 0.82,  0.00,
                0.57, -0.59, 0.57, -3.86,
                0.00,  0.00, 0.00,  1.00);
            Matrix4D mInverse = m.Inverse();

            Assert.IsTrue(Matrix4D.Identity.EqualsEpsilon(mInverse * m, 0.0000001));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Inverse4()
        {
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            Matrix4D mInverse = m.Inverse();
        }

        [Test]
        public void InverseTransformation()
        {
            Matrix4D m = new Matrix4D(Matrix3D.Identity, Vector3D.Zero);
            Matrix4D mInverse = m.InverseTransformation();

            Vector4D v = new Vector4D(1.0, 2.0, 3.0, 1.0);
            Vector4D vPrime = m * v;
            Vector4D vv = mInverse * vPrime;

            Assert.AreEqual(v, vv);
        }

        [Test]
        public void InverseTransformation2()
        {
            Matrix3D rotation = new Matrix3D(
                1.0, 0.0, 0.0,
                0.0, 0.0, 1.0,
                0.0, 1.0, 0.0);
            Vector3D translation = new Vector3D(10.0, 20, 30.0);

            Matrix4D m = new Matrix4D(rotation, translation);
            Matrix4D mInverse = m.InverseTransformation();

            Vector4D v = new Vector4D(1.0, 2.0, 3.0, 1.0);
            Vector4D vPrime = m * v;
            Vector4D vv = mInverse * vPrime;

            Assert.AreEqual(v, vv);
        }

        [Test]
        public void InverseTransformation3()
        {
            Matrix3D rotation = new Matrix3D(
                1.0, 0.0, 0.0,
                0.0, 0.0, 1.0,
                0.0, 1.0, 0.0);
            Vector3D translation = new Vector3D(1.0, 2, 3.0);

            Matrix4D m = new Matrix4D(rotation, translation);
            Matrix4D mInverse = m.InverseTransformation();

            Assert.AreEqual(Matrix4D.Identity, mInverse * m);
        }

        [Test]
        public void TransformationGetters()
        {
            Matrix3D rotation = new Matrix3D(
                1.0, 4.0, 7.0,
                2.0, 5.0, 8.0,
                3.0, 6.0, 9.0);
            Vector3D translation = new Vector3D(10.0, 11.0, 12.0);

            Matrix4D m = new Matrix4D(rotation, translation);
            Assert.AreEqual(rotation, m.Rotation);
            Assert.AreEqual(rotation.Transpose(), m.RotationTranspose());
            Assert.AreEqual(translation, m.Translation);
        }

        [Test]
        public void Multiply0()
        {
            Matrix4D zero = new Matrix4D(0.0);
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            Assert.AreEqual(zero, zero * m);
        }

        [Test]
        public void Multiply1()
        {
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            Assert.AreEqual(m, Matrix4D.Identity * m);
        }

        [Test]
        public void Multiply2()
        {
            Matrix4D left = new Matrix4D(1.0);
            Matrix4D right = new Matrix4D(1.0);
            Matrix4D result = new Matrix4D(
                4.0, 4.0, 4.0, 4.0,
                4.0, 4.0, 4.0, 4.0,
                4.0, 4.0, 4.0, 4.0,
                4.0, 4.0, 4.0, 4.0);
            Assert.AreEqual(result, left * right);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Multiply3()
        {
            Matrix4D m = null * new Matrix4D();
			GC.KeepAlive(m);	// Fix MonoDevelop 2.2.1 CS0219 Warning
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Multiply4()
        {
            Matrix4D m = new Matrix4D() * null;
			GC.KeepAlive(m);	// Fix MonoDevelop 2.2.1 CS0219 Warning
        }

        [Test]
        public void MultiplyVector0()
        {
            Matrix4D zero = new Matrix4D(0.0);
            Vector4D v = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Assert.AreEqual(Vector4D.Zero, zero * v);
        }

        [Test]
        public void MultiplyVector1()
        {
            Vector4D v = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Assert.AreEqual(v, Matrix4D.Identity * v);
        }

        [Test]
        public void MultiplyVector2()
        {
            Matrix4D m = new Matrix4D(
                1.0, 0.0, 2.0, 0.0,
                0.0, 1.0, 0.0, 2.0,
                2.0, 0.0, 1.0, 0.0,
                0.0, 2.0, 0.0, 1.0);
            Vector4D v = new Vector4D(1.0, 2.0, 3.0, 4.0);
            Vector4D result = new Vector4D(7.0, 10.0, 5.0, 8.0);
            Assert.AreEqual(result, m * v);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyVector3()
        {
            Matrix4D m = null;
            Vector4D result = m * new Vector4D();
			GC.KeepAlive(result);	// Fix MonoDevelop 2.2.1 CS0219 Warning
        }

        [Test]
        public void Equals()
        {
            Matrix4D a = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            Matrix4D b = new Matrix4D(0.0);
            Matrix4D c = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);

            Assert.IsTrue(a.Equals(c));
            Assert.IsTrue(c.Equals(a));
            Assert.IsTrue(a == c);
            Assert.IsTrue(c == a);
            Assert.IsFalse(c != a);
            Assert.IsFalse(c != a);
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
            Assert.IsFalse(a == b);
            Assert.IsFalse(b == a);
            Assert.IsTrue(a != b);
            Assert.IsTrue(b != a);

            object objA = a;
            object objB = b;
            object objC = c;

            Assert.IsTrue(a.Equals(objA));
            Assert.IsTrue(a.Equals(objC));
            Assert.IsFalse(a.Equals(objB));

            Assert.IsTrue(objA.Equals(objC));
            Assert.IsFalse(objA.Equals(objB));

            Assert.IsFalse(a.Equals(null));
            Assert.IsFalse(a.Equals(5));
        }

        [Test]
        public void EqualsEpsilon()
        {
            Matrix4D m = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            Matrix4D m2 = new Matrix4D(
                1.1, 2.1, 3.1, 4.1,
                5.1, 6.1, 7.1, 8.1,
                9.1, 10.1, 11.1, 12.1,
                13.1, 14.1, 15.1, 16.1);

            Assert.IsTrue(m.EqualsEpsilon(m2, 0.2));
            Assert.IsFalse(m.EqualsEpsilon(m2, 0.05));
        }

        [Test]
        public void TestGetHashCode()
        {
            Matrix4D a = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            Matrix4D b = new Matrix4D(0.0);
            Matrix4D c = new Matrix4D(
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);

            Assert.AreEqual(a.GetHashCode(), c.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
