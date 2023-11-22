using CommunityToolkit.Mvvm.ComponentModel;
using Fab.Data;
using System.Diagnostics;

namespace Fab.Client.Desktop.ViewModels;

partial class OrderedContentEntryViewModel : ObservableObject, IFabRenderable {
	[ObservableProperty] private IFabRenderable _content;
	private OrderedContentEntry _orderedContentEntry;

	public OrderedContentEntryViewModel(OrderedContentEntry orderedContentEntry) {
		_orderedContentEntry = orderedContentEntry;
		if (_orderedContentEntry is not null && _orderedContentEntry.Content is not null) {
			switch (_orderedContentEntry.Content) {
				case Paragraph p: Content = new ParagraphViewModel(p); break;
				default: Debug.WriteLine($"Unsupported content type: {_orderedContentEntry.Content.GetType()}"); break;
			}
			

		}
	}
}
