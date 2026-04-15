using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;

namespace Fab.Client.Core.ViewModels;

public partial class OrderedContentEntryViewModel : ObservableObject, IFabRenderable {
	[ObservableProperty] private IFabRenderable? _content;
	public int Order { get; }

	public OrderedContentEntryViewModel(OrderedContentEntry entry) {
		Order = entry.Order;
		Content = entry.Content switch {
			Paragraph p => new ParagraphViewModel(p),
			Image i => new ImageViewModel(i),
			Heading h => new HeadingViewModel(h),
			Link l => new LinkViewModel(l),
			Divider d => new DividerViewModel(d),
			null => null,
			_ => LogAndNull(entry.Content),
		};
	}

	private static IFabRenderable? LogAndNull(ContentBase content) {
		Debug.WriteLine($"Unsupported content type: {content.GetType()}");
		return null;
	}
}
