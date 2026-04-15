using System.Collections;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fab.Data;
using Fab.Editor.Core.Services;

namespace Fab.Editor.Core.ViewModels;

public sealed partial class EditListViewModel : ObservableObject, IEditableField {
	public sealed record AddOption(string Label, Func<object> Create);

	private readonly object _model;
	private readonly PropertyInfo _property;
	private readonly Type _itemType;
	private readonly IConfirmationService? _confirmation;
	private readonly Action? _onChanged;
	private readonly UndoService? _undo;

	public string Name => _property.Name;

	[ObservableProperty] private List<IEditableField> _items = [];
	public IReadOnlyList<AddOption> AddOptions { get; }

	public EditListViewModel(object model, PropertyInfo property, IConfirmationService? confirmation = null, Action? onChanged = null, UndoService? undo = null) {
		_model = model;
		_property = property;
		_itemType = property.PropertyType.GetGenericArguments()[0];
		_confirmation = confirmation;
		_onChanged = onChanged;
		_undo = undo;
		AddOptions = BuildAddOptions(_itemType);
		RebuildItems();
	}

	[RelayCommand]
	private void Add(AddOption? option) {
		if (option is null) return;
		if (_property.GetValue(_model) is not IList list) return;

		var item = option.Create();
		list.Add(item);
		RenumberOrder(list);
		RebuildItems();
		_undo?.Push(new UndoAction(
			Undo: () => {
				list.Remove(item);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Redo: () => {
				list.Add(item);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Description: $"Add {_itemType.Name}"));
		_onChanged?.Invoke();
	}

	[RelayCommand]
	private async Task DeleteAsync(EditGenericModelViewModel? item) {
		if (item is null) return;
		if (_confirmation is not null) {
			var ok = await _confirmation.ConfirmAsync($"Delete this {item.TypeName}?\n\n\"{item.Summary}\"");
			if (!ok) return;
		}
		if (_property.GetValue(_model) is not IList list) return;
		var originalIndex = list.IndexOf(item.Model);
		var removed = item.Model;
		list.Remove(removed);
		RenumberOrder(list);
		RebuildItems();
		_undo?.Push(new UndoAction(
			Undo: () => {
				list.Insert(originalIndex, removed);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Redo: () => {
				list.Remove(removed);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Description: $"Delete {item.TypeName}"));
		_onChanged?.Invoke();
	}

	[RelayCommand]
	private void MoveUp(EditGenericModelViewModel? item) {
		if (item is null) return;
		if (_property.GetValue(_model) is not IList list) return;
		var idx = list.IndexOf(item.Model);
		if (idx <= 0) return;
		Swap(list, idx - 1, idx);
		RenumberOrder(list);
		RebuildItems();
		_undo?.Push(new UndoAction(
			Undo: () => {
				Swap(list, idx - 1, idx);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Redo: () => {
				Swap(list, idx - 1, idx);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Description: "Move up"));
		_onChanged?.Invoke();
	}

	[RelayCommand]
	private void MoveDown(EditGenericModelViewModel? item) {
		if (item is null) return;
		if (_property.GetValue(_model) is not IList list) return;
		var idx = list.IndexOf(item.Model);
		if (idx < 0 || idx >= list.Count - 1) return;
		Swap(list, idx, idx + 1);
		RenumberOrder(list);
		RebuildItems();
		_undo?.Push(new UndoAction(
			Undo: () => {
				Swap(list, idx, idx + 1);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Redo: () => {
				Swap(list, idx, idx + 1);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Description: "Move down"));
		_onChanged?.Invoke();
	}

	public void MoveToEnd(EditGenericModelViewModel source) {
		if (_property.GetValue(_model) is not IList list) return;
		var srcIdx = list.IndexOf(source.Model);
		if (srcIdx < 0 || srcIdx == list.Count - 1) return;
		var item = list[srcIdx]!;
		list.RemoveAt(srcIdx);
		list.Add(item);
		RenumberOrder(list);
		RebuildItems();
		_undo?.Push(new UndoAction(
			Undo: () => {
				list.Remove(item);
				list.Insert(srcIdx, item);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Redo: () => {
				list.Remove(item);
				list.Add(item);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Description: "Move to end"));
		_onChanged?.Invoke();
	}

	public void MoveBefore(EditGenericModelViewModel source, EditGenericModelViewModel target) {
		if (_property.GetValue(_model) is not IList list) return;
		var srcIdx = list.IndexOf(source.Model);
		var tgtIdx = list.IndexOf(target.Model);
		if (srcIdx < 0 || tgtIdx < 0 || srcIdx == tgtIdx) return;
		var item = list[srcIdx]!;
		list.RemoveAt(srcIdx);
		var insertIdx = srcIdx < tgtIdx ? tgtIdx - 1 : tgtIdx;
		list.Insert(insertIdx, item);
		RenumberOrder(list);
		RebuildItems();
		_undo?.Push(new UndoAction(
			Undo: () => {
				list.Remove(item);
				list.Insert(srcIdx, item);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Redo: () => {
				list.Remove(item);
				list.Insert(insertIdx, item);
				RenumberOrder(list);
				RebuildItems();
				_onChanged?.Invoke();
			},
			Description: "Reorder"));
		_onChanged?.Invoke();
	}

	private void RebuildItems() {
		if (_property.GetValue(_model) is not IEnumerable list) return;
		Items = list.OfType<object>()
			.Select(item => (IEditableField)new EditGenericModelViewModel(item, _confirmation, _onChanged, _undo))
			.ToList();
	}

	internal static IReadOnlyList<AddOption> BuildAddOptions(Type itemType) {
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

	private static void RenumberOrder(IList list) {
		for (int i = 0; i < list.Count; i++) {
			var item = list[i];
			if (item is null) continue;
			var orderProp = item.GetType().GetProperty("Order");
			if (orderProp?.PropertyType == typeof(int) && orderProp.CanWrite)
				orderProp.SetValue(item, i);
		}
	}

	private static void Swap(IList list, int a, int b) =>
		(list[a], list[b]) = (list[b], list[a]);
}
