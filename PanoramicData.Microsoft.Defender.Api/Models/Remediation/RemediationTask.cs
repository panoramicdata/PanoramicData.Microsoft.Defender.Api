using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Models.Remediation;

/// <summary>A remediation activity created in the tenant.</summary>
public class RemediationTask
{
	/// <summary>The activity's id.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The activity's title.</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The activity's description. Seen in response examples only.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>Software or Security configuration.</summary>
	[JsonPropertyName("category")]
	public string? Category { get; set; }

	/// <summary>Active or Completed.</summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>The remediation type, such as Update.</summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>High, Medium or Low.</summary>
	[JsonPropertyName("priority")]
	public string? Priority { get; set; }

	/// <summary>When the activity was created.</summary>
	[JsonPropertyName("createdOn")]
	public DateTimeOffset? CreatedOn { get; set; }

	/// <summary>The due date its creator set.</summary>
	[JsonPropertyName("dueOn")]
	public DateTimeOffset? DueOn { get; set; }

	/// <summary>When the status last changed.</summary>
	[JsonPropertyName("statusLastModifiedOn")]
	public DateTimeOffset? StatusLastModifiedOn { get; set; }

	/// <summary>Automatic when all devices were fixed, or Manual when someone marked it complete.</summary>
	[JsonPropertyName("completionMethod")]
	public string? CompletionMethod { get; set; }

	/// <summary>The email of whoever completed it manually.</summary>
	[JsonPropertyName("completerEmail")]
	public string? CompleterEmail { get; set; }

	/// <summary>The object id of whoever completed it manually.</summary>
	[JsonPropertyName("completerId")]
	public string? CompleterId { get; set; }

	/// <summary>The creator's email.</summary>
	[JsonPropertyName("requesterEmail")]
	public string? RequesterEmail { get; set; }

	/// <summary>The creator's object id.</summary>
	[JsonPropertyName("requesterId")]
	public string? RequesterId { get; set; }

	/// <summary>The creator's free-text notes.</summary>
	[JsonPropertyName("requesterNotes")]
	public string? RequesterNotes { get; set; }

	/// <summary>How many devices have been fixed.</summary>
	[JsonPropertyName("fixedDevices")]
	public long? FixedDevices { get; set; }

	/// <summary>How many exposed devices the activity applies to.</summary>
	[JsonPropertyName("targetDevices")]
	public long? TargetDevices { get; set; }

	/// <summary>The related product name.</summary>
	[JsonPropertyName("nameId")]
	public string? NameId { get; set; }

	/// <summary>The related product id.</summary>
	[JsonPropertyName("productId")]
	public string? ProductId { get; set; }

	/// <summary>The related vendor name.</summary>
	[JsonPropertyName("vendorId")]
	public string? VendorId { get; set; }

	/// <summary>The related device groups.</summary>
	[JsonPropertyName("rbacGroupNames")]
	public IReadOnlyList<string> RbacGroupNames { get; set; } = [];

	/// <summary>The version to move to.</summary>
	[JsonPropertyName("recommendedVersion")]
	public string? RecommendedVersion { get; set; }

	/// <summary>The program to move to.</summary>
	[JsonPropertyName("recommendedProgram")]
	public string? RecommendedProgram { get; set; }

	/// <summary>The vendor of the program to move to.</summary>
	[JsonPropertyName("recommendedVendor")]
	public string? RecommendedVendor { get; set; }

	/// <summary>The related software component.</summary>
	[JsonPropertyName("relatedComponent")]
	public string? RelatedComponent { get; set; }

	/// <summary>The id of the related security recommendation.</summary>
	[JsonPropertyName("scid")]
	public string? Scid { get; set; }

	/// <summary>Whether it covers all exposed devices or only those with no user impact.</summary>
	[JsonPropertyName("productivityImpactRemediationType")]
	public string? ProductivityImpactRemediationType { get; set; }
}
