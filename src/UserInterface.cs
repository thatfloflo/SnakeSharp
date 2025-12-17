using System.Diagnostics;

namespace Snake;

/// <summary>
/// Encapsulates the user interface and related utility functions for the game.
/// </summary>
public static class UserInterface
{
    private static readonly BoxDimensions s_gameAreaDimensions = new(38, 12, new Coordinates(0, 1));
    /// <summary>The dimensions and origin of the game area on the console.</summary>
    public static BoxDimensions GameAreaDimensions { get => s_gameAreaDimensions; }
    private static readonly BoxDimensions s_windowDimensions = new(GameAreaDimensions.Width, GameAreaDimensions.Height + 2, new Coordinates(0, 0));
    /// <summary>The dimensions and origin of the 'window' (the area in which the user interface is drawn) on the console.</summary>
    public static BoxDimensions WindowDimensions { get => s_windowDimensions; }

    /// <summary>The program's title shown in the user interface.</summary>
    public static string Title
    {
        get => Program.Resources.GetString("App.Name")! + " v" + Program.Resources.GetString("App.Version");
    }

    /// <summary>Initialises and draws the user interface.</summary>
    /// <remarks>
    /// This clears the current buffer and modifies the settings for
    /// <see cref="Console.OutputEncoding"/>, <see cref="Console.InputEncoding"/>,
    /// and <see cref="Console.CursorVisible"/>. If you wish to be able
    /// to restore any of these to their original state later, you must
    /// store the current values and then reset them yourself as needed.
    /// </remarks>
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

    /// <summary>Displays a modal overlay with a message indicating that the player has won the game.</summary>
    /// <param name="score">The player's current score.</param>
    /// <param name="snakeLength">The current length of the snake.</param>
    /// <param name="difficulty">The difficulty level of the current game.</param>
    /// <returns>True if the player indicates they want to play again, False if they want to quit.</returns>
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

    /// <summary>Displays a modal overlay with a message indicating that the game is over (i.e. the loss condition met).</summary>
    /// <param name="score">The player's current score.</param>
    /// <param name="snakeLength">The current length of the snake.</param>
    /// <param name="difficulty">The difficulty level of the current game.</param>
    /// <returns>True if the player indicates they want to play again, False if they want to quit.</returns>
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

    /// <summary>Displays a modal overlay to show that the game is in a paused state.</summary>
    /// <remarks>
    /// This function blocks until the user presses Enter to continue. Note that the user cannot
    /// quit the game by pressing Escape while paused, though of course control interrupts continue
    /// to be received as normal, so e.g. Ctrl+C is still effective.
    /// </remarks>
    /// <param name="score">The player's current score.</param>
    /// <param name="snakeLength">The current length of the snake.</param>
    /// <param name="difficulty">The difficulty level of the current game.</param>
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

    /// <summary>Draws the title in the top left corner.</summary>
    public static void DrawTitle()
    {
        Console.Title = Title;
        Console.SetCursorPosition(0, 0);
        Console.Write(Title);
    }

    /// <summary>Draws the initial, all-zero score in the top right corner.</summary>
    /// <remarks>Use <see cref="UpdateScore"/> to update the score to a particulat value.</remarks>
    public static void DrawScore()
    {
        Console.SetCursorPosition(WindowDimensions.XEnd - 12, 0);
        Console.Write("Score: 000000");
    }

    /// <summary>Updates the score displayed the value passed as <i>score</i>.</summary>
    /// <param name="score">The value of the score to display.</param>
    public static void UpdateScore(int score)
    {
        Console.SetCursorPosition(WindowDimensions.XEnd - 5, 0);
        Console.Write(score.ToString("000000"));
    }

    /// <summary>Draws a line of information about input controls at the bottom.</summary>
    public static void DrawInfobar()
    {
        Console.SetCursorPosition(0, WindowDimensions.YEnd);
        Console.Write(
            Ansify(Program.Resources.GetString("UserInterface.InfoBarInstructions")!, dim: true)
        );
    }

    /// <summary>Draws the boundary around the game area.</summary>
    public static void DrawGameArea()
    {
        DrawBox(GameAreaDimensions, BoxSymbols.RoundSingle);
    }

    /// <summary>Draws an overlay box displaying a message.</summary>
    /// <param name="content">The message to display, split by lines.</param>
    /// <param name="clip">Whether to clip content that overruns the boundaries of the box or not.</param>
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

