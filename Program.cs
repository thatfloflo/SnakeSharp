using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        bool playAgain;
        do {
            Game game = new Game();
            playAgain = game.Run();
        } while(playAgain);
        UserInterface.ResetCursor();
        Environment.Exit(0);
    }
}

public enum Direction
{
    None,
    Up,
    Down,
    Left,
    Right,
}

public readonly struct Coordinates
{
    public Coordinates(int x, int y, string? annotation = null)
    {
        X = x;
        Y = y;
        Annotation = annotation;
    }

    public int X { get; init;}
    public int Y {get; init;}
    public string? Annotation { get; init; }

    public override string ToString()
    {
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
    public Coordinates Origin {get; init;}

    public override string ToString()
    {
        return $"({Width} × {Height}; origin: {Origin.X}, {Origin.Y})";
    }

    public int GetWidth()
    {
        return Width;
    }

    public int GetHeight()
    {
        return Height;
    }

    public Coordinates GetOrigin()
    {
        return Origin;
    }

    public int GetXStart()
    {
        return Origin.X;
    }

    public int GetXEnd()
    {
        return Origin.X + Width - 1;
    }

    public int GetYStart()
    {
        return Origin.Y;
    }

    public int GetYEnd()
    {
        return Origin.Y + Height - 1;
    }

    public int GetArea()
    {
        return Width * Height;
    }

    public BoxDimensions GetInnerDimensions(int offset = 1)
    {
        int width = Width - (2 * offset);
        int height = Height - (2 * offset);
        return new BoxDimensions(width, height, new Coordinates(Origin.X + offset, Origin.Y + offset));
    }

    public bool ContainsPoint(Coordinates point, bool excludeBorder = true)
    {
        if ( excludeBorder )
            return point.X > GetXStart() && point.X < GetXEnd() && point.Y > GetYStart() && point.Y < GetYEnd();
        return point.X >= GetXStart() && point.X <= GetXEnd() && point.Y >= GetYStart() && point.Y <= GetYEnd();
    }
}

public readonly struct BlockSymbols
{
    public BlockSymbols(char topLeft, char topRight, char bottomLeft, char bottomRight, char vertical, char horizontal, char fill = ' ')
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

    public static char Transliterate(char symbol, BlockSymbols fromSet, BlockSymbols toSet)
    {
        int index = Array.FindIndex(fromSet.ToArray(), c => c == symbol);
        if ( index >= 0 )
            return toSet.ToArray()[index];
        return symbol;
    }

    public static BlockSymbols SquareSingle = new BlockSymbols(
        '┌', '┐', '└', '┘', '│', '─'
    );
    public static BlockSymbols SquareDouble = new BlockSymbols(
        '╔', '╗', '╚', '╝', '║', '═'
    );
    public static BlockSymbols RoundSingle = new BlockSymbols(
        '╭', '╮', '╰', '╯', '│', '─'
    );
}

public class UserInterface
{

    public static readonly BoxDimensions GameAreaDimensions = new BoxDimensions(38, 12, new Coordinates(0, 1));
    public static readonly BoxDimensions WindowDimensions = new BoxDimensions(GameAreaDimensions.Width, GameAreaDimensions.Height + 2, new Coordinates(0, 0));

    public static readonly string Title = "Snake# v0.2";

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

    public static bool ShowWinMessage(int score, int snakeLength)
    {
        DrawMessageBox([
            "     " + Ansify("YOU HAVE WON!!", color: 15, bold: true, underlined: true),
            "",
            $" Score: {score}",
            $" Snake length: {snakeLength}",
            "",
            "",
            Ansify("      Esc to quit", dim: true),
            Ansify("  Enter to play again", dim: true)
        ]);
        return PollInputKey([ConsoleKey.Escape, ConsoleKey.Enter]) == ConsoleKey.Enter;
    }
    public static bool ShowGameOverMessage(int score, int snakeLength)
    {
        DrawMessageBox([
            "       " + Ansify("Game Over", color: 15, bold: true, underlined: true),
            "",
            $" Score: {score}",
            $" Snake length: {snakeLength}",
            "",
            "",
            Ansify("      Esc to quit", dim: true),
            Ansify("  Enter to play again", dim: true)
        ]);
        return PollInputKey([ConsoleKey.Escape, ConsoleKey.Enter]) == ConsoleKey.Enter;
    }
    public static void ShowPausedMessage(int score, int snakeLength)
    {
        DrawMessageBox([
            "      " + Ansify("Game Paused", color: 15, bold: true, underlined: true),
            "",
            $" Score: {score}",
            $" Snake length: {snakeLength}",
            "",
            "",
            "",
            Ansify("    Enter to resume", dim: true)
        ]);
        PollInputKey([ConsoleKey.Enter]);
    }

    public static void DrawTitle()
    {
        Console.Title = Title;
        Console.SetCursorPosition(0,0);
        Console.Write(Title);
    }

    public static void DrawScore()
    {
        Console.SetCursorPosition(WindowDimensions.GetXEnd() - 11, 0);
        Console.Write("Score: 00000");
    }

    public static void UpdateScore(int score)
    {
        Console.SetCursorPosition(WindowDimensions.GetXEnd() - 4, 0);
        Console.Write(score.ToString("00000"));
    }

    public static void DrawInfobar()
    {
        Console.SetCursorPosition(0, WindowDimensions.GetYEnd());
        Console.Write(
            Ansify("Esc to quit, p to pause, ←↑↓→ to move", dim: true)
        );
    }

    public static void DrawGameArea()
    {
        DrawBox(GameAreaDimensions, BlockSymbols.RoundSingle);
    }

    public static void DrawMessageBox(string[] content, bool clip = false)
    {
        BoxDimensions innerDimensions = DrawBox(26, 10, BlockSymbols.SquareDouble);
        int i = 0;
        int y = innerDimensions.GetYStart();
        int yEnd = clip ? innerDimensions.GetYEnd() : int.MaxValue;
        while( y < yEnd && i < content.Length )
        {
            Console.SetCursorPosition(innerDimensions.GetXStart(), y);
            if ( clip )
                Console.Write(content[i].AsSpan(0, Math.Min(content[i].Length, innerDimensions.Width - 1)));
            else
                Console.Write(content[i]);
            i++; y++;
        }
    }

    public static BoxDimensions DrawBox(int width, int height, BlockSymbols? blockSymbols = null)
    {
        BlockSymbols symbols = blockSymbols.HasValue ? blockSymbols.Value : BlockSymbols.SquareSingle;
        Coordinates origin = new Coordinates(
            (WindowDimensions.Width - width) / 2,
            (WindowDimensions.Height - height) / 2
        );
        return DrawBox(new BoxDimensions(width, height, origin), blockSymbols);
    }
    public static BoxDimensions DrawBox(BoxDimensions dimensions, BlockSymbols? blockSymbols = null)
    {
        BlockSymbols symbols = blockSymbols.HasValue ? blockSymbols.Value : BlockSymbols.SquareSingle;
        int width = dimensions.Width - 1;
        int height = dimensions.Height - 1;
        int xStart = dimensions.GetXStart();
        int xEnd = xStart + width;
        int yStart = dimensions.GetYStart();
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
        int yStart = innerDimensions.GetYStart();
        int yEnd = innerDimensions.GetYEnd();
        int xStart = innerDimensions.GetXStart();
        string filler = new string(' ', innerDimensions.GetWidth());
        for(int y = yStart; y <= yEnd; y++)
        {
            Console.SetCursorPosition(xStart, y);
            Console.Write(filler);
        }
    }

    public static void ResetCursor()
    {
        Console.SetCursorPosition(WindowDimensions.GetXEnd(), WindowDimensions.GetYEnd());
    }

    public static ConsoleKey PollInputKey(ConsoleKey[]? acceptKeys = null, long? timeout = null)
    {
        ConsoleKey result = ConsoleKey.None;
        if (timeout == null)
        {
            if (acceptKeys == null)
            {
                result = Console.ReadKey(true).Key;
            }
            else
            {
                do
                {
                    result = Console.ReadKey(true).Key;
                } while (!acceptKeys.Contains(result));
            }
        }
        else
        {
            Stopwatch stopWatch = new Stopwatch();
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
        return result;
    }

    public static string Ansify(string text, int? color = null, bool bold = false, bool dim = false, bool italic = false, bool underlined = false)
    {
        string?[] controlSequences = new string[5];
        controlSequences[0] = color.HasValue ? $"38:5:{color.Value}" : null;
        controlSequences[1] = bold ? "1" : null;
        controlSequences[2] = dim ? "2" : null;
        controlSequences[3] = italic ? "3" : null;
        controlSequences[4] = underlined ? "4" : null;
        controlSequences = controlSequences.Where(s => s != null).ToArray();
        return $"\x1b[{String.Join(";", controlSequences)}m{text}\x1b[m";
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
        Path = new Coordinates?[maxLength];
        Head = spawnPosition;
        CurrentDirection = Direction.None;
        PreviousDirection = Direction.None;
        State = SnakeState.Normal;
    }

    public int Length { get; set; }
    public Coordinates?[] Path { get; set; }
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
                    bodyShape = BlockSymbols.SquareDouble.Horizontal.ToString();
                    break;
                case Direction.Up:
                case Direction.Down:
                    bodyShape = BlockSymbols.SquareDouble.Vertical.ToString();
                    break;
            }
        }
        else if (PreviousDirection == Direction.Up)
        {
            switch (CurrentDirection)
            {
                case Direction.Right:
                    bodyShape = BlockSymbols.SquareDouble.TopLeft.ToString();
                    break;
                case Direction.Left:
                    bodyShape = BlockSymbols.SquareDouble.TopRight.ToString();
                    break;
            }
        }
        else if (PreviousDirection == Direction.Down)
        {
            switch (CurrentDirection)
            {
                case Direction.Right:
                    bodyShape = BlockSymbols.SquareDouble.BottomLeft.ToString();
                    break;
                case Direction.Left:
                    bodyShape = BlockSymbols.SquareDouble.BottomRight.ToString();
                    break;
            }
        }
        else if (PreviousDirection == Direction.Right)
        {
            switch (CurrentDirection)
            {
                case Direction.Up:
                    bodyShape = BlockSymbols.SquareDouble.BottomRight.ToString();
                    break;
                case Direction.Down:
                    bodyShape = BlockSymbols.SquareDouble.TopRight.ToString();
                    break;
            }
        }
        else if (PreviousDirection == Direction.Left)
        {
            switch (CurrentDirection)
            {
                case Direction.Up:
                    bodyShape = BlockSymbols.SquareDouble.BottomLeft.ToString();
                    break;
                case Direction.Down:
                    bodyShape = BlockSymbols.SquareDouble.TopLeft.ToString();
                    break;
            }
        }
        Path[0] = new Coordinates(Head.X, Head.Y, bodyShape);
    }

    public void AutoMove()
    {
        Move(CurrentDirection);
    }

    public void Move(Direction direction)
    {
        PreviousDirection = CurrentDirection;
        CurrentDirection = direction;
        StorePosition();
        switch (CurrentDirection)
        {
            case Direction.Up:
                Head = Head.OffsetY(-1);
                break;
            case Direction.Down:
                Head = Head.OffsetY(+1);
                break;
            case Direction.Left:
                Head = Head.OffsetX(-1);
                break;
            case Direction.Right:
                Head = Head.OffsetX(+1);
                break;
        }
    }

    public void Draw(bool deleteOldTail = true)
    {
        if ( deleteOldTail )
        {
            Coordinates? tailPoint = Path[int.Max(0, Length - 1)];
            if (tailPoint.HasValue)
            {
                Console.SetCursorPosition(tailPoint.Value.X, tailPoint.Value.Y);
                Console.Write(' ');
            }
        }
        int color = 255;
        for (int i = 0; i < Length - 1; i++)
        {
            if (Path[i] == null)
                throw new InvalidOperationException($"Snake.Length is {Length} but Snake.Path[{i}] is null");
            Console.SetCursorPosition(Path[i].Value.X, Path[i].Value.Y);
            char bodySymbol = DetermineSnakeDrawSymbol(Path[i].Value.Annotation, 'x');
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
        return BlockSymbols.Transliterate(symbol, BlockSymbols.SquareDouble, BlockSymbols.SquareSingle);
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
        int index = Array.FindIndex(Path, 0, Length - 1, coords => coords != null && coords.Value.X == point.X && coords.Value.Y == point.Y);
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
            fruitSymbol = builtinFruits[RndRng.Next(0, builtinFruits.Length)];
        if (!color.HasValue)
            color = builtinColors[RndRng.Next(0, builtinColors.Length)];
        Symbol = fruitSymbol.Value;
        Color = color.Value;
        Position = position;
    }
    private static readonly Random RndRng = new Random();
    public static readonly char[] builtinFruits = ['•', '◦', '▴', '■', '□', '᛭', '⨯', 'ꚛ', '★', '☆'];
    public static readonly int[] builtinColors = [11, 12, 13, 14, 15];
    public readonly char Symbol;
    public readonly int Color;
    public readonly Coordinates Position;
    public static Coordinates FindSpawnPosition(Snake Snake)
    {
        BoxDimensions innerGameArea = UserInterface.GameAreaDimensions.GetInnerDimensions();
        while(true)
        {
            Coordinates trial = new Coordinates(
                RndRng.Next(innerGameArea.GetXStart(), innerGameArea.GetXEnd()),
                RndRng.Next(innerGameArea.GetYStart(), innerGameArea.GetYEnd())
            );
            if(!Snake.CollidesWithPoint(trial))
                return trial;
        }
    }

    public void Draw()
    {
        Console.SetCursorPosition(Position.X, Position.Y);
        Console.Write(UserInterface.Ansify(Symbol.ToString(), color: Color));
    }
}

