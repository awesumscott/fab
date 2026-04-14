using System.Text.Json.Serialization;

namespace Fab.Data;

public class LayoutBase : IUnique {
	public int Id { get; set; }
}

public class LayoutNode : LayoutBase {
	public ContentBase Value { get; set; }
}

public class LayoutGroup : LayoutNode {
	public int Id { get; set; }
	public ICollection<LayoutBase> Children { get; set; } = [];
}

public class Article : LayoutGroup {
	public int Id { get; set; }
	public string Title { get; set; } = string.Empty;
	//public List<OrderedContentEntry> Entries { get; set; } = [];
}

//public class OrderedContentEntry : LayoutBase {
//	public int Id { get; set; }
//	public ContentBase Content { get; set; }
//	public int Order { get; set; } = 0;

//	public OrderedContentEntry() { }

//	public OrderedContentEntry(int order, ContentBase content) {
//		Order = order;
//		Content = content;
//	}
//}

[JsonDerivedType(typeof(ContentBase))]
[JsonDerivedType(typeof(TextSection))]
[JsonDerivedType(typeof(Image))]
public class ContentBase : IUnique {
	public int Id { get; set; }
}

public class TextSection : ContentBase {
	public string Heading { get; set; } = string.Empty;
	public string Body { get; set; } = string.Empty;
}

public class Image : ContentBase {
	public string Url { get; set; } = string.Empty;
	[Accessibility] public string Alt { get; set; } = string.Empty;
}
