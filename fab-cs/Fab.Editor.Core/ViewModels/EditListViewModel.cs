using System.Collections;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fab.Data;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditListViewModel : ObservableObject, IEditableField {
	public sealed record AddOption(string Label, Func<object> Create);

	private readonly object _model;
	private readonly PropertyInfo _property;
	private readonly Type _itemType;

	public string Name => _property.Name;

	[ObservableProperty] private List<IEditableField> _items = [];
	public IReadOnlyList<AddOption> AddOptions { get; }

	public EditListViewModel(object model, PropertyInfo property) {
		_model = model;
		_property = property;
		_itemType = property.PropertyType.GetGenericArguments()[0];
		AddOptions = BuildAddOptions(_itemType);
		RebuildItems();
	}

	[RelayCommand]
	private void Add(AddOption? option) {
		if (option is null) return;
		if (_property.GetValue(_model) is not IList list) return;

		var item = option.Create();
		TrySetOrder(item, list.Count);
		list.Add(item);
		RebuildItems();
	}

	private void RebuildItems() {
		if (_property.GetValue(_model) is not IEnumerable list) return;
		Items = list.OfType<object>()
			.Select(item => (IEditableField)new EditGenericModelViewModel(item))
			.ToList();
	}

	internal static IReadOnlyList<AddOption> BuildAddOptions(Type itemType) {
		// Pattern: a concrete "wrapper" item (e.g. OrderedContentEntry) with
		// exactly one abstract IUnique property (e.g. Content : ContentBase)
		// — surface one option per concrete subclass of that abstract type.
		var abstractChildProp = itemType.GetProperties()
			.Where(p => p.PropertyType.IsAbstract && typeof(IUnique).IsAssignableFrom(p.PropertyType))
			.SingleOrDefault();

		if (abstractChildProp is not null && HasParameterlessCtor(itemType)) {
			var subclasses = abstractChildProp.PropertyType.Assembly.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract
					&& abstractChildProp.PropertyType.IsAssignableFrom(t)
					&& HasParameterlessCtor(t))
				.OrderBy(t => t.Name)
				.ToList();
			return subclasses
				.Select(sub => new AddOption(sub.Name, () => {
					var wrapper = Activator.CreateInstance(itemType)!;
					abstractChildProp.SetValue(wrapper, Activator.CreateInstance(sub)!);
					return wrapper;
				}))
				.ToList();
		}

		if (!itemType.IsAbstract && HasParameterlessCtor(itemType))
			return [new AddOption(itemType.Name, () => Activator.CreateInstance(itemType)!)];

		return [];
	}

	private static bool HasParameterlessCtor(Type type) =>
		type.GetConstructor(Type.EmptyTypes) is not null;

	private static void TrySetOrder(object item, int order) {
		var orderProp = item.GetType().GetProperty("Order");
		if (orderProp?.PropertyType == typeof(int) && orderProp.CanWrite)
			orderProp.SetValue(item, order);
	}
}
