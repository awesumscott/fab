using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Client.Core.ViewModels;

public partial class LinkViewModel : ObservableObject, IFabRenderable {
	[ObservableProperty] private string _text = string.Empty;
	[ObservableProperty] private string _url = string.Empty;
	[ObservableProperty] private bool _isExternal;

	public LinkViewModel(Link link) {
		Text = link.Text;
		Url = link.Url;
		IsExternal = link.IsExternal;
	}
}
