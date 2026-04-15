namespace Fab.Editor.Core.Services;

public interface IConfirmationService {
	Task<bool> ConfirmAsync(string message, string title = "Confirm");
}
