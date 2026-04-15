using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fab.Editor.Core.ViewModels;

namespace Fab.Editor.Desktop.Views;

public partial class EditListView : UserControl {
	private Point _dragStart;
	private EditGenericModelViewModel? _dragItem;
	private FrameworkElement? _dragSourceVisual;

	public EditListView() {
		InitializeComponent();
	}

	private void Handle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
		if (sender is FrameworkElement fe && fe.DataContext is EditGenericModelViewModel item) {
			_dragStart = e.GetPosition(null);
			_dragItem = item;
			_dragSourceVisual = FindItemRoot(fe);
		}
	}

	private void Handle_PreviewMouseMove(object sender, MouseEventArgs e) {
		if (_dragItem is null || e.LeftButton != MouseButtonState.Pressed) return;
		var pos = e.GetPosition(null);
		if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
			Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
			return;

		var source = _dragItem;
		var visual = _dragSourceVisual;
		_dragItem = null;
		_dragSourceVisual = null;

		if (visual is not null) visual.Opacity = 0.5;
		try {
			DragDrop.DoDragDrop((DependencyObject)sender, source, DragDropEffects.Move);
		}
		finally {
			if (visual is not null) visual.Opacity = 1.0;
		}
	}

	private void ItemRoot_DragEnter(object sender, DragEventArgs e) {
		if (!IsValidDrag(sender, e, out var source, out var target)) return;
		if (!ReferenceEquals(source, target))
			SetIndicator(sender, Visibility.Visible);
	}

	private void ItemRoot_DragOver(object sender, DragEventArgs e) {
		var present = e.Data.GetDataPresent(typeof(EditGenericModelViewModel));
		e.Effects = present ? DragDropEffects.Move : DragDropEffects.None;
		e.Handled = true;
	}

	private void ItemRoot_DragLeave(object sender, DragEventArgs e) {
		SetIndicator(sender, Visibility.Collapsed);
	}

	private void ItemRoot_Drop(object sender, DragEventArgs e) {
		SetIndicator(sender, Visibility.Collapsed);
		if (!IsValidDrag(sender, e, out var source, out var target)) return;
		if (DataContext is not EditListViewModel listVm) return;
		listVm.MoveBefore(source, target);
		e.Handled = true;
	}

	private void EndDropZone_DragEnter(object sender, DragEventArgs e) {
		if (sender is Border b && e.Data.GetDataPresent(typeof(EditGenericModelViewModel)))
			b.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
	}

	private void EndDropZone_DragOver(object sender, DragEventArgs e) {
		e.Effects = e.Data.GetDataPresent(typeof(EditGenericModelViewModel))
			? DragDropEffects.Move
			: DragDropEffects.None;
		e.Handled = true;
	}

	private void EndDropZone_DragLeave(object sender, DragEventArgs e) {
		if (sender is Border b) b.BorderBrush = System.Windows.Media.Brushes.Transparent;
	}

	private void EndDropZone_Drop(object sender, DragEventArgs e) {
		if (sender is Border b) b.BorderBrush = System.Windows.Media.Brushes.Transparent;
		if (e.Data.GetData(typeof(EditGenericModelViewModel)) is not EditGenericModelViewModel source) return;
		if (DataContext is not EditListViewModel listVm) return;
		listVm.MoveToEnd(source);
		e.Handled = true;
	}

	private static bool IsValidDrag(object sender, DragEventArgs e, out EditGenericModelViewModel source, out EditGenericModelViewModel target) {
		source = null!;
		target = null!;
		if (e.Data.GetData(typeof(EditGenericModelViewModel)) is not EditGenericModelViewModel s) return false;
		if (sender is not FrameworkElement fe || fe.DataContext is not EditGenericModelViewModel t) return false;
		source = s;
		target = t;
		return true;
	}

	private static void SetIndicator(object sender, Visibility visibility) {
		if (sender is not DockPanel dp) return;
		if (dp.Parent is not StackPanel sp || sp.Children.Count == 0) return;
		if (sp.Children[0] is Border indicator) indicator.Visibility = visibility;
	}

	private static FrameworkElement? FindItemRoot(DependencyObject start) {
		DependencyObject? current = start;
		while (current is not null) {
			if (current is DockPanel dp && dp.Name == "ItemRoot") return dp;
			current = System.Windows.Media.VisualTreeHelper.GetParent(current);
		}
		return null;
	}
}
