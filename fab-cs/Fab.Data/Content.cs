namespace Fab.Data;

public class Content : IUnique {
	public int Id { get; set; }
	public string Type {  get; set; }
	public int Version {  get; set; }
	public ContentBase Value { get; set; }
}

public class PublishedContent : IUnique {
	public int Id { get; set; }
	public string Type { get; set; }
	public ContentBase Value { get; set; }
}

public class Page : IUnique {
	public int Id { get; set; }
	public string Name { get; set; }
	public string Type { get; set; }
	public int Version { get; set; }
	public ICollection<Content> Contents { get; set; } = [];
	public LayoutBase Value { get; set; }
}

public class PublishedPage : IUnique {
	public int Id { get; set; }
	public string Type { get; set; }
	public LayoutBase Value { get; set; }
}
