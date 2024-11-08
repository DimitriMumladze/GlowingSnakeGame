namespace SnakeGame.models
{
    public class GameSettings
    {
        public int ScreenWidth { get; } = 40;
        public int ScreenHeight { get; } = 20;
        public int InitialGameSpeed { get; } = 100;
        public int MaxGameSpeed { get; } = 50;  // Faster maximum speed
        public int SpeedIncreaseInterval { get; } = 5; // Increase speed every 5 points
        public double SpeedIncreaseFactor { get; } = 0.9; // Multiply interval by this factor
    }
}