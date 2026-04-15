using System.ComponentModel;
using System.Windows;
using Fab.Editor.Core.ViewModels;

namespace Fab.Editor.Desktop;

public partial class MainWindow : Window {
	private bool _forceClose;

	public MainWindow() {
		InitializeComponent();
	}

	protected override async void OnClosing(CancelEventArgs e) {
		if (_forceClose) { base.OnClosing(e); return; }
		if (DataContext is not EditorMainWindowViewModel vm) { base.OnClosing(e); return; }

		e.Cancel = true;
		var proceed = await vm.ConfirmCloseAsync();
		if (!proceed) return;

		_forceClose = true;
		// Defer to the next dispatcher tick: calling Close() directly here
		// races the in-flight Closing cycle and throws
		// "Cannot ... call Close ... while a Window is closing".
		await Dispatcher.InvokeAsync(Close, System.Windows.Threading.DispatcherPriority.Background);
	}
}
