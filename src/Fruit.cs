namespace Snake;

/// <summary>Represents a single fruit in a game of snake.</summary>
public class Fruit
{
    /// <summary>Instantiates a new fruit to use in a game.</summary>
    /// <remarks>
    /// <para>
    /// The fruit's properties, including its <i>position</i> (absolute coordinates on the screen)
    /// are fixed upon instantiation. If a symbol and/or color code is supplied, this will be used
    /// for rendering the fruit on the screen using the <see cref="Fruit.Draw"/> method.
    /// If a symbol or color code is not supplied, this will be selected randomly from the built-in
    /// sets of symbols and colors exposed via <see cref="Fruit.FruitSelection"/>
    /// ('•', '◦', '▴', '■', '□', '᛭', '⨯', 'ꚛ', '★', '☆') and <see cref="Fruit.ColorSelection"/>
    /// (8-15), respectively.
    /// </para>
    /// <para>
    /// The static method <see cref="Fruit.FindSpawnPosition"/> can be used
    /// to assist in finding a suitable set of coordinates to <i>position</i> the fruit.
    /// </para>
    /// <example>
    /// The following example shows how to instantiate and render (spawn) a new fruit, assuming
    /// you have already initialized the UI with <see cref="UserInterface.Initialize"/> and that
    /// <c>mySnake</c> refers to an instance of <see cref="Snake"/>.
    /// <code>
    ///     Coordinates? fruitPosition = Fruit.FindSpawnPosition(mySnake, 10000);
    ///     if(!fruitPosition.HasValue)
    ///     {
    ///         // Handle error...
    ///     }
    ///     var myFruit = new Fruit(fruitPosition.Value);
    ///     Fruit.Draw();
    /// </code>
    /// </example>
    /// </remarks>
    /// <param name="position">The coordinates (absolute) where the fruit should be placed.</param>
    /// <param name="fruitSymbol">Optionally, a symbol that should be used to render the fruit.</param>
    /// <param name="color">Optionally, an ANSI 256-color mode color index to use for rendering the fruit.</param>
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
    /// <summary>The set of built-in symbols used for rendering fruits.</summary>
    public static char[] FruitSelection { get => _fruitSelection; }
    private static readonly int[] _colorSelection = [8, 9, 10, 11, 12, 13, 14, 15];
    /// <summary>The set of built-in ANSI 256-color mode color codes used for rendering fruits.</summary>
    public static int[] ColorSelection { get => _colorSelection; }
    /// <summary>The symbol used to render the fruit.</summary>
    public char Symbol { get; init; }
    /// <summary>The ANSI 256-color mode color code used to render the fruit.</summary>
    public int Color { get; init; }
    /// <summary>The coordinates (absolute) where the fruit is rendered on the screen.</summary>
    public Coordinates Position { get; init; }

    /// <summary>Draws the fruit on the screen.</summary>
    public void Draw()
    {
        Console.SetCursorPosition(Position.X, Position.Y);
        Console.Write(UserInterface.Ansify(Symbol.ToString(), color: Color));
    }

    /// <summary>Finds a random set of available coordinates to position a fruit.</summary>
    /// <remarks>
    /// <b>Caution:</b> This method <b>may potentially run infinitely</b>. This condition can occur
    /// if no value for <i>maxAttempts</i> <c>&lt; int.MaxValue</c> is provided and there is
    /// no remaining free coordinate within the <see cref="UserInterface.GameAreaDimensions"/>.
    /// It is your responsibility to either ensure that the method is not called if the game
    /// area is exhausted or to limit its <i>maxAttempts</i>. 
    /// </remarks>
    /// <param name="snake">The instance of the current game's <see cref="Snake"/>.</param>
    /// <param name="maxAttempts">The maximal number of attempts to make to find a set of
    /// suitable coordinates. If <c>maxAttempts &gt;= int.MaxValue</c> (the default), then there
    /// is <b>no limit</b> to the number of attempts.</param>
    /// <returns>
    /// Returns a set of random coordinates that is within the
    /// <see cref="UserInterface.GameAreaDimensions"/> and which does not collide with
    /// the <i>snake</i>, or <c>null</c> if no such coordinates could be found in fewer
    /// than <i>maxAttempts</i> attempts.
    /// </returns>
    public static Coordinates? FindSpawnPosition(Snake snake, int maxAttempts = int.MaxValue)
    {
        BoxDimensions innerGameArea = UserInterface.GameAreaDimensions.GetInnerDimensions();
        for(var i = 0; i <= int.MaxValue; i++)
        {
            var trial = new Coordinates(
                s_random.Next(innerGameArea.XStart, innerGameArea.XEnd),
                s_random.Next(innerGameArea.YStart, innerGameArea.YEnd)
            );
            if (!snake.CollidesWithPoint(trial))
                return trial;
        }
        return null;
    }
}
