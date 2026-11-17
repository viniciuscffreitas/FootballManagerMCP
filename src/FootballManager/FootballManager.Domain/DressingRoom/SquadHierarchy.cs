namespace FootballManager.Domain.DressingRoom;

/// <summary>The squad's hierarchy, level by level from the most influential, as the game ranks it.</summary>
public class SquadHierarchy
{
    public List<HierarchyMember> TeamLeaders { get; set; } = [];
    public List<HierarchyMember> HighlyInfluential { get; set; } = [];
    public List<HierarchyMember> Influential { get; set; } = [];
    public List<HierarchyMember> Other { get; set; } = [];
}
