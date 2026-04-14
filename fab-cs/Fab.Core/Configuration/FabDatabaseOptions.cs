using System.ComponentModel.DataAnnotations;

namespace Fab.Core.Configuration;

public sealed class FabDatabaseOptions {
	public const string Section = "Database";

	[Required(AllowEmptyStrings = false)]
	public string ConnectionString { get; set; } = string.Empty;
}
