using System.Diagnostics;
using System.CommandLine;
using System.Resources;
using System.Dynamic;

namespace Snake;

public static class Program
{
    public static void Main(string[] args)
    {
        var difficultyOption = new Option<GameDifficulty>("--difficulty", "-d")
        {
            Description = Resources.GetString("Help.DifficultyOption"),
            DefaultValueFactory = parseResult => GameDifficulty.Medium
        };
        var rootCommand = new RootCommand(Resources.GetString("Help.AppDescription")!);
        rootCommand.Add(difficultyOption);
        rootCommand.SetAction(parseResult => LaunchGame(parseResult.GetValue(difficultyOption)));
        rootCommand.Parse(args).Invoke();
    }

    public static void LaunchGame(GameDifficulty difficulty)
    {
        bool playAgain;
        do
        {
            var game = new Game(difficulty);
            playAgain = game.Run();
        } while (playAgain);
        UserInterface.ResetCursor();
    }

    public static ResourceManager Resources = new("Snake.Strings", typeof(Program).Assembly);
}

public enum Direction
{
    None,
    Up,
    Down,
    Left,
    Right,
}

public static class DirectionExtension
{
    extension(Direction direction)
    {
        public bool IsOpposite(Direction other)
        {
            if (direction != other
                && direction != Direction.None
                && other != Direction.None
                && ((direction == Direction.Up && other == Direction.Down)
                    || (direction == Direction.Down && other == Direction.Up)
                    || (direction == Direction.Left && other == Direction.Right)
                    || (direction == Direction.Right && other == Direction.Left)))
                return true;
            return false;
        }
    }
}

public readonly struct Coordinates
{
    public Coordinates(int x, int y, string? annotation = null)
    {
        X = x;
        Y = y;
        Annotation = annotation ?? "";
    }

    public int X { get; init; }
    public int Y { get; init; }
    public string Annotation { get; init; }

    public override string ToString()
    {
        if ( Annotation != "" )
            return $"({X}, {Y}; {Annotation})";
        return $"({X}, {Y})";
    }

    public Coordinates OffsetX(int offset)
    {
        return new Coordinates(X + offset, Y);
    }

    public Coordinates OffsetY(int offset)
    {
        return new Coordinates(X, Y + offset);
    }
}

public readonly struct BoxDimensions
{
    public BoxDimensions(int width, int height, Coordinates origin)
    {
        Width = width;
        Height = height;
        Origin = origin;
    }

    public int Width { get; init; }
    public int Height { get; init; }
    public Coordinates Origin { get; init; }

    public int XStart
    {
        get => Origin.X;
    }

    public int XEnd
    {
        get => Origin.X + Width - 1;
    }

    public int YStart
    {
        get => Origin.Y;
    }

    public int YEnd
    {
        get => Origin.Y + Height - 1;
    }

    public int Area
    {
        get => Width * Height;
    }

    public override string ToString()
    {
        return $"({Width} × {Height}; origin: {Origin.X}, {Origin.Y})";
    }

    public BoxDimensions GetInnerDimensions(int offset = 1)
    {
        int width = Width - (2 * offset);
        int height = Height - (2 * offset);
        return new BoxDimensions(width, height, new Coordinates(Origin.X + offset, Origin.Y + offset));
    }

    public bool ContainsPoint(Coordinates point, bool excludeBorder = true)
    {
        if (excludeBorder)
            return point.X > XStart && point.X < XEnd && point.Y > YStart && point.Y < YEnd;
        return point.X >= XStart && point.X <= XEnd && point.Y >= YStart && point.Y <= YEnd;
    }
}

public readonly struct BoxSymbols
{
    public BoxSymbols(char topLeft, char topRight, char bottomLeft, char bottomRight, char vertical, char horizontal, char fill = ' ')
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
        Vertical = vertical;
        Horizontal = horizontal;
        Fill = fill;
    }

    public char TopLeft { get; init; }
    public char TopRight { get; init; }
    public char BottomLeft { get; init; }
    public char BottomRight { get; init; }
    public char Vertical { get; init; }
    public char Horizontal { get; init; }
    public char Fill { get; init; }

    public char[] ToArray()
    {
        return [TopLeft, TopRight, BottomLeft, BottomRight, Vertical, Horizontal, Fill];
    }

    public static char Transliterate(char symbol, BoxSymbols fromSet, BoxSymbols toSet)
    {
        int index = Array.FindIndex(fromSet.ToArray(), c => c == symbol);
        if (index >= 0)
            return toSet.ToArray()[index];
        return symbol;
    }

    private static readonly BoxSymbols s_squareSingle = new('┌', '┐', '└', '┘', '│', '─');
    public static BoxSymbols SquareSingle { get => s_squareSingle; }
    private static readonly BoxSymbols s_squareDouble = new('╔', '╗', '╚', '╝', '║', '═');
    public static BoxSymbols SquareDouble { get => s_squareDouble; }
    private static readonly BoxSymbols s_roundSingle = new('╭', '╮', '╰', '╯', '│', '─');
    public static BoxSymbols RoundSingle { get => s_roundSingle; }
}

