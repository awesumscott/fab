namespace Fab.Editor.Core.Services;

public enum UnsavedChangesResult {
	Save,
	Discard,
	Cancel,
}

public interface IConfirmationService {
	Task<bool> ConfirmAsync(string message, string title = "Confirm");
	Task<UnsavedChangesResult> PromptUnsavedAsync(string message, string title = "Unsaved changes");
}
