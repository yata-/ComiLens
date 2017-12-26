using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstValue {

    public static Matrix4x4 GetProjectMatrix()
    {
        // HoloLensFaceDetectionOverlayExampleよりコピー
        //This value is obtained from PhotoCapture's TryGetProjectionMatrix() method.I do not know whether this method is good.
        //Please see the discussion of this thread.Https://forums.hololens.com/discussion/782/live-stream-of-locatable-camera-webcam-in-unity
        var projectionMatrix = Matrix4x4.identity;
        projectionMatrix.m00 = 2.31029f;
        projectionMatrix.m01 = 0.00000f;
        projectionMatrix.m02 = 0.09614f;
        projectionMatrix.m03 = 0.00000f;
        projectionMatrix.m10 = 0.00000f;
        projectionMatrix.m11 = 4.10427f;
        projectionMatrix.m12 = -0.06231f;
        projectionMatrix.m13 = 0.00000f;
        projectionMatrix.m20 = 0.00000f;
        projectionMatrix.m21 = 0.00000f;
        projectionMatrix.m22 = -1.00000f;
        projectionMatrix.m23 = 0.00000f;
        projectionMatrix.m30 = 0.00000f;
        projectionMatrix.m31 = 0.00000f;
        projectionMatrix.m32 = -1.00000f;
        projectionMatrix.m33 = 0.00000f;
        return projectionMatrix;
    }
}
