using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Client.Desktop.ViewModels.Edit;

internal sealed partial class EditGenericModelViewModel : ObservableObject, IEditableField {
	//private object _model;

	[ObservableProperty] private List<IEditableField> _fields = [];

	public EditGenericModelViewModel(object model) {
		//_model = model;

		//var properties = model.GetType().GetProperties();
		//foreach (var property in properties) {
		//	var pType = property.PropertyType;
		//	if (pType == typeof(string)) {
		//		_fields.Add(new EditTextFieldViewModel(model, property));
		//	} else if (pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(List<>)) {
		//		_fields.Add(new EditListViewModel(model, property, pType.GetGenericArguments()[0]));
		//	}
		//}
		IdkTheNameYetProperties(model, ref _fields);

		//Type type = pi.PropertyType;
		//if (type.IsGenericType && type.GetGenericTypeDefinition()
		//		== typeof(List<>)) {
		//	Type itemType = type.GetGenericArguments()[0]; // use this...
		//}


	}

	internal static void IdkTheNameYetProperties(object model, ref List<IEditableField> fields) {
		var properties = model.GetType().GetProperties();
		foreach (var property in properties) {
			var pType = property.PropertyType;
			if (pType == typeof(string)) {
				fields.Add(new EditTextFieldViewModel(model, property));
			} else if (pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(List<>)) {
				fields.Add(new EditListViewModel(model, property, pType.GetGenericArguments()[0]));
			} else if (typeof(IUnique).IsAssignableFrom(pType)) {
				fields.Add(new EditGenericModelViewModel(property.GetValue(model)));
			}
		}
	}
	internal static EditGenericModelViewModel? IdkTheNameYet_Class(object model) {
		var pType = model.GetType();
		if (typeof(IUnique).IsAssignableFrom(pType)) {
			return new EditGenericModelViewModel(model);
		}
		return null;
	}
}
