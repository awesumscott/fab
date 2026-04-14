using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditTextFieldViewModel : ObservableObject, IEditableField {
	private readonly object _model;
	private readonly PropertyInfo _property;

	public string Name => _property.Name;

	public string Text {
		get => _property.GetValue(_model)?.ToString() ?? string.Empty;
		set {
			OnPropertyChanging();
			_property.SetValue(_model, value);
			OnPropertyChanged();
		}
	}

	public EditTextFieldViewModel(object model, PropertyInfo property) {
		_model = model;
		_property = property;
	}
}
