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
		if (proceed) {
			_forceClose = true;
			Close();
		}
	}
}
