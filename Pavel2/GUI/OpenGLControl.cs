using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Tao.Platform.Windows;
using System.Runtime.InteropServices;
using Tao.OpenGl;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using Pavel2.Framework;
using Tao.FreeGlut;

namespace Pavel2.GUI {
    /// <summary>
    /// Various Control elements for OpenGL.
    /// </summary>
    public abstract class OpenGLControl : Control {

        #region Fields
        private Font baseFont = new Font("Arial", 12);
        private IntPtr deviceContext = IntPtr.Zero;
        private IntPtr renderContext = IntPtr.Zero;
        private int fontbase;
        private Gdi.GLYPHMETRICSFLOAT[] gmf = new Gdi.GLYPHMETRICSFLOAT[256];

        /// <value>
        /// Setting this to true or false controls whether the glOrtho
        /// cuboid stays quadratic in the xy plane or adjusts itself to compensate
        /// for different aspect ratios of the viewport.
        /// keepAspect = true results in the cuboid adjusting, keeping aspect ratios in the viewport.
        /// </value>
        public bool keepAspect = true;

        /// <summary>
        /// Half-height of the glOrtho clipping cube
        /// </summary>
        protected const float halfHeight = 0.8f;

        protected float halfEyeDistance = 0.02f;

        protected bool stereoMode = false;

        /// <summary>
        /// Enumeration to determine from which position and in which direction to render
        /// LeftEye: View of left eye in stereo mode
        /// RightEye: View of left eye in stereo mode
        /// Picking: Intermediate view for picking in stereo mode
        /// </summary>
        public enum StereoEye { LeftEye, RightEye, Picking };

        #endregion

        #region Properties

        #region Window Aspect Properties

        /// <value>
        /// Gets the aspect ratio of this control.
        /// Depending on whether keepAspect is set, returns the actual aspect ratio (keepAspect=true), or 1
        /// </value>
        public float WindowAspect {
            get { return keepAspect ? (float)this.Width / this.Height : 1f; }
        }

        /// <value>Gets the OpenGLControls half height</value>
        public float HalfHeight { get { return halfHeight; } }

        /// <value>Gets the OpenGLControls half width</value>
        public float HalfWidth { get { return halfHeight * WindowAspect; } }

        /// <value>
        /// If windowAspect is less than 1, halfWidth is less than halfHeight.
        /// That can lead to unwanted cropping at the left and right image borders.
        /// To fix this, Halfwidth has to be scaled up. To keep the aspect ratio,
        /// HalfHeight has to be scaled up as well. AspectCap provides the scaling factor for this.
        /// </value>
        public float AspectCap {
            get { return WindowAspect < 1 ? HalfHeight / HalfWidth : 1; }
        }

        /// <summary>
        /// Gets the capped HalfHeight. See AspectCap Property
        /// </summary>
        public float HalfHeightCapped {
            get { return HalfHeight * AspectCap; }
        }

        /// <summary>
        /// Gets the capped HalfWidth. See AspectCap Property
        /// </summary>
        public float HalfWidthCapped {
            get { return HalfWidth * AspectCap; }
        }

        /// <summary>
        /// Enables or disables the stereo projection
        /// </summary>
        public bool StereoMode {
            get { return stereoMode; }
            set { stereoMode = value; }
        }
        #endregion

        /// <value>Gets the baseFont or set it</value>
        public Font BaseFont {
            get { return baseFont; }
            set {
                baseFont = value;
                BuildFont();
                RenderScene();
                SwapBuffers();
            }
        }