public static class UserInterface
{
    private static readonly BoxDimensions s_gameAreaDimensions = new(38, 12, new Coordinates(0, 1));
    public static BoxDimensions GameAreaDimensions { get => s_gameAreaDimensions; }
    private static readonly BoxDimensions s_windowDimensions = new(GameAreaDimensions.Width, GameAreaDimensions.Height + 2, new Coordinates(0, 0));
    public static BoxDimensions WindowDimensions { get => s_windowDimensions; }

    public static string Title
    {
        get => Program.Resources.GetString("App.Name")! + " v" + Program.Resources.GetString("App.Version");
    }

    public static void Initialize()
    {
        Console.OutputEncoding = System.Text.Encoding.Unicode;
        Console.InputEncoding = System.Text.Encoding.Unicode;
        Console.CursorVisible = false;
        Console.Clear();
        DrawGameArea();
        DrawTitle();
        DrawScore();
        DrawInfobar();
        ResetCursor();
    }

    public static bool ShowWinMessage(int score, int snakeLength, GameDifficulty difficulty)
    {
        DrawMessageBox([
            "     " + Ansify(Program.Resources.GetString("UserInterface.WinnerHeading")!, color: 15, bold: true, underlined: true),
            "",
            $" {Program.Resources.GetString("UserInterface.Score")} {score}",
            $" {Program.Resources.GetString("UserInterface.SnakeLength")} {snakeLength}",
            $" {Program.Resources.GetString("UserInterface.Difficulty")} {difficulty}",
            "",
            Ansify(PadToCenter(Program.Resources.GetString("UserInterface.EscapeToQuit")!, 24), dim: true, skipWhitespace: true),
            Ansify(PadToCenter(Program.Resources.GetString("UserInterface.EnterToPlayAgain")!, 24), dim: true, skipWhitespace: true)
        ]);
        return PollInputKey([ConsoleKey.Escape, ConsoleKey.Enter]) == ConsoleKey.Enter;
    }

    public static bool ShowGameOverMessage(int score, int snakeLength, GameDifficulty difficulty)
    {
        DrawMessageBox([
            Ansify(PadToCenter(Program.Resources.GetString("UserInterface.GameOverHeading")!, 24), color: 15, bold: true, underlined: true, skipWhitespace: true),
            "",
            $" {Program.Resources.GetString("UserInterface.Score")} {score}",
            $" {Program.Resources.GetString("UserInterface.SnakeLength")} {snakeLength}",
            $" {Program.Resources.GetString("UserInterface.Difficulty")} {difficulty}",
            "",
            Ansify(PadToCenter(Program.Resources.GetString("UserInterface.EscapeToQuit")!, 24), dim: true, skipWhitespace: true),
            Ansify(PadToCenter(Program.Resources.GetString("UserInterface.EnterToPlayAgain")!, 24), dim: true, skipWhitespace: true)
        ]);
        return PollInputKey([ConsoleKey.Escape, ConsoleKey.Enter]) == ConsoleKey.Enter;
    }

    public static void ShowPausedMessage(int score, int snakeLength, GameDifficulty difficulty)
    {
        DrawMessageBox([
            Ansify(PadToCenter(Program.Resources.GetString("UserInterface.GamePausedHeading")!, 24), color: 15, bold: true, underlined: true, skipWhitespace: true),
            "",
            $" {Program.Resources.GetString("UserInterface.Score")} {score}",
            $" {Program.Resources.GetString("UserInterface.SnakeLength")} {snakeLength}",
            $" {Program.Resources.GetString("UserInterface.Difficulty")} {difficulty}",
            "",
            "",
            Ansify(PadToCenter(Program.Resources.GetString("UserInterface.EnterToResume")!, 24), dim: true, skipWhitespace: true)
        ]);
        PollInputKey([ConsoleKey.Enter]);
    }

