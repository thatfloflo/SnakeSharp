namespace Snake;

public enum GameDifficulty : ushort
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}

public class Game
{
    public Game(GameDifficulty difficulty = GameDifficulty.Medium)
    {
        Score = 0;
        BoxDimensions innerGameArea = UserInterface.GameAreaDimensions.GetInnerDimensions();
        Snake = new Snake(
            UserInterface.WindowDimensions.Area,
            new Coordinates(
                s_random.Next(innerGameArea.XStart, innerGameArea.XEnd),
                s_random.Next(innerGameArea.YStart, innerGameArea.YEnd)
            )
        );
        Difficulty = difficulty;
        switch (Difficulty)
        {
            case GameDifficulty.Easy:
                AutoMoveDelayMax = 1500;
                AutoMoveDelayMin = 250;
                PreventBoundaryCollisions = true;
                PreventTurnbacks = true;
                break;
            case GameDifficulty.Medium:
                AutoMoveDelayMax = 1000;
                AutoMoveDelayMin = 80;
                PreventBoundaryCollisions = false;
                PreventTurnbacks = true;
                break;
            case GameDifficulty.Hard:
                AutoMoveDelayMax = 500;
                AutoMoveDelayMin = 50;
                PreventBoundaryCollisions = false;
                PreventTurnbacks = false;
                break;
        }
        AutoMoveDelay = AutoMoveDelayMax;
    }

    private static readonly Random s_random = new();

    public Snake Snake { get; init; }

    public Fruit? Fruit { get; set; }

    public int AutoMoveDelayMax { get; set; }

    public int AutoMoveDelayMin { get; set; }

    public int AutoMoveDelay { get; set; }

    public bool PreventBoundaryCollisions { get; set; }

    public bool PreventTurnbacks { get; set; }

    public GameDifficulty Difficulty { get; set; }

    public int Score { get; set; }

    private void SpawnFruit()
    {
        Coordinates? fruitPosition = Fruit.FindSpawnPosition(Snake, int.MaxValue - 1);
        if(!fruitPosition.HasValue)
            throw new InvalidOperationException("Could not find a position to spawn a new fruit");
        Fruit = new Fruit(fruitPosition.Value);
    }

    public bool Run()
    {
        if (!UserInterface.ConsoleSizeOK())
        {
            // TODO
            throw new InvalidOperationException("Console window too small");
        }
        UserInterface.Initialize();
        Snake.Draw();
        if (Fruit != null)
            Fruit.Draw();
        while (true)
        {
            if (!UserInterface.ConsoleSizeOK())
            {
                // TODO
                throw new InvalidOperationException("Console window too small");
            }
            Snake.State = SnakeState.Normal;
            if (Fruit == null)
            {
                SpawnFruit();
                Fruit?.Draw();
            }
            switch (UserInterface.PollInputKey(
                acceptKeys: [
                    ConsoleKey.W, ConsoleKey.UpArrow,
                    ConsoleKey.A, ConsoleKey.LeftArrow,
                    ConsoleKey.S, ConsoleKey.DownArrow,
                    ConsoleKey.D, ConsoleKey.RightArrow,
                    ConsoleKey.P, ConsoleKey.Escape
                ],
                timeout: AutoMoveDelay
            ))
            {
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    MoveSnakeIfPossible(Direction.Up);
                    break;
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    MoveSnakeIfPossible(Direction.Left);
                    break;
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    MoveSnakeIfPossible(Direction.Down);
                    break;
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    MoveSnakeIfPossible(Direction.Right);
                    break;
                case ConsoleKey.None:
                    MoveSnakeIfPossible(Snake.CurrentDirection);
                    break;
                case ConsoleKey.P:
                    UserInterface.ShowPausedMessage(Score, Snake.Length, Difficulty);
                    UserInterface.ClearGameArea();
                    Snake.Draw();
                    if (Fruit != null)
                        Fruit.Draw();
                    break;
                case ConsoleKey.Escape:
                    return false;
            }
            if (Snake.Length > 1 && Snake.CollidesWithPoint(Snake.Head, includeHead: false))
            {
                Snake.State = SnakeState.Dead;
            }
            if (!UserInterface.GameAreaDimensions.ContainsPoint(Snake.Head))
            {
                Snake.State = SnakeState.Dead;
            }
            else if (Fruit != null && Snake.CollidesWithPoint(Fruit.Position))
            {
                Snake.State = SnakeState.Chomping;
            }
            switch (Snake.State)
            {
                case SnakeState.Chomping:
                    Score += Snake.Length;
                    UserInterface.UpdateScore(Score);
                    Snake.Length++;
                    AutoMoveDelay -= AutoMoveDelayMax / 100;
                    if (AutoMoveDelay < AutoMoveDelayMin)
                        AutoMoveDelay = AutoMoveDelayMin;
                    Fruit = null;
                    break;
                case SnakeState.Dead:
                    Snake.Draw();
                    return UserInterface.ShowGameOverMessage(Score, Snake.Length, Difficulty);
            }
            if (Snake.Length >= Snake.Path.Length)
                return UserInterface.ShowWinMessage(Score, Snake.Length, Difficulty);
            Snake.Draw();
        }
    }

    private void MoveSnakeIfPossible(Direction direction)
    {
        if (PreventBoundaryCollisions && WillCollideWithBoundary(direction))
            return;
        if (PreventTurnbacks && WillMakeDeadlyTurnback(direction))
            return;
        Snake.Move(direction);
    }
    public bool WillCollideWithBoundary(Direction snakeDirection)
    {
        Coordinates? predictedPosition = Snake.SimulateMove(snakeDirection);
        return predictedPosition.HasValue && !UserInterface.GameAreaDimensions.ContainsPoint(predictedPosition.Value);
    }

    public bool WillChompItself(Direction snakeDirection)
    {
        if (Snake.Length < 3)
            return false; // Snake of lengths < 3 will always have moved their tail away after moving
        Coordinates? predictedPosition = Snake.SimulateMove(snakeDirection);
        return predictedPosition.HasValue && Snake.CollidesWithPoint(predictedPosition.Value, includeHead: false);
    }

    public bool WillMakeDeadlyTurnback(Direction snakeDirection)
    {
        return snakeDirection.IsOpposite(Snake.CurrentDirection) && WillChompItself(snakeDirection);
    }
}
