using System.Windows;
using System.Windows.Input;
using SharpGL;
using SharpGL.SceneGraph;
using RobotRendering;
using RobotControl;
using System.Numerics;
using System.IO.Ports;
using System.Windows.Controls;
using static System.Math;
using System;
using System.Globalization;


namespace RobotControl
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try 
            {
                port.Open();
            } 
            catch (Exception ex) 
            {
                MessageBox.Show("Fehler beim Öffnen des COM-Ports: " + ex.Message);
            }
        }

        Vector3 cameraPos = new Vector3(20.0f, 20.0f, 10.0f);
        System.Windows.Point mousePos;
        System.Windows.Point prevMousePos;
        Vector3 mousePoint = Vector3.Zero;

        Vector3 posJoint1 = Vector3.Zero;
        Vector3 posJoint2 = new Vector3(0, 0, 5f);
        Vector3 posJoint3 = new Vector3(0, 0, 10f);

        int jointOverNum = 0;
        bool ignoreHover = false;

        OpenGL gl;
        
        SerialPort port = new SerialPort("COM5", 115200);

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            gl = args.OpenGL;

            gl.ClearColor(0.93f, 0.93f, 0.93f, 1.0f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            float aspectratio = (float)glControl.ActualWidth / (float)glControl.ActualHeight;
            gl.Perspective(60.0f, aspectratio, 0.1f, 100.0f);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            Control.setPos(gl, cameraPos);
            
            Render.drawGrid(gl);

            Render.drawThickLine(gl, posJoint1, posJoint2, 0.3f, new Vector3(1f, 0.647f, 0));
            Render.drawThickLine(gl, posJoint2, posJoint3, 0.3f, new Vector3(1f, 0.647f, 0));

            
            Render.drawControlSpheres(jointOverNum, gl, posJoint1, posJoint2, posJoint3, cameraPos, mousePoint);
            
            
        }

        private void sendAnglesToArduino()
        {
            string data = Control.createAngleData(posJoint2, posJoint3);
            try
            {
                port.WriteLine(data);
            } catch
            {
                MessageBox.Show("Port nicht gefunden: " + data);
            }
            
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }
            App.Current.Shutdown();
        }

        private void screenSizeChange(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                {
                    if (Grid.RowDefinitions.Count == 1)
                    {
                        RowDefinition header = new RowDefinition();
                        header.Height = new GridLength(35f);
                        Grid.RowDefinitions.Insert(0, header);    
                    }
                    
                    WindowState = WindowState.Normal;
                } else
                {
                    if (Grid.RowDefinitions.Count == 2)
                    {
                        Grid.RowDefinitions.RemoveAt(0);
                    }
                    
                    WindowState = WindowState.Maximized;
                }
        }
        
        private void KeyHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
               case Key.F11:
               if (WindowState == WindowState.Maximized)
                {
                    if (Grid.RowDefinitions.Count == 1)
                    {
                        RowDefinition header = new RowDefinition();
                        header.Height = new GridLength(35f);
                        Grid.RowDefinitions.Insert(0, header);    
                    }
                    
                    WindowState = WindowState.Normal;
                } else
                {
                    if (Grid.RowDefinitions.Count == 2)
                    {
                        Grid.RowDefinitions.RemoveAt(0);
                    }
                    
                    WindowState = WindowState.Maximized;
                }
                break;

                case Key.Space:
                    sendAnglesToArduino();
                break;

                case Key.R:
                    posJoint2 = new Vector3(0, 0, 5f);
                    posJoint3 = new Vector3(0, 0, 10f);
                break;
            }
            
        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                cameraPos = Control.zoomIn(cameraPos);
            }
            else if (e.Delta < 0)
            {
                cameraPos = Control.zoomOut(cameraPos);
            }
        }

        private void MouseMovementHandler(object sender, MouseEventArgs e)
        {
            mousePos = e.GetPosition(glControl);
            mousePoint = Control.screenToWorld(gl, mousePos, (float)glControl.ActualHeight, (float)glControl.ActualWidth);
            
            if (!ignoreHover)
            {
                jointOverNum = Control.checkHoveredJoint(ignoreHover, e, mousePoint, cameraPos, posJoint1, posJoint2, posJoint3);
            }

            if (e.LeftButton == MouseButtonState.Pressed && (jointOverNum == 1 || jointOverNum == 2))
            {
                switch (jointOverNum)
                {
                    case 1:
                        ignoreHover = true;
                        Vector3 temPos = posJoint2;
                        posJoint2 = Control.getClosestSphereIntersection(posJoint1, 5f, cameraPos, mousePoint);
                        Vector3 delta = posJoint2 - temPos;
                        posJoint3 += delta;
                    break;

                    case 2:
                        ignoreHover = true;
                        posJoint3 = Control.getClosestSphereIntersection(posJoint2, 5f, cameraPos, mousePoint);
                    break;
                }
            } else if (e.LeftButton == MouseButtonState.Pressed)
            {
                cameraPos = Control.rotateCamera(cameraPos, mousePos, prevMousePos, (float)this.ActualWidth, (float)this.ActualHeight);    
            } else
            {
                ignoreHover = false;
            }

            prevMousePos = mousePos; //ganz am Ende
        }
    }
}