    public static void DrawTitle()
    {
        Console.Title = Title;
        Console.SetCursorPosition(0, 0);
        Console.Write(Title);
    }

    public static void DrawScore()
    {
        Console.SetCursorPosition(WindowDimensions.XEnd - 12, 0);
        Console.Write("Score: 000000");
    }

    public static void UpdateScore(int score)
    {
        Console.SetCursorPosition(WindowDimensions.XEnd - 5, 0);
        Console.Write(score.ToString("000000"));
    }

    public static void DrawInfobar()
    {
        Console.SetCursorPosition(0, WindowDimensions.YEnd);
        Console.Write(
            Ansify(Program.Resources.GetString("UserInterface.InfoBarInstructions")!, dim: true)
        );
    }

    public static void DrawGameArea()
    {
        DrawBox(GameAreaDimensions, BoxSymbols.RoundSingle);
    }

    public static void DrawMessageBox(string[] content, bool clip = false)
    {
        BoxDimensions innerDimensions = DrawBox(26, 10, BoxSymbols.SquareDouble);
        int i = 0;
        int y = innerDimensions.YStart;
        int yEnd = clip ? innerDimensions.YEnd : int.MaxValue;
        while (y < yEnd && i < content.Length)
        {
            Console.SetCursorPosition(innerDimensions.XStart, y);
            if (clip)
                Console.Write(content[i].AsSpan(0, Math.Min(content[i].Length, innerDimensions.Width - 1)));
            else
                Console.Write(content[i]);
            i++; y++;
        }
    }

    public static BoxDimensions DrawBox(int width, int height, BoxSymbols? boxSymbols = null)
    {
        BoxSymbols symbols = boxSymbols ?? BoxSymbols.SquareSingle;
        var origin = new Coordinates(
            (WindowDimensions.Width - width) / 2,
            (WindowDimensions.Height - height) / 2
        );
        return DrawBox(new BoxDimensions(width, height, origin), boxSymbols);
    }

    public static BoxDimensions DrawBox(BoxDimensions dimensions, BoxSymbols? boxSymbols = null)
    {
        BoxSymbols symbols = boxSymbols ?? BoxSymbols.SquareSingle;
        int width = dimensions.Width - 1;
        int height = dimensions.Height - 1;
        int xStart = dimensions.XStart;
        int xEnd = xStart + width;
        int yStart = dimensions.YStart;
        int yEnd = yStart + height;
        for (int y = yStart; y <= yEnd; y++)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                Console.SetCursorPosition(x, y);
                if (x == xStart && y == yStart)
                    Console.Write(symbols.TopLeft);
                else if (x == xStart && y == yEnd)
                    Console.Write(symbols.BottomLeft);
                else if (x == xEnd && y == yStart)
                    Console.Write(symbols.TopRight);
                else if (x == xEnd && y == yEnd)
                    Console.Write(symbols.BottomRight);
                else if (x == xStart || x == xEnd)
                    Console.Write(symbols.Vertical);
                else if (y == yStart || y == yEnd)
                    Console.Write(symbols.Horizontal);
                else
                    Console.Write(symbols.Fill);
            }
        }
        return new BoxDimensions(width, height, new Coordinates(xStart + 1, yStart + 1));
    }
    public static void ClearGameArea()
    {
        BoxDimensions innerDimensions = GameAreaDimensions.GetInnerDimensions();
        int yStart = innerDimensions.YStart;
        int yEnd = innerDimensions.YEnd;
        int xStart = innerDimensions.XStart;
        var filler = new string(' ', innerDimensions.Width);
        for (int y = yStart; y <= yEnd; y++)
        {
            Console.SetCursorPosition(xStart, y);
            Console.Write(filler);
        }
    }

    public static void ResetCursor()
    {
        Console.SetCursorPosition(WindowDimensions.XEnd, WindowDimensions.YEnd);
    }

    public static ConsoleKey PollInputKey(ConsoleKey[]? acceptKeys = null, long? timeout = null)
    {
        ConsoleKey result = ConsoleKey.None;

        if(timeout.HasValue)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            do
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    if (acceptKeys == null || acceptKeys.Contains(key))
                    {
                        result = key;
                        break;
                    }
                }
                Thread.Sleep(0);
            } while (stopWatch.ElapsedMilliseconds < timeout);
            stopWatch.Stop();
        }
        else
        {
            do
            {
                result = Console.ReadKey(true).Key;
            } while (acceptKeys != null && !acceptKeys.Contains(result));
        }

        return result;
    }

    public static string Ansify(string text, int? color = null, bool bold = false, bool dim = false, bool italic = false, bool underlined = false, bool skipWhitespace = false)
    {
        var result = new string[5];

        if (skipWhitespace)
        {
            result[0] = String.Join("", text.TakeWhile(Char.IsWhiteSpace));
            result[2] = text.Trim();
            result[4] = String.Join("", text.Reverse().TakeWhile(Char.IsWhiteSpace));
        }
        else
        {
            result[2] = text;
        }

        string?[] controlSequences =
        [
            color.HasValue ? $"38:5:{color.Value}" : null,
            bold ? "1" : null,
            dim ? "2" : null,
            italic ? "3" : null,
            underlined ? "4" : null,
        ];
        controlSequences = controlSequences.Where(s => s != null).ToArray();

        result[1] = $"\x1b[{String.Join(";", controlSequences)}m";
        result[3] = "\x1b[m";
        return String.Join("", result);
    }

    public static string PadToCenter(string text, int width)
    {
        if (text.Length >= width)
            return text;
        return new string(' ', width / 2 - (int)Math.Ceiling(text.Length / 2.0)) + text;
    }

    public static bool ConsoleSizeOK()
    {
        return Console.WindowWidth >= WindowDimensions.Width && Console.WindowHeight >= WindowDimensions.Height;
    }
}

