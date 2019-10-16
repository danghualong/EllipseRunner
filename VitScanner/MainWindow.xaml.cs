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
        private double a=100;
        private double b=40;
        private Point startPoint;
        private Size offset;
        private BackgroundWorker worker;
        private bool isRunning;
        private double degrees;
        
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            offset = new Size(100, 40);
            startPoint = new Point(2*a, b);
            startPoint.Offset(offset.Width, offset.Height);
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            var p = new Point(a,b);
            p.Offset(offset.Width, offset.Height);
            eg.Center = p;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            showPath(degrees);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (isRunning)
            {
                degrees += 2;
                degrees = degrees % 360;
                worker.ReportProgress(0);
                Thread.Sleep(100);
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
            double degree = 0;
            double.TryParse(tb.Text, out degree);
            degree = degree % 360;
            
            showPath(degree);
        }

        private void showPath(double degree)
        {
            var r = degree / 180 * Math.PI;
            double x = a * b * Math.Cos(r) / Math.Sqrt(Math.Pow(a * Math.Sin(r), 2) + Math.Pow(b * Math.Cos(r), 2)) + a+offset.Width;
            double y = b - a * b * Math.Sin(r) / Math.Sqrt(Math.Pow(a * Math.Sin(r), 2) + Math.Pow(b * Math.Cos(r), 2))+offset.Height;
            var figures = new PathFigureCollection();
            var segment = new ArcSegment(new Point(x, y), new Size(a, b), 0, degree>180?true:false, SweepDirection.Counterclockwise, true);
            var figure = new PathFigure();
            figure.StartPoint = startPoint;
            figure.Segments.Add(segment);
            figures.Add(figure);
            pg.Figures = figures;
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
            degrees = 0;
            worker.RunWorkerAsync();
        }

        private void Stop()
        {
            isRunning = false;
        }
    }
}
