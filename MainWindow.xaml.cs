using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;

using System;


namespace AngryBirdsPro
{
    public partial class MainWindow : Window
    {
        private Point _startPoint;
        private bool _isDragging = false;
        private readonly DispatcherTimer _timer;
        private Vector _velocity;
        private Point _birdPosition;
        private readonly double _gravity = 0.5;
        private const int trajectoryPointCount = 20; // Количество точек траектории
        private readonly Ellipse[] _trajectoryPoints;

        public MainWindow()
        {
            InitializeComponent();
            gameCanvas.MouseDown += GameCanvas_MouseDown;
            gameCanvas.MouseMove += GameCanvas_MouseMove;
            gameCanvas.MouseUp += GameCanvas_MouseUp;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(20);
            _timer.Tick += Timer_Tick;

            _trajectoryPoints = new Ellipse[trajectoryPointCount];
            for (int i = 0; i < trajectoryPointCount; i++)
            {
                Ellipse point = new Ellipse
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Gray,
                    Visibility = Visibility.Hidden
                };
                gameCanvas.Children.Add(point);
                _trajectoryPoints[i] = point;
            }
        }

        private void GameCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(gameCanvas);
            _birdPosition = new Point(Canvas.GetLeft(bird), Canvas.GetTop(bird));
            if (IsPointInCircle(_startPoint, _birdPosition, 70))
            {
                _isDragging = true;
            }
        }

        private void GameCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPoint = e.GetPosition(gameCanvas);
                Vector dragVector = _startPoint - currentPoint;
                bird.SetValue(Canvas.LeftProperty, _startPoint.X - bird.Width / 2 - dragVector.X);
                bird.SetValue(Canvas.TopProperty, _startPoint.Y - bird.Height / 2 - dragVector.Y);
                UpdateTrajectory(dragVector);
            }
        }

        private void GameCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Point releasePoint = e.GetPosition(gameCanvas);
                Vector launchVector = _startPoint - releasePoint;
                _velocity = launchVector / 10; // Скорость запуска
                _birdPosition = new Point(Canvas.GetLeft(bird), Canvas.GetTop(bird));
                ClearTrajectoryPoints();
                _timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _birdPosition += _velocity;
            _velocity.Y += _gravity; 

            bird.SetValue(Canvas.LeftProperty, _birdPosition.X);
            bird.SetValue(Canvas.TopProperty, _birdPosition.Y);

            CheckCollision();
        }

        private void CheckCollision()
        {
            Rect birdRect = new Rect(_birdPosition.X, _birdPosition.Y, bird.Width / 1.5, bird.Height / 1.5);
            Rect pigRect = new Rect(Canvas.GetLeft(pig), Canvas.GetTop(pig), pig.Width / 1.5, pig.Height / 1.5);
            Rect landRect = new Rect(Canvas.GetLeft(land), Canvas.GetTop(land), land.Width, land.Height);
            Rect kamniRect = new Rect(Canvas.GetLeft(kamni), Canvas.GetTop(kamni), kamni.Width-50, kamni.Height);
            


            if (birdRect.IntersectsWith(pigRect))
            {
                _timer.Stop();
                MessageBox.Show("Победа!");
                ResetGame();
            }
            if (birdRect.IntersectsWith(landRect))
            {
                _timer.Stop();
                MessageBox.Show("Проигрыш.Попробуйте еще раз");
                ResetGame();
            }
            if (birdRect.IntersectsWith(kamniRect)) 
            {
                ChangeFlightDirection();
            }

            if (_birdPosition.Y > gameCanvas.ActualHeight || _birdPosition.X > gameCanvas.ActualWidth)
            {
                _timer.Stop();
                ResetGame();
            }
        }

        private void ResetGame()
        {
            bird.SetValue(Canvas.LeftProperty, 160.0);
            bird.SetValue(Canvas.TopProperty, 355.0);
        }

        private void UpdateTrajectory(Vector dragVector)
        {
            Point start = new Point(Canvas.GetLeft(bird) + bird.Width / 2, Canvas.GetTop(bird) + bird.Height / 2);
            Vector velocity = dragVector / 10;
            for (int i = 0; i < trajectoryPointCount; i++)
            {
                double t = i * 4;
                double x = start.X + velocity.X * t;
                double y = start.Y + velocity.Y * t + 0.5 * _gravity * t * t;
                _trajectoryPoints[i].SetValue(Canvas.LeftProperty, x - _trajectoryPoints[i].Width / 2);
                _trajectoryPoints[i].SetValue(Canvas.TopProperty, y - _trajectoryPoints[i].Height / 2);
                _trajectoryPoints[i].Visibility = Visibility.Visible;
            }
        }
        private void ChangeFlightDirection()
        {
            _velocity.X = -_velocity.X * 0.9;
        }

        private bool IsPointInCircle(Point point, Point circleCenter, double radius)
        {
            double dx = point.X - circleCenter.X;
            double dy = point.Y - circleCenter.Y;
            return (dx * dx + dy * dy) <= (radius * radius);
        }


        private void ClearTrajectoryPoints()
        {
            foreach (var point in _trajectoryPoints)
            {
                point.Visibility = Visibility.Hidden;
            }
        }
    }
}

