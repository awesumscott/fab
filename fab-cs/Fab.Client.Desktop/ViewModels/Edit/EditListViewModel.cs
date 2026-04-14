using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections;
using System.Reflection;

namespace Fab.Client.Desktop.ViewModels.Edit;

internal partial class EditListViewModel : ObservableObject, IEditableField {
	private readonly object _model;
	private readonly PropertyInfo _propertyInfo;
	private readonly MethodInfo? _getMethod;
	private readonly MethodInfo? _setMethod;

	//[ObservableProperty] private List<IEditableField> _fields = [];

	//[ObservableProperty] private string _text;
	//partial void OnTextChanged(string value) => _setMethod?.Invoke(_model, [value]);
	private IList _list;
	public IList List {
		get => _list;//_getMethod?.Invoke(_model, null) as IList ?? new List<object>();
		set {
			OnPropertyChanging();
			_setMethod?.Invoke(_model, [value]);
			OnPropertyChanged();
		}
	}

	public EditListViewModel(object model, PropertyInfo propertyInfo, Type listType) {
		_model = model;
		_propertyInfo = propertyInfo;
		_getMethod = propertyInfo.GetMethod;
		_setMethod = propertyInfo.SetMethod;
		//List = _getMethod?.Invoke(model, null) as List<OrderedContentEntry> ?? [];


		var list = _getMethod?.Invoke(model, null) as IEnumerable;
		var l2 = list.OfType<object>();
		_list = l2.Select(EditGenericModelViewModel.IdkTheNameYet_Class).ToList();



		//var outerType = list.GetType();
		//var t = outerType.MakeGenericType(listType);
		//var x = Convert.ChangeType(list, t);


		//this for each item in list, in some EditClassViewModel
		//EditGenericModelViewModel.IdkTheNameYet(model, ref _fields);
	}
}
