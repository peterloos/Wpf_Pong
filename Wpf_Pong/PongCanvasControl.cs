using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Wpf_Pong
{
    internal enum BallDirection { RightTop, RightBottom, LeftTop, LeftBottom }

    public class PongCanvasControl : Canvas
    {
        public const double CanvasWidth = 500.0;
        public const double CanvasHeight = 340.0;

        // miscellaneous constants
        private const int PaddleWidth = 20;
        private const int PaddleHeight = 80;
        private const int PaddleMargin = 5;
        private const int BallWidth = 20;
        private const int BallHeight = 20;

        // two paddles and ball
        private Rectangle leftPaddle;
        private Rectangle rightPaddle;
        private Ellipse ball;
        private double leftPaddleTop;
        private double rightPaddleTop;
        private double ballLeft;
        private double ballTop;

        private BallDirection dir;

        private bool isRightPaddleMoving;
        private bool isRunning;
        private bool isGameOver;

        private DispatcherTimer timer;
        private const int TimerIntervalMSecs = 5;
        private const double BallStep = 3.0;

        public PongCanvasControl ()
        {
            // adjusting pong canvas control
            this.Background = Brushes.DarkGray;

            // setup left paddle (without location)
            this.leftPaddle = new Rectangle();
            this.leftPaddle.Fill = Brushes.White;
            this.leftPaddle.Width = PaddleWidth;
            this.leftPaddle.Height = PaddleHeight;
            this.Children.Add(this.leftPaddle);

            // setup right paddle (without location)
            this.rightPaddle = new Rectangle();
            this.rightPaddle.Fill = Brushes.White;
            this.rightPaddle.Width = PaddleWidth;
            this.rightPaddle.Height = PaddleHeight;
            this.Children.Add(this.rightPaddle);

            // setup ball (without location)
            this.ball = new Ellipse();
            this.ball.Fill = Brushes.White;
            this.ball.Width = BallWidth;
            this.ball.Height = BallHeight;
            this.Children.Add(this.ball);

            // timer setup
            this.timer = new DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            this.timer.Tick += this.PongTimerTick;

            // register event handler
            this.MouseMove += this.PongCanvasControl_MouseMove;
            this.Loaded += this.PongCanvasControl_Loaded;
        }

        private void PongCanvasControl_Loaded(Object sender, RoutedEventArgs e)
        {
            // actual width and height are now known:
            // setup locations of paddles and ball
            this.InitControls();
        }

        private void InitControls()
        {
            this.leftPaddle.SetValue(Canvas.LeftProperty, (double)PaddleMargin);
            this.leftPaddleTop =
                (this.ActualHeight - PaddleHeight - PaddleMargin) / 2;
            this.leftPaddle.SetValue(Canvas.TopProperty, this.leftPaddleTop);

            this.rightPaddle.SetValue(Canvas.LeftProperty, 
                this.ActualWidth - BallWidth - PaddleMargin);
            this.rightPaddleTop =
                (this.ActualHeight - PaddleHeight - PaddleMargin) / 2;
            this.rightPaddle.SetValue(Canvas.TopProperty, this.rightPaddleTop);

            this.ballLeft = PaddleMargin + BallWidth;
            this.ballTop = this.ActualHeight - BallHeight - PaddleMargin;
            this.PaintBall();
        }

        public void Start()
        {
            this.InitControls();

            this.isRightPaddleMoving = false;
            this.isRunning = true;
            this.isGameOver = false;

            this.dir = BallDirection.RightTop;

            this.timer.Start(); 
        }

        public void Stop()
        {
            this.isRunning = false;
            this.timer.Stop();
        }

        private void PongTimerTick(Object sender, EventArgs e)
        {
            this.MoveBall();
            this.PaintBall();
        }

        private void MoveBall()
        {
            // move ball
            switch (dir)
            {
                case BallDirection.RightTop:
                    this.ballLeft += BallStep;
                    this.ballTop -= BallStep;
                    break;
                case BallDirection.RightBottom:
                    this.ballLeft += BallStep;
                    this.ballTop += BallStep;
                    break;
                case BallDirection.LeftTop:
                    this.ballLeft -= BallStep;
                    this.ballTop -= BallStep;
                    break;
                case BallDirection.LeftBottom:
                    this.ballLeft -= BallStep;
                    this.ballTop += BallStep;
                    break;
            }

            if (this.isGameOver)
            {
                // has ball left canvas
                if (this.ballLeft + BallWidth < 0)
                {
                    // stop current game
                    this.Stop();

                    // ask user for another game
                    MessageBoxResult result = MessageBox.Show(
                        "Game Over!\nAnother Game?", "Another Pong Game",
                        MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                        this.Start();
                }
                
                return;
            }

            // collision detection
            if (this.ballLeft <= (BallWidth + PaddleMargin))
            {
                if (this.IsLeftPaddleDefending())
                {
                    if (dir == BallDirection.LeftTop)
                        dir = BallDirection.RightTop;
                    else if (dir == BallDirection.LeftBottom)
                        dir = BallDirection.RightBottom;
                }
                else
                {
                    // game is over ... let ball move out of the window
                    this.isGameOver = true;
                }
            }
            else if (this.ballLeft >= (this.ActualWidth - 2 * BallWidth - PaddleMargin))
            {
                if (dir == BallDirection.RightTop)
                    dir = BallDirection.LeftTop;
                else if (dir == BallDirection.RightBottom)
                    dir = BallDirection.LeftBottom;
            }

            if (this.ballTop <= PaddleMargin)
            {
                if (dir == BallDirection.RightTop)
                    dir = BallDirection.RightBottom;
                else if (dir == BallDirection.LeftTop)
                    dir = BallDirection.LeftBottom;
            }
            else if (this.ballTop >= (this.ActualHeight - BallHeight))
            {
                if (dir == BallDirection.RightBottom)
                    dir = BallDirection.RightTop;
                else if (dir == BallDirection.LeftBottom)
                    dir = BallDirection.LeftTop;
            }

            this.MoveRightPaddleIfNecessary();
        }

        private bool IsLeftPaddleDefending()
        {
            double verticalBallCenter = this.ballTop + (BallHeight / 2);

            return (
                verticalBallCenter >= this.leftPaddleTop &&
                verticalBallCenter <= this.leftPaddleTop + PaddleHeight) ?
                    true : false;
        }

        private void PongCanvasControl_MouseMove(Object sender, MouseEventArgs e)
        {
            if (!this.isRunning)
                return;

            Point p = e.GetPosition(this);
            if ((p.Y >= PaddleMargin + (PaddleHeight / 2)) &&
                (p.Y <= this.ActualHeight - PaddleMargin - (PaddleHeight / 2)))
            {
                this.leftPaddleTop = p.Y - (PaddleHeight / 2);
                this.leftPaddle.SetValue(Canvas.TopProperty, this.leftPaddleTop);
            }
        }

        private void PaintBall()
        {
            this.ball.SetValue(Canvas.LeftProperty, this.ballLeft);
            this.ball.SetValue(Canvas.TopProperty, this.ballTop);
        }

        private void MoveRightPaddleIfNecessary()
        {
            if (dir == BallDirection.LeftTop || dir == BallDirection.LeftBottom)
                return;

            if (this.ballLeft < (this.ActualWidth / 3))
                return;

            if (!this.isRightPaddleMoving)
            {
                // activate right paddle, if ball is "on same height"
                if (this.ballTop >= this.rightPaddleTop &&
                    this.ballTop <= this.rightPaddleTop + BallHeight)
                        this.isRightPaddleMoving = true;
            }

            if (this.isRightPaddleMoving)
            {
                double dist = BallStep / Math.Sqrt(2.0);

                if (dir == BallDirection.RightTop)
                {
                    // can paddle move to top
                    if (this.rightPaddleTop >= (PaddleMargin + dist))
                        this.rightPaddleTop -= dist;
                }
                else
                {
                    // can paddle move to bottom
                    double bottom =
                        this.ActualHeight - PaddleMargin - PaddleHeight - dist;
                    if (this.rightPaddleTop <= bottom)
                        this.rightPaddleTop += dist;
                }

                this.rightPaddle.SetValue(Canvas.TopProperty, this.rightPaddleTop);
            }
        }
    }
}