    /// <summary>
    /// Draws a box of the specified width and height centered vertically and horizontally and returns the
    /// resulting dimensions and origin.
    /// </summary>
    /// <param name="width">The width of the box to draw.</param>
    /// <param name="height">The height of the box to draw.</param>
    /// <param name="boxSymbols">The set of box drawing symbols to use.</param>
    /// <returns>The (outer) dimensions and origin of the box that has been drawn.</returns>
    public static BoxDimensions DrawBox(int width, int height, BoxSymbols? boxSymbols = null)
    {
        BoxSymbols symbols = boxSymbols ?? BoxSymbols.SquareSingle;
        var origin = new Coordinates(
            (WindowDimensions.Width - width) / 2,
            (WindowDimensions.Height - height) / 2
        );
        return DrawBox(new BoxDimensions(width, height, origin), boxSymbols);
    }

    /// <summary>
    /// Draws a box of the specified dimensions placed at the specified origin and returns the
    /// resulting dimensions and origins.
    /// </summary>
    /// <remarks>
    /// Call <see cref="DrawBox(int, int, BoxSymbols?)"/> instead for a box that is
    /// automatically centred vertically and horizontaly.
    /// </remarks>
    /// <param name="dimensions">The dimensions and origin of the box to draw.</param>
    /// <param name="boxSymbols">The set of box drawing symbols to use.</param>
    /// <returns>The (outer) dimensions and origin of the box that has been drawn.</returns>
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
    
    /// <summary>Clears the game area by overwriting its inner dimensions with spaces.</summary>
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

    /// <summary>Resets the cursor position to the bottom right of the window drawn in the console.</summary>
    public static void ResetCursor()
    {
        Console.SetCursorPosition(WindowDimensions.XEnd, WindowDimensions.YEnd);
    }

    /// <summary>
    /// Read a single key input with optional filtering by an accept list and optional timeout.
    /// </summary>
    /// <remarks>
    /// <para>If an accept list (<i>acceptKeys</i>) is provided, key input that is not in the accept list is ignored.</para>
    /// <para>
    /// If a <i>timeout</i> is specified, <see cref="ConsoleKey.None"/> is returned if no user input (possibly
    /// filtered by the accept list) is received by the time the timeout expires.
    /// </para>
    /// </remarks>
    /// <param name="acceptKeys">A list of <see cref="ConsoleKey"/>s to accept as input, or Null to accept any key.</param>
    /// <param name="timeout">The maximum time in milliseconds to wait for user input before returning.</param>
    /// <returns>
    /// The <see cref="ConsoleKey"/> the user has pressed, or <see cref="ConsoleKey.None"/> if the<i>timeout</i>
    /// expires before an acceptable key input is received.
    /// </returns>
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

    /// <summary>
    /// Wraps a string in ANSI control sequences ready for drawing to the console.
    /// </summary>
    /// <remarks>
    /// The returned string includes the reset sequence <c>\x1b[m</c> at the end of the string, so that
    /// the markup will not persist past the end of the string when drawn to the console.
    /// </remarks>
    /// <param name="text">The string to wrap.</param>
    /// <param name="color">A 256-colour mode color code (range 0-255).</param>
    /// <param name="bold">Whether the text should be marked as bold or bright (implementation is terminal dependent).</param>
    /// <param name="dim">Whether the text should be marked as dim.</param>
    /// <param name="italic">Whether the text should be marked as italic.</param>
    /// <param name="underlined">Whether the text should be marked as underlined.</param>
    /// <param name="skipWhitespace">Whether to skip whitespace from the ANSI markup or include it.</param>
    /// <returns>The marked up string including the required ANSI constrol sequences.</returns>
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

    /// <summary>Pads a string with spaces to center it relative to the specified <i>width</i>.</summary>
    /// <remarks>
    /// <para>
    /// Note that the returned string only includes the spaces to the left of the text which are needed to
    /// center it relative to the specified <i>width</i>, but omits the spaces to the right (because these
    /// are usually not required).
    /// </para>
    /// <example>
    /// Example:
    /// <code>
    /// string myString = UserInterface.PadToCenter("Hello!", 10);
    /// Console.Write(@"\"{myString}\"");
    /// // Output: "  Hello!"
    /// </code>
    /// </example>
    /// <para>
    /// If the required padding is not equally divisible among the left and right sides, the string is
    /// aligned one space further to the left than the right.
    /// </para>
    /// </remarks>
    /// <param name="text">The string to pad.</param>
    /// <param name="width">The width relative to which the padded string should be centred.</param>
    /// <returns>The padded string.</returns>
    public static string PadToCenter(string text, int width)
    {
        if (text.Length >= width)
            return text;
        return new string(' ', width / 2 - (int)Math.Ceiling(text.Length / 2.0)) + text;
    }

    /// <summary>Checks whether the size of the console window is sufficient to contain the <see cref="WindowDimensions"/>.</summary>
    /// <returns>True if the console window is of sufficient size, false otherwise.</returns>
    public static bool ConsoleSizeOK()
    {
        return Console.WindowWidth >= WindowDimensions.Width && Console.WindowHeight >= WindowDimensions.Height;
    }
}
