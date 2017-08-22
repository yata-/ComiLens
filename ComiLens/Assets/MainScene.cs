using System;
using System.Collections.Generic;
using HoloLensWithOpenCVForUnityExample;
using OpenCVForUnity;
using UnityEngine;

namespace Assets
{
    [RequireComponent(typeof(OptimizationWebCamTextureToMatHelper))]
    public class MainScene : MonoBehaviour
    {
        private readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

        public bool EnableDetection { get; set; }

        public bool UseSeparateDetection { get; set; }

        object _sync = new object();

        Texture2D texture;

        Mat _grayMat;
        // 物体検出
        private CascadeClassifier _cascade;


        private bool _isDetecting = false;
        private Mat _grayMat4Thread;
        private CascadeClassifier _cascade4Thread;

        private Matrix4x4 _projectionMatrix;
        private OptimizationWebCamTextureToMatHelper _webCamTextureToMatHelper;
        // 結果
        private MatOfRect _detectionResult;
        
        void Start ()
        {
            _webCamTextureToMatHelper = gameObject.GetComponent<OptimizationWebCamTextureToMatHelper>();
            _webCamTextureToMatHelper.Initialize();
            EnableDetection = true;
        }

        void Update()
        {

            lock (_sync)
            {
                while (ExecuteOnMainThread.Count > 0)
                {
                    ExecuteOnMainThread.Dequeue().Invoke();
                }
            }


            if (_webCamTextureToMatHelper.IsPlaying() && _webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                Mat rgbaMat = _webCamTextureToMatHelper.GetDownScaleMat(_webCamTextureToMatHelper.GetMat());
                // グレースケールに取得
                Imgproc.cvtColor(rgbaMat, _grayMat, Imgproc.COLOR_RGBA2GRAY);
                // グレースケール画像のヒストグラムを均一化
                Imgproc.equalizeHist(_grayMat, _grayMat);

                if (EnableDetection && !_isDetecting)
                {
                    _isDetecting = true;

                    _grayMat.copyTo(_grayMat4Thread);

                    //StartThread(ThreadWorker);
                }

                OpenCVForUnity.Rect[] rects;
                if (!UseSeparateDetection)
                {

                }
                else
                {
                }
            }
        }

        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = _webCamTextureToMatHelper.GetDownScaleMat(_webCamTextureToMatHelper.GetMat());

            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            // HoloLensFaceDetectionOverlayExampleよりコピー
            //This value is obtained from PhotoCapture's TryGetProjectionMatrix() method.I do not know whether this method is good.
            //Please see the discussion of this thread.Https://forums.hololens.com/discussion/782/live-stream-of-locatable-camera-webcam-in-unity
            _projectionMatrix = Matrix4x4.identity;
            _projectionMatrix.m00 = 2.31029f;
            _projectionMatrix.m01 = 0.00000f;
            _projectionMatrix.m02 = 0.09614f;
            _projectionMatrix.m03 = 0.00000f;
            _projectionMatrix.m10 = 0.00000f;
            _projectionMatrix.m11 = 4.10427f;
            _projectionMatrix.m12 = -0.06231f;
            _projectionMatrix.m13 = 0.00000f;
            _projectionMatrix.m20 = 0.00000f;
            _projectionMatrix.m21 = 0.00000f;
            _projectionMatrix.m22 = -1.00000f;
            _projectionMatrix.m23 = 0.00000f;
            _projectionMatrix.m30 = 0.00000f;
            _projectionMatrix.m31 = 0.00000f;
            _projectionMatrix.m32 = -1.00000f;
            _projectionMatrix.m33 = 0.00000f;


            _grayMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC1);
            _cascade = new CascadeClassifier();
            _cascade.load(Utils.getFilePath("lbpcascade_frontalface.xml"));

            _grayMat4Thread = new Mat();
            _cascade4Thread = new CascadeClassifier();
            _cascade4Thread.load(Utils.getFilePath("haarcascade_frontalface_alt.xml"));

            _detectionResult = new MatOfRect();
        }

        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            //StopThread();

            if (_grayMat != null)
            {
                _grayMat.Dispose();
            }
            if (_cascade != null)
            {
                _cascade.Dispose();
            }
            if (_grayMat4Thread != null)
            {
                _grayMat4Thread.Dispose();
            }
            if (_cascade4Thread != null)
            {
                _cascade4Thread.Dispose();
            }
        }

        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

    }
}
