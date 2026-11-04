namespace QTKD.Api.Entities;

public class Contract
{
    public const string DraftStatus = "Draft";
    public const string RevisionRequestedStatus = "RevisionRequested";
    public const string PendingApprovalStatus = "PendingApproval";

    public int Id { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public Customer Customer { get; set; } = null!;
}
