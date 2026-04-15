using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Editor.Core.Services;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditTextFieldViewModel : ObservableObject, IEditableField {
	private readonly object _model;
	private readonly PropertyInfo _property;
	private readonly Action? _onChanged;
	private readonly UndoService? _undo;

	public string Name => _property.Name;

	public string Text {
		get => _property.GetValue(_model)?.ToString() ?? string.Empty;
		set {
			var oldValue = Text;
			if (oldValue == value) return;
			OnPropertyChanging();
			_property.SetValue(_model, value);
			OnPropertyChanged();
			_undo?.Push(new UndoAction(
				Undo: () => {
					_property.SetValue(_model, oldValue);
					OnPropertyChanged(nameof(Text));
					_onChanged?.Invoke();
				},
				Description: $"Edit {Name}",
				CoalesceKey: $"text:{_model.GetHashCode()}:{_property.Name}"));
			_onChanged?.Invoke();
		}
	}

	public EditTextFieldViewModel(object model, PropertyInfo property, Action? onChanged = null, UndoService? undo = null) {
		_model = model;
		_property = property;
		_onChanged = onChanged;
		_undo = undo;
	}
}
