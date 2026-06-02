using System.Runtime.Intrinsics.X86;
using SharpGL;
using SharpGL.SceneGraph.Quadrics;
using System.Numerics;
using System.Drawing;
using System.Windows;
using static System.Math;
using RobotRendering;
using System.Net;
using System;
using System.Security.Cryptography.Pkcs;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace RobotControl
{
    public static class Control
    {
        public static void setPos(OpenGL gl, Vector3 cameraPos)
        {
            gl.LookAt(cameraPos.X, cameraPos.Y, cameraPos.Z,  
                      0.0, 0.0, 0.0,
                      0.0, 0.0, 1.0);
        }

        public static float getZoom(Vector3 cameraPos)
        {
            return cameraPos.Length();
        }

        public static Vector3 zoomIn(Vector3 cameraPos)
        {
            float zoom = getZoom(cameraPos);
            zoom -= 1;

            if (zoom <= 1)
            {
                zoom = 1;
            }

            Vector3 newPos = Vector3.Normalize(cameraPos);
            newPos = zoom*newPos;

            return newPos;
        }

        public static Vector3 zoomOut(Vector3 cameraPos)
        {
            float zoom = getZoom(cameraPos);
            zoom += 1;

            if (zoom >= 30)
            {
                zoom = 30;
            }

            Vector3 newPos = Vector3.Normalize(cameraPos);
            newPos = zoom*newPos;

            return newPos;
        }

        public static Vector3 rotateCamera(Vector3 cameraPos, System.Windows.Point mousePos, System.Windows.Point prevMousePos, float screenWidth, float screenHeight)
        {
            System.Windows.Vector dMouse = System.Windows.Point.Subtract(mousePos, prevMousePos);

            float percentageChangeX = (float)dMouse.X/screenWidth;
            float percentageChangeY = (float)dMouse.Y/screenHeight;
            float angleChangeHorizontal = 2*(float)PI*percentageChangeX;
            float angleChangeVertical = 2*(float)PI*percentageChangeY;

            float cameraAngleHorizontal = (float)Atan2((float)cameraPos.Y, (float)cameraPos.X);
            float cameraAngleVertical = (float)Atan2(cameraPos.Z, (float)Sqrt(cameraPos.X*cameraPos.X+cameraPos.Y*cameraPos.Y));

            float newAngleHorizontal = cameraAngleHorizontal - angleChangeHorizontal;
            float newAngleVertical = cameraAngleVertical + angleChangeVertical;

            if (newAngleVertical >= PI/3)
            {
                newAngleVertical = (float)PI/3;
            } else if (newAngleVertical <= -PI/3)
            {
                newAngleVertical = -(float)PI/3;
            }
            float rad = cameraPos.Length();
            float projRad = rad*(float)Cos(newAngleVertical);

            Vector3 newCameraPos = new Vector3(projRad*(float)Cos(newAngleHorizontal), projRad*(float)Sin(newAngleHorizontal), rad*(float)Sin(newAngleVertical));

            return newCameraPos;
        }

        public static int checkHoveredJoint(bool ignoreHover, MouseEventArgs e, Vector3 mousePos, Vector3 cameraPos, Vector3 posJoint1, Vector3 posJoint2, Vector3 posJoint3)
        {
            float dist1 = Control.distToPoint(cameraPos, mousePos, posJoint2);
            float dist2 = Control.distToPoint(cameraPos, mousePos, posJoint3);
            if (dist1 <= 0.3f && ignoreHover == false)
            {
                return 1;
            } else if (dist2 <= 0.3f && ignoreHover == false) 
            {
                return 2;    
            } else
            {
                return 0;
            }
        }

        public static Vector3 getClosestSphereIntersection(Vector3 basePoint, float radius, Vector3 cameraPos, Vector3 mousePos)
        {   
            Vector3 rayDir = mousePos - cameraPos;
            float a = Vector3.Dot(rayDir, rayDir);
            float b = 2*Vector3.Dot(cameraPos-basePoint, rayDir);
            float c = Vector3.Dot(cameraPos-basePoint, cameraPos-basePoint)-radius*radius;
            
            float discriminant = b*b-4*a*c;
            if (discriminant < 0)
            {
                Vector3 normalizedRayDir = Vector3.Normalize(rayDir);
                Vector3 v = basePoint - cameraPos;
                float t = Vector3.Dot(v, normalizedRayDir);
        
                Vector3 closestPointOnRay = cameraPos + normalizedRayDir * t;
        
                Vector3 dirFromCenter = Vector3.Normalize(closestPointOnRay - basePoint);
                return basePoint + dirFromCenter * radius;
            }

            float t1 = (-b+(float)Math.Sqrt(discriminant))/(2*a);
            float t2 = (-b-(float)Math.Sqrt(discriminant))/(2*a);

            Vector3 intersection1 = cameraPos + t1*rayDir;
            Vector3 intersection2 = cameraPos + t2*rayDir;
            if ((intersection1-cameraPos).Length() <= (intersection2-cameraPos).Length())
            {
                return intersection1;
            } else
            {
                return intersection2;
            }
        } 

        public static Vector3 screenToWorld(OpenGL gl, System.Windows.Point mousePos, float screenHeight, float screenWidth)
        {

            float normalizedX = 2*(float)mousePos.X/screenWidth - 1;
            float normalizedY = -(2*(float)mousePos.Y/screenHeight - 1);

            Vector4 normalizedScreen = new Vector4(normalizedX, normalizedY, 1, 1);
            
            float[] projectionMatrixArray = new float[16];
            gl.GetFloat(OpenGL.GL_PROJECTION_MATRIX, projectionMatrixArray);
            Matrix4x4 projectionMatrix = Control.arrayToMatrix(projectionMatrixArray);

            float[] viewMatrixArray = new float[16];
            gl.GetFloat(OpenGL.GL_MODELVIEW_MATRIX, viewMatrixArray);
            Matrix4x4 viewMatrix = Control.arrayToMatrix(viewMatrixArray);

            Matrix4x4 inverseMatrix;
            Matrix4x4.Invert(viewMatrix*projectionMatrix, out inverseMatrix);

            Vector4 worldPoint = Vector4.Transform(normalizedScreen, inverseMatrix);

            Vector3 endPoint = Vector3.Zero;
            if (worldPoint.W != 0f)
            {
                endPoint.X = worldPoint.X/worldPoint.W;
                endPoint.Y = worldPoint.Y/worldPoint.W;
                endPoint.Z = worldPoint.Z/worldPoint.W;
            }
            
            return endPoint;
        }

        public static float distToPoint(Vector3 cameraPos, Vector3 mousePos, Vector3 point)
        {
            Vector3 diff = point - cameraPos;
            Vector3 direction = mousePos - cameraPos;
            Vector3 cross = Vector3.Cross(diff, direction);
            float distance = cross.Length()/direction.Length();
            
            return distance;
        }

        public static Matrix4x4 arrayToMatrix(float[] matrixArray)
        {
            Matrix4x4 matrix = new Matrix4x4(
                matrixArray[0], matrixArray[1], matrixArray[2], matrixArray[3], 
                matrixArray[4], matrixArray[5], matrixArray[6], matrixArray[7], 
                matrixArray[8], matrixArray[9], matrixArray[10], matrixArray[11], 
                matrixArray[12], matrixArray[13], matrixArray[14], matrixArray[15]
            );

            return matrix;
        }

        public static string createAngleData(Vector3 posJoint2, Vector3 posJoint3)
        {
            float treshold = 0.1f;

            float angleHorizontalLower = (float)Math.Atan2(posJoint2.Y, posJoint2.X);
            float angleVerticalLower;
            if (Math.Abs(posJoint2.X) < treshold && Math.Abs(posJoint2.Y) < treshold)
            {
                angleVerticalLower = 0;
            } else
            {
                angleVerticalLower = (float)Math.Atan2((float)Sqrt(posJoint2.X*posJoint2.X+posJoint2.Y*posJoint2.Y), posJoint2.Z);
            }
            
            Vector3 delta = posJoint3-posJoint2;
            float newX = delta.X*(float)Cos(angleHorizontalLower)*(float)Cos(angleVerticalLower) + delta.Y*(float)Sin(angleHorizontalLower)*(float)Cos(angleVerticalLower) - delta.Z*(float)Sin(angleVerticalLower);
            float newY = -delta.X*(float)Sin(angleHorizontalLower) + delta.Y*(float)Cos(angleHorizontalLower);
            float newZ = delta.X*(float)Cos(angleHorizontalLower)*(float)Sin(angleVerticalLower) + delta.Y*(float)Sin(angleHorizontalLower)*(float)Sin(angleVerticalLower) + delta.Z*(float)Cos(angleVerticalLower);

            Vector3 rotatedPos = new Vector3(newX, newY, newZ);
            float angleHorizontalUpper = (float)Math.Atan2(rotatedPos.Y, rotatedPos.X);
            float angleVerticalUpper;
            if (Math.Abs(rotatedPos.X) < treshold && Math.Abs(rotatedPos.Y) < treshold)
            {
                angleVerticalUpper = 0;
            } else
            {
                angleVerticalUpper = (float)Math.Atan2((float)Sqrt(rotatedPos.X*rotatedPos.X+rotatedPos.Y*rotatedPos.Y), rotatedPos.Z);
            }

            string data = String.Format(CultureInfo.InvariantCulture, "{0:F4},{1:F4},{2:F4},{3:F4}", angleHorizontalLower, angleVerticalLower, angleHorizontalUpper, angleVerticalUpper);
            return data;
        }
        
    }
}
