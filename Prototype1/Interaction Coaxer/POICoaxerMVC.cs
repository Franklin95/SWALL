using System;
using Microsoft.Kinect;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
//https://github.com/XamlAnimatedGif/WpfAnimatedGif
using WpfAnimatedGif;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace InteractionCoaxer.PoiCoaxer
{

    #region Models

    /// <summary>
    /// Model holding array of points of interest to be used by Interaction Coaxer
    /// </summary>
    public class PoiCoaxerModel : ICoaxerModel
    {
        /// <summary>
        /// Represent an invalid point for this model
        /// </summary>
        public readonly static Point InvalidPoint = new Point(double.NaN, double.NaN);

        /// <summary>
        /// List of observers to update when model has changed
        /// </summary>
        protected event ModelHandler<ICoaxerModel> changed;

        protected Point[] _poi = null;

        /// <summary>
        /// The Points of Interest
        /// </summary>
        public Point[] Pois { get { return _poi; } }

        private int _PoiCount = 0;

        private double planerWidth;
        private double planerHeight;

        public int POICount
        {
            get { return _PoiCount; }
            set { _PoiCount = value; }
        }

        /// <summary>
        /// Width that the POIs are relative to
        /// </summary>
        public double PlanerWidth
        {
            get { return planerWidth; }
            set { planerWidth = value; }
        }

        /// <summary>
        /// Height that the POIs are relative to
        /// </summary>
        public double PlanerHeight
        {
            get { return planerHeight; }
            set { planerHeight = value; }
        }

        public PoiCoaxerModel()
        {
            planerWidth = 1920;
            planerHeight = 1080;
        }

        public void AddObserver(ICoaxerModelObserver observer)
        {
            changed += observer.ModelUpdated;
        }

        protected void NotifyObservers()
        {
            changed.Invoke(this, null);
        }

        public void decodeAndUpdate(byte[] data)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                _poi = (Point[])binForm.Deserialize(memStream);
                NotifyObservers();
            }
        }

        public byte[] encode()
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, _poi);
                return ms.ToArray();
            }
        }

    }

    /// <summary>
    /// Model to get POIS from the body data taken from a Kinect v2
    /// </summary>
    public class PoiFromKinectBodyCoaxerModel : PoiCoaxerModel
    {

        private JointType _trackingJoint;

        private CoordinateMapper mapper;

        /// <summary>
        /// The joint that will be used to get the POIs
        /// </summary>
        public JointType TrackingJoint
        {
            get { return _trackingJoint; }
            set { _trackingJoint = value; }
        }

        /// <summary>
        /// Mapper used to map joint position to position on screen
        /// </summary>
        public CoordinateMapper Mapper
        {
            get { return mapper; }
            set { mapper = value; }
        }

        public void Update(Body[] bodies)
        {

            Point[] newPOI = new Point[POICount];  //create Point for each body
            ColorSpacePoint point;
            Body body;

            for (int i = 0; i < POICount; ++i)
            {
                body = bodies[i];
                if (body.IsTracked &&
                    (body.Joints[_trackingJoint].TrackingState == TrackingState.Tracked ||
                     body.Joints[_trackingJoint].TrackingState == TrackingState.Inferred))
                {
                    point = Mapper.MapCameraPointToColorSpace(body.Joints[_trackingJoint].Position);
                    newPOI[i] = new Point(point.X, point.Y); //Add the position of the tracked joint to the array
                }
                else
                {
                    newPOI[i] = InvalidPoint;   //if the body or joint is not tracked, then set the point to an invalid point
                }
            }

            _poi = newPOI;
        }

    }

    #endregion

    #region Views

    /// <summary>
    /// View to have animated birds follow POIs
    /// </summary>
    public class BirdFollowingPoiCoaxerView : CoaxerViewBase
    {

        private Image[] birds = null;
        private double halfBirdHeight;
        private double halfBirdWidth;
        private int length;

        private int monitorCount;
        private int position;

        public override ICoaxerModel getModel()
        {
            return new PoiCoaxerModel();
        }

        public BirdFollowingPoiCoaxerView(int MonitorCount, int Position)
        {
            position = Position;
            monitorCount = MonitorCount;
        }

        public override void ModelUpdated(ICoaxerModel model, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())  //This section of code must be executed on the UI Thread
            {
                try
                {
                    Dispatcher.Invoke(new Action(() => { ModelUpdated(model, e); }));
                }
                catch (TaskCanceledException)
                {
                    //This would occur if the program were to exit during the Invoke method
                }
                return;
            }

            if (!(model is PoiCoaxerModel))
            {
                throw new ArgumentException("Model must be of type POICoaxerModel");
            }

            PoiCoaxerModel poiModel = (PoiCoaxerModel)model;

            Point[] pois = poiModel.Pois;

            if (birds == null)  //If on first update initialize all bird images
            {
                length = pois.Length;
                birds = new Image[length];
                BitmapImage image;
                for (int i = 0; i < length; i++)
                {
                    birds[i] = new Image() { Visibility = Visibility.Hidden, IsHitTestVisible = false };
                    image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri("pack://siteoforigin:,,,/Resources/bird.gif");
                    image.EndInit();
                    ImageBehavior.SetAnimatedSource(birds[i], image);
                    Children.Add(birds[i]);
                }
                halfBirdHeight = birds[0].ActualHeight / 2;
                halfBirdWidth = birds[0].ActualWidth / 2;
            }
            else if (pois.Length != length)
            {
                throw new ArgumentException("Lenght of Point-of-Interest array is expected to be same length every update");
            }

            Point point;
            double top;
            double left;

            //TODO: Modify to span accross all slave monitors

            //need to calcualte ratio in order to display bird in appropriate location on screen respective to user
            double xRatio = (Application.Current.MainWindow != null ? Application.Current.MainWindow.ActualWidth * monitorCount : 0
                            - halfBirdWidth * 2) / poiModel.PlanerWidth;
            double yRatio = (Application.Current.MainWindow != null ? Application.Current.MainWindow.ActualHeight : 0
                            - halfBirdHeight * 2) / poiModel.PlanerHeight;

            for (int i = 0; i < length; ++i)
            {
                point = pois[i];
                if (!Point.Equals(point, PoiCoaxerModel.InvalidPoint) && Application.Current.MainWindow != null)
                {
                    //convert poi to position on canvas
                    Point relativePoint = new Point(Math.Floor(point.X * xRatio) - (Application.Current.MainWindow.ActualWidth * (position - 1))
                        , Math.Floor(point.Y * yRatio));

                    //Console.WriteLine("x: " + point.X + " y: " + point.Y + " rx: " + relativePoint.X + " ry: " + relativePoint.Y + " w: " + ActualWidth + " h: " + ActualHeight);

                    //If bird was not visible/off screen, move it to bottom of screen
                    if (double.IsNaN(GetTop(birds[i])) || double.IsNaN(GetLeft(birds[i])))
                    {
                        SetTop(birds[i], ActualHeight);
                        SetLeft(birds[i], relativePoint.X - halfBirdWidth);
                    }

                    top = GetTop(birds[i]) - halfBirdHeight;
                    left = GetLeft(birds[i]) - halfBirdWidth;

                    //Move bird 1/15 towards its POI
                    SetTop(birds[i], (top + ((relativePoint.Y - top) / 15) - halfBirdHeight));
                    SetLeft(birds[i], (left + ((relativePoint.X - left) / 15) - halfBirdWidth));
                    birds[i].Opacity = 1.0;
                    birds[i].Visibility = Visibility.Visible;
                }
                else
                {
                    //If POI is no longer valid fade bird away
                    if (!double.IsNaN(GetTop(birds[i])) && !double.IsNaN(GetLeft(birds[i])))
                    {
                        top = GetTop(birds[i]);
                        SetTop(birds[i], top + ((ActualHeight - top) / 30));
                        birds[i].Opacity -= 0.02;
                        //If bird has faded or reached bottom of screen set to not visible and off screen
                        if (Math.Abs((ActualHeight - top)) < 10 || birds[i].Opacity <= 0.0)
                        {
                            SetTop(birds[i], double.NaN);
                            SetLeft(birds[i], double.NaN);
                            birds[i].Opacity = 1.0;
                            birds[i].Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
        }

    }

    /// <summary>
    /// View to have ellipses follow and rotate around POI
    /// </summary>
    public class CircularRotatorAroundPoiCoaxerView : CoaxerViewBase
    {
        private Ellipse[] ellipses;
        private double eHeight = 50;
        private double eWidth = 50;
        private int length;

        public override void ModelUpdated(ICoaxerModel model, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())  //This section of code must be executed on the UI Thread
            {
                try
                {
                    Dispatcher.Invoke(new Action(() => { ModelUpdated(model, e); }));
                }
                catch (TaskCanceledException)
                {
                    //This would occur if the program were to exit during the Invoke method
                }
                return;
            }

            if (!(model is PoiCoaxerModel))
            {
                throw new ArgumentException("Model must be of type POICoaxerModel");
            }

            PoiCoaxerModel poiModel = (PoiCoaxerModel)model;

            Point[] pois = poiModel.Pois;

            if (ellipses == null)  //If on first update initialize all ellipses
            {
                length = pois.Length;
                ellipses = new Ellipse[length];
                for (int i = 0; i < 6; i++)
                {
                    ellipses[i] = new Ellipse()
                    {
                        Width = eWidth
                        ,
                        Height = eHeight
                        ,
                        Visibility = Visibility.Hidden
                        ,
                        Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"))
                        ,
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"))
                    };
                    Children.Add(ellipses[i]);
                }
            }
            else if (pois.Length != length)
            {
                throw new ArgumentException("Lenght of Point-of-Interest array is expected to be same length every update");
            }

            Point point;
            double top;
            double left;

            //TODO: Modify to span accross all slave monitors

            //need to calcualte ratio in order to display ellipse in appropriate location on screen respective to user
            double xRatio = Application.Current.MainWindow != null ? Application.Current.MainWindow.ActualWidth : 0
                            / poiModel.PlanerWidth;
            double yRatio = Application.Current.MainWindow != null ? Application.Current.MainWindow.ActualHeight : 0
                            / poiModel.PlanerHeight;

            for (int i = 0; i < length; ++i)
            {
                point = pois[i];
                if (!Point.Equals(point, PoiCoaxerModel.InvalidPoint))
                {
                    //convert poi to position on canvas
                    Point centerPoint = new Point(Math.Floor(point.X * xRatio), Math.Floor(point.Y * yRatio));

                    //Console.WriteLine("x: " + point.X + " y: " + point.Y + " rx: " + relativePoint.X + " ry: " + relativePoint.Y + " w: " + ActualWidth + " h: " + ActualHeight);

                    //If ellipse was not visible/off screen, move it to bottom of screen
                    if (double.IsNaN(GetTop(ellipses[i])) || double.IsNaN(GetLeft(ellipses[i])))
                    {
                        SetTop(ellipses[i], centerPoint.Y + eHeight * 2);
                        SetLeft(ellipses[i], centerPoint.X + eWidth * 2);
                    }

                    top = GetTop(ellipses[i]);
                    left = GetLeft(ellipses[i]);

                    //calculate points in order to have ellipse rotate around the POI
                    Point currentEllipseRelativeToCenter = new Point((left - eWidth / 2) - centerPoint.X, (top - eHeight / 2) - centerPoint.Y);

                    Point newPointRelativeToCenter = RotatePoint(currentEllipseRelativeToCenter, 45);

                    Point newPointRelativeToScreen = new Point(newPointRelativeToCenter.X + centerPoint.X - eWidth / 2, newPointRelativeToCenter.Y + centerPoint.Y - eHeight / 2);


                    //Move half the distsance to the calculated point
                    SetTop(ellipses[i], (top + (newPointRelativeToScreen.Y - top) / 2));
                    SetLeft(ellipses[i], (left + (newPointRelativeToScreen.X - left) / 2));
                    ellipses[i].Visibility = Visibility.Visible;

                }
                else
                {
                    //if POI is no longer valid remove ellipse from the screen
                    SetTop(ellipses[i], double.NaN);
                    SetLeft(ellipses[i], double.NaN);
                    ellipses[i].Visibility = Visibility.Hidden;
                }
            }

        }

        /// <summary>
        /// Rotates one point around another
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="centerPoint">The center point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        /// <summary>
        /// Rotates a point around the origin
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        static Point RotatePoint(Point pointToRotate, double angleInDegrees)
        {
            return RotatePoint(pointToRotate, new Point(0, 0), angleInDegrees);
        }

        public override ICoaxerModel getModel()
        {
            return new PoiCoaxerModel(); ;
        }
    }

    #endregion

}
