namespace FootballManager.Domain.Injuries;

/// <summary>The medical centre's season summary for the user's team, its texts in the game's words.</summary>
public class SeasonInjurySummary
{
    public int? InjuriesThisSeason { get; set; }

    /// <summary>Against last season at this point, e.g. "17 a mais".</summary>
    public string? ComparedWithLastSeason { get; set; }

    /// <summary>The staff's forecast for the whole season against last, e.g. "68 a menos".</summary>
    public string? ExpectedOverSeason { get; set; }

    /// <summary>E.g. "1 Lesão (Abaixo da média da liga de 2)".</summary>
    public string? LeagueAverage { get; set; }

    public int? SlightInjuries { get; set; }
    public int? MinorInjuries { get; set; }
    public int? ModerateInjuries { get; set; }
    public int? MajorInjuries { get; set; }

    /// <summary>How many players the game lists at high injury risk.</summary>
    public int? HighRiskPlayers { get; set; }

    /// <summary>E.g. "Nenhum exausto e 1 lesão recorrente".</summary>
    public string? ExhaustedAndRecurring { get; set; }

    /// <summary>Who's expected back for the next match, e.g. "Previsão de retorno de 1 jogador contra ...".</summary>
    public string? ExpectedReturnsNextMatch { get; set; }
}
