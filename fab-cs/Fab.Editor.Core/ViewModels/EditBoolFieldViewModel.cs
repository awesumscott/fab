using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Editor.Core.Services;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditBoolFieldViewModel : ObservableObject, IEditableField {
	private readonly object _model;
	private readonly PropertyInfo _property;
	private readonly Action? _onChanged;
	private readonly UndoService? _undo;

	public string Name => _property.Name;

	public bool Value {
		get => _property.GetValue(_model) is true;
		set {
			var oldValue = Value;
			if (oldValue == value) return;
			OnPropertyChanging();
			_property.SetValue(_model, value);
			OnPropertyChanged();
			_undo?.Push(new UndoAction(
				Undo: () => {
					_property.SetValue(_model, oldValue);
					OnPropertyChanged(nameof(Value));
					_onChanged?.Invoke();
				},
				Redo: () => {
					_property.SetValue(_model, value);
					OnPropertyChanged(nameof(Value));
					_onChanged?.Invoke();
				},
				Description: $"Toggle {Name}",
				CoalesceKey: null));
			_onChanged?.Invoke();
		}
	}

	public EditBoolFieldViewModel(object model, PropertyInfo property, Action? onChanged = null, UndoService? undo = null) {
		_model = model;
		_property = property;
		_onChanged = onChanged;
		_undo = undo;
	}
}
