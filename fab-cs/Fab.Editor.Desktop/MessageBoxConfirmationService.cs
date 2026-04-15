using System.Windows;
using Fab.Editor.Core.Services;

namespace Fab.Editor.Desktop;

public sealed class MessageBoxConfirmationService : IConfirmationService {
	public Task<bool> ConfirmAsync(string message, string title = "Confirm") {
		var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Question);
		return Task.FromResult(result == MessageBoxResult.OK);
	}
}
