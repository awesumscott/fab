namespace Fab.Shell.Commands;

public sealed class Menu : ShellCommand {
	private Dictionary<string, Func<ShellCommand>> _commands;

	public Menu(string name, Dictionary<string, Func<ShellCommand>> commands) {
		Name = name;
		_commands = commands;
	}

	public override async Task Execute(CancellationToken cancellationToken) {
		while (true) {
			Console.Clear();
			Console.WriteLine($"{Name}\n");
			var commandList = _commands.ToList();
			for (var i = 0; i < commandList.Count; i++) {
				Console.WriteLine($"{i}.\t{commandList[i].Key}");
			}
			Console.WriteLine($"Esc.\tExit\n");

			while (true) {
				var key = Console.ReadKey(true);
				if (key.Key == ConsoleKey.Escape) return; ;

				var val = (int)char.GetNumericValue(key.KeyChar);
				if (val >= 0 && val < commandList.Count) {
					try {
						await commandList[val].Value.Invoke().Execute(cancellationToken);
					} catch (Exception e) {
						Console.WriteLine(e.Message);
						Console.WriteLine(e.StackTrace);
						Program.Pause();
					}
					break;
				}
			}
		}
	}
}
