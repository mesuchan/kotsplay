using System;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Org.Opencv.Android;
using Org.Opencv.Core;
using Android.Util;
using Org.Opencv.Imgproc;
using Size = Org.Opencv.Core.Size;
using Java.Util;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Widget;

namespace kotsplay.camera
{
    [Activity(Label = "CameraViewActivity")]
    public class CameraViewActivity : Activity, Org.Opencv.Android.CameraBridgeViewBase.ICvCameraViewListener2
    {
        public const string TAG = "CameraViewActivity::Activity";

        public const int VIEW_MODE_RGBA = 0;
        public const int VIEW_MODE_HIST = 1;
        public const int VIEW_MODE_CANNY = 2;
        public const int VIEW_MODE_SEPIA = 3;
        public const int VIEW_MODE_SOBEL = 4;
        public const int VIEW_MODE_ZOOM = 5;
        public const int VIEW_MODE_PIXELIZE = 6;
        public const int VIEW_MODE_POSTERIZE = 7;

        private IMenuItem mItemPreviewRGBA;
        private IMenuItem mItemPreviewHist;
        private IMenuItem mItemPreviewCanny;
        private IMenuItem mItemPreviewSepia;
        private IMenuItem mItemPreviewSobel;
        private IMenuItem mItemPreviewZoom;
        private IMenuItem mItemPreviewPixelize;
        private IMenuItem mItemPreviewPosterize;
        private CameraBridgeViewBase mOpenCvCameraView;

        private Size mSize0;

        private Mat mIntermediateMat;
        private Mat mMat0;
        private MatOfInt[] mChannels;
        private MatOfInt mHistSize;
        private int mHistSizeNum = 25;
        private MatOfFloat mRanges;
        private Scalar[] mColorsRGB;
        private Scalar[] mColorsHue;
        private Scalar mWhilte;
        private Point mP1;
        private Point mP2;
        private float[] mBuff;
        private Mat mSepiaKernel;

        public static int viewMode = VIEW_MODE_RGBA;
        private Callback mLoaderCallback;

        private Mat rgba;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Info(TAG, "called onCreate");
            base.OnCreate(savedInstanceState);

            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.activity_cameraview);
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;