public class Snake
{
    public Snake(int maxLength, Coordinates spawnPosition)
    {
        Length = 1;
        Path = new Coordinates[maxLength];
        Head = spawnPosition;
        CurrentDirection = Direction.None;
        PreviousDirection = Direction.None;
        State = SnakeState.Normal;
    }

    public int Length { get; set; }
    public Coordinates[] Path { get; set; }
    public Coordinates Head { get; set; }
    public Direction CurrentDirection { get; set; }
    public Direction PreviousDirection { get; set; }
    public SnakeState State { get; set; }

    private void ShiftPathArrayRight()
    {
        for (int i = Path.Length - 2; i >= 0; i--)
            Path[i + 1] = Path[i];
    }

    public void StorePosition()
    {
        ShiftPathArrayRight();
        string bodyShape = "o";
        if (CurrentDirection == PreviousDirection)
        {
            switch (CurrentDirection)
            {
                case Direction.Right:
                case Direction.Left:
                    bodyShape = BoxSymbols.SquareDouble.Horizontal.ToString();
                    break;
                case Direction.Up:
                case Direction.Down:
                    bodyShape = BoxSymbols.SquareDouble.Vertical.ToString();
                    break;
            }
        }
        else if (PreviousDirection == Direction.Up)
        {
            switch (CurrentDirection)
            {
                case Direction.Right:
                    bodyShape = BoxSymbols.SquareDouble.TopLeft.ToString();
                    break;
                case Direction.Left:
                    bodyShape = BoxSymbols.SquareDouble.TopRight.ToString();
                    break;
            }
        }
        else if (PreviousDirection == Direction.Down)
        {
            switch (CurrentDirection)
            {
                case Direction.Right:
                    bodyShape = BoxSymbols.SquareDouble.BottomLeft.ToString();
                    break;
                case Direction.Left:
                    bodyShape = BoxSymbols.SquareDouble.BottomRight.ToString();
                    break;
            }
        }
        else if (PreviousDirection == Direction.Right)
        {
            switch (CurrentDirection)
            {
                case Direction.Up:
                    bodyShape = BoxSymbols.SquareDouble.BottomRight.ToString();
                    break;
                case Direction.Down:
                    bodyShape = BoxSymbols.SquareDouble.TopRight.ToString();
                    break;
            }
        }
        else if (PreviousDirection == Direction.Left)
        {
            switch (CurrentDirection)
            {
                case Direction.Up:
                    bodyShape = BoxSymbols.SquareDouble.BottomLeft.ToString();
                    break;
                case Direction.Down:
                    bodyShape = BoxSymbols.SquareDouble.TopLeft.ToString();
                    break;
            }
        }
        Path[0] = new Coordinates(Head.X, Head.Y, bodyShape);
    }

    public void Move(Direction direction)
    {
        PreviousDirection = CurrentDirection;
        CurrentDirection = direction;
        StorePosition();
        Coordinates? newPosition = SimulateMove(direction);
        if (newPosition.HasValue)
            Head = newPosition.Value;
    }

