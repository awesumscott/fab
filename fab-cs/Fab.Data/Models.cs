using System.Text.Json.Serialization;

namespace Fab.Data;

[AttributeUsage(AttributeTargets.Property)]
public class AccessibilityAttribute : Attribute {}

public class Unique {
	//public Guid Id { get; set; }
	public int Id { get; set; }
}

//public class UniqueNamed : Unique {
//	public string Name { get; set; } = string.Empty;
//}

[JsonDerivedType(typeof(ContentBase))]
[JsonDerivedType(typeof(Paragraph))]
[JsonDerivedType(typeof(Image))]
public class ContentBase : Unique {}

public class Paragraph : ContentBase {
	public string Text { get; set; } = string.Empty;
}

public class Image : ContentBase {
	public string Url { get; set; } = string.Empty;
	[Accessibility] public string Alt { get; set; } = string.Empty;
}

public class Article : Unique {
	public string Title { get; set; } = string.Empty;
	public List<OrderedContentEntry> Entries { get; set; } = [];
}

public class OrderedContentEntry : Unique {
	public ContentBase Content {  get; set; }
	public int Order {  get; set; } = 0;

	public OrderedContentEntry() {}

	public OrderedContentEntry(int order, ContentBase content) {
		Order = order;
		Content = content;
	}
}