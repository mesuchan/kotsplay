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
                OpenCVLoader.InitAsync(OpenCVLoader.OpencvVersion300, this, mLoaderCallback);
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
            Mat rgba = inputFrame.Rgba();
            Size sizeRgba = rgba.Size();

            Mat rgbaInnerWindow;

            int rows = (int)sizeRgba.Height;
            int cols = (int)sizeRgba.Width;

            int left = cols / 8;
            int top = rows / 8;

            int width = cols * 3 / 4;
            int height = rows * 3 / 4;

            switch (CameraViewActivity.viewMode)
            {
                case CameraViewActivity.VIEW_MODE_RGBA:
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
                    break;

                case CameraViewActivity.VIEW_MODE_HIST:
                    Mat hist = new Mat();
                    int thikness = (int)(sizeRgba.Width / (mHistSizeNum + 10) / 5);
                    if (thikness > 5) thikness = 5;
                    int offset = (int)((sizeRgba.Width - (5 * mHistSizeNum + 4 * 10) * thikness) / 2);
                    // RGB
                    for (int c = 0; c < 3; c++)
                    {
                        Imgproc.CalcHist(Arrays.AsList(rgba).Cast<Mat>().ToList(), mChannels[c], mMat0, hist, mHistSize, mRanges);
                        Core.Normalize(hist, hist, sizeRgba.Height / 2, 0, Core.NormInf);
                        hist.Get(0, 0, mBuff);
                        for (int h = 0; h < mHistSizeNum; h++)
                        {
                            mP1.X = mP2.X = offset + (c * (mHistSizeNum + 10) + h) * thikness;
                            mP1.Y = sizeRgba.Height - 1;
                            mP2.Y = mP1.Y - 2 - (int)mBuff[h];
                            Imgproc.Line(rgba, mP1, mP2, mColorsRGB[c], thikness);
                        }
                    }
                    // Value and Hue
                    Imgproc.CvtColor(rgba, mIntermediateMat, Imgproc.ColorRgb2hsvFull);
                    // Value
                    Imgproc.CalcHist(Arrays.AsList(mIntermediateMat).Cast<Mat>().ToList(), mChannels[2], mMat0, hist, mHistSize, mRanges);
                    Core.Normalize(hist, hist, sizeRgba.Height / 2, 0, Core.NormInf);
                    hist.Get(0, 0, mBuff);
                    for (int h = 0; h < mHistSizeNum; h++)
                    {
                        mP1.X = mP2.X = offset + (3 * (mHistSizeNum + 10) + h) * thikness;
                        mP1.Y = sizeRgba.Height - 1;
                        mP2.Y = mP1.Y - 2 - (int)mBuff[h];
                        Imgproc.Line(rgba, mP1, mP2, mWhilte, thikness);
                    }
                    // Hue
                    Imgproc.CalcHist(Arrays.AsList(mIntermediateMat).Cast<Mat>().ToList(), mChannels[0], mMat0, hist, mHistSize, mRanges);
                    Core.Normalize(hist, hist, sizeRgba.Height / 2, 0, Core.NormInf);
                    hist.Get(0, 0, mBuff);
                    for (int h = 0; h < mHistSizeNum; h++)
                    {
                        mP1.X = mP2.X = offset + (4 * (mHistSizeNum + 10) + h) * thikness;
                        mP1.Y = sizeRgba.Height - 1;
                        mP2.Y = mP1.Y - 2 - (int)mBuff[h];
                        Imgproc.Line(rgba, mP1, mP2, mColorsHue[h], thikness);
                    }
                    break;

                case CameraViewActivity.VIEW_MODE_CANNY:
                    rgbaInnerWindow = rgba.Submat(top, top + height, left, left + width);
                    Imgproc.Canny(rgbaInnerWindow, mIntermediateMat, 80, 90);
                    Imgproc.CvtColor(mIntermediateMat, rgbaInnerWindow, Imgproc.ColorGray2bgra, 4);
                    rgbaInnerWindow.Release();
                    break;

                case CameraViewActivity.VIEW_MODE_SOBEL:
                    Mat gray = inputFrame.Gray();
                    Mat grayInnerWindow = gray.Submat(top, top + height, left, left + width);
                    rgbaInnerWindow = rgba.Submat(top, top + height, left, left + width);
                    Imgproc.Sobel(grayInnerWindow, mIntermediateMat, CvType.Cv8u, 1, 1);
                    Core.ConvertScaleAbs(mIntermediateMat, mIntermediateMat, 10, 0);
                    Imgproc.CvtColor(mIntermediateMat, rgbaInnerWindow, Imgproc.ColorGray2bgra, 4);
                    grayInnerWindow.Release();
                    rgbaInnerWindow.Release();
                    break;

                case CameraViewActivity.VIEW_MODE_SEPIA:
                    rgbaInnerWindow = rgba.Submat(top, top + height, left, left + width);
                    Core.Transform(rgbaInnerWindow, rgbaInnerWindow, mSepiaKernel);
                    rgbaInnerWindow.Release();
                    break;

                case CameraViewActivity.VIEW_MODE_ZOOM:
                    Mat zoomCorner = rgba.Submat(0, rows / 2 - rows / 10, 0, cols / 2 - cols / 10);
                    Mat mZoomWindow = rgba.Submat(rows / 2 - 9 * rows / 100, rows / 2 + 9 * rows / 100, cols / 2 - 9 * cols / 100, cols / 2 + 9 * cols / 100);
                    Imgproc.Resize(mZoomWindow, zoomCorner, zoomCorner.Size());
                    Size wsize = mZoomWindow.Size();
                    Imgproc.Rectangle(mZoomWindow, new Point(1, 1), new Point(wsize.Width - 2, wsize.Height - 2), new Scalar(255, 0, 0, 255), 2);
                    zoomCorner.Release();
                    mZoomWindow.Release();
                    break;

                case CameraViewActivity.VIEW_MODE_PIXELIZE:
                    rgbaInnerWindow = rgba.Submat(top, top + height, left, left + width);
                    Imgproc.Resize(rgbaInnerWindow, mIntermediateMat, mSize0, 0.1, 0.1, Imgproc.InterNearest);
                    Imgproc.Resize(mIntermediateMat, rgbaInnerWindow, rgbaInnerWindow.Size(), 0.0, 0.0, Imgproc.InterNearest);
                    rgbaInnerWindow.Release();
                    break;

                case CameraViewActivity.VIEW_MODE_POSTERIZE:
                    /*
                    Imgproc.cvtColor(rgbaInnerWindow, mIntermediateMat, Imgproc.COLOR_RGBA2RGB);
                    Imgproc.pyrMeanShiftFiltering(mIntermediateMat, mIntermediateMat, 5, 50);
                    Imgproc.cvtColor(mIntermediateMat, rgbaInnerWindow, Imgproc.COLOR_RGB2RGBA);
                    */
                    rgbaInnerWindow = rgba.Submat(top, top + height, left, left + width);
                    Imgproc.Canny(rgbaInnerWindow, mIntermediateMat, 80, 90);
                    rgbaInnerWindow.SetTo(new Scalar(0, 0, 0, 255), mIntermediateMat);
                    Core.ConvertScaleAbs(rgbaInnerWindow, mIntermediateMat, 1.0 / 16, 0);
                    Core.ConvertScaleAbs(mIntermediateMat, rgbaInnerWindow, 16, 0);
                    rgbaInnerWindow.Release();
                    break;
            }

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