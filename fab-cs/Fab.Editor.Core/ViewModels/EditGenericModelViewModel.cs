using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;
using Fab.Editor.Core.Services;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditGenericModelViewModel : ObservableObject, IEditableField {
	public object Model { get; }
	public string TypeName => Model.GetType().Name;

	[ObservableProperty] private List<IEditableField> _fields = [];

	public EditGenericModelViewModel(object model, IConfirmationService? confirmation = null, Action? onChanged = null) {
		Model = model;
		Fields = BuildFields(model, confirmation, onChanged);
	}

	public static List<IEditableField> BuildFields(object model, IConfirmationService? confirmation = null, Action? onChanged = null) {
		var fields = new List<IEditableField>();
		foreach (var property in model.GetType().GetProperties()) {
			var type = property.PropertyType;
			if (type == typeof(string)) {
				fields.Add(new EditTextFieldViewModel(model, property, onChanged));
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
				fields.Add(new EditListViewModel(model, property, confirmation, onChanged));
			}
			else if (typeof(IUnique).IsAssignableFrom(type)) {
				var nested = property.GetValue(model);
				if (nested is not null)
					fields.Add(new EditGenericModelViewModel(nested, confirmation, onChanged));
			}
		}
		return fields;
	}
}
