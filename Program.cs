using System.Diagnostics;

Console.OutputEncoding = System.Text.Encoding.Unicode;
Console.InputEncoding = System.Text.Encoding.Unicode;

const int gameAreaHeight = 12;
const int gameAreaWidth = 38;
const int gameAreaXStart = 1;
const int gameAreaXEnd = gameAreaWidth + 1;
const int gameAreaYStart = 2;
const int gameAreaYEnd = gameAreaHeight + 1;
const int minWindowHeight = gameAreaHeight + 4;
const int minWindowWidth = gameAreaWidth + 2;
const int maxSnakeLength = gameAreaHeight * gameAreaWidth;
Random random = new Random();
string[] fruitSymbols = {
    "•", "◦", "▴", "■", "□", "᛭", "⨯", "ꚛ", "★", "☆"
};
// Game state
int score;
int snakeLength;
AnnotatedCoordinates?[] snakePath;
AnnotatedCoordinates snakeHead;
Direction? currentSnakeDirection;
Direction? previousSnakeDirection;
int autoMoveTimeout;
AnnotatedCoordinates fruit;
bool playAgain;


main();


void main()
{
    int errorInGame = 0;
    do
    {
        score = 0;
        snakeLength = 1;
        snakePath = new AnnotatedCoordinates?[maxSnakeLength];
        snakeHead = new AnnotatedCoordinates(random.Next(gameAreaXStart, gameAreaXEnd + 1), random.Next(gameAreaYStart, gameAreaYEnd + 1), "ö");
        currentSnakeDirection = null;
        previousSnakeDirection = null;
        autoMoveTimeout = 500;
        fruit = new AnnotatedCoordinates(0, 0, fruitSymbols[0]);
        playAgain = false;
        errorInGame = RunGame();
    } while (playAgain && errorInGame == 0);
    ResetCursor();
    Environment.Exit(errorInGame);
}

int RunGame()
{
    if (!ConsoleSizeOK())
    {
        PrintConsoleSizeError();
        return 1;
    }
    InitializeGameArea();
    DrawSnake();
    SpawnFruit();
    DrawFruit();
    while (true)
    {
        if (!ConsoleSizeOK())
        {
            PrintConsoleSizeError();
            return 1;
        }
        switch (PollInputKey(
            acceptKeys: [
                ConsoleKey.W, ConsoleKey.A,
                ConsoleKey.S, ConsoleKey.D,
                ConsoleKey.UpArrow, ConsoleKey.LeftArrow,
                ConsoleKey.DownArrow, ConsoleKey.RightArrow,
                ConsoleKey.Escape, ConsoleKey.P
            ],
            timeout: autoMoveTimeout
        ))
        {
            case ConsoleKey.UpArrow:
            case ConsoleKey.W:
                MoveSnakeUp();
                break;
            case ConsoleKey.DownArrow:
            case ConsoleKey.S:
                MoveSnakeDown();
                break;
            case ConsoleKey.LeftArrow:
            case ConsoleKey.A:
                MoveSnakeLeft();
                break;
            case ConsoleKey.RightArrow:
            case ConsoleKey.D:
                MoveSnakeRight();
                break;
            case ConsoleKey.None:
                MoveSnakeAuto();
                break;
            case ConsoleKey.P:
                DisplayPauseMessage();
                break;
            case ConsoleKey.Escape:
                return 0;
        }
        bool isChomping = CollidesWithSnake(fruit.X, fruit.Y);
        if (isChomping)
        {
            UpdateScore(snakeLength);
            snakeLength++;
            if (autoMoveTimeout - (snakeLength * 2) > 50 )
                autoMoveTimeout -= snakeLength * 2;
            SpawnFruit();
        }
        bool isDead = CollidesWithSnake(snakeHead.X, snakeHead.Y, ignoreHead: true);
        if (snakeLength >= maxSnakeLength)
        {
            DisplayWinMessage();
            return 0;
        }
        ClearGameArea();
        DrawFruit();
        DrawSnake(isChomping, isDead);
        ResetCursor();
        if (isDead)
        {
            DisplayGameOverMessage();
            return 0;
        }
    }
}

void ResetCursor()
{
    // Set cursor to bottom right corner
    // This makes for a "cleaner" exit, esp. if interrupted
    Console.SetCursorPosition(minWindowWidth, minWindowHeight - 1);
}

