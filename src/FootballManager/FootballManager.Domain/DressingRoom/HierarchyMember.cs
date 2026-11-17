namespace FootballManager.Domain.DressingRoom;

/// <summary>A player on one level of the squad's hierarchy.</summary>
public class HierarchyMember
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
