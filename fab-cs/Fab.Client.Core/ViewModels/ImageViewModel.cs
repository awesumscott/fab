using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Client.Core.ViewModels;

public partial class ImageViewModel : ObservableObject, IFabRenderable {
	[ObservableProperty] private string _url = string.Empty;
	[ObservableProperty] private string? _alt;

	public ImageViewModel(Image img) {
		Url = img.Url;
		Alt = string.IsNullOrEmpty(img.Alt) ? null : img.Alt;
	}
}
