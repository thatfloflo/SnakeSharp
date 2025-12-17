using System.CommandLine;
using System.Resources;

namespace Snake;

/// <summary>Encapsulates the main program logic.</summary>
public static class Program
{
    /// <summary>The program's entry point, which handles command line arguments and launches the game.</summary>
    /// <param name="args">The command line arguments received by the program.</param>
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

    /// <summary>Launches a new <see cref="Game"/> for as long as a player chooses to play (and play again).</summary>
    /// <param name="difficulty">
    /// The difficulty level with which the game(s) should run.
    /// See <see cref="Game.Game"/> for a description of what the various levels do.
    /// </param>
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

    /// <summary>Static reference to the program's bundled string resources.</summary>
    public static ResourceManager Resources = new("Snake.Strings", typeof(Program).Assembly);
}
