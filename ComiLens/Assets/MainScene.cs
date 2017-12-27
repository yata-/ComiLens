using System;
using System.Collections.Generic;
using System.Linq;
using Assets.SpeechClient;
using HoloLensWithOpenCVForUnityExample;
using OpenCVForUnity;
using OpenCVForUnity.RectangleTrack;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using Rect = OpenCVForUnity.Rect;

namespace Assets
{
    [RequireComponent(typeof(OptimizationWebCamTextureToMatHelper))]
    public class MainScene : MonoBehaviour
    {
        private Subject<bool> _visibleSubject;

        private TalkBaloonComponent _talkBaloonComponent;
        private CognitiveService _cognitiveService;

        private const float OverlayDistance = 1;
        private const float MinDetectionSizeRatio = 0.07f;

        private readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

        public bool EnableDetection { get; set; }

        public bool UseSeparateDetection { get; set; }

        object _sync = new object();

        Mat _grayMat;
        // 物体検出
        private CascadeClassifier _cascade;

        private bool _isDetecting = false;
        private bool _hasUpdatedDetectionResult = false;
        private Mat _grayMat4Thread;
        private CascadeClassifier _cascade4Thread;


        private Matrix4x4 _projectionMatrix;
        private OptimizationWebCamTextureToMatHelper _webCamTextureToMatHelper;
        // 結果
        private MatOfRect _detectionResult;

        private RectangleTracker _rectangleTracker;
        List<Rect> _resultObjects = new List<Rect>();
        private TalkDataContainer _talkDataContainer;
        
        bool _isThreadRunning = false;

        bool isThreadRunning
        {
            get
            {
                lock (_sync)
                    return _isThreadRunning;
            }
            set
            {
                lock (_sync)
                    _isThreadRunning = value;
            }
        }

        protected CanvasGroup CanvasGroup
        {
            get { return gameObject.GetComponentInChildren<CanvasGroup>(); }
        }

        void Start ()
        {
            _talkDataContainer = new TalkDataContainer();
            Debug.Log(string.Format("{0}, {1},  {2},{3}", Screen.width, Screen.height, Camera.main.pixelWidth, Camera.main.pixelHeight));
            //trans.localPosition = new Vector3(Screen.width / 2 * FacePosition.Value.x, Screen.height / 2 * FacePosition.Value.y, 600);
            _visibleSubject = new Subject<bool>();
            _talkBaloonComponent = GetComponentInChildren<TalkBaloonComponent>();
            _cognitiveService = GetComponentInChildren<CognitiveService>();
            _cognitiveService.MessageObservable.Subscribe(p =>
            {
                if (p.Type == PayloadType.Phrase)
                {
                    var message = p.GetMessage();
                    Debug.Log("WebSocket Message Phrase" + p.Content);
                    if (message.Status == RecognitionStatus.Success)
                    {
                        _talkDataContainer.AddMessage(message.DisplayText, Time.time);
                        _talkBaloonComponent.Text = _talkDataContainer.GetString();
                    }
                    _visibleSubject.OnNext(true);
                }
                // 日本語だとこない？
                //else if (p.Type == PayloadType.Hypothesis)
                //{
                //    var message = p.GetMessage();
                //    Debug.Log("WebSocket Message Hypothesis" + p.Content);
                //    _talkBaloonComponent.Text = message.Text;
                //    _visibleSubject.OnNext(true);
                //}
                else if (p.Type == PayloadType.StartDetected)
                {
                    CanvasGroup.alpha = 1;
                    _talkBaloonComponent.Text = _talkDataContainer.GetString() + "****";
                    _visibleSubject.OnNext(true);
                }
                else if (p.Type == PayloadType.EndDetected)
                {
                }
            });
            _visibleSubject.Throttle(TimeSpan.FromSeconds(5)).Subscribe(p =>
            {
                _talkDataContainer.Clear();
                CanvasGroup.alpha = 0;
            });
            CanvasGroup.alpha = 0;

            _rectangleTracker = new RectangleTracker();
            _webCamTextureToMatHelper = gameObject.GetComponent<OptimizationWebCamTextureToMatHelper>();
            _webCamTextureToMatHelper.Initialize();
            EnableDetection = true;


            //CanvasGroup.alpha = 1;
            //_talkBaloonComponent.Text = _currentText + "****";
        }

        private Vector2 EstimateMousePosition(Rect rect)
        {
            var x = rect.x + (rect.width * 1.3f);// / 4 * 3);
            var y = rect.y + (rect.height / 2);
            return new Vector2(x, y);
        }

        private Vector3 GetGazePoint()
        {
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;
            RaycastHit hitInfo;
            Physics.Raycast(headPosition, gazeDirection, out hitInfo);
            Debug.Log(string.Format("hitInfo.point {0}, {1}", hitInfo.point.x, hitInfo.point.y));
            return hitInfo.point;
        }