public class Game
{
    public Game()
    {
        BoxDimensions innerGameArea = UserInterface.GameAreaDimensions.GetInnerDimensions();
        Snake = new Snake(
            UserInterface.WindowDimensions.GetArea(),
            new Coordinates(
                RndRng.Next(innerGameArea.GetXStart(), innerGameArea.GetXEnd()),
                RndRng.Next(innerGameArea.GetYStart(), innerGameArea.GetYEnd())
            )
        );
        AutoMoveDelayMax = 1000;
        AutoMoveDelayMin = 80;
        AutoMoveDelay = AutoMoveDelayMax;
    }

    private static readonly Random RndRng = new Random();

    public Snake Snake { get; init; }

    public Fruit? Fruit { get; set; }

    protected int AutoMoveDelayMax { get; set; }

    protected int AutoMoveDelayMin { get; set; }

    protected int AutoMoveDelay { get; set; }

    protected int Score = 0;

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
        while( true )
        {
            if(!UserInterface.ConsoleSizeOK())
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
                    Snake.Move(Direction.Up);
                    break;
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    Snake.Move(Direction.Left);
                    break;
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    Snake.Move(Direction.Down);
                    break;
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    Snake.Move(Direction.Right);
                    break;
                case ConsoleKey.None:
                    Snake.AutoMove();
                    break;
                case ConsoleKey.P:
                    UserInterface.ShowPausedMessage(Score, Snake.Length);
                    UserInterface.ClearGameArea();
                    Snake.Draw();
                    if (Fruit != null)
                        Fruit.Draw();
                    break;
                case ConsoleKey.Escape:
                    return false;
            }
            if( Snake.Length > 1 && Snake.CollidesWithPoint(Snake.Head, includeHead: false) )
            {
                Snake.State = SnakeState.Dead;
            }
            if ( !UserInterface.GameAreaDimensions.ContainsPoint(Snake.Head) )
            {
                Snake.State = SnakeState.Dead;
            }
            else if ( Snake.CollidesWithPoint(Fruit.Position) )
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
                    if (AutoMoveDelay < AutoMoveDelayMin )
                        AutoMoveDelay = AutoMoveDelayMin;
                    Fruit = null;
                    break;
                case SnakeState.Dead:
                    Snake.Draw();
                    return UserInterface.ShowGameOverMessage(Score, Snake.Length);
            }
            if (Snake.Length >= Snake.Path.Length)
                return UserInterface.ShowWinMessage(Score, Snake.Length);
            Snake.Draw();
        }
    }
}

