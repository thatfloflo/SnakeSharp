namespace Snake;

/// <summary>Game difficulty levels</summary>
public enum GameDifficulty : ushort
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}

/// <summary>Represents and runs a game of snake</summary>
public class Game
{
    /// <summary>Instantiates a new game of snake</summary>
    /// <remarks>
    /// <para>
    /// Each instance of <see cref="Game"/> represents a single game. To run more
    /// than one game (typically consecutively), e.g. when the player chooses to
    /// play again after the game ends, you should instantiate a new <see cref="Game"/>
    /// each time.
    /// </para>
    /// <para>
    /// The <i>difficulty</i> of the game instance determines several parameters:
    /// <list type="bullet">
    /// <item>The delay timing before the snake is moved automatically if no user input is received.</item>
    /// <item>Whether the snake is allowed to collide with the game area boundary or not.</item>
    /// <item>Whether the snake is allowed to turn back on itself (if <see cref="Snake.Length"/> <c>&gt; 2</c>) or not.</item>
    /// </list>
    /// These parameters are set as follows, given a specific difficulty level:
    /// <list type="table">
    /// <item>
    ///     <term><see cref="Difficulty.Easy"/></term>
    ///     <description>Auto move delay starts at 1500ms and advances to 250ms.
    ///     Both boundary collisions and turnbacks are prevented.</description>
    /// </item>
    /// <item>
    ///     <term><see cref="Difficulty.Medium"/></term>
    ///     <description>Auto move delay starts at 1000ms and advances to 80ms.
    ///     Boundary collisions are allowed but turnbacks are prevented.</description>
    /// </item>
    /// <item>
    ///     <term><see cref="Difficulty.Hard"/></term>
    ///     <description>Auto move delay starts at 500ms and advances to 50ms.
    ///     Neither boundary collisions nor turnbacks are prevented.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="difficulty">The difficulty level for the game</param>
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

    /// <summary>The game's instance of <see cref="Snake.Snake"/>.</summary>
    public Snake Snake { get; init; }

    /// <summary>The current <see cref="Fruit.Fruit"/> in the game, if any.</summary>
    public Fruit? Fruit { get; set; }

    /// <summary>The maximal (i.e. initial) delay in milliseconds before the <see cref="Snake"/> is moved automatically.</summary>
    public int AutoMoveDelayMax { get; set; }

    /// <summary>The minimal (i.e. final) delay in milliseconds before the <see cref="Snake"/> is moved automatically.</summary>
    public int AutoMoveDelayMin { get; set; }

    /// <summary>The current delay in milliseconds before the <see cref="Snake"/> is moved automatically.</summary>
    public int AutoMoveDelay { get; set; }

    /// <summary>Whether to prevent the <see cref="Snake"/> from colliding with the game area boundary or not.</summary>
    public bool PreventBoundaryCollisions { get; set; }

    /// <summary>Whether to prevent the <see cref="Snake"/> from turning back on itself or not.</summary>
    public bool PreventTurnbacks { get; set; }

    /// <summary>The difficulty level of the game.</summary>
    /// <seealso cref="GameDifficulty"/>
    public GameDifficulty Difficulty { get; set; }

    /// <summary>The current score.</summary>
    public int Score { get; set; }

    /// <summary>Spawns a new <see cref="Fruit.Fruit"/> in a random location.</summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no new position to spawn the fruit could be found (e.g. the game area has been exhausted).
    /// </exception>
    private void SpawnFruit()
    {
        Coordinates? fruitPosition = Fruit.FindSpawnPosition(Snake, int.MaxValue - 1);
        if(!fruitPosition.HasValue)
            throw new InvalidOperationException("Could not find a position to spawn a new fruit");
        Fruit = new Fruit(fruitPosition.Value);
    }

    /// <summary>Runs the game.</summary>
    /// <remarks>
    /// This runs the main loop of the game and returns only once the game has ended, either by the user
    /// quitting, winning, or loosing.
    /// </remarks>
    /// <returns>True if the player indicated they want to play again after the game ended, False otherwise.</returns>
    /// <exception cref="InvalidOperationException"></exception>
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
    
    /// <summary>
    /// Checks whether the <see cref="Snake"/> would collide with the game area boundary if it
    /// were to move one step into <i>snakeDirection</i>.
    /// </summary>
    /// <param name="snakeDirection">The <see cref="Direction"/> in which the <see cref="Snake"/> is moving.</param>
    /// <returns>
    /// True if the <see cref="Snake"/> would collide with the game area boundary if it
    /// were to move one step into <i>snakeDirection</i>, False otherwise.
    /// </returns>
    public bool WillCollideWithBoundary(Direction snakeDirection)
    {
        Coordinates? predictedPosition = Snake.SimulateMove(snakeDirection);
        return predictedPosition.HasValue && !UserInterface.GameAreaDimensions.ContainsPoint(predictedPosition.Value);
    }

    /// <summary>
    /// Checks whether the <see cref="Snake"/> would chomp (eat/collide with) itself if it
    /// were to move one step into <i>snakeDirection</i>.
    /// </summary>
    /// <param name="snakeDirection">The <see cref="Direction"/> in which the <see cref="Snake"/> is moving.</param>
    /// <returns>
    /// True if the <see cref="Snake"/> would chomp (eat/collide with) itself if it
    /// were to move one step into <i>snakeDirection</i>, False otherwise.
    /// </returns>
    public bool WillChompItself(Direction snakeDirection)
    {
        if (Snake.Length < 3)
            return false; // Snake of lengths < 3 will always have moved their tail away after moving
        Coordinates? predictedPosition = Snake.SimulateMove(snakeDirection);
        return predictedPosition.HasValue && Snake.CollidesWithPoint(predictedPosition.Value, includeHead: false);
    }

    /// <summary>
    /// Checks whether the <see cref="Snake"/> would die by turning back on itself if it
    /// were to move one step into <i>snakeDirection</i>.
    /// </summary>
    /// <remarks>
    /// Note that a <see cref="Snake"/> of length <c>&lt;= 2</c> will never die from a turnback, because it will
    /// have moved its own tail out of the way by completingt he next move.
    /// </remarks>
    /// <param name="snakeDirection">The <see cref="Direction"/> in which the <see cref="Snake"/> is moving.</param>
    /// <returns>
    /// True if the <see cref="Snake"/> would die by turning back on itself if it
    /// were to move one step into <i>snakeDirection</i>, False otherwise.
    /// </returns>
    public bool WillMakeDeadlyTurnback(Direction snakeDirection)
    {
        return snakeDirection.IsOpposite(Snake.CurrentDirection) && WillChompItself(snakeDirection);
    }
}
