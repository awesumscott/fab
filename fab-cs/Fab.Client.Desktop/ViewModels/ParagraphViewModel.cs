using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Client.Desktop.ViewModels;

partial class ParagraphViewModel : ObservableObject, IFabRenderable {
	[ObservableProperty] private string _text;
	private Paragraph _paragraph;

	public ParagraphViewModel(Paragraph p) {
		_paragraph = p;
		Text = p.Text;
	}
}