        private Vector3? MatPointToWorldPoint(Vector2 point, Mat image)
        {
            // 画像座標を正規化，Y軸の向きを反転し，ビューポート座標を求める．
            float viewportPointX = (float)(point.x / (double)image.width());
            float viewportPointY = (float)(1.0 - point.y / (double)image.height());
            Vector3 viewportPoint = new Vector3(viewportPointX, viewportPointY);
            Ray ray = Camera.main.ViewportPointToRay(viewportPoint);
            //// Rayを使って，Z軸も含んだワールド座標を求める．
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                return hitInfo.point;
            }
            return null;
        }

        private Vector3 GetViewPoint(Vector2 point, Mat image)
        {
            // 画像座標を正規化，Y軸の向きを反転し，ビューポート座標を求める．
            float viewportPointX = (float)(point.x / (double)image.width());
            float viewportPointY = (float)(1.0 - point.y / (double)image.height());
            Vector3 viewportPoint = new Vector3(viewportPointX, viewportPointY);
            return viewportPoint;
        }


        void Update()
        {
            _talkDataContainer.Update(Time.time);
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
                    StartThread(ThreadWorker);
                }

                // SeparateDetection?はとりあえず決め打ち
                if (UseSeparateDetection == false)
                {
                    if (_hasUpdatedDetectionResult)
                    {
                        _hasUpdatedDetectionResult = false;

                        _rectangleTracker.UpdateTrackedObjects(_detectionResult.toList());
                    }

                    _rectangleTracker.GetObjects(_resultObjects, true);
                    var rects = _resultObjects.ToArray();
                    var rect = rects.FirstOrDefault();
                    if (rect != null)
                    {
                        var estimate = EstimateMousePosition(rect);
                        Debug.Log(string.Format("estimated {0}, {1}", estimate.x, estimate.y));
                        _talkBaloonComponent.FacePosition = GetViewPoint(estimate, rgbaMat);
                        _talkBaloonComponent.IsTailVisible = true;
                    }
                    else
                    {
                        _talkBaloonComponent.IsTailVisible = false;
                        //Debug.Log("Face not found");
                    }
                }
            }

            if (_webCamTextureToMatHelper.IsPlaying())
            {

                Matrix4x4 cameraToWorldMatrix = Camera.main.cameraToWorldMatrix; ;

                Vector3 ccCameraSpacePos = UnProjectVector(_projectionMatrix, new Vector3(0.0f, 0.0f, OverlayDistance));
                Vector3 tlCameraSpacePos = UnProjectVector(_projectionMatrix, new Vector3(-OverlayDistance, OverlayDistance, OverlayDistance));

                //position
                Vector3 position = cameraToWorldMatrix.MultiplyPoint3x4(ccCameraSpacePos);
                gameObject.transform.position = position;

                //scale
                Vector3 scale = new Vector3(Mathf.Abs(tlCameraSpacePos.x - ccCameraSpacePos.x) * 2, Mathf.Abs(tlCameraSpacePos.y - ccCameraSpacePos.y) * 2, 1);
                gameObject.transform.localScale = scale;

                // Rotate the canvas object so that it faces the user.
                Quaternion rotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));
                gameObject.transform.rotation = rotation;
            }
        }
        private Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to)
        {
            Vector3 from = new Vector3(0, 0, 0);
            var axsX = proj.GetRow(0);
            var axsY = proj.GetRow(1);
            var axsZ = proj.GetRow(2);
            from.z = to.z / axsZ.z;
            from.y = (to.y - (from.z * axsY.z)) / axsY.y;
            from.x = (to.x - (from.z * axsX.z)) / axsX.x;
            return from;
        }

        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = _webCamTextureToMatHelper.GetDownScaleMat(_webCamTextureToMatHelper.GetMat());

            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            _projectionMatrix = ConstValue.GetProjectMatrix();

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

            StopThread();

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

        private void StartThread(Action action)
        {
#if UNITY_METRO && NETFX_CORE
            System.Threading.Tasks.Task.Run(() => action());
#elif UNITY_METRO
            action.BeginInvoke(ar => action.EndInvoke(ar), null);
#else
            ThreadPool.QueueUserWorkItem (_ => action());
#endif
        }

        private void StopThread()
        {
            if (_isThreadRunning == false)
                return;

            while (isThreadRunning)
            {
                //Wait threading stop
            }
        }
        private void ThreadWorker()
        {
            isThreadRunning = true;

            DetectObject();

            lock (_sync)
            {
                if (ExecuteOnMainThread.Count == 0)
                {
                    ExecuteOnMainThread.Enqueue(() => {
                        OnDetectionDone();
                    });
                }
            }

            isThreadRunning = false;
        }
        private void OnDetectionDone()
        {
            _hasUpdatedDetectionResult = true;

            _isDetecting = false;
        }
        private void DetectObject()
        {
            MatOfRect objects = new MatOfRect();
            if (_cascade4Thread != null)
            {
                var matSize = new Size(_grayMat.cols() * MinDetectionSizeRatio, _grayMat.rows() * MinDetectionSizeRatio);
                _cascade4Thread.detectMultiScale(_grayMat, objects, 1.1, 2,
                    Objdetect.CASCADE_SCALE_IMAGE, // TODO: objdetect.CV_HAAR_SCALE_IMAGE
                    matSize,
                    new Size());
            }
            _detectionResult = objects;
        }
    }
}