ConsoleKey PollInputKey(ConsoleKey[]? acceptKeys = null, long? timeout = null)
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

void ClearGameArea()
{
    for (int x = gameAreaXStart; x <= gameAreaXEnd; x++)
    {
        for (int y = gameAreaYStart; y <= gameAreaYEnd; y++)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(" ");
        }
    }
}

void DisplayWinMessage()
{
    var (xStart, xEnd, yStart, yEnd) = DrawBox(26, 10);
    Console.SetCursorPosition(xStart, yStart);
    Console.Write("     ");
    Console.Write(AnsiMarkup("YOU HAVE WON!!", 15, bold: true, underlined: true));
    Console.SetCursorPosition(xStart, yStart + 2);
    Console.Write($" Score: {score}");
    Console.SetCursorPosition(xStart, yStart + 3);
    Console.Write($" Snake length: {snakeLength}");
    Console.SetCursorPosition(xStart, yEnd - 1);
    Console.Write(AnsiMarkup("      Esc to quit", dim: true));
    Console.SetCursorPosition(xStart, yEnd);
    Console.Write(AnsiMarkup("  Enter to play again", dim: true));
    playAgain = PollInputKey([ConsoleKey.Escape, ConsoleKey.Enter]) == ConsoleKey.Enter;
}

void DisplayGameOverMessage()
{
    var (xStart, xEnd, yStart, yEnd) = DrawBox(26, 10);
    Console.SetCursorPosition(xStart, yStart);
    Console.Write("       ");
    Console.Write(AnsiMarkup("Game Over", 15, bold: true, underlined: true));
    Console.SetCursorPosition(xStart, yStart + 2);
    Console.Write($" Score: {score}");
    Console.SetCursorPosition(xStart, yStart + 3);
    Console.Write($" Snake length: {snakeLength}");
    Console.SetCursorPosition(xStart, yEnd - 1);
    Console.Write(AnsiMarkup("      Esc to quit", dim: true));
    Console.SetCursorPosition(xStart, yEnd);
    Console.Write(AnsiMarkup("  Enter to play again", dim: true));
    playAgain = PollInputKey([ConsoleKey.Escape, ConsoleKey.Enter]) == ConsoleKey.Enter;
}

void DisplayPauseMessage()
{
    var (xStart, xEnd, yStart, yEnd) = DrawBox(26, 10);
    Console.SetCursorPosition(xStart, yStart);
    Console.Write("       ");
    Console.Write(AnsiMarkup("Game Paused", 15, bold: true, underlined: true));
    Console.SetCursorPosition(xStart, yStart + 2);
    Console.Write($" Score: {score}");
    Console.SetCursorPosition(xStart, yStart + 3);
    Console.Write($" Snake length: {snakeLength}");
    Console.SetCursorPosition(xStart, yEnd);
    Console.Write(AnsiMarkup("  Press Enter to resume", dim: true));
    playAgain = PollInputKey([ConsoleKey.Enter]) == ConsoleKey.Enter;
}

(int xStart, int xEnd, int yStart, int yEnd) DrawBox(int width, int height)
{
    int xStart = (minWindowWidth - --width) / 2;
    int xEnd = xStart + width;
    int yStart = (minWindowHeight - --height) / 2;
    int yEnd = yStart + height;
    for (int x = xStart; x <= xEnd; x++)
    {
        for (int y = yStart; y <= yEnd; y++)
        {
            Console.SetCursorPosition(x, y);
            if (x == xStart && y == yStart)
            {
                Console.Write("╔");
            }
            else if (x == xStart && y == yEnd)
            {
                Console.Write("╚");
            }
            else if (x == xEnd && y == yStart)
            {
                Console.Write("╗");
            }
            else if (x == xEnd && y == yEnd)
            {
                Console.Write("╝");
            }
            else if (x == xStart || x == xEnd)
            {
                Console.Write("║");
            }
            else if (y == yStart || y == yEnd)
            {
                Console.Write("═");
            }
            else
            {
                Console.Write(" ");
            }
        }
    }
    return (xStart + 1, xEnd - 1, yStart + 1, yEnd - 1);
}

void ShiftSnakePathArray()
{
    for (int i = snakePath.Length - 2; i >= 0; i--)
    {
        snakePath[i + 1] = snakePath[i];
    }
}

