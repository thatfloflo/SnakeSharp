namespace Snake;

/// <summary>A relative direction.</summary>
/// <remarks>Note that the direction may be <see cref="Direction.None"/>.</remarks>
public enum Direction
{
    None,
    Up,
    Down,
    Left,
    Right,
}

/// <summary>Extension for the <see cref="Direction"/> enum.</summary>
public static class DirectionExtension
{
    // extension(Direction direction)
    // {
    //     public bool IsOpposite(Direction other)
    //     {
    //         if (direction != other
    //             && direction != Direction.None
    //             && other != Direction.None
    //             && ((direction == Direction.Up && other == Direction.Down)
    //                 || (direction == Direction.Down && other == Direction.Up)
    //                 || (direction == Direction.Left && other == Direction.Right)
    //                 || (direction == Direction.Right && other == Direction.Left)))
    //             return true;
    //         return false;
    //     }
    // }

    /// <summary>
    /// Whether the direction is the opposite relative direction to <i>other</i>.
    /// </summary>
    /// <param name="other">The other relative direction.</param>
    /// <returns>True if the two directions are relative opposites, False otherwise.</returns>
    public static bool IsOpposite(this Direction direction, Direction other)
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

/// <summary>Represents absolute coordinates on the console, with an optional annotation.</summary>
/// <remarks>
/// Console coordinates are zero-indexed from the top left corner, so the origin is at X=0, Y=0 and
/// increasing values of X represent positions toward the right, while increasing values of Y represent
/// positions toward the bottom.
/// </remarks>
public readonly struct Coordinates
{
    /// <summary>Instantiates a new set of coordinates with optional annotation.</summary>
    /// <param name="x">The X-coordinate, equivalent to <i>left</i> in <see cref="Console.SetCursorPosition"/></param>
    /// <param name="y">The Y-coordinate, equivalent to <i>top</i> in <see cref="Console.SetCursorPosition"/></param>
    /// <param name="annotation">An optional annotation, defaults to the empty string (<c>""</c>)</param>
    public Coordinates(int x, int y, string? annotation = null)
    {
        X = x;
        Y = y;
        Annotation = annotation ?? "";
    }

    /// <summary>The X-coordinate (equivalent to <i>left</i> in <see cref="Console.SetCursorPosition"/>)</summary>
    public int X { get; init; }
    /// <summary>The Y-coordinate (equivalent to <i>top</i> in <see cref="Console.SetCursorPosition"/>)</summary>
    public int Y { get; init; }
    /// <summary>The (optional) annotation of the coordinate. <c>""</c> (empty string) if no annotation was specified for the coordinate.</summary>
    public string Annotation { get; init; }

    public override string ToString()
    {
        if ( Annotation != "" )
            return $"({X}, {Y}; {Annotation})";
        return $"({X}, {Y})";
    }

    /// <summary>Returns a new set of <see cref="Coordinates"/> with X offset by <i>offset</i>.</summary>
    /// <param name="offset">How far to offset the X-coordinate</param>
    /// <returns>A new set of offset <see cref="Coordinates"/></returns>
    public Coordinates OffsetX(int offset)
    {
        return new Coordinates(X + offset, Y);
    }

    /// <summary>Returns a new set of <see cref="Coordinates"/> with Y offset by <i>offset</i>.</summary>
    /// <param name="offset">How far to offset the Y-coordinate</param>
    /// <returns>A new set of offset <see cref="Coordinates"/></returns>
    public Coordinates OffsetY(int offset)
    {
        return new Coordinates(X, Y + offset);
    }
}

/// <summary>Abstract representation of an area (box) of characters on the screen.</summary>
public readonly struct BoxDimensions
{
    /// <summary>Instantiates a new set of box dimensions.</summary>
    /// <param name="width">The width of the box</param>
    /// <param name="height">The height of the box</param>
    /// <param name="origin">The origin (top, left) of the box</param>
    public BoxDimensions(int width, int height, Coordinates origin)
    {
        Width = width;
        Height = height;
        Origin = origin;
    }

    /// <summary>The width of the box</summary>
    public int Width { get; init; }
    /// <summary>The height of the box</summary>
    public int Height { get; init; }
    /// <summary>The origin (top, left) of the box</summary>
    public Coordinates Origin { get; init; }

    /// <summary>The X-coordinate or the box's left-hand boundary</summary>
    public int XStart
    {
        get => Origin.X;
    }

    /// <summary>The X-coordinate of the box's right-hand boundary</summary>
    public int XEnd
    {
        get => Origin.X + Width - 1;
    }

    /// <summary>The Y-coordinate of the box's top boundary</summary>
    public int YStart
    {
        get => Origin.Y;
    }

    /// <summary>The Y-coordinate of the box's bottom boundary</summary>
    public int YEnd
    {
        get => Origin.Y + Height - 1;
    }

    /// <summary>The box's area (width × height)</summary>
    public int Area
    {
        get => Width * Height;
    }

    public override string ToString()
    {
        return $"({Width} × {Height}; origin: {Origin.X}, {Origin.Y})";
    }

    /// <summary>Calculates dimensions and coordinates for an inset box.</summary>
    /// <param name="offset">
    /// The offset by which the new box dimensions should be inset from the current box.
    /// This will typically be the character-width of any border plus any desired padding.
    /// </param>
    /// <returns>The dimensions and origin for an abstact inset box</returns>
    public BoxDimensions GetInnerDimensions(int offset = 1)
    {
        int width = Width - (2 * offset);
        int height = Height - (2 * offset);
        return new BoxDimensions(width, height, new Coordinates(Origin.X + offset, Origin.Y + offset));
    }

    /// <summary>Checks wheter a given point (coordinate) is contained within the box's boundaries.</summary>
    /// <param name="point">Coordinates of the point to check</param>
    /// <param name="excludeBorder">Whether to inset the allowable area by one character to compensate for a border</param>
    /// <returns>True if the <i>point</i> is contained within the box's bounds, False otherwise</returns>
    public bool ContainsPoint(Coordinates point, bool excludeBorder = true)
    {
        if (excludeBorder)
            return point.X > XStart && point.X < XEnd && point.Y > YStart && point.Y < YEnd;
        return point.X >= XStart && point.X <= XEnd && point.Y >= YStart && point.Y <= YEnd;
    }
}

/// <summary>Encapsulates a set of symbols (characters) for drawing rectangular boxes</summary>
public readonly struct BoxSymbols
{
    /// <summary>Instantiates a new set of box drawing symbols.</summary>
    /// <param name="topLeft">The symbol for the top left corner</param>
    /// <param name="topRight">The symbol for the top right corner</param>
    /// <param name="bottomLeft">The symbol for the bottom left corner</param>
    /// <param name="bottomRight">The symbol for the bottom right corner</param>
    /// <param name="vertical">The symbol for vertical lines</param>
    /// <param name="horizontal">The symbol for horizontal lines</param>
    /// <param name="fill">An optional fill character (default: <c>' '</c>)</param>
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

    /// <summary>The symbol for the top left corner</summary>
    public char TopLeft { get; init; }
    /// <summary>The symbol for the top right corner</summary>
    public char TopRight { get; init; }
    /// <summary>The symbol for the bottom left corner</summary>
    public char BottomLeft { get; init; }
    /// <summary>The symbol for the bottom right corner</summary>
    public char BottomRight { get; init; }
    /// <summary>The symbol for vertical lines</summary>
    public char Vertical { get; init; }
    /// <summary>The symbol for horizontal lines</summary>
    public char Horizontal { get; init; }
    /// <summary>An appropriate fill character for 'empty' areas</summary>
    public char Fill { get; init; }

    /// <summary>Convert the set of box symbols to an array.</summary>
    /// <remarks>
    /// The returned array will always have the order
    /// [TopLeft, TopRight, BottomLeft, BottomRight, Vertical, Horizontal, Fill].
    /// </remarks>
    /// <returns>An array of box symbols in a consistent order</returns>
    public char[] ToArray()
    {
        return [TopLeft, TopRight, BottomLeft, BottomRight, Vertical, Horizontal, Fill];
    }

    /// <summary>Transliterates a character from one set of box symbols to another.</summary>
    /// <param name="symbol">The symbol/character to transliterate to the new set of box symbols</param>
    /// <param name="fromSet">The source set from which <i>symbol</i> is taken</param>
    /// <param name="toSet">The target set to which the symbol should be transliterated</param>
    /// <returns>
    /// The transliterated symbol, or the input character if the symbol is not in <i>fromSet</i>/<i>toSet</i>
    /// </returns>
    public static char Transliterate(char symbol, BoxSymbols fromSet, BoxSymbols toSet)
    {
        int index = Array.FindIndex(fromSet.ToArray(), c => c == symbol);
        if (index >= 0)
            return toSet.ToArray()[index];
        return symbol;
    }

    private static readonly BoxSymbols s_squareSingle = new('┌', '┐', '└', '┘', '│', '─');
    /// <summary>Predefined set of square-corner, single line box drawing characters</summary>
    /// <remarks><example>Example:<code>
    ///     ┌──┐
    ///     │  │
    ///     └──┘
    /// </code></example></remarks>
    public static BoxSymbols SquareSingle { get => s_squareSingle; }
    private static readonly BoxSymbols s_squareDouble = new('╔', '╗', '╚', '╝', '║', '═');
    /// <summary>Predefined set of square-corner, double line box drawing characters</summary>
    /// <remarks><example>Example:<code>
    ///     ╔══╗
    ///     ║  ║
    ///     ╚══╝
    /// </code></example></remarks>public static BoxSymbols SquareDouble { get => s_squareDouble; }
    public static BoxSymbols SquareDouble { get => s_squareDouble; }
    private static readonly BoxSymbols s_roundSingle = new('╭', '╮', '╰', '╯', '│', '─');
    /// <summary>Predefined set of round-corner, single line box drawing characters</summary>
    /// <remarks><example>Example:<code>
    ///     ╭──╮
    ///     │  │
    ///     ╰──╯
    /// </code></example></remarks>
    public static BoxSymbols RoundSingle { get => s_roundSingle; }
}
