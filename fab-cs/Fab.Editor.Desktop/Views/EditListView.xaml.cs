using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fab.Editor.Core.ViewModels;

namespace Fab.Editor.Desktop.Views;

public partial class EditListView : UserControl {
	private Point _dragStart;
	private EditGenericModelViewModel? _dragItem;

	public EditListView() {
		InitializeComponent();
	}

	private void Handle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
		if (sender is FrameworkElement fe && fe.DataContext is EditGenericModelViewModel item) {
			_dragStart = e.GetPosition(null);
			_dragItem = item;
		}
	}

	private void Handle_PreviewMouseMove(object sender, MouseEventArgs e) {
		if (_dragItem is null || e.LeftButton != MouseButtonState.Pressed) return;
		var pos = e.GetPosition(null);
		if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
			Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
			return;

		var source = _dragItem;
		_dragItem = null;
		DragDrop.DoDragDrop((DependencyObject)sender, source, DragDropEffects.Move);
	}

	private void ItemRoot_DragOver(object sender, DragEventArgs e) {
		e.Effects = e.Data.GetDataPresent(typeof(EditGenericModelViewModel))
			? DragDropEffects.Move
			: DragDropEffects.None;
		e.Handled = true;
	}

	private void ItemRoot_Drop(object sender, DragEventArgs e) {
		if (e.Data.GetData(typeof(EditGenericModelViewModel)) is not EditGenericModelViewModel source) return;
		if (sender is not FrameworkElement fe || fe.DataContext is not EditGenericModelViewModel target) return;
		if (DataContext is not EditListViewModel listVm) return;
		listVm.MoveBefore(source, target);
		e.Handled = true;
	}
}
