using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;
using Fab.Editor.Core.Services;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditGenericModelViewModel : ObservableObject, IEditableField {
	public object Model { get; }
	public string TypeName => Model.GetType().Name;

	/// <summary>
	/// Short human-readable description of this model for confirmation
	/// dialogs and the like. Picks the first non-empty string property;
	/// falls back to just the type name.
	/// </summary>
	public string Summary {
		get {
			foreach (var prop in Model.GetType().GetProperties()) {
				if (prop.PropertyType != typeof(string)) continue;
				if (prop.GetValue(Model) is string s && !string.IsNullOrWhiteSpace(s))
					return s.Length > 50 ? s[..50] + "..." : s;
			}
			return TypeName;
		}
	}

	[ObservableProperty] private List<IEditableField> _fields = [];

	public EditGenericModelViewModel(object model, IConfirmationService? confirmation = null, Action? onChanged = null, UndoService? undo = null) {
		Model = model;
		Fields = BuildFields(model, confirmation, onChanged, undo);
	}

	public static List<IEditableField> BuildFields(object model, IConfirmationService? confirmation = null, Action? onChanged = null, UndoService? undo = null) {
		var fields = new List<IEditableField>();
		foreach (var property in model.GetType().GetProperties()) {
			var type = property.PropertyType;
			if (type == typeof(string)) {
				fields.Add(new EditTextFieldViewModel(model, property, onChanged, undo));
			}
			else if (type == typeof(int)) {
				fields.Add(new EditIntFieldViewModel(model, property, onChanged, undo));
			}
			else if (type == typeof(bool)) {
				fields.Add(new EditBoolFieldViewModel(model, property, onChanged, undo));
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
				fields.Add(new EditListViewModel(model, property, confirmation, onChanged, undo));
			}
			else if (typeof(IUnique).IsAssignableFrom(type)) {
				var nested = property.GetValue(model);
				if (nested is not null)
					fields.Add(new EditGenericModelViewModel(nested, confirmation, onChanged, undo));
			}
		}
		return fields;
	}
}
