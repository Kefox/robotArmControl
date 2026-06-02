using SharpGL;
using System.Numerics;
using System.Runtime.CompilerServices;
using System;
using System.Windows.Media.Animation;
using RobotControl;

namespace RobotRendering
{
    public static class Render
    {
        public static void drawLine(OpenGL gl, Vector3 point1, Vector3 point2, Vector3 rgb)
        {
            gl.Begin(OpenGL.GL_LINES);
            gl.Color(rgb.X, rgb.Y, rgb.Z, 1);

            gl.Vertex(point1.X, point1.Y, point1.Z);
            gl.Vertex(point2.X, point2.Y, point2.Z);

            gl.End();
            gl.Flush();
        }

        public static void drawArrow(OpenGL gl, Vector3 point1, Vector3 point2, Vector3 direction, Vector3 rgb)
        {
            drawLine(gl, point1, point2, rgb);
            drawCone(gl, point2, 0.2f, direction, 0.4f, rgb);
        }

        public static void drawThickLine(OpenGL gl, Vector3 point1, Vector3 point2, float diameter, Vector3 rgb)
        {
            gl.Color(rgb.X, rgb.Y, rgb.Z, 1);
            float precision = 16;
            float angle = 2*(float)Math.PI/precision;

            Vector3 vector12 = point2 - point1;
            float verticalAngle = 90f - (float)Math.Atan2(vector12.Z, Math.Sqrt(Math.Pow(vector12.X, 2)+Math.Pow(vector12.Y, 2)))*180f/(float)Math.PI;

            drawSphere(gl, point1, diameter/2, new Vector4(rgb, 1));
            gl.PushMatrix();
            gl.Translate(point1.X, point1.Y, point1.Z);
            Vector3 rotAxis = Vector3.Normalize(new Vector3(-vector12.Y, vector12.X, 0));
            if (rotAxis.Length() > 0)
            {
                gl.Rotate(verticalAngle, rotAxis.X, rotAxis.Y, rotAxis.Z);
            }
            
            

            for (int i = 0; i <= precision; i++)
                {
                    gl.Begin(OpenGL.GL_TRIANGLE_FAN);

                    

                    gl.Vertex(diameter/2*Math.Cos(i*angle), diameter/2*Math.Sin(i*angle), 0);
                    gl.Vertex(diameter/2*Math.Cos((i+1)*angle), diameter/2*Math.Sin((i+1)*angle), 0);
                    gl.Vertex(diameter/2*Math.Cos((i+1)*angle), diameter/2*Math.Sin((i+1)*angle), vector12.Length());
                    gl.Vertex(diameter/2*Math.Cos(i*angle), diameter/2*Math.Sin(i*angle), vector12.Length());
                    gl.Vertex(diameter/2*Math.Cos((i+1)*angle), diameter/2*Math.Sin((i+1)*angle), vector12.Length());
                    
                    gl.End();
                    
                }

                gl.PopMatrix();

            drawSphere(gl, point2, diameter/2, new Vector4(rgb, 1));

            gl.Flush();

        }

        public static void drawCone(OpenGL gl, Vector3 basePoint, float baseRad, Vector3 direction, float height, Vector3 rgb)
        {
            gl.Color(rgb.X, rgb.Y, rgb.Z, 1);
 
            float precision = 12;
            float angle = 2*(float)Math.PI/precision;

            if (Math.Abs(direction.X) == 1)
            {
                //Base
                gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                gl.Vertex(basePoint.X, basePoint.Y, basePoint.Z);
                for (int i = 0; i <= precision; i++)
                {
                gl.Vertex(basePoint.X, basePoint.Y+baseRad*Math.Cos(i*angle), basePoint.Z+baseRad*Math.Sin(i*angle));
                }
                gl.End();
                //Top   
                gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                gl.Vertex(basePoint.X + direction.X*height, basePoint.Y, basePoint.Z);
                for (int i = 0; i <= precision; i++)
                {
                gl.Vertex(basePoint.X, basePoint.Y+baseRad*Math.Cos(i*angle), basePoint.Z+baseRad*Math.Sin(i*angle));
                }
                gl.End();
            } else if (Math.Abs(direction.Y) == 1)
            {
                //Base
                gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                gl.Vertex(basePoint.X, basePoint.Y, basePoint.Z);
                for (int i = 0; i <= precision; i++)
                {
                gl.Vertex(basePoint.X+baseRad*Math.Cos(i*angle), basePoint.Y, basePoint.Z+baseRad*Math.Sin(i*angle));
                }
                gl.End();
                //Top   
                gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                gl.Vertex(basePoint.X, basePoint.Y + direction.Y*height, basePoint.Z);
                for (int i = 0; i <= precision; i++)
                {
                gl.Vertex(basePoint.X+baseRad*Math.Cos(i*angle), basePoint.Y, basePoint.Z+baseRad*Math.Sin(i*angle));
                }
                gl.End();        
            } else if (Math.Abs(direction.Z) == 1)
            {
                //Base
                gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                gl.Vertex(basePoint.X, basePoint.Y, basePoint.Z);
                for (int i = 0; i <= precision; i++)
                {
                gl.Vertex(basePoint.X+baseRad*Math.Cos(i*angle), basePoint.Y+baseRad*Math.Sin(i*angle), basePoint.Z);
                }
                gl.End();
                //Top   
                gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                gl.Vertex(basePoint.X, basePoint.Y, basePoint.Z + direction.Z*height);
                for (int i = 0; i <= precision; i++)
                {
                gl.Vertex(basePoint.X+baseRad*Math.Cos(i*angle), basePoint.Y+baseRad*Math.Sin(i*angle), basePoint.Z);
                }
                gl.End();        
            }
            gl.Flush();
        }
        
