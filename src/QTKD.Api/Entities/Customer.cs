namespace QTKD.Api.Entities;

public class Customer
{
    public const string ActiveStatus = "Active";
    public const string InactiveStatus = "Inactive";

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
