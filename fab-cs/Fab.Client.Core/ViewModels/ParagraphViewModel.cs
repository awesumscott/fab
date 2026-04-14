using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Client.Core.ViewModels;

public partial class ParagraphViewModel : ObservableObject, IFabRenderable {
	[ObservableProperty] private string _text = string.Empty;

	public ParagraphViewModel(Paragraph p) {
		Text = p.Text;
	}
}
