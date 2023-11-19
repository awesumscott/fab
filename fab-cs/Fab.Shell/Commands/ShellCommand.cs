namespace Fab.Shell.Commands;

public abstract class ShellCommand {
	public static string Name { get; protected set; }
	public virtual Task Execute(CancellationToken cancellationToken) => null;
}