void StoreSnakePosition()
{
    ShiftSnakePathArray();
    string bodyShape = "o";
    if (currentSnakeDirection == previousSnakeDirection)
    {
        switch (currentSnakeDirection)
        {
            case Direction.Right:
            case Direction.Left:
                bodyShape = "═";
                break;
            case Direction.Up:
            case Direction.Down:
                bodyShape = "║";
                break;
        }
    }
    else if (previousSnakeDirection == Direction.Up)
    {
        switch (currentSnakeDirection)
        {
            case Direction.Right:
                bodyShape = "╔";
                break;
            case Direction.Left:
                bodyShape = "╗";
                break;
        }
    }
    else if (previousSnakeDirection == Direction.Down)
    {
        switch (currentSnakeDirection)
        {
            case Direction.Right:
                bodyShape = "╚";
                break;
            case Direction.Left:
                bodyShape = "╝";
                break;
        }
    }
    else if (previousSnakeDirection == Direction.Right)
    {
        switch (currentSnakeDirection)
        {
            case Direction.Up:
                bodyShape = "╝";
                break;
            case Direction.Down:
                bodyShape = "╗";
                break;
        }
    }
    else if (previousSnakeDirection == Direction.Left)
    {
        switch (currentSnakeDirection)
        {
            case Direction.Up:
                bodyShape = "╚";
                break;
            case Direction.Down:
                bodyShape = "╔";
                break;
        }
    }
    snakePath[0] = new AnnotatedCoordinates(snakeHead.X, snakeHead.Y, bodyShape);
}

void MoveSnakeAuto()
{
    switch (currentSnakeDirection)
    {
        case Direction.Up:
            MoveSnakeUp();
            break;
        case Direction.Down:
            MoveSnakeDown();
            break;
        case Direction.Left:
            MoveSnakeLeft();
            break;
        case Direction.Right:
            MoveSnakeRight();
            break;
    }
}

void MoveSnakeUp()
{
    if (snakeHead.Y - 1 < gameAreaYStart)
        return;
    previousSnakeDirection = currentSnakeDirection;
    currentSnakeDirection = Direction.Up;
    StoreSnakePosition();
    snakeHead = snakeHead.MoveY(-1);
}

void MoveSnakeDown()
{
    if (snakeHead.Y + 1 > gameAreaYEnd)
        return;
    previousSnakeDirection = currentSnakeDirection;
    currentSnakeDirection = Direction.Down;
    StoreSnakePosition();
    snakeHead = snakeHead.MoveY(+1);
}

void MoveSnakeLeft()
{
    if (snakeHead.X - 1 < gameAreaXStart)
        return;
    previousSnakeDirection = currentSnakeDirection;
    currentSnakeDirection = Direction.Left;
    StoreSnakePosition();
    snakeHead = snakeHead.MoveX(-1);
}

void MoveSnakeRight()
{
    if (snakeHead.X + 1 > gameAreaXEnd)
        return;
    previousSnakeDirection = currentSnakeDirection;
    currentSnakeDirection = Direction.Right;
    StoreSnakePosition();
    snakeHead = snakeHead.MoveX(+1);
}

void DrawSnake(bool chomping = false, bool dead = false)
{
    string headSymbol = dead ? "X" : (chomping ? "Ö" : "ö");
    // Draw snake body
    int color = 255;
    for (int i = 0; i < snakeLength - 1; i++)
    {
        if (snakePath[i] == null)
            throw new InvalidOperationException($"snakeLength is {snakeLength} but snakePath[{i}] is null");
        Console.SetCursorPosition(snakePath[i].Value.X, snakePath[i].Value.Y);
        string bodyShape = dead ? "x" : snakePath[i].Value.Annotation;
        if (i == snakeLength - 2)
            bodyShape = ThinOutTail(bodyShape);
        Console.Write(AnsiMarkup(bodyShape, color));
        if (color > 240)
            color--;
    }
    // Draw snake head
    Console.SetCursorPosition(snakeHead.X, snakeHead.Y);
    Console.Write(AnsiMarkup(headSymbol.ToString(), 15));
}

bool ConsoleSizeOK()
{
    return Console.WindowHeight >= minWindowHeight && Console.WindowWidth >= minWindowWidth;
}