    public Coordinates? SimulateMove(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Head.OffsetY(-1);
            case Direction.Down:
                return Head.OffsetY(+1);
            case Direction.Left:
                return Head.OffsetX(-1);
            case Direction.Right:
                return Head.OffsetX(+1);
            default:
                return null;
        }
    }

    public void Draw(bool deleteOldTail = true)
    {
        if (deleteOldTail)
        {
            Coordinates tailPoint = Path[int.Max(0, Length - 1)];
            if (tailPoint.X != 0 && tailPoint.Y != 0)
            {
                Console.SetCursorPosition(tailPoint.X, tailPoint.Y);
                Console.Write(' ');
            }
        }
        int color = 255;
        for (int i = 0; i < Length - 1; i++)
        {
            Console.SetCursorPosition(Path[i].X, Path[i].Y);
            char bodySymbol = DetermineSnakeDrawSymbol(Path[i].Annotation, 'x');
            if (i == Length - 2)
                bodySymbol = ThinOutTail(bodySymbol);
            Console.Write(UserInterface.Ansify(bodySymbol.ToString(), color));
            if (color > 240)
                color--;
        }
        char headSymbol = DetermineSnakeDrawSymbol('ö', 'X', 'Ö');
        Console.SetCursorPosition(Head.X, Head.Y);
        Console.Write(UserInterface.Ansify(headSymbol.ToString(), 15));
    }

    private char ThinOutTail(char symbol)
    {
        return BoxSymbols.Transliterate(symbol, BoxSymbols.SquareDouble, BoxSymbols.SquareSingle);
    }

    private char DetermineSnakeDrawSymbol(string? normal, char dead, char? chomping = null, char fallback = 'o')
    {
        if (normal == null || normal.Length < 1)
        {
            normal = fallback.ToString();
        }
        return DetermineSnakeDrawSymbol(normal.ToCharArray()[0], dead, chomping);
    }

    public bool CollidesWithPoint(Coordinates point, bool includeHead = true)
    {
        if (includeHead && Head.X == point.X && Head.Y == point.Y)
            return true;
        int index = Array.FindIndex(Path, 0, Length - 1, coords => coords.X == point.X && coords.Y == point.Y);
        return index >= 0;
    }

    private char DetermineSnakeDrawSymbol(char normal, char dead, char? chomping = null)
    {
        if (!chomping.HasValue)
            chomping = normal;
        switch (State)
        {
            case SnakeState.Dead:
                return dead;
            case SnakeState.Chomping:
                return chomping.Value;
            default:
                return normal;
        }
    }
}

public enum SnakeState
{
    Normal,
    Chomping,
    Dead
}

public class Fruit
{
    public Fruit(Coordinates position, char? fruitSymbol = null, int? color = null)
    {
        if (!fruitSymbol.HasValue)
            fruitSymbol = FruitSelection[s_random.Next(0, FruitSelection.Length)];
        if (!color.HasValue)
            color = ColorSelection[s_random.Next(0, ColorSelection.Length)];
        Symbol = fruitSymbol.Value;
        Color = color.Value;
        Position = position;
    }
    private static Random s_random = new();
    private static readonly char[] _fruitSelection = ['•', '◦', '▴', '■', '□', '᛭', '⨯', 'ꚛ', '★', '☆'];
    public static char[] FruitSelection { get => _fruitSelection; }
    private static readonly int[] _colorSelection = [8, 9, 10, 11, 12, 13, 14, 15];
    public static int[] ColorSelection { get => _colorSelection; }
    public char Symbol { get; init; }
    public int Color { get; init; }
    public Coordinates Position { get; init; }

    public void Draw()
    {
        Console.SetCursorPosition(Position.X, Position.Y);
        Console.Write(UserInterface.Ansify(Symbol.ToString(), color: Color));
    }

    public static Coordinates FindSpawnPosition(Snake Snake)
    {
        BoxDimensions innerGameArea = UserInterface.GameAreaDimensions.GetInnerDimensions();
        while (true)
        {
            var trial = new Coordinates(
                s_random.Next(innerGameArea.XStart, innerGameArea.XEnd),
                s_random.Next(innerGameArea.YStart, innerGameArea.YEnd)
            );
            if (!Snake.CollidesWithPoint(trial))
                return trial;
        }
    }

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
        Fruit = new Fruit(
            Fruit.FindSpawnPosition(Snake)
        );
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

public enum GameDifficulty : ushort
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}