        public static void drawGrid(OpenGL gl)
        {   
            int gridSize = 20;
            int gridDist = 1;
            int gridLength = 20;

            gl.Begin(OpenGL.GL_LINES);
            Vector3 rgb = new Vector3(0.67f, 0.67f, 0.67f);

            for (int i = -gridSize/2; i <= gridSize/2; i++)
            {   
                if (i != 0)
                {
                    Vector3 point1 = new Vector3(-gridLength/2, i*gridDist, 0);
                    Vector3 point2 = new Vector3(gridLength/2, i*gridDist, 0);
                    drawLine(gl, point1, point2, rgb);

                    Vector3 point3 = new Vector3(i*gridDist, -gridLength/2, 0);
                    Vector3 point4 = new Vector3(i*gridDist, gridLength/2, 0);
                    drawLine(gl, point3, point4, rgb);

                    Vector3 point5 = new Vector3(-gridLength/2, 0, i*gridDist);
                    Vector3 point6 = new Vector3(gridLength/2, 0, i*gridDist);      
                    drawLine(gl, point5, point6, rgb);

                    Vector3 point7 = new Vector3(i*gridDist, 0, -gridLength/2);
                    Vector3 point8 = new Vector3(i*gridDist, 0, gridLength/2);
                    drawLine(gl, point7, point8, rgb);

                    Vector3 point9 = new Vector3(0, -gridLength/2, i*gridDist);
                    Vector3 point10 = new Vector3(0, gridLength/2, i*gridDist);      
                    drawLine(gl, point9, point10, rgb);

                    Vector3 point11 = new Vector3(0, i*gridDist, -gridLength/2);
                    Vector3 point12 = new Vector3(0, i*gridDist, gridLength/2);
                    drawLine(gl, point11, point12, rgb);
                }  
                    
                    Vector3 red = new Vector3(0.8f, 0, 0);
                    Vector3 arrowX1 = new Vector3(-gridSize/2, 0, 0);
                    Vector3 arrowX2 = new Vector3(gridSize/2, 0, 0);
                    drawArrow(gl, arrowX1, arrowX2, new Vector3(1, 0, 0), red);
                    drawXYZ(gl, arrowX2, "X", red);
                    Vector3 blue = new Vector3(0, 0, 0.8f);
                    Vector3 arrowY1 = new Vector3(0, -gridSize/2, 0);
                    Vector3 arrowY2 = new Vector3(0, gridSize/2, 0);
                    drawArrow(gl, arrowY1, arrowY2, new Vector3(0, 1, 0), blue);
                    drawXYZ(gl, arrowY2, "Y", blue);
                    Vector3 green = new Vector3(0, 0.8f, 0);
                    Vector3 arrowZ1 = new Vector3(0, 0, -gridSize/2);
                    Vector3 arrowZ2 = new Vector3(0, 0, gridSize/2);
                    drawArrow(gl, arrowZ1, arrowZ2, new Vector3(0, 0, 1), green);
                    drawXYZ(gl, arrowZ2, "Z", green);

            gl.End();
            gl.Flush();
            }

        }

