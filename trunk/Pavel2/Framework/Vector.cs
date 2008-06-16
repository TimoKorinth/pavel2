using System;

namespace Pavel2.GUI {

    #region Vector

    /// <summary>
    /// An integer Vector of arbitrary dimension.
    /// </summary>
    public class Vector : IEquatable<Vector> {

        #region Fields

        /// <value>The actual numerical values</value>
        public readonly int[] intValues;

        #endregion

        #region Properties

        /// <value>Gets the first value of the vector (X-Coordinate)</value>
        public int X { get { return intValues[0]; } }

        /// <value>Gets the second value of the vector (Y-Coordinate)</value>
        public int Y { get { return intValues[1]; } }

        /// <value>Gets the third value of the vector (Z-Coordinate)</value>
        public int Z { get { return intValues[2]; } }

        /// <value>Gets the fourth value of the vector (Alpha-value)</value>
        public int A { get { return intValues[3]; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new Vector with a dimension as large as the passed array.
        /// </summary>
        /// <param name="values">Values for the Vector</param>
        public Vector(params int[] values) { intValues = values; }

        #endregion

        #region IEquatable<Vector> Member

        /// <summary>
        /// Returns true if this Vector equals the one given by <paramref name="other"/>.
        /// </summary>
        /// <param name="other">Vector to compare this one to</param>
        /// <returns>True if this Vector equals the one given by <paramref name="other"/></returns>
        public bool Equals(Vector other) {
            if (this.intValues.Length != other.intValues.Length) return false;
            for (int i = 0; i < intValues.Length; i++) {
                if (this.intValues[i] != other.intValues[i]) return false;
            }
            return true;
        }

        #endregion
    }

    #endregion

    #region VectorF

    /// <summary>
    /// A float Vector of arbitrary dimension.
    /// </summary>
    public struct VectorF {

        #region Fields

        /// <value>The actual numerical values </value>
        public float X, Y, Z, W;
        /// <value>Treedimensional VectorF with values 1</value>
        public static VectorF Unit = new VectorF(1f, 1f, 1f);

        #endregion

        #region Properties

        /// <value> Gets an alias for W or sets it </value>
        public float C { get { return W; } set { W = value; } }

        /// <value> Gets an array of the X-, Y- and Z-values of the VectorF</value>
        public float[] XYZ { get { return new float[] { X, Y, Z }; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the VectorF with <paramref name="x"/>, <paramref name="y"/> and <paramref name="z"/>.
        /// The W-value is set to 1.
        /// </summary>
        /// <param name="x">X-value</param>
        /// <param name="y">Y-value</param>
        /// <param name="z">Z-value</param>
        public VectorF(float x, float y, float z) {
            X = x; Y = y; Z = z; W = 1;
        }

        /// <summary>
        /// Initializes the VectorF with <paramref name="x"/>, <paramref name="y"/>,
        /// <paramref name="z"/> and <paramref name="w"/>.
        /// </summary>
        /// <param name="x">X-value</param>
        /// <param name="y">Y-value</param>
        /// <param name="z">Z-value</param>
        /// <param name="w">W-value</param>
        public VectorF(float x, float y, float z, float w) {
            X = x; Y = y; Z = z; W = w;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Adds two VectorFs.
        /// </summary>
        /// <param name="a">First VectorF</param>
        /// <param name="b">Second VectorF</param>
        /// <returns>Sum of <paramref name="a"/> and <paramref name="b"/></returns>
        public static VectorF operator +(VectorF a, VectorF b) {
            return new VectorF(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }

        /// <summary>
        /// Substracts two VectorFs.
        /// </summary>
        /// <param name="a">First VectorF</param>
        /// <param name="b">Second VectorF</param>
        /// <returns><paramref name="a"/> minus <paramref name="b"/></returns>
        public static VectorF operator -(VectorF a, VectorF b) {
            return new VectorF(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        }

        /// <summary>
        /// Element-wise multiplication.
        /// </summary>
        /// <param name="a">First VectorF</param>
        /// <param name="b">Second VectorF</param>
        /// <returns>Element-wise product of <paramref name="a"/> and <paramref name="b"/></returns>
        public static VectorF operator *(VectorF a, VectorF b) {
            return new VectorF(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
        }

        /// <summary>
        /// Element-wise division
        /// x = a.x / b.x etc
        /// </summary>
        /// <param name="a">First VectorF</param>
        /// <param name="b">Second VectorF</param>
        /// <returns>Element-wise quotient of <paramref name="a"/> and <paramref name="b"/></returns>
        public static VectorF operator /(VectorF a, VectorF b) {
            return new VectorF(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
        }

        /// <summary>
        /// Scales VectorF <paramref name="a"/> by <paramref name="b"/>
        /// </summary>
        /// <param name="a">VectorF</param>
        /// <param name="b">Factor</param>
        /// <returns>Product of <paramref name="a"/> and <paramref name="b"/></returns>
        public static VectorF operator *(VectorF a, float b) {
            return new VectorF(a.X * b, a.Y * b, a.Z * b, a.W * b);
        }

        /// <summary>
        /// Scales VectorF <paramref name="a"/> by <paramref name="b"/>
        /// </summary>
        /// <param name="b">Factor</param>
        /// <param name="a">VectorF</param>
        /// <returns>Product of <paramref name="a"/> and <paramref name="b"/></returns>
        public static VectorF operator *(float b, VectorF a) {
            return new VectorF(a.X * b, a.Y * b, a.Z * b, a.W * b);
        }

        /// <summary>
        /// Divides every element of <paramref name="a"/> by <paramref name="b"/>.
        /// </summary>
        /// <param name="a">VectorF</param>
        /// <param name="b">Factor</param>
        /// <returns>Product of <paramref name="a"/> and the inverse of <paramref name="b"/></returns>
        public static VectorF operator /(VectorF a, float b) {
            return a * (1 / b);
        }

        #endregion
    }

    #endregion
}
