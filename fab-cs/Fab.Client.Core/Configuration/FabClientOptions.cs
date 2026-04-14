using System.ComponentModel.DataAnnotations;

namespace Fab.Client.Core.Configuration;

public sealed class FabClientOptions {
	public const string Section = "FabClient";

	[Required(AllowEmptyStrings = false)]
	public string BaseUrl { get; set; } = string.Empty;
}
