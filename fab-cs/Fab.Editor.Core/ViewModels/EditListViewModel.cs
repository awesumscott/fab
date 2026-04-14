using System.Collections;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditListViewModel : ObservableObject, IEditableField {
	private readonly object _model;
	private readonly PropertyInfo _property;

	public string Name => _property.Name;

	[ObservableProperty] private List<IEditableField> _items = [];

	public EditListViewModel(object model, PropertyInfo property) {
		_model = model;
		_property = property;
		RebuildItems();
	}

	private void RebuildItems() {
		if (_property.GetValue(_model) is not IEnumerable list) return;
		Items = list.OfType<object>()
			.Select(item => (IEditableField)new EditGenericModelViewModel(item))
			.ToList();
	}
}
