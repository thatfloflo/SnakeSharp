using System.Diagnostics;

namespace Snake;

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
