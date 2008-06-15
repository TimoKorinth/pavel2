using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace Pavel2.Framework {
    public class ColorOGL {

        #region Fields

        private Color color;
        private float[] rgbaFloat;
        private String description;

        #endregion

        #region Properties

        /// <value>Gets the alpha value of the ColorOGL as a float or sets it</value>
        [Browsable(false)]
        public float A {
            get { return rgbaFloat[3]; }
            private set { rgbaFloat[3] = value; color = colorFromFloat(rgbaFloat); }
        }

        /// <value>Gets the red value of the ColorOGL as a float or sets it</value>
        [Browsable(false)]
        public float R {
            get { return rgbaFloat[0]; }
            private set { rgbaFloat[0] = value; color = colorFromFloat(rgbaFloat); }
        }

        /// <value>Gets the green value of the ColorOGL as a float or sets it</value>
        [Browsable(false)]
        public float G {
            get { return rgbaFloat[1]; }
            private set { rgbaFloat[1] = value; color = colorFromFloat(rgbaFloat); }
        }

        /// <value>Gets the blue value of the ColorOGL as a float or sets it</value>
        [Browsable(false)]
        public float B {
            get { return rgbaFloat[2]; }
            private set { rgbaFloat[2] = value; color = colorFromFloat(rgbaFloat); }
        }

        /// <value>Gets a float-array of the RGB values of the ColorOGL</value>
        [Browsable(false)]
        public float[] RGB {
            get { return new float[] { rgbaFloat[0], rgbaFloat[1], rgbaFloat[2] }; }
        }

        /// <value>Gets a float-array of the RGBA values of the ColorOGL</value>
        [Browsable(false)]
        public float[] RGBA { get { return rgbaFloat; } }

        /// <summary>
        /// Gets a float-array of the RGB values of the ColorOGL, with an alpha value mixed in on-the-fly
        /// </summary>
        /// <param name="alpha">The desired alpha-value</param>
        /// <returns>A float-array of the RGB values of the ColorOGL, with an alpha value mixed in on-the-fly</returns>
        public float[] RGBwithA(float alpha) {
            float[] retval = new float[4];
            rgbaFloat.CopyTo(retval,0);
            retval[3] = alpha;
            return retval;
        }

        /// <value>Gets the ColorOGL as Color or sets it</value>
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotifyParentProperty(true)]
        public Color Color {
            get { return color; }
            set {
                color = value;
                rgbaFloat = floatFromColor(color);
            }
        }

        /// <value>Gets a description of the ColorOGL or sets it</value>
        public String Description {
            get { return description; }
            set { description = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Sets the color to white and the description to "(none)".
        /// </summary>
        public ColorOGL() {
            Color = Color.White;
            Description = "(none)";
        }

        /// <summary>
        /// Sets the color to <paramref name="color"/> and the description to the Name of <paramref name="color"/>.
        /// </summary>
        /// <param name="color">Color to be set</param>
        public ColorOGL(Color color) {
            Color = color;
            Description = color.Name;
        }

        /// <summary>
        /// Sets the color to <paramref name="color"/> and the description to <paramref name="desc"/>.
        /// </summary>
        /// <param name="color">Color to be set</param>
        /// <param name="desc">Description for the color</param>
        public ColorOGL(Color color, String desc) {
            Color = color;
            Description = desc;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the float-values of the given Color.
        /// </summary>
        /// <param name="color">Given Color</param>
        /// <returns>Float-array of the transformed RGBA-values</returns>
        private static float[] floatFromColor(Color color) {
            float[] rgbaFloat = new float[4];
            rgbaFloat[0] = color.R / 255.0f;
            rgbaFloat[1] = color.G / 255.0f;
            rgbaFloat[2] = color.B / 255.0f;
            rgbaFloat[3] = color.A / 255.0f;
            return rgbaFloat;
        }

        /// <summary>
        /// Calculates the Color from the given float-values.
        /// </summary>
        /// <param name="colors">Float-array of RGBA-values</param>
        /// <returns>The transformed Color</returns>
        private static Color colorFromFloat(float[] colors) {
            byte r = (byte) (colors[0] * 255);
            byte g = (byte) (colors[1] * 255);
            byte b = (byte) (colors[2] * 255);
            byte a = (byte) (colors[3] * 255);
            return Color.FromArgb(a,r,g,b);
        }

        /// <summary>
        /// Returns the color converted to System.Drawing.Color.
        /// </summary>
        /// <returns>The color as System.Drawing.Color</returns>
        public Color ToColor() {
            return Color.FromArgb((int)(rgbaFloat[0] * 255), (int)(rgbaFloat[1] * 255), (int)(rgbaFloat[2] * 255));
        }

        /// <summary>
        /// Calculates the linear interpolation between two Colors,
        /// this color being dist=0, the other being dist=1
        /// </summary>
        /// <param name="other">Other Color</param>
        /// <param name="dist">Keep this between 0 and 1 please</param>
        /// <returns>Interpolated Color</returns>
        public ColorOGL Interpolate(ColorOGL other, float dist) {
            ColorOGL c = new ColorOGL();
            for (int i = 0; i < 4; i++)
                c.rgbaFloat[i] = this.rgbaFloat[i] + (other.rgbaFloat[i] - this.rgbaFloat[i]) * dist;
            c.color = colorFromFloat(c.rgbaFloat);
            return c;
        }

        /// <summary>
        /// Calculates a color table with values interpolated between
        /// <paramref name="first"/> and <paramref name="second"/>
        /// </summary>
        /// <param name="first">Start-ColorOGL</param>
        /// <param name="second">End-ColorOGL</param>
        /// <returns>An array of interpolated ColorOGLs</returns>
        public static ColorOGL[] InterpolationArray(ColorOGL first, ColorOGL second) {
            ColorOGL[] colorTable = new ColorOGL[byte.MaxValue + 1];
            for (int i = 0; i <= byte.MaxValue; i++) {
                colorTable[i] = first.Interpolate(second, (float)i / byte.MaxValue);
            }
            return colorTable;
        }

        /// <summary>
        /// Calculates a color table with values interpolated between
        /// <paramref name="first"/> and <paramref name="second"/>
        /// </summary>
        /// <param name="first">Start-System.Drawing.Color</param>
        /// <param name="second">End-System.Drawing.Color</param>
        /// <returns>An array of interpolated ColorOGLs</returns>
        public static ColorOGL[] InterpolationArray(Color first, Color second) {
            ColorOGL firstOGL  = new ColorOGL(first);
            ColorOGL secondOGL = new ColorOGL(second);
            ColorOGL[] colorTable = new ColorOGL[byte.MaxValue + 1];
            for (int i = 0; i <= byte.MaxValue; i++) {
                colorTable[i] = firstOGL.Interpolate(secondOGL, (float)i / byte.MaxValue);
            }
            return colorTable;
        }

        /// <summary>
        /// Calculates a color table with values interpolated between
        /// <paramref name="first"/>, <paramref name="second"/> and <paramref name="third"/>.
        /// </summary>
        /// <param name="first">First System.Drawing.Color</param>
        /// <param name="second">Second System.Drawing.Color</param>
        /// <param name="third">Third System.Drawing.Color</param>
        /// <returns>An array of interpolated ColorOGLs</returns>
        public static ColorOGL[] InterpolationArray(Color first, Color second, Color third) {
            ColorOGL firstOGL = new ColorOGL(first);
            ColorOGL secondOGL = new ColorOGL(second);
            ColorOGL thirdOGL = new ColorOGL(third);
            ColorOGL[] colorTable = new ColorOGL[short.MaxValue + 1];
            for (int i = 0; i <= short.MaxValue/2; i++) {
                colorTable[i] = firstOGL.Interpolate(secondOGL, (float)(i*2) / short.MaxValue);
            }
            for (int i = short.MaxValue / 2; i <= short.MaxValue ; i++) {
                colorTable[i] = secondOGL.Interpolate(thirdOGL, (float)((i*2-short.MaxValue*0.5) / short.MaxValue));
            }
            return colorTable;
        }

        /// <summary>
        /// Overrides the ToString method to return the description of this ColorOGL.
        /// </summary>
        /// <returns>Description of this ColorOGL</returns>
        public override string ToString() {
            return Description;
        }

        #endregion
    }
}
