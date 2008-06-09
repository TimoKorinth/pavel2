﻿using System;
using System.Windows;
using System.Windows.Controls;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.Windows.Forms.Integration;
using Pavel2.Framework;
using System.Windows.Media;

namespace Pavel2.GUI {
    /// <summary>
    /// Interaktionslogik für ParallelPlot.xaml
    /// </summary>
    public partial class ParallelPlot : UserControl, Visualization {

        internal class OpenGLRenderWind : SimpleOpenGlControl {
            public OpenGLRenderWind() {
                this.InitializeContexts();
                Gl.glClearColor(1.0f, 1.0f, 1.0f, 0.0f);
                Gl.glShadeModel(Gl.GL_FLAT);
                Gl.glEnable(Gl.GL_LINE_SMOOTH);
                Gl.glEnable(Gl.GL_BLEND);
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                Gl.glDisable(Gl.GL_CULL_FACE);
                Gl.glMatrixMode(Gl.GL_PROJECTION);
                Gl.glLoadIdentity();
                Gl.glOrtho(0.0, 1.0, 0.0, 1.0, -1.0, 1.0);
            }
        }

        System.Windows.Forms.Control wfPA;
        DataGrid dataGrid;
        double step;

        public ParallelPlot() {
            InitializeComponent();
            wfPA = new OpenGLRenderWind();
            wfPA.Paint += DrawData;
            host.Child = wfPA;
            SetViewPort();
        }

        private void DrawLines() {
            Gl.glColor3f(0.043f, 0.729f, 0.878f);
            Gl.glEnable(Gl.GL_LINE_SMOOTH);
            Gl.glLineWidth(1f);
            bool breakLine = false;
            if (dataGrid == null) return;
            for (int row = 0; row < dataGrid.Columns[dataGrid.MaxColumn].Points.Length; row++) {
                Gl.glBegin(Gl.GL_LINE_STRIP);
                for (int col = 0; col < dataGrid.Columns.Length; col++) {
                    if (dataGrid.DoubleDataField[row][col] == double.NaN) {
                        Gl.glEnd();
                        breakLine = true;
                    } else {
                        if (breakLine) Gl.glBegin(Gl.GL_LINE_STRIP);
                        double nValue = Normalize(dataGrid.DoubleDataField[row][col], dataGrid.Columns[col]);
                        Gl.glVertex2d(step * col, nValue);
                    }
                }
                if (!breakLine) Gl.glEnd();
            }
        }

        private double Normalize(double value, Column col) {
            if ((col.Max - col.Min) == 0) {
                return 0.5;
            }
            return ((value - col.Min) / (col.Max - col.Min));
        }

        private void DrawAxes() {
            Gl.glColor3f(0.5f, 0.5f, 0.5f);
            Gl.glDisable(Gl.GL_LINE_SMOOTH);
            Gl.glLineWidth(2f);
            if (dataGrid == null) return;
            for (int col = 0; col < dataGrid.Columns.Length; col++) {
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex2d(step * col, 0);
                Gl.glVertex2d(step * col, 1);
                Gl.glEnd();
            }
        }

        private void DrawData(object sender, System.Windows.Forms.PaintEventArgs e) {
            RenderScene();
        }

        private void RenderScene() {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);

            DrawLines();
            DrawAxes();

            Gl.glFlush();
        }


        private void SetViewPort() {
            Gl.glViewport(0, 0, (int)host.ActualWidth, (int)host.ActualHeight);
        }

        private void SetLabelPanel() {
            labelGrid.Children.Clear();
            labelGrid.ColumnDefinitions.Clear();
            if (dataGrid == null) return;
            for (int col = 0; col < dataGrid.Columns.Length-1; col++) {
                labelGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int col = 0; col < dataGrid.Columns.Length-1; col++) {
                Label lab = new Label();
                lab.HorizontalAlignment = HorizontalAlignment.Left;
                lab.Content = dataGrid.Columns[col].Header;
                labelGrid.Children.Add(lab);
                Grid.SetColumn(lab, col);
            }
            Label lastLab = new Label();
            lastLab.HorizontalAlignment = HorizontalAlignment.Right;
            lastLab.Content = dataGrid.Columns[dataGrid.Columns.Length - 1].Header;
            labelGrid.Children.Add(lastLab);
            Grid.SetColumn(lastLab, dataGrid.Columns.Length - 1);
        }

        #region Visualization Member

        public void Render() {
            dataGrid = MainData.CurrentDataGrid;
            if (dataGrid == null) return;
            SetLabelPanel();
            step = (double)1 / (dataGrid.Columns.Length - 1);
            SetViewPort();
            RenderScene();
        }

        public void RenderAfterResize() {
            SetViewPort();
        }

        #endregion
    }
}