        /// <summary>
        /// Gets or sets the EyeDistance for Stereomode
        /// </summary>
        public float EyeDistance {
            get { return halfEyeDistance * 2; }
            set { halfEyeDistance = value / 2f; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Excecuted only once, initialises GLUT
        /// </summary>
        static OpenGLControl() {
            //Init OpenGl via glut
            Glut.glutInit();
        }

        /// <summary>
        /// Initializes the OpenGLControl.
        /// </summary>
        public OpenGLControl() {
            this.InitStyles();
            this.InitContexts();
            this.InitOpenGL();
        }

        #endregion

        #region Methods

        #region Setup Functions

        protected virtual void InitOpenGL() { }

        protected abstract void SetupModelViewMatrixOperations();

        /// <summary>
        /// Initializes the styles.
        /// </summary>
        private void InitStyles() {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, false);
            this.SetStyle(ControlStyles.Opaque, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Sets up the viewport.
        /// </summary>
        private void SetupViewPort() {
            Gl.glViewport(0, 0, this.Width, this.Height);
        }

        /// <summary>
        /// Sets up a projection matrix.
        /// </summary>
        /// <remarks>Does not alter the current matrix mode</remarks>
        /// <param name="loadIdentityFirst">Set to true if you want LoadIdentity to be executed on
        /// the projection Matrix before the Projection Matrix is set</param>
        protected void SetupProjection(bool loadIdentityFirst) {
            Gl.glPushAttrib(Gl.GL_TRANSFORM_BIT);
            Gl.glMatrixMode(Gl.GL_PROJECTION);

            if (loadIdentityFirst) Gl.glLoadIdentity();

            Gl.glOrtho(-HalfWidthCapped,
                        HalfWidthCapped,
                       -HalfHeightCapped,
                        HalfHeightCapped,
                       -1000,
                        1000);

            Gl.glPopAttrib();
        }

        /// <summary>
        /// Setd up a flat projection matrix for drawing in 2D
        /// </summary>
        /// <remarks>You still have to adjust the modelview accordingly by loading the identity matrix. Does not alter the current matrix mode</remarks>
        /// <param name="loadIdentityFirst">Set to true if you want LoadIdentity to be executed on
        /// the projection Matrix before the Camera is set</param>
        /// <param name="absolutePixelMode">If this is false, 0 represents the lower/left border of the drawing area
        /// 1 the upper/right border, paying respect to the keepAspect setting.
        /// If you set this to true, the drawing coordinates match the control's width/height
        /// in pixel, this.Width being the right, this,Height the top border. keepAspect has no effect in this mode.</param>
        /// <param name="flipVertical">If you set this to true, the projection is flipped vertical, allowing for Windows-based
        /// coordinates (0=top) instead of OpenGL based (0=bottom)</param>
        protected void SetupProjectionFlat(bool loadIdentityFirst, bool absolutePixelMode, bool flipVertical) {
            Gl.glPushAttrib(Gl.GL_TRANSFORM_BIT);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            if (loadIdentityFirst)
                Gl.glLoadIdentity();
            if (flipVertical) {
                if (absolutePixelMode)
                    Gl.glOrtho(0, this.Width, this.Height, 0, -100, +100);
                else
                    Gl.glOrtho(0, WindowAspect * AspectCap, AspectCap, 0, -100, +100);
            } else {
                if (absolutePixelMode)
                    Gl.glOrtho(0, this.Width, 0, this.Height, -100, +100);
                else
                    Gl.glOrtho(0, WindowAspect * AspectCap, 0, AspectCap, -100, +100);
            }
            Gl.glPopAttrib();
        }

        /// <summary>
        /// Sets up the modelview matrix
        /// </summary>
        /// <remarks>Does not change the current matrix mode</remarks>
        /// <param name="loadIdentityFirst">Set to true if you want LoadIdentity to be executed on
        /// the modelview Matrix before the Camera is set</param>
        protected void SetupModelView(bool loadIdentityFirst) {
            Gl.glPushAttrib(Gl.GL_TRANSFORM_BIT);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);

            if (loadIdentityFirst)
                Gl.glLoadIdentity();

            SetupModelViewMatrixOperations();

            Gl.glPopAttrib();
        }

        /// <summary>
        /// Sets up the projection matrix for stereo viewing or picking
        /// </summary>
        /// <param name="mode">Selects matrix creation for left eye, right eye or picking</param>
        protected void SetupProjectionStereo(StereoEye mode) {
            Gl.glPushAttrib(Gl.GL_TRANSFORM_BIT);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            //Prevent a loaded Pickmatrix from being overwritten:
            //TODO: Bad bad BAD Design! Making assumptions about things outside of the functions scope. No dinner for you!
            if (mode != StereoEye.Picking) { Gl.glLoadIdentity(); }
            double zNear = 0.5;
            double zFar = 1000;

            float viewPaneCorrection = 0.4f;
            float focus = (float)(zNear / (2 * halfHeight));

            //Create asymmetric viewing-frustrums
            double left = viewPaneCorrection * (-HalfWidthCapped);
            double right = viewPaneCorrection * (HalfWidthCapped);
            double bottom = viewPaneCorrection * (-HalfHeightCapped);
            double top = viewPaneCorrection * (HalfHeightCapped);

            if (mode == StereoEye.LeftEye) {
                left = left + halfEyeDistance * focus;
                right = right + halfEyeDistance * focus;
            } else if (mode == StereoEye.RightEye) {
                left = left - halfEyeDistance * focus;
                right = right - halfEyeDistance * focus;
            }

            //Projection
            Gl.glFrustum(left, right, bottom, top, zNear, zFar);

            //Eye Position
            Gl.glTranslatef(0, 0, -(2 * halfHeight));
            if (mode == StereoEye.LeftEye) {
                Gl.glTranslatef(+halfEyeDistance, 0, 0);
            } else if (mode == StereoEye.RightEye) {
                Gl.glTranslatef(-halfEyeDistance, 0, 0);
            }
            Gl.glPopAttrib();
        }

        /// <summary>
        /// Initialize the PickingMatrix
        ///
        /// Sets the MatrixMode to Projection,
        /// creates a picking region for the given coordinates and
        /// calls SetupProjection
        /// </summary>
        /// <param name="x">x-Coordinate (Window based, left is 0) of the picking region's center</param>
        /// <param name="y">y-Coordinate (Window based, top is 0) of the picking region's center</param>
        /// <param name="w">Width of the Picking Rectangle</param>
        /// <param name="h">Height of the Picking Rectangle</param>
        protected void InitializePickingMatrix(int x, int y, int w, int h) {
            int[] viewport = new int[4];

            //Extract viewport
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            // create picking region near cursor location
            Glu.gluPickMatrix(x, (viewport[3] - y), w, h, viewport);

            if (!this.stereoMode) {
                SetupProjection(false);
            } else {
                SetupProjectionStereo(StereoEye.Picking);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates and sets the pixel format and creates and connects the deviceContext and renderContext.
        /// </summary>
        private void InitContexts() {
            int selectedPixelFormat;

            //Make sure the handle for this control has been created
            if (this.Handle == IntPtr.Zero) {
                throw new Exception("InitContexts: The control's window handle has not been created!");
            }

            //Setup pixel format
            Gdi.PIXELFORMATDESCRIPTOR pixelFormat = new Gdi.PIXELFORMATDESCRIPTOR();
            pixelFormat.nSize = (short)Marshal.SizeOf(pixelFormat);                     //Größe in Byte der Struktur
            pixelFormat.nVersion = 1;                                                   //Versionsnummer
            pixelFormat.dwFlags = Gdi.PFD_DRAW_TO_WINDOW | Gdi.PFD_SUPPORT_OPENGL |     //Kennzeichen
                Gdi.PFD_DOUBLEBUFFER;
            pixelFormat.iPixelType = (byte)Gdi.PFD_TYPE_RGBA;                           //Echtfarb- bzw. Farbindexwerte
            pixelFormat.cColorBits = 32;                                                //Anzahl der Farbbits pro Pixel; 4,8,16,24,32 = Summe von cRedBits+cGreenBits+cBlueBits
            pixelFormat.cRedBits = 0;                                                   //Anzahl Bits pro Pixel für Rotanteil
            pixelFormat.cRedShift = 0;                                                  //Verschiebung der Rotbits in der Farbe
            pixelFormat.cGreenBits = 0;                                                 //Anzahl Bits pro Pixel für Grünanteil
            pixelFormat.cGreenShift = 0;                                                //Verschiebung der Grünbits in der Farbe
            pixelFormat.cBlueBits = 0;                                                  //Anzahl Bits pro Pixel für Blauanteil
            pixelFormat.cBlueShift = 0;                                                 //Verschiebung der Blaubits in der Farbe
            pixelFormat.cAlphaBits = 0;                                                 //nicht in der generischen Version
            pixelFormat.cAlphaShift = 0;                                                //nicht in der generischen Version
            pixelFormat.cAccumBits = 0;                                                 //Anzahl Bits pro Pixel im Akkumulationsbuffer = Summe der folgenden vier Angaben
            pixelFormat.cAccumRedBits = 0;                                              //BpP für Rot
            pixelFormat.cAccumGreenBits = 0;                                            //BpP für Grün
            pixelFormat.cAccumBlueBits = 0;                                             //BpP für Blau
            pixelFormat.cAccumAlphaBits = 0;                                            //BpP für Alpha-Anteil
            pixelFormat.cDepthBits = 16;                                                //16 oder 32
            pixelFormat.cStencilBits = 0;                                               //BpP im Stencilbuffer
            pixelFormat.cAuxBuffers = 0;                                                //nicht in der generischen Version
            pixelFormat.iLayerType = (byte)Gdi.PFD_MAIN_PLANE;                          //PFD_MAIN_PLANE
            pixelFormat.bReserved = 0;                                                  // = 0
            pixelFormat.dwLayerMask = 0;                                                //nicht in der generischen Version
            pixelFormat.dwVisibleMask = 0;                                              //nicht in der generischen Version
            pixelFormat.dwDamageMask = 0;                                               //nicht in der generischen Version

            //Create device context
            this.deviceContext = User.GetDC(this.Handle);
            if (this.deviceContext == IntPtr.Zero) {
                MessageBox.Show("InitContexts: Unable to create an OpenGL device context!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }

            //Choose the Pixel Format that is the closest to our pixelFormat
            selectedPixelFormat = Gdi.ChoosePixelFormat(this.deviceContext, ref pixelFormat);

            //Make sure the requested pixel format is available
            if (selectedPixelFormat == 0) {
                MessageBox.Show("InitContexts: Unable to find a suitable pixel format!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }

            //Sets the selected Pixel Format
            if (!Gdi.SetPixelFormat(this.deviceContext, selectedPixelFormat, ref pixelFormat)) {
                MessageBox.Show("InitContexts: Unable to set the requested pixel format!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }

            //Create rendering context
            this.renderContext = Wgl.wglCreateContext(this.deviceContext);
            if (this.renderContext == IntPtr.Zero) {
                MessageBox.Show("InitContexts: Unable to create an OpenGL rendering context!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }

            this.MakeCurrentContext();
            this.BuildFont();
        }

        /// <summary>
        /// Connects the deviceContext with the renderContext.
        /// </summary>
        public void MakeCurrentContext() {
            if (!Wgl.wglMakeCurrent(this.deviceContext, this.renderContext)) {
                MessageBox.Show("MakeCurrentContext: Unable to activate this control's OpenGL rendering context!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Deletes the deviceContext and renderContext.
        /// </summary>
        private void DestroyContexts() { //TODO In die Dispose Methode/Destruktor
            if (this.renderContext != IntPtr.Zero) {
                Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                Wgl.wglDeleteContext(this.renderContext);
                this.renderContext = IntPtr.Zero;
            }

            if (this.deviceContext != IntPtr.Zero) {
                if (this.Handle != IntPtr.Zero) {
                    User.ReleaseDC(this.Handle, this.deviceContext);
                }
                this.deviceContext = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Swaps the back and front buffer.
        /// </summary>
        protected void SwapBuffers() {
            Gdi.SwapBuffersFast(this.deviceContext);
        }

        /// <summary>
        /// Save the front buffer as a Bitmap.
        /// </summary>
        /// <returns></returns>
        public Bitmap Screenshot() {
            // Make this current first!
            this.MakeCurrentContext();

            // Get size from viewport
            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            // array for pixeldata
            byte[] read_data = new byte[4 * viewport[2] * viewport[3]];

            // swap buffers to be save working on every card
            this.SwapBuffers();
            // read front buffer
            Gl.glReadBuffer(Gl.GL_BACK);

            // read the pixels in BGRA-format (for some graphic cards faster)
            Gl.glReadPixels(0, 0, viewport[2], viewport[3], Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, read_data);

            // swap it back
            this.SwapBuffers();

            // Create clean bitmap
            Bitmap nbmp = new Bitmap(viewport[2], viewport[3], PixelFormat.Format24bppRgb);

            // Get the real data from bitmap, lock the original data
            BitmapData bmpData = nbmp.LockBits(new Rectangle(0, 0, viewport[2], viewport[3]), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            // Get pointer from first data
            IntPtr intPtr = bmpData.Scan0;

            // Calc the offset for a row
            int nOffset = bmpData.Stride - viewport[2] * 4;
            int loc = 0;

            // Set the pixels
            for (int i = 0; i < viewport[3]; i++) {
                loc = bmpData.Stride * (viewport[3] - i - 1);
                for (int j = 0; j < viewport[2] * 4; j += 4) {
                    // construct pointers for the pixels color
                    IntPtr Alpha = new IntPtr(intPtr.ToInt32() + 3);
                    IntPtr Red = new IntPtr(intPtr.ToInt32() + 2);
                    IntPtr Green = new IntPtr(intPtr.ToInt32() + 1);
                    IntPtr Blue = intPtr;
                    // set pixels to color from buffer
                    Marshal.WriteByte(Alpha, read_data[loc + j + 3]);
                    Marshal.WriteByte(Red, read_data[loc + j + 2]);
                    Marshal.WriteByte(Green, read_data[loc + j + 1]);
                    Marshal.WriteByte(Blue, read_data[loc + j]);
                    intPtr = new IntPtr(intPtr.ToInt32() + 4);
                }
                intPtr = new IntPtr(intPtr.ToInt32() + nOffset);
            }
            // unlock the data from bitmap
            nbmp.UnlockBits(bmpData);

            return nbmp;
        }

        /// <summary>
        /// Standard rendering operations supporting normal and stereo-viewing
        /// Override this method to implement own routines        
        /// </summary>
        /// <remarks>if this method is not overridden put all drawing operations into
        /// the RenderContent method
        /// This method works only properly if the model is renderend into a normalized
        /// coordinate system
        /// </remarks>
        protected virtual void RenderScene() {
            Gl.glDrawBuffer(Gl.GL_BACK);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            SetupModelView(true);
            if (!stereoMode) {
                SetupProjection(true);
                RenderContent();
            } else {
                SetupProjectionStereo(StereoEye.LeftEye);
                Gl.glColorMask(0, 1, 1, 1);

                RenderContent();

                SetupProjectionStereo(StereoEye.RightEye);
                Gl.glDrawBuffer(Gl.GL_BACK);
                Gl.glClear(Gl.GL_DEPTH_BUFFER_BIT);
                Gl.glColorMask(1, 0, 0, 1);

                RenderContent();
                Gl.glColorMask(1, 1, 1, 1);
            }
        }

        /// <summary>
        /// Put the content to be rendered here
        /// </summary>
        protected virtual void RenderContent() { }

        /// <summary>
        /// Builds a font for OpenGL.
        /// </summary>
        private void BuildFont() {
            IntPtr font;                                                        // Windows Font ID
            fontbase = Gl.glGenLists(256);                                      // Storage For 256 Characters

            font = Gdi.CreateFont(                                              // Create The Font
                -(int)(BaseFont.Size),                                                            // Height Of Font
                0,                                                              // Width Of Font
                0,                                                              // Angle Of Escapement
                0,                                                              // Orientation Angle
                300,                                                            // Font Weight
                false,                                                          // Italic
                false,                                                          // Underline
                false,                                                          // Strikeout
                Gdi.ANSI_CHARSET,                                               // Character Set Identifier
                Gdi.OUT_TT_PRECIS,                                              // Output Precision
                Gdi.CLIP_DEFAULT_PRECIS,                                        // Clipping Precision
                Gdi.ANTIALIASED_QUALITY,                                        // Output Quality
                Gdi.FF_DONTCARE | Gdi.DEFAULT_PITCH,                            // Family And Pitch
                BaseFont.FontFamily.Name);                                                       // Font Name
            IntPtr hDC = User.GetDC(this.Handle);
            Gdi.SelectObject(hDC, font);                                        // Selects The Font We Created
            //Wgl.wglUseFontOutlines(
            //    hDC,                                                            // Select The Current DC
            //    0,                                                              // Starting Character
            //    255,                                                            // Number Of Display Lists To Build
            //    fontbase,                                                       // Starting Display Lists
            //    0,                                                              // Deviation From The True Outlines
            //    2f,                                                             // Font Thickness In The Z Direction
            //    Wgl.WGL_FONT_POLYGONS,                                          // Use Polygons, Not Lines
            //    gmf);                                                           // Address Of Buffer To Receive Data
        }

        /// <summary>
        /// This method draws a string with OpenGl. It needs the coordinates to draw the String at, a 
        /// degree for rotating the String counter clockwise, a size modifier and the string to be drawn.
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <param name="degree">Degree for rotating counter clockwise</param>
        /// <param name="scaling">Size modifier for scaling</param>
        /// <param name="text">String to be drawn</param>
        protected void PrintText(float x, float y, float degree, float scaling, string text) {
            //TODO Sowas aehnliches bauen das unter ProjectionFlat arbeitet und keepAspect beruecksichtigt
            if (text == null || text.Length == 0) {                             // If There's No Text
                return;                                                         // Do Nothing
            }
            float length = 0;                                                   // Used To Find The Length Of The Text
            char[] chars = text.ToCharArray();                                  // Holds Our String

            for (int loop = 0; loop < text.Length; loop++) {                    // Loop To Find Text Length
                length += gmf[chars[loop]].gmfCellIncX;                         // Increase Length By Each Characters Width
            }

            Gl.glPushMatrix();
            Gl.glPushAttrib(Gl.GL_LIST_BIT);                                    // Pushes The Display List Bits
            Gl.glEnable(Gl.GL_POLYGON_SMOOTH);
            Gl.glHint(Gl.GL_POLYGON_SMOOTH_HINT, Gl.GL_NICEST);

            Gl.glListBase(fontbase);

            Gl.glTranslatef(x, y, 0);
            Gl.glRotatef(degree, 0.0F, 0.0F, 1.0F);
            Gl.glScalef(scaling, scaling, 0.0F);

            byte[] textbytes = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
                textbytes[i] = (byte)text[i];
            Gl.glCallLists(text.Length, Gl.GL_UNSIGNED_BYTE, textbytes);        // Draws The Display List Text
            Gl.glPopMatrix();
            Gl.glPopAttrib();
            Gl.glDisable(Gl.GL_POLYGON_SMOOTH);
        }

        #endregion

        /// <summary>
        /// Checks the various OpenGL errors and shows them.
        /// </summary>
        protected void CheckErrors() {
            int errorCode = Gl.glGetError();
            if (errorCode != Gl.GL_NO_ERROR) {
                String err;
                switch (errorCode) {
                    case Gl.GL_INVALID_ENUM:
                        err = "GL_INVALID_ENUM - An unacceptable value has been specified for an enumerated argument.  The offending function has been ignored.";
                        break;
                    case Gl.GL_INVALID_VALUE:
                        err = "GL_INVALID_VALUE - A numeric argument was out of range.  The offending function has been ignored.";
                        break;
                    case Gl.GL_INVALID_OPERATION:
                        err = "GL_INVALID_OPERATION - An operation was not allowed in its current state.  The offending function has been ignored.";
                        break;
                    case Gl.GL_STACK_OVERFLOW:
                        err = "GL_STACK_OVERFLOW - A function would have caused a stack overflow.  The offending function has been ignored.";
                        break;
                    case Gl.GL_STACK_UNDERFLOW:
                        err = "GL_STACK_UNDERFLOW - A function would have caused a stack underflow.  The offending function has been ignored.";
                        break;
                    case Gl.GL_OUT_OF_MEMORY:
                        err = "GL_OUT_OF_MEMORY - There was not enough memory left to execute a function.  The state of OpenGL has been left undefined.";
                        break;
                    default:
                        err = "Unknown GL error.  This should never happen.";
                        break;
                }
#if DEBUG
                throw new ApplicationException(err);
#else
                ShowError(err);
#endif
            }
        }

        /// <summary>
        /// Shows a MessageBox displaying a description of an OpenGL error.
        /// </summary>
        /// <param name="err">description of an OpenGL error</param>
        private void ShowError(String err) {
            MessageBox.Show(err, "OpenGL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Performs PushMatrix for both projection and modelview matrices.
        /// Doesn't change the current MatrixMode.
        /// </summary>
        protected void PushMatrices() {
            Gl.glPushAttrib(Gl.GL_TRANSFORM_BIT);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();

            Gl.glPopAttrib();
        }

        /// <summary>
        /// Performs PopMatrix for both projection and modelview matrices.
        /// Doesn't change the current MatrixMode.
        /// </summary>
        protected void PopMatrices() {
            Gl.glPushAttrib(Gl.GL_TRANSFORM_BIT);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();

            Gl.glPopAttrib();
        }

        /// <summary>
        /// Converts absolute window coordinates to relative coordinates.
        /// Useful when working with ProjectionFlat, wishing to mix relative and absolute coordinates.
        /// </summary>
        /// <param name="absolute">Absolute window coordinates</param>
        protected VectorF RelFromAbs(VectorF absolute) {
            absolute.X /= this.Width;
            absolute.Y /= this.Height;
            return absolute;
        }

        /// <summary>
        /// Converts relative coordinates to absolute window coordinates.
        /// Useful when working with ProjectionFlat, wishing to mix relative and absolute coordinates.
        /// </summary>
        /// <param name="relative">Relative coordinates</param>
        protected VectorF AbsFromRel(VectorF relative) {
            relative.X *= this.Width;
            relative.Y *= this.Height;
            return relative;
        }

        /// <summary>
        /// Copies pixels from one buffer to another.
        /// </summary>
        /// <param name="fromBuffer">The source buffer. Use one of the OpenGL buffer constants (GL_FRONT, GL_BACK, ...)</param>
        /// <param name="toBuffer">The destination buffer. Use one of the OpenGL buffer constants (GL_FRONT, GL_BACK, ...)</param>
        /// <param name="x">The x-coordinate of the lower left corner of the area to be copied (pixel coordinates, 0 is left, this.Width is right)</param>
        /// <param name="y">The y-coordinate of the lower left corner of the area to be copied (pixel coordinates, 0 is bottom, this.Height is top)</param>
        /// <param name="width">The width of the area to be copied (in pixels)</param>
        /// <param name="height">The height of the area to be copied (in pixels)</param>
        protected void CopyPixels(int fromBuffer, int toBuffer, int x, int y, int width, int height) {
            Gl.glPushAttrib(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_PIXEL_MODE_BIT | Gl.GL_CURRENT_BIT | Gl.GL_TRANSFORM_BIT);
            Gl.glDisable(Gl.GL_BLEND);
            PushMatrices();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            SetupProjectionFlat(true, true, false);
            Gl.glReadBuffer(fromBuffer);
            Gl.glDrawBuffer(toBuffer);
            Gl.glRasterPos2i(x, y);
            Gl.glCopyPixels(x, y, width, height, Gl.GL_COLOR);
            PopMatrices();
            Gl.glPopAttrib();
        }

        #endregion

        #region Event Handling Stuff

        /// <summary>
        /// Handles the MouseEnter event for an OpenGLControl.
        /// </summary>
        /// <param name="e">Standard EventArgs</param>
        protected override void OnMouseEnter(EventArgs e) {
            base.OnMouseEnter(e);
            this.Focus();
        }

        /// <summary>
        /// Handles the Paint event for an OpenGLControl.
        /// </summary>
        /// <param name="e">Standard PaintEventArgs</param>
        protected override void OnPaint(PaintEventArgs e) {
            if (this.deviceContext == IntPtr.Zero || this.renderContext == IntPtr.Zero) {
                MessageBox.Show("No device or rendering context available!");
                return;
            }

            base.OnPaint(e);

            //Only switch contexts if this is already not the current context
            if (this.renderContext != Wgl.wglGetCurrentContext()) {
                this.MakeCurrentContext();
            }
            this.RenderScene();
            this.SwapBuffers();
            this.CheckErrors();
        }

        /// <summary>
        /// Handles Resizing Events, currently adjusts the Viewport
        /// </summary>
        /// <param name="e">Standard EventArgs</param>
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            MakeCurrentContext();
            SetupViewPort();
        }

        #endregion
    }
}
