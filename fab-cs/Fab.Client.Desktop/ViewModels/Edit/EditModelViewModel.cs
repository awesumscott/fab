using CommunityToolkit.Mvvm.ComponentModel;

namespace Fab.Client.Desktop.ViewModels.Edit;

internal sealed partial class EditModelViewModel : ObservableObject, IEditableField {
	[ObservableProperty] private List<IEditableField> _fields = [];

	public EditModelViewModel(object model) {
		EditGenericModelViewModel.IdkTheNameYetProperties(model, ref _fields);
	}
}
