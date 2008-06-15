using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pavel2.Framework {
    public static class ColorManagement {

        #region Fields

        private static List<ColorOGL> selectionColors;
        private static ColorOGL backgroundColor = new ColorOGL(Color.Black);
        private static ColorOGL axesColor = new ColorOGL(Color.LightGray);
        private static ColorOGL descriptionColor = new ColorOGL(Color.LightGray);

        #endregion

        #region Properties

        /// <value>Gets the color for descriptions</value>
        public static ColorOGL DescriptionColor { get { return ColorManagement.descriptionColor; } }

        /// <value>Gets the color for axes</value>
        public static ColorOGL AxesColor { get { return ColorManagement.axesColor; } }

        /// <value>Gets the color for the background</value>
        public static ColorOGL BackgroundColor { get { return backgroundColor; } }

        /// <value>Gets the color for unselected points</value>
        public static ColorOGL UnselectedColor {
            get {
                if (null == selectionColors) { InitializeSelectionColors(2); }
                return GetColor(0);
            }
        }

        /// <value>Gets the color for currently selected points</value>
        public static ColorOGL CurrentSelectionColor {
            get {
                if (null == selectionColors) { InitializeSelectionColors(2); }
                return GetColor(1);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the selectionColors.
        /// </summary>
        /// <param name="count">Total number of colors</param>
        private static void InitializeSelectionColors(int count) {
            selectionColors = new List<ColorOGL>();
            FillSelectionColors(count, true);
        }

        /// <summary>
        /// Returns a color at a position. If no color exists at this position
        /// the selectionColors list is expanded.
        /// </summary>
        /// <param name="position">Position of the Color.</param>
        /// <returns>ColorOGL</returns>
        public static ColorOGL GetColor(int position) {
            if (selectionColors == null) {
                InitializeSelectionColors(position + 1);
            }
            if (position >= selectionColors.Count) {
                FillSelectionColors(position + 1, false);
            }
            return selectionColors[position];
        }

        /// TODO: Change from precomputed array to inline computation???
        /// <summary>
        /// Fills the selectionColors list with colors, which are optimized
        /// for good readability. selectionColors will be filled up to the
        /// next number that isgreater or eaqual to count and a power of two
        /// </summary>
        /// <param name="count">Total number of colors</param>
        /// <param name="clearList">Recreate list by clearing it before refill</param>
        private static void FillSelectionColors(int count, bool clearList) {
            if (clearList || selectionColors.Count <= 2) {
                selectionColors.Clear();
                float unselected = 180f;    //blue-green
                float selected = 0f;      //red
                selectionColors.Add(HSVToRGB(unselected, 1, 0.78f));
                selectionColors.Add(HSVToRGB(selected, 1, 0.78f));
            }
            // The colorvalues are taken from HSV-Colorspace, starting with
            // angels 0° and 180° and iterating by bisecting each partition:
            // basis: 2, 3, 4, 5, ...
            float basis = (float)(Math.Ceiling(Math.Log((double)selectionColors.Count + 1.0, 2.0)));
            // 4, 8, 16, 32, ...
            int index = (int)Math.Pow(2.0, basis);
            // (1, 3), (1, 3, 5, 7), (1, 3, 5, 7, 9, 11, 13, 15), ...
            int i = (int)Math.Pow(2.0, basis - 1) - selectionColors.Count + 1;
            while (selectionColors.Count < count) {
                for (; i < index /*&& selectionColors.Count < count*/; i += 2) {
                    float h = (360.0f / index) * i;       //Values: 0, 180, 90, 270, 45, 135
                    float s = 0.5f + (((basis - 2f) * 0.25f) % 0.5f); // Values: 0.5, 0.75, 1.0
                    float v = 0.78f + (((basis - 2f) * 0.2f) % 0.2f);  // Values: 0.78, 0.98
                    selectionColors.Add(HSVToRGB(h, s, v));
                }
                index *= 2;     // Next Index
                basis++;        // Correxponding Base
                i = 1;          // Restart local position in this index
            }
        }

        /// <summary>
        /// Converts HSV Colors to RGB Colors.
        /// </summary>
        /// <param name="h">Hue (0 to 360)</param>
        /// <param name="s">Saturation (0 to 1)</param>
        /// <param name="v">Value (0 to 1)</param>
        /// <returns>OpenGL Color</returns>
        public static ColorOGL HSVToRGB(float h, float s, float v) {
            ColorOGL color = new ColorOGL();
            int i;
            float f, p, q, t;
            if (s == 0) {
                color.Color = Color.FromArgb(0, (int)(v * 255), (int)(v * 255), (int)(v * 255));
                return color;
            }
            h /= 60;
            i = (int)Math.Floor(h);
            f = h - i;
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));
            switch (i) {
                case 0:
                    color.Color = Color.FromArgb(0, (int)(v * 255), (int)(t * 255), (int)(p * 255));
                    break;
                case 1:
                    color.Color = Color.FromArgb(0, (int)(q * 255), (int)(v * 255), (int)(p * 255));
                    break;
                case 2:
                    color.Color = Color.FromArgb(0, (int)(p * 255), (int)(v * 255), (int)(t * 255));
                    break;
                case 3:
                    color.Color = Color.FromArgb(0, (int)(p * 255), (int)(q * 255), (int)(v * 255));
                    break;
                case 4:
                    color.Color = Color.FromArgb(0, (int)(t * 255), (int)(p * 255), (int)(v * 255));
                    break;
                default:
                    color.Color = Color.FromArgb(0, (int)(v * 255), (int)(p * 255), (int)(q * 255));
                    break;
            }
            return color;
        }

        #endregion

    }
}
