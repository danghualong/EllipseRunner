using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EllipseRunner
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged
    {
        private static int LINE_WIDTH = 200;
        private static int RADIUS = 40;
        private static int OFFSET_X = 100;
        private static int OFFSET_Y = 40;
        private static int TOTAL_SECONDS = 10;
        private static Size CIRCLE_SIZE = new Size(RADIUS, RADIUS);
        private double v = 0;


        private BackgroundWorker worker;
        private bool isRunning;
        private double totalMilliSeconds;

        public Point TopLineStartPoint
        {
            get;set;
        }
        public Point TopLineStopPoint
        {
            get; set;
        }
        public Point BottomLineStartPoint
        {
            get; set;
        }
        public Point BottomLineStopPoint
        {
            get; set;
        }
        private Point startPoint;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            TopLineStartPoint = new Point(RADIUS + OFFSET_X, 0 + OFFSET_Y);
            TopLineStopPoint = new Point(RADIUS+LINE_WIDTH + OFFSET_X, 0 + OFFSET_Y);
            BottomLineStartPoint = new Point(RADIUS + OFFSET_X, 0+2*RADIUS + OFFSET_Y);
            BottomLineStopPoint = new Point(RADIUS + LINE_WIDTH + OFFSET_X, 0 + 2 * RADIUS + OFFSET_Y);
            startPoint = new Point((BottomLineStartPoint.X + BottomLineStopPoint.X) / 2, BottomLineStartPoint.Y);
            pfLeft.StartPoint = TopLineStartPoint;
            pfRight.StartPoint = TopLineStopPoint;
            pfLeft.Segments.Add(new ArcSegment(BottomLineStartPoint, CIRCLE_SIZE, 0, false, SweepDirection.Counterclockwise, true));
            pfRight.Segments.Add(new ArcSegment(BottomLineStopPoint, CIRCLE_SIZE, 0, false, SweepDirection.Clockwise, true));
            v = (RADIUS * Math.PI + LINE_WIDTH) * 2 / TOTAL_SECONDS;
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            
            //var p = new Point(a,b);
            //p.Offset(offset.Width, offset.Height);
            //eg.Center = p;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            showPath(totalMilliSeconds);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int interval = 100;
            while (isRunning)
            {
                totalMilliSeconds += interval;
                totalMilliSeconds = totalMilliSeconds % (1000* TOTAL_SECONDS);
                worker.ReportProgress(0);
                Thread.Sleep(interval);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            Stop();
            double degree = 0;
            double.TryParse(tb.Text, out degree);
            degree = (degree % TOTAL_SECONDS)*1000;
            showPath(degree);
        }

        private void showPath(double milliSeconds)
        {
            var distance = milliSeconds * v / 1000;

            pg.Figures.Clear();
            AddArcFigure(pg, distance);

        }

        private void AddArcFigure(PathGeometry pg,double distance)
        {
            var figure = new PathFigure();
            figure.StartPoint = startPoint;
            if (distance <= LINE_WIDTH / 2)
            {
                LineSegment line = new LineSegment();
                line.Point= new Point(startPoint.X + distance, startPoint.Y);
                figure.Segments.Add(line);
            }
            else if (distance <= LINE_WIDTH / 2 + Math.PI * RADIUS)
            {
                LineSegment line = new LineSegment();
                line.Point = new Point(BottomLineStopPoint.X, BottomLineStopPoint.Y);
                figure.Segments.Add(line);
                double s = distance - LINE_WIDTH / 2;
                double x = BottomLineStopPoint.X+RADIUS * Math.Sin(s / RADIUS);
                double y = BottomLineStopPoint.Y - RADIUS + RADIUS * Math.Cos(s / RADIUS);
                var segment = new ArcSegment(new Point(x, y), CIRCLE_SIZE, 0, false, SweepDirection.Counterclockwise, true);
                figure.Segments.Add(segment);
            }
            else if (distance <= LINE_WIDTH * 3 / 2 + Math.PI * RADIUS)
            {
                LineSegment line = new LineSegment();
                line.Point = new Point(BottomLineStopPoint.X, startPoint.Y);
                figure.Segments.Add(line);
                double s = Math.PI * RADIUS;
                double x = RADIUS * Math.Sin(s / RADIUS) + BottomLineStopPoint.X;
                double y = BottomLineStopPoint.Y - RADIUS + RADIUS * Math.Cos(s / RADIUS);
                var segment = new ArcSegment(new Point(x, y), CIRCLE_SIZE, 0, false, SweepDirection.Counterclockwise, true);
                figure.Segments.Add(segment);
                s= distance - LINE_WIDTH / 2- Math.PI * RADIUS;
                line = new LineSegment();
                line.Point = new Point(TopLineStopPoint.X-s, TopLineStopPoint.Y);
                figure.Segments.Add(line);

            }
            else if (distance <= LINE_WIDTH * 3 / 2 + Math.PI * RADIUS * 2)
            {
                LineSegment line = new LineSegment();
                line.Point = new Point(BottomLineStopPoint.X, startPoint.Y);
                figure.Segments.Add(line);
                double s = Math.PI * RADIUS;
                double x = RADIUS * Math.Sin(s / RADIUS) + BottomLineStopPoint.X;
                double y = BottomLineStopPoint.Y - RADIUS+RADIUS * Math.Cos(s / RADIUS);
                var segment = new ArcSegment(new Point(x, y), CIRCLE_SIZE, 0, false, SweepDirection.Counterclockwise, true);
                figure.Segments.Add(segment);
                s =LINE_WIDTH;
                line = new LineSegment();
                line.Point = new Point(TopLineStopPoint.X - s, TopLineStopPoint.Y);
                figure.Segments.Add(line);
                s = distance - 3*LINE_WIDTH / 2 - Math.PI * RADIUS;
                x = TopLineStartPoint.X-RADIUS * Math.Sin(s / RADIUS);
                y = TopLineStartPoint.Y + RADIUS-RADIUS * Math.Cos(s / RADIUS);
                segment = new ArcSegment(new Point(x, y), CIRCLE_SIZE, 0, false, SweepDirection.Counterclockwise, true);
                figure.Segments.Add(segment);
            }
            else
            {
                LineSegment line = new LineSegment();
                line.Point = new Point(BottomLineStopPoint.X, startPoint.Y);
                figure.Segments.Add(line);
                double s = Math.PI * RADIUS;
                double x = RADIUS * Math.Sin(s / RADIUS) + BottomLineStopPoint.X;
                double y = BottomLineStopPoint.Y - RADIUS + RADIUS * Math.Cos(s / RADIUS);
                var segment = new ArcSegment(new Point(x, y), CIRCLE_SIZE, 0, false, SweepDirection.Counterclockwise, true);
                figure.Segments.Add(segment);
                s = LINE_WIDTH;
                line = new LineSegment();
                line.Point = new Point(TopLineStopPoint.X - s, TopLineStopPoint.Y);
                figure.Segments.Add(line);
                s = Math.PI * RADIUS;
                x = TopLineStartPoint.X - RADIUS * Math.Sin(s / RADIUS);
                y = TopLineStartPoint.Y + RADIUS - RADIUS * Math.Cos(s / RADIUS);
                segment = new ArcSegment(new Point(x, y), CIRCLE_SIZE, 0, false, SweepDirection.Counterclockwise, true);
                figure.Segments.Add(segment);

                s = distance - 3 * LINE_WIDTH / 2 - 2*Math.PI * RADIUS;
                line = new LineSegment();
                line.Point = new Point(BottomLineStartPoint.X +s, BottomLineStartPoint.Y);
                figure.Segments.Add(line);
            }
            pg.Figures.Add(figure);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content.Equals("开始"))
            {
                Start();
                btnStart.Content = "停止";
            }
            else
            {
                Stop();
                btnStart.Content = "开始";
            }
        }

        private void Start()
        {
            isRunning = true;
            totalMilliSeconds = 0;
            worker.RunWorkerAsync();
        }

        private void Stop()
        {
            isRunning = false;
        }
    }
}
