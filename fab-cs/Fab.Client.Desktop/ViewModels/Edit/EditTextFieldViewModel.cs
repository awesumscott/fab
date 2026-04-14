using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;

namespace Fab.Client.Desktop.ViewModels.Edit;

internal partial class EditTextFieldViewModel : ObservableObject, IEditableField {
	private readonly object _model;
	//private readonly PropertyInfo _propertyInfo;
	private readonly MethodInfo? _getMethod;
	private readonly MethodInfo? _setMethod;

	//[ObservableProperty] private string _text;
	//partial void OnTextChanged(string value) => _setMethod?.Invoke(_model, [value]);
	public string Text {
		get => _getMethod?.Invoke(_model, null)?.ToString() ?? string.Empty;
		set {
			OnPropertyChanging();
			_setMethod?.Invoke(_model, [value]);
			OnPropertyChanged();
		}
	}

	public EditTextFieldViewModel(object model, PropertyInfo propertyInfo) {
		_model = model;
		//_propertyInfo = propertyInfo;
		_getMethod = propertyInfo.GetMethod;
		_setMethod = propertyInfo.SetMethod;
		//Text = _getMethod?.Invoke(model, null)?.ToString() ?? string.Empty;
	}
}