void PrintConsoleSizeError()
{
    Console.Clear();
    Console.SetCursorPosition(0, 0);
    Console.WriteLine("ERROR: Console window too small.");
    Console.Write($"Please ensure the console window is at least ");
    Console.Write($"{minWindowHeight} characters high and ");
    Console.WriteLine($"{minWindowWidth} characters wide.");
    Console.Write($"(Your console window is {Console.WindowHeight} characters high ");
    Console.WriteLine($"and {Console.WindowWidth} characters wide)");
}

// Returns true if the passed coordinate collides with any part of the snake
bool CollidesWithSnake(int x, int y, bool ignoreHead = false)
{
    for (int i = 0; i < snakeLength - 1; i++)
    {
        if (x == snakePath[i].Value.X && y == snakePath[i].Value.Y)
            return true;
    }
    if (ignoreHead)
        return false;
    return x == snakeHead.X && y == snakeHead.Y;
}

void SpawnFruit()
{
    int x, y;
    do
    {
        x = random.Next(gameAreaXStart, gameAreaXEnd + 1);
        y = random.Next(gameAreaYStart, gameAreaYEnd + 1);
    } while (CollidesWithSnake(x, y));
    string symbol = AnsiMarkup(fruitSymbols[random.Next(fruitSymbols.Length)], random.Next(1, 16));
    fruit = new AnnotatedCoordinates(x, y, symbol);
}

void DrawFruit()
{
    // Console.SetCursorPosition(fruitX, fruitY);
    // Console.Write(fruitCharacter);
    Console.SetCursorPosition(fruit.X, fruit.Y);
    Console.Write(fruit.Annotation);
}

void UpdateScore(int increment = 0)
{
    score += increment;
    Console.SetCursorPosition(minWindowWidth - 5, 0);
    Console.Write(score.ToString("000000"));
}

void InitializeGameArea()
{
    Console.Clear();
    Console.CursorVisible = false;
    Console.SetCursorPosition(0, 0);
    Console.Write("Snake# v0.1");
    Console.SetCursorPosition(minWindowWidth - 12, 0);
    Console.Write("Score: 000000");
    for (int left = 0; left <= gameAreaWidth + 2; left++)
    {
        switch (left)
        {
            case 0:
                Console.SetCursorPosition(left, 1);
                Console.Write("╭");
                Console.SetCursorPosition(left, gameAreaHeight + 2);
                Console.Write("╰");
                break;
            case gameAreaWidth + 2:
                Console.SetCursorPosition(left, 1);
                Console.Write("╮");
                Console.SetCursorPosition(left, gameAreaHeight + 2);
                Console.Write("╯");
                break;
            default:
                Console.SetCursorPosition(left, 1);
                Console.Write("─");
                Console.SetCursorPosition(left, gameAreaHeight + 2);
                Console.Write("─");
                break;
        }
    }
    for (int top = 2; top <= gameAreaHeight + 1; top++)
    {
        Console.SetCursorPosition(0, top);
        Console.Write("│");
        Console.SetCursorPosition(gameAreaWidth + 2, top);
        Console.Write("│");
    }
    Console.SetCursorPosition(0, minWindowHeight - 1);
    Console.Write(AnsiMarkup("Esc to quit, ← ↑ ↓ → to move, p to pause", dim: true));
}

string ThinOutTail(string tail)
{
    switch (tail)
    {
        case "═":
            return "─";
        case "║":
            return "│";
        case "╔":
            return "┌";
        case "╝":
            return "┘";
        case "╚":
            return "└";
        case "╗":
            return "┐";
        default:
            return tail;
    }
}

string AnsiMarkup(string text, int? color = null, bool bold = false, bool dim = false, bool italic = false, bool underlined = false)
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

public readonly struct AnnotatedCoordinates
{
    public AnnotatedCoordinates(int x, int y, string annotation)
    {
        X = x;
        Y = y;
        Annotation = annotation;
    }

    public int X { get; init; }
    public int Y { get; init; }
    public string Annotation { get; init; }

    public override string ToString()
    {
        return $"({X}, {Y}; {Annotation})";
    }

    public AnnotatedCoordinates MoveX(int amount)
    {
        return new AnnotatedCoordinates(X + amount, Y, Annotation);
    }

    public AnnotatedCoordinates MoveY(int amount)
    {
        return new AnnotatedCoordinates(X, Y + amount, Annotation);
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}