            //Check if permission is already granted
            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this,
                            Android.Manifest.Permission.Camera)
                    != Android.Content.PM.Permission.Granted)
            {

                // Give first an explanation, if needed.
                if (Android.Support.V4.App.ActivityCompat.ShouldShowRequestPermissionRationale(this,
                        Android.Manifest.Permission.Camera))
                {

                    // Show an explanation to the user *asynchronously* -- don't block
                    // this thread waiting for the user's response! After the user
                    // sees the explanation, try again to request the permission.

                }
                else
                {

                    // No explanation needed, we can request the permission.
                    Android.Support.V4.App.ActivityCompat.RequestPermissions(this,
                            new String[] { Android.Manifest.Permission.Camera },
                            1);
                }
            }

            mOpenCvCameraView = FindViewById<CameraBridgeViewBase>(Resource.Id.cameraview_id);
            mOpenCvCameraView.Visibility = ViewStates.Visible;
            mOpenCvCameraView.SetCvCameraViewListener(this);
            mLoaderCallback = new Callback(this, mOpenCvCameraView);

            // Force fullscreen
            Window.DecorView.SystemUiVisibility = Android.Views.StatusBarVisibility.Hidden;
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);

            Button button = FindViewById<Button>(Resource.Id.buttonShot);
            button.Click += (sender, args) =>
            {
                using (var client = new HttpClient())
                {
                    var content = new ByteArrayContent(Encoding.ASCII.GetBytes(rgba.Dump()));
                    var t = client.PostAsync("http://192.168.43.151:5000", content);
                }
            };
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (mOpenCvCameraView != null)
                mOpenCvCameraView.DisableView();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!OpenCVLoader.InitDebug())
            {
                Log.Debug(TAG, "Internal OpenCV library not found. Using OpenCV Manager for initialization");
                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion310, this, mLoaderCallback);
            }
            else
            {
                Log.Debug(TAG, "OpenCV library found inside package. Using it!");
                mLoaderCallback.OnManagerConnected(LoaderCallbackInterface.Success);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (mOpenCvCameraView != null)
                mOpenCvCameraView.DisableView();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            Log.Info(TAG, "called onCreateOptionsMenu");
            mItemPreviewRGBA = menu.Add("Preview RGBA");
            mItemPreviewHist = menu.Add("Histograms");
            mItemPreviewCanny = menu.Add("Canny");
            mItemPreviewSepia = menu.Add("Sepia");
            mItemPreviewSobel = menu.Add("Sobel");
            mItemPreviewZoom = menu.Add("Zoom");
            mItemPreviewPixelize = menu.Add("Pixelize");
            mItemPreviewPosterize = menu.Add("Posterize");
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Log.Info(TAG, "called onOptionsItemSelected; selected item: " + item);
            if (item == mItemPreviewRGBA)
                viewMode = VIEW_MODE_RGBA;
            if (item == mItemPreviewHist)
                viewMode = VIEW_MODE_HIST;
            else if (item == mItemPreviewCanny)
                viewMode = VIEW_MODE_CANNY;
            else if (item == mItemPreviewSepia)
                viewMode = VIEW_MODE_SEPIA;
            else if (item == mItemPreviewSobel)
                viewMode = VIEW_MODE_SOBEL;
            else if (item == mItemPreviewZoom)
                viewMode = VIEW_MODE_ZOOM;
            else if (item == mItemPreviewPixelize)
                viewMode = VIEW_MODE_PIXELIZE;
            else if (item == mItemPreviewPosterize)
                viewMode = VIEW_MODE_POSTERIZE;
            return true;
        }

        public void OnCameraViewStarted(int width, int height)
        {
            mIntermediateMat = new Mat();
            mSize0 = new Size();
            mChannels = new MatOfInt[] { new MatOfInt(0), new MatOfInt(1), new MatOfInt(2) };
            mBuff = new float[mHistSizeNum];
            mHistSize = new MatOfInt(mHistSizeNum);
            mRanges = new MatOfFloat(0f, 256f);
            mMat0 = new Mat();
            mColorsRGB = new Scalar[] { new Scalar(200, 0, 0, 255), new Scalar(0, 200, 0, 255), new Scalar(0, 0, 200, 255) };
            mColorsHue = new Scalar[] {
                new Scalar(255, 0, 0, 255),   new Scalar(255, 60, 0, 255),  new Scalar(255, 120, 0, 255), new Scalar(255, 180, 0, 255), new Scalar(255, 240, 0, 255),
                new Scalar(215, 213, 0, 255), new Scalar(150, 255, 0, 255), new Scalar(85, 255, 0, 255),  new Scalar(20, 255, 0, 255),  new Scalar(0, 255, 30, 255),
                new Scalar(0, 255, 85, 255),  new Scalar(0, 255, 150, 255), new Scalar(0, 255, 215, 255), new Scalar(0, 234, 255, 255), new Scalar(0, 170, 255, 255),
                new Scalar(0, 120, 255, 255), new Scalar(0, 60, 255, 255),  new Scalar(0, 0, 255, 255),   new Scalar(64, 0, 255, 255),  new Scalar(120, 0, 255, 255),
                new Scalar(180, 0, 255, 255), new Scalar(255, 0, 255, 255), new Scalar(255, 0, 215, 255), new Scalar(255, 0, 85, 255),  new Scalar(255, 0, 0, 255)
        };
            mWhilte = Scalar.All(255);
            mP1 = new Point();
            mP2 = new Point();

            // Fill sepia kernel
            mSepiaKernel = new Mat(4, 4, CvType.Cv32f);
            mSepiaKernel.Put(0, 0, /* R */0.189f, 0.769f, 0.393f, 0f);
            mSepiaKernel.Put(1, 0, /* G */0.168f, 0.686f, 0.349f, 0f);
            mSepiaKernel.Put(2, 0, /* B */0.131f, 0.534f, 0.272f, 0f);
            mSepiaKernel.Put(3, 0, /* A */0.000f, 0.000f, 0.000f, 1f);
        }

        public void OnCameraViewStopped()
        {
            // Explicitly deallocate Mats
            if (mIntermediateMat != null)
                mIntermediateMat.Release();

            mIntermediateMat = null;
        }

        public Mat OnCameraFrame(CameraBridgeViewBase.ICvCameraViewFrame inputFrame)
        {
            rgba = inputFrame.Rgba();
            Size sizeRgba = rgba.Size();

            int rows = (int)sizeRgba.Height;
            int cols = (int)sizeRgba.Width;
            //client.UploadDataAsync(new Uri("http://192.168.43.151:5000"), Encoding.ASCII.GetBytes(rgba.Dump()));

            Point center = new Point(100, 200);
            int radius = 100;
            Scalar color = new Scalar(0, 255, 0);

            // canvas, center, radius, color, linewidth
            Imgproc.Circle(rgba, center, radius, color, 5);

            center.X = 400;
            center.Y = 700;

            color.Val[0] = 255;

            // canvas, string, left up, font id, size, color
            Imgproc.PutText(rgba, "Surprise,\nmotherfucker!", center, 1, 4, color, 16);

            return rgba;
        }
    }

    class Callback : BaseLoaderCallback
    {
        private readonly CameraBridgeViewBase mOpenCvCameraView;
        public Callback(Context context, CameraBridgeViewBase cameraView)
            : base(context)
        {
            mOpenCvCameraView = cameraView;
        }

        public override void OnManagerConnected(int status)
        {
            switch (status)
            {
                case LoaderCallbackInterface.Success:
                    {
                        Log.Info(CameraViewActivity.TAG, "OpenCV loaded successfully");
                        mOpenCvCameraView.EnableView();
                    }
                    break;
                default:
                    {
                        base.OnManagerConnected(status);
                    }
                    break;
            }
        }
    }
}