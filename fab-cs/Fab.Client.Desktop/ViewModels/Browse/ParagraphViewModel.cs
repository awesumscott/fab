using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Client.Desktop.ViewModels.Browse;

partial class ParagraphViewModel : ObservableObject, IFabRenderable {
	[ObservableProperty] private string _heading;
	[ObservableProperty] private string _body;
	private readonly TextSection _paragraph;

	public ParagraphViewModel(TextSection p) {
		_paragraph = p;
		Heading = p.Heading;
		Body = p.Body;
	}
}
