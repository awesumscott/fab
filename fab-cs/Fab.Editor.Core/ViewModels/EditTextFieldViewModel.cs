using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditTextFieldViewModel : ObservableObject, IEditableField {
	private readonly object _model;
	private readonly PropertyInfo _property;
	private readonly Action? _onChanged;

	public string Name => _property.Name;

	public string Text {
		get => _property.GetValue(_model)?.ToString() ?? string.Empty;
		set {
			OnPropertyChanging();
			_property.SetValue(_model, value);
			OnPropertyChanged();
			_onChanged?.Invoke();
		}
	}

	public EditTextFieldViewModel(object model, PropertyInfo property, Action? onChanged = null) {
		_model = model;
		_property = property;
		_onChanged = onChanged;
	}
}