        public static void drawXYZ(OpenGL gl, Vector3 position, string axis, Vector3 rgb)
        {
            float thickness = 0.15f;
            Vector3 adjustedPosition;

            gl.PushMatrix();
            switch (axis)
            {
                case "X":
                    adjustedPosition = position + new Vector3(0, -thickness/2, -0.3f) + new Vector3(0.5f, 0, 0);
                    
                    gl.Translate(adjustedPosition.X, adjustedPosition.Y, adjustedPosition.Z);
                    gl.Rotate(90, 1, 0, 0);
                    
                    
                    gl.Color(rgb.X, rgb.Y, rgb.Z, 1);
                    gl.DrawText3D("Arial", 12, thickness, axis);
                break;

                case "Y":
                    adjustedPosition = position + new Vector3(thickness/2, 0, -0.3f) + new Vector3(0, 0.5f, 0);
                    
                    gl.Translate(adjustedPosition.X, adjustedPosition.Y, adjustedPosition.Z);
                    gl.Rotate(90, 0, 0, 1);
                    gl.Rotate(90, 1, 0, 0);
                    
                    gl.Color(rgb.X, rgb.Y, rgb.Z, 1);
                    gl.DrawText3D("Arial", 12, thickness, axis);
                break;

                case "Z":
                    adjustedPosition = position + new Vector3(0.25f , -0.15f, 0) + new Vector3(0, 0, 0.5f);
                    
                    gl.Translate(adjustedPosition.X, adjustedPosition.Y, adjustedPosition.Z);
                    gl.Rotate(-45-180, 0, 0, 1);
                    gl.Rotate(90, 1, 0, 0);
                    
                    gl.Color(rgb.X, rgb.Y, rgb.Z, 1);
                    gl.DrawText3D("Arial", 12, thickness, axis);

                break;
            }
            gl.PopMatrix();
            
        }

        public static void drawSphere(OpenGL gl, Vector3 basePoint, float radius, Vector4 rgba)
        {
            gl.Color(rgba.X, rgba.Y, rgba.Z, rgba.W);
 
            float precision = 16;
            float angle = 2*(float)Math.PI/precision;

            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.DepthMask((byte)OpenGL.GL_FALSE); 
            
            for (int i = 0; i <= precision; i++)
            {
                for (int j = 0; j <= precision; j++)
                {
                    gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                    gl.Vertex(basePoint.X+radius*Math.Cos(j*angle)*Math.Sin(i*angle), basePoint.Y+radius*Math.Cos(j*angle)*Math.Cos(i*angle), basePoint.Z+radius*Math.Sin(j*angle));
                    gl.Vertex(basePoint.X+radius*Math.Cos((j+1)*angle)*Math.Sin(i*angle), basePoint.Y+radius*Math.Cos((j+1)*angle)*Math.Cos(i*angle), basePoint.Z+radius*Math.Sin((j+1)*angle));
                    gl.Vertex(basePoint.X+radius*Math.Cos((j+1)*angle)*Math.Sin((i+1)*angle), basePoint.Y+radius*Math.Cos((j+1)*angle)*Math.Cos((i+1)*angle), basePoint.Z+radius*Math.Sin((j+1)*angle));
                    gl.Vertex(basePoint.X+radius*Math.Cos(j*angle)*Math.Sin((i+1)*angle), basePoint.Y+radius*Math.Cos(j*angle)*Math.Cos((i+1)*angle), basePoint.Z+radius*Math.Sin(j*angle));
                    gl.Vertex(basePoint.X+radius*Math.Cos((j+1)*angle)*Math.Sin((i+1)*angle), basePoint.Y+radius*Math.Cos((j+1)*angle)*Math.Cos((i+1)*angle), basePoint.Z+radius*Math.Sin((j+1)*angle));
                    gl.End();
                }
                
            }

            gl.DepthMask((byte)OpenGL.GL_TRUE);
            gl.Flush(); 
            gl.Disable(OpenGL.GL_BLEND);

        }

        public static void drawControlSpheres(int jointOverNum, OpenGL gl, Vector3 posJoint1, Vector3 posJoint2, Vector3 posJoint3, Vector3 cameraPos, Vector3 mousePos)
        {   
            if (jointOverNum == 1)
            {
                Render.drawSphere(gl, posJoint1, 5, new Vector4(0.56f, 0.84f, 5f, 0.1f));
            }
            if (jointOverNum == 2)
            {
                Render.drawSphere(gl, posJoint2, 5, new Vector4(0.56f, 0.84f, 5f, 0.1f));
            }
        }
    }
}
