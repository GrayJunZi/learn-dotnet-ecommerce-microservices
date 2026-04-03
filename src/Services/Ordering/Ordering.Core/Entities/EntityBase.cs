namespace Ordering.Core.Entities;

public abstract class EntityBase
{
    public int Id { get; protected set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}