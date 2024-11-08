using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using SnakeGame;
using SnakeGame.models;

namespace WPFSnakeGame
{
    public partial class MainWindow : Window
    {
        private GameSettings settings;
        private Snake snake;
        private Position food;
        private int score;
        private int highScore;
        private bool isGameRunning;
        private DispatcherTimer gameTimer;
        private const int GridSize = 20;
        private const int SnakeSize = 18;
        private List<(Position Position, double Opacity)> snakeTrail;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            KeyDown += Window_KeyDown;
            GameCanvas.Background = new SolidColorBrush(Color.FromRgb(0, 0, 32));

            // Add initial start screen
            ShowStartScreen();
        }

        private void ShowStartScreen()
        {
            isGameRunning = false;
            gameTimer?.Stop();
            GameCanvas.Children.Clear();

            // Create semi-transparent overlay
            var overlay = new Rectangle
            {
                Width = GameCanvas.Width,
                Height = GameCanvas.Height,
                Fill = new SolidColorBrush(Color.FromArgb(180, 0, 0, 32))
            };
            GameCanvas.Children.Add(overlay);

            // Create start button
            var startButton = new Button
            {
                Content = "START GAME",
                Width = 200,
                Height = 60,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Colors.LimeGreen),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 5,
                    ShadowDepth = 2
                }
            };
            startButton.Click += StartGame_Click;
            Canvas.SetLeft(startButton, (GameCanvas.Width - startButton.Width) / 2);
            Canvas.SetTop(startButton, (GameCanvas.Height - startButton.Height) / 2);
            GameCanvas.Children.Add(startButton);

            // Hide GameOverPanel and reset buttons
            GameOverPanel.Visibility = Visibility.Collapsed;
            BottomRestartButton.Content = "Restart";
            BottomRestartButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
        }

        //Start game click


        private void GameTimer_Tick(object sender, EventArgs e)
        {
            Update();
            RedrawGame();
        }
        private void InitializeGame()
        {
            settings = new GameSettings();
            snake = new Snake(new Position(settings.ScreenWidth / 2, settings.ScreenHeight / 2));
            snakeTrail = new List<(Position, double)>();
            score = 0;

            GameCanvas.Width = settings.ScreenWidth * GridSize;
            GameCanvas.Height = settings.ScreenHeight * GridSize;

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(settings.MaxGameSpeed);
            gameTimer.Tick += GameTimer_Tick;

            PlaceFood();
            UpdateScore();
            GameOverPanel.Visibility = Visibility.Collapsed;

            // Don't start the game immediately
            isGameRunning = false;
        }

        private void Update()
        {
            var nextHead = snake.GetNextHeadPosition();

            if (IsCollision(nextHead))
            {
                GameOver();
                return;
            }

            // Add trail effect
            snakeTrail.Add((snake.Head, 0.6));
            for (int i = snakeTrail.Count - 1; i >= 0; i--)
            {
                var (pos, opacity) = snakeTrail[i];
                snakeTrail[i] = (pos, opacity - 0.1);
                if (snakeTrail[i].Opacity <= 0)
                {
                    snakeTrail.RemoveAt(i);
                }
            }

            bool grow = Math.Abs(nextHead.X - food.X) < 1 && Math.Abs(nextHead.Y - food.Y) < 1;
            if (grow)
            {
                score++;
                UpdateScore();
                PlaceFood();
            }

            snake.Move(grow);
        }

        private void RedrawGame()
        {
            GameCanvas.Children.Clear();

            
            for (int x = 0; x < settings.ScreenWidth; x++)
            {
                for (int y = 0; y < settings.ScreenHeight; y++)
                {
                    var gridCell = new Rectangle
                    {
                        Width = GridSize,
                        Height = GridSize,
                        Stroke = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(gridCell, x * GridSize);
                    Canvas.SetTop(gridCell, y * GridSize);
                    GameCanvas.Children.Add(gridCell);
                }
            }

            //
            foreach (var (pos, opacity) in snakeTrail)
            {
                var trailRect = new Rectangle
                {
                    Width = SnakeSize,
                    Height = SnakeSize,
                    Fill = new SolidColorBrush(Color.FromArgb((byte)(opacity * 255), 0, 255, 0)),
                    Effect = new BlurEffect { Radius = 3 }
                };
                Canvas.SetLeft(trailRect, pos.X * GridSize + (GridSize - SnakeSize) / 2);
                Canvas.SetTop(trailRect, pos.Y * GridSize + (GridSize - SnakeSize) / 2);
                GameCanvas.Children.Add(trailRect);
            }

            //
            var foodGroup = new Canvas { Width = SnakeSize, Height = SnakeSize };
            var foodRect = new Rectangle
            {
                Width = SnakeSize,
                Height = SnakeSize,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.Yellow, 0.0),
                        new GradientStop(Colors.Red, 1.0)
                    }
                },
                Effect = new DropShadowEffect
                {
                    Color = Colors.Red,
                    BlurRadius = 10,
                    ShadowDepth = 0
                }
            };
            foodGroup.Children.Add(foodRect);
            Canvas.SetLeft(foodGroup, food.X * GridSize + (GridSize - SnakeSize) / 2);
            Canvas.SetTop(foodGroup, food.Y * GridSize + (GridSize - SnakeSize) / 2);
            GameCanvas.Children.Add(foodGroup);

            // Draw snake
            for (int i = 0; i < snake.Body.Count; i++)
            {
                var segment = snake.Body[i];
                var isHead = i == 0;

                if (isHead)
                {
                    DrawSnakeHead(segment);
                }
                else
                {
                    DrawSnakeBody(segment, i);
                }
            }
        }

        private void DrawSnakeHead(Position position)
        {
            var headGroup = new Canvas { Width = SnakeSize, Height = SnakeSize };

            // Glowing head
            var headRect = new Rectangle
            {
                Width = SnakeSize,
                Height = SnakeSize,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.LightGreen, 0.0),
                        new GradientStop(Colors.LimeGreen, 1.0)
                    }
                },
                Effect = new DropShadowEffect
                {
                    Color = Colors.LimeGreen,
                    BlurRadius = 10,
                    ShadowDepth = 0
                }
            };
            headGroup.Children.Add(headRect);

            //eyes
            double eyeSize = SnakeSize / 3;
            var leftEye = new Ellipse
            {
                Width = eyeSize,
                Height = eyeSize,
                Fill = Brushes.White,
                Effect = new DropShadowEffect
                {
                    Color = Colors.White,
                    BlurRadius = 5,
                    ShadowDepth = 0
                }
            };

            var rightEye = new Ellipse
            {
                Width = eyeSize,
                Height = eyeSize,
                Fill = Brushes.White,
                Effect = new DropShadowEffect
                {
                    Color = Colors.White,
                    BlurRadius = 5,
                    ShadowDepth = 0
                }
            };

            // Position eyes based on direction
            switch (snake.CurrentDirection)
            {
                case Direction.Right:
                    Canvas.SetLeft(leftEye, SnakeSize - eyeSize * 1.2);
                    Canvas.SetTop(leftEye, eyeSize * 0.3);
                    Canvas.SetLeft(rightEye, SnakeSize - eyeSize * 1.2);
                    Canvas.SetTop(rightEye, SnakeSize - eyeSize * 1.3);
                    break;
                case Direction.Left:
                    Canvas.SetLeft(leftEye, eyeSize * 0.2);
                    Canvas.SetTop(leftEye, eyeSize * 0.3);
                    Canvas.SetLeft(rightEye, eyeSize * 0.2);
                    Canvas.SetTop(rightEye, SnakeSize - eyeSize * 1.3);
                    break;
                case Direction.Up:
                    Canvas.SetLeft(leftEye, eyeSize * 0.3);
                    Canvas.SetTop(leftEye, eyeSize * 0.2);
                    Canvas.SetLeft(rightEye, SnakeSize - eyeSize * 1.3);
                    Canvas.SetTop(rightEye, eyeSize * 0.2);
                    break;
                case Direction.Down:
                    Canvas.SetLeft(leftEye, eyeSize * 0.3);
                    Canvas.SetTop(leftEye, SnakeSize - eyeSize * 1.2);
                    Canvas.SetLeft(rightEye, SnakeSize - eyeSize * 1.3);
                    Canvas.SetTop(rightEye, SnakeSize - eyeSize * 1.2);
                    break;
            }

            headGroup.Children.Add(leftEye);
            headGroup.Children.Add(rightEye);

            Canvas.SetLeft(headGroup, position.X * GridSize + (GridSize - SnakeSize) / 2);
            Canvas.SetTop(headGroup, position.Y * GridSize + (GridSize - SnakeSize) / 2);
            GameCanvas.Children.Add(headGroup);
        }

        private void DrawSnakeBody(Position position, int segmentIndex)
        {
            var bodyGroup = new Canvas { Width = SnakeSize, Height = SnakeSize };

            var bodyRect = new Rectangle
            {
                Width = SnakeSize,
                Height = SnakeSize,
                Fill = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(0, (byte)(200 - segmentIndex * 2), 0), 0),
                        new GradientStop(Color.FromRgb(0, (byte)(160 - segmentIndex * 2), 0), 1)
                    }
                },
                Effect = new DropShadowEffect
                {
                    Color = Colors.Green,
                    BlurRadius = Math.Max(1, 8 - segmentIndex),
                    ShadowDepth = 0
                }
            };
            bodyGroup.Children.Add(bodyRect);

            Canvas.SetLeft(bodyGroup, position.X * GridSize + (GridSize - SnakeSize) / 2);
            Canvas.SetTop(bodyGroup, position.Y * GridSize + (GridSize - SnakeSize) / 2);
            GameCanvas.Children.Add(bodyGroup);
        }

        private void UpdateScore()
        {
            if (score > highScore)
            {
                highScore = score;
            }
            ScoreText.Text = $"Score: {score} | High Score: {highScore}";
        }

        private void GameOver()
        {
            gameTimer.Stop();
            isGameRunning = false;
            GameOverPanel.Visibility = Visibility.Visible;
            PauseButton.IsEnabled = false;
            BottomRestartButton.IsEnabled = true;

            // Hide the GameOverRestartButton since we're using the bottom one
            //if (GameOverRestartButton != null)
            //    GameOverRestartButton.Visibility = Visibility.Collapsed;

            // Add special effect for game over
            var gameOverEffect = new Rectangle
            {
                Width = GameCanvas.Width,
                Height = GameCanvas.Height,
                Fill = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)),
                Effect = new BlurEffect { Radius = 3 }
            };
            GameCanvas.Children.Add(gameOverEffect);
        }

        private bool IsCollision(Position position)
        {
            // Wall collision with visual effect
            bool isWallCollision = position.X < 0 || position.X >= settings.ScreenWidth ||
                                 position.Y < 0 || position.Y >= settings.ScreenHeight;

            if (isWallCollision)
            {
                // Add collision effect at the collision point
                AddCollisionEffect(position);
                return true;
            }

            // Self collision
            bool isSelfCollision = snake.CollidesWith(position);
            if (isSelfCollision)
            {
                AddCollisionEffect(position);
            }

            return isSelfCollision;
        }

        private void AddCollisionEffect(Position position)
        {
            var collisionEffect = new Ellipse
            {
                Width = GridSize * 2,
                Height = GridSize * 2,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.Yellow, 0),
                        new GradientStop(Colors.Red, 0.5),
                        new GradientStop(Colors.Transparent, 1)
                    }
                },
                Effect = new BlurEffect { Radius = 5 }
            };

            Canvas.SetLeft(collisionEffect, position.X * GridSize - GridSize / 2);
            Canvas.SetTop(collisionEffect, position.Y * GridSize - GridSize / 2);
            GameCanvas.Children.Add(collisionEffect);
        }

        private void PlaceFood()
        {
            Random random = new Random();
            do
            {
                food = new Position(
                    random.Next(0, settings.ScreenWidth),
                    random.Next(0, settings.ScreenHeight)
                );
            } while (snake.CollidesWith(food));

            // Add spawning effect for food
            AddFoodSpawnEffect();
        }

        private void AddFoodSpawnEffect()
        {
            var spawnEffect = new Ellipse
            {
                Width = GridSize * 2,
                Height = GridSize * 2,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.Yellow, 0),
                        new GradientStop(Colors.Transparent, 1)
                    }
                },
                Effect = new BlurEffect { Radius = 5 }
            };

            Canvas.SetLeft(spawnEffect, food.X * GridSize - GridSize / 2);
            Canvas.SetTop(spawnEffect, food.Y * GridSize - GridSize / 2);
            GameCanvas.Children.Add(spawnEffect);

            // Create animation timer for spawn effect
            var spawnTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };

            double opacity = 1.0;
            spawnTimer.Tick += (s, e) =>
            {
                opacity -= 0.1;
                if (opacity <= 0)
                {
                    GameCanvas.Children.Remove(spawnEffect);
                    spawnTimer.Stop();
                }
                else
                {
                    spawnEffect.Opacity = opacity;
                }
            };

            spawnTimer.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isGameRunning)
            {
                if (e.Key == Key.Space)
                {
                    StartGame_Click(null, null);
                    return;
                }
                return;
            }

            Direction? newDirection = null;

            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    newDirection = Direction.Up;
                    break;
                case Key.Down:
                case Key.S:
                    newDirection = Direction.Down;
                    break;
                case Key.Left:
                case Key.A:
                    newDirection = Direction.Left;
                    break;
                case Key.Right:
                case Key.D:
                    newDirection = Direction.Right;
                    break;
                case Key.Space:
                    PauseGame_Click(null, null);
                    break;
                case Key.R:
                    RestartGame_Click(null, null);
                    break;
            }

            if (newDirection.HasValue)
            {
                snake.ChangeDirection(newDirection.Value);
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
            isGameRunning = true;
            gameTimer.Start();
            GameOverPanel.Visibility = Visibility.Collapsed;
            BottomRestartButton.IsEnabled = true;
            PauseButton.IsEnabled = true;
            RedrawGame();
            AddGameStartEffect();
        }


        private void AddGameStartEffect()
        {
            var startEffect = new Rectangle
            {
                Width = GameCanvas.Width,
                Height = GameCanvas.Height,
                Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
            };

            GameCanvas.Children.Add(startEffect);

            var startTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };

            double opacity = 1.0;
            startTimer.Tick += (s, e) =>
            {
                opacity -= 0.1;
                if (opacity <= 0)
                {
                    GameCanvas.Children.Remove(startEffect); 
                    startTimer.Stop();
                }
                else
                {
                    startEffect.Opacity = opacity;
                }
            };

            startTimer.Start();
        }

        private void PauseGame_Click(object sender, RoutedEventArgs e)
        {
            if (isGameRunning)
            {
                gameTimer.Stop();
                PauseButton.Content = "Resume";

                // pauzis overlay
                var pauseOverlay = new Rectangle
                {
                    Width = GameCanvas.Width,
                    Height = GameCanvas.Height,
                    Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))
                };
                GameCanvas.Children.Add(pauseOverlay);

                // dapazuebis teqsti
                var pauseText = new TextBlock
                {
                    Text = "PAUSED",
                    FontSize = 48,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        BlurRadius = 5,
                        ShadowDepth = 2
                    }
                };

                Canvas.SetLeft(pauseText, (GameCanvas.Width - 200) / 2);
                Canvas.SetTop(pauseText, (GameCanvas.Height - 60) / 2);
                GameCanvas.Children.Add(pauseText);
            }
            else
            {
                gameTimer.Start();
                PauseButton.Content = "Pause";
                RedrawGame(); 
            }
            isGameRunning = !isGameRunning;
        }

        private void RestartGame_Click(object sender, RoutedEventArgs e)
        {
            BottomRestartButton.IsEnabled = false;
            PauseButton.IsEnabled = false;

            // fade-out effect
            var fadeEffect = new Rectangle
            {
                Width = GameCanvas.Width,
                Height = GameCanvas.Height,
                Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))
            };
            GameCanvas.Children.Add(fadeEffect);

            var fadeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };

            double opacity = 0.0;
            fadeTimer.Tick += (s, evt) =>
            {
                opacity += 0.1;
                if (opacity >= 1.0)
                {
                    fadeTimer.Stop();
                    GameCanvas.Children.Remove(fadeEffect);
                    ShowStartScreen();
                }
                else
                {
                    fadeEffect.Fill = new SolidColorBrush(Color.FromArgb((byte)(opacity * 255), 0, 0, 0));
                }
            };

            fadeTimer.Start();
        }

        private void ShowScorePopup(int points)
        {
            var scorePopup = new TextBlock
            {
                Text = $"+{points}",
                FontSize = 24,
                Foreground = Brushes.Yellow,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 5,
                    ShadowDepth = 2
                }
            };

            Canvas.SetLeft(scorePopup, food.X * GridSize);
            Canvas.SetTop(scorePopup, food.Y * GridSize);
            GameCanvas.Children.Add(scorePopup);

            var popupTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };

            double offset = 0;
            popupTimer.Tick += (s, e) =>
            {
                offset += 1;
                Canvas.SetTop(scorePopup, food.Y * GridSize - offset);
                scorePopup.Opacity -= 0.05;

                if (scorePopup.Opacity <= 0)
                {
                    GameCanvas.Children.Remove(scorePopup);
                    popupTimer.Stop();
                }
            };

            popupTimer.Start();
        }
    }
}
