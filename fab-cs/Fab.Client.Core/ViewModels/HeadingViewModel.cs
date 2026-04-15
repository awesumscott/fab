using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Client.Core.ViewModels;

public partial class HeadingViewModel : ObservableObject, IFabRenderable {
	[ObservableProperty] private int _level;
	[ObservableProperty] private string _text = string.Empty;

	public double FontSize => Level switch {
		1 => 28,
		2 => 22,
		3 => 18,
		4 => 16,
		_ => 14,
	};

	public HeadingViewModel(Heading h) {
		Level = h.Level;
		Text = h.Text;
	}
}
