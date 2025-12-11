using System.CommandLine;
using System.Resources;

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
