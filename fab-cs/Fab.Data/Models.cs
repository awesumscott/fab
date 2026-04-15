using System.Text.Json.Serialization;

namespace Fab.Data;

[AttributeUsage(AttributeTargets.Property)]
public class AccessibilityAttribute : Attribute {}

public interface IUnique {
	int Id { get; set; }
}

public class Unique : IUnique {
	//public Guid Id { get; set; }
	public int Id { get; set; }
}

//public class UniqueNamed : Unique {
//	public string Name { get; set; } = string.Empty;
//}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Paragraph), "paragraph")]
[JsonDerivedType(typeof(Image), "image")]
[JsonDerivedType(typeof(Heading), "heading")]
[JsonDerivedType(typeof(Link), "link")]
[JsonDerivedType(typeof(Divider), "divider")]
public abstract class ContentBase : Unique {}

public class Paragraph : ContentBase {
	public string Text { get; set; } = string.Empty;
}

public class Image : ContentBase {
	public string Url { get; set; } = string.Empty;
	[Accessibility] public string Alt { get; set; } = string.Empty;
}

public class Heading : ContentBase {
	public int Level { get; set; } = 1;
	public string Text { get; set; } = string.Empty;
}

public class Link : ContentBase {
	public string Text { get; set; } = string.Empty;
	public string Url { get; set; } = string.Empty;
	public bool IsExternal { get; set; }
}

public class Divider : ContentBase {}

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