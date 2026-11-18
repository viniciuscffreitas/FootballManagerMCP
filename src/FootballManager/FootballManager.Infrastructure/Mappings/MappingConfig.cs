using System.Globalization;
using System.Text.Json;
using FootballManager.Domain.Availability;
using FootballManager.Domain.Board;
using FootballManager.Domain.Competitions;
using FootballManager.Domain.Contracts;
using FootballManager.Domain.Database;
using FootballManager.Domain.Finances;
using FootballManager.Domain.DressingRoom;
using FootballManager.Domain.Injuries;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Messages;
using FootballManager.Domain.Opposition;
using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Standings;
using FootballManager.Domain.Staff;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;
using FootballManager.Domain.Tactics.Effectiveness;
using FootballManager.Domain.Training;
using FootballManager.Domain.Youth;
using FootballManager.Domain.Transfers;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Availability;
using FootballManager.Infrastructure.Dtos.Board;
using FootballManager.Infrastructure.Dtos.Competitions;
using FootballManager.Infrastructure.Dtos.Contracts;
using FootballManager.Infrastructure.Dtos.Database;
using FootballManager.Infrastructure.Dtos.Finances;
using FootballManager.Infrastructure.Dtos.DressingRoom;
using FootballManager.Infrastructure.Dtos.Injuries;
using FootballManager.Infrastructure.Dtos.Matches;
using FootballManager.Infrastructure.Dtos.Messages;
using FootballManager.Infrastructure.Dtos.Opposition;
using FootballManager.Infrastructure.Dtos.Players;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Dtos.Standings;
using FootballManager.Infrastructure.Dtos.Staff;
using FootballManager.Infrastructure.Dtos.Tactics;
using FootballManager.Infrastructure.Dtos.Tactics.Editing;
using FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;
using FootballManager.Infrastructure.Dtos.Training;
using FootballManager.Infrastructure.Dtos.Youth;
using FootballManager.Infrastructure.Dtos.Transfers;
using Mapster;

namespace FootballManager.Infrastructure.Mappings;

public class MappingConfig
{
    /// <summary>The Mapster parameter giving a phase's role names by mask, for a role's key (TacticKeys.Roles).</summary>
    public const string RoleKeysParameter = "roleKeys";

    /// <summary>The Mapster parameter giving the game's names for its tactic ids (TacticKeys), for a scouted player's roles.</summary>
    public const string TacticKeysParameter = "tacticKeys";

    /// <summary>The Mapster parameter giving the game's names for a player's status flags (GameVocabulary.StatusKeysAsync).</summary>
    public const string StatusKeysParameter = "statusKeys";

    /// <summary>
    /// The Mapster parameter giving a team's players by their record's index (Dictionary of int to BridgePlayerDto), for
    /// the players an opposition report names by reference.
    /// </summary>
    public const string PlayersByRecordParameter = "playersByRecord";

    private const string UnknownAmount = "-";

    // What the game sends for a player it has never scouted: 30000 days since (about 82 years), a report dated 1900.
    private const int NeverScoutedDays = 30000;
    private const string BlankYear = "1900";

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BridgePlayerDto, Player>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Condition, src => src.Condition >= 0 ? src.Condition : null)
            .Map(dest => dest.IsInjured, src => src.IsInjured ?? false)
            // The game sends 0 for a player without appearances.
            .Map(dest => dest.AverageRating, src => src.AverageRating > 0 ? (double?)Math.Round(src.AverageRating.Value, 2) : null)
            // Status keys need the game's names for the flags, passed as StatusKeysParameter.
            .Map(dest => dest.Statuses, src => AllStatuses(src.Statuses, src.TeamStatuses))
            .Map(dest => dest.TrialUntil, src => src.TrialIsValid == true ? src.TrialEndDate : null)
            .Map(dest => dest.Attributes, src => AttributeBoundsMapper.ToAttributes(src.AttributeBounds))
            // Appearances always comes back when the season's numbers were asked for.
            .Map(dest => dest.SeasonStats, src => src.Appearances == null ? null : src);

        config.NewConfig<BridgePlayerDto, PlayerSeasonStats>();

        RegisterTactics(config);
        RegisterMatches(config);
        RegisterScouting(config);
        RegisterMessages(config);
        RegisterDatabase(config);
        RegisterTraining(config);
        RegisterCompetitions(config);
        RegisterAvailability(config);
        RegisterStandings(config);
        RegisterFinances(config);
        RegisterBoard(config);
        RegisterStaff(config);
        RegisterContracts(config);
        RegisterSquadDynamics(config);
        RegisterInjuries(config);
        RegisterOpposition(config);
        RegisterYouth(config);
        RegisterTransferActivity(config);
    }

    // An opposition report. Players come by reference, named through PlayersByRecordParameter; role keys need the
    // game's role names (TacticKeys), passed as TacticKeysParameter. The game marks an expected tactic it has nothing
    // for as not valid.
    private static void RegisterOpposition(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeOppositionReportDto, OppositionReport>()
            .Map(dest => dest.TeamId, src => src.Id ?? 0)
            .Map(dest => dest.TeamName, src => src.Name ?? string.Empty)
            .Map(dest => dest.Formation, src => src.ExpectedTactic != null && src.ExpectedTactic.IsValid != false ? Text(src.ExpectedTactic.Formation) : null)
            .Map(dest => dest.TacticSummary, src => src.ExpectedTactic != null ? Text(src.ExpectedTactic.Summary) : null)
            .Map(dest => dest.ExpectedLineup, src => src.ExpectedTactic != null && src.ExpectedTactic.IsValid != false
                ? (src.ExpectedTactic.Players ?? new List<BridgeExpectedPlayerDto>()).Select(place => ExpectedPlayer(place)).ToList()
                : new List<ExpectedPlayer>())
            .Map(dest => dest.Points, src => (src.Visuals ?? new List<BridgeReportVisualDto>())
                .Select(visual => visual.ObservationData)
                .Where(observation => observation != null && !string.IsNullOrWhiteSpace(observation.GraphType))
                .Select(observation => ReportPoint(observation!))
                .ToList())
            // Set by the service, which knows whether the team is the next opponent.
            .Ignore(dest => dest.IsNextOpponent)
            .Ignore(dest => dest.ThreatLevel);
    }

    // The role mask carries the duty's bit beside the role's; the role's key is the mask without it.
    private static ExpectedPlayer ExpectedPlayer(BridgeExpectedPlayerDto place)
    {
        var keys = MapContext.Current?.Parameters.TryGetValue(TacticKeysParameter, out var names) == true ? names as TacticKeys : null;
        var duties = keys?.InPossessionDuties ?? new Dictionary<ulong, string>();
        var dutyBits = duties.Keys.Aggregate(0UL, (all, bit) => all | bit);
        var player = RecordPlayer(place.Player?.Index);

        return new ExpectedPlayer
        {
            PlayerId = player?.Id,
            Name = player?.Name,
            Position = place.Position is { } position ? PositionMasks.ToPosition(position) : null,
            RoleKey = place.Role is { } role ? keys?.InPossessionRoles.GetValueOrDefault(role & ~dutyBits) : null,
            Duty = place.Role is { } mask ? duties.Where(duty => (mask & duty.Key) != 0).Select(duty => duty.Value).FirstOrDefault() : null
        };
    }

    private static ReportPoint ReportPoint(BridgeObservationDto observation)
    {
        return new ReportPoint
        {
            Topic = observation.GraphType!,
            Summary = Text(observation.PerformanceSummary),
            TimeFrame = Text(observation.TimeFrame),
            // The team's own record when the point is about the team: no player then.
            PlayerName = RecordPlayer(observation.SubjectRecordId?.Index)?.Name
        };
    }

    private static BridgePlayerDto? RecordPlayer(int? index)
    {
        return index is { } record
               && MapContext.Current?.Parameters.TryGetValue(PlayersByRecordParameter, out var players) == true
               && players is Dictionary<int, BridgePlayerDto> byRecord
            ? byRecord.GetValueOrDefault(record)
            : null;
    }

    // A competition's stats screen, and the contract of a player in it as the game shows the user.
    private static void RegisterCompetitions(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeCompetitionDto, CompetitionInfo>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Nation, src => Text(src.Nation))
            // Set by the client, which knows which league is the user's.
            .Ignore(dest => dest.IsUserLeague);

        config.NewConfig<BridgeStatRowDto, CompetitionStatRow>()
            .Map(dest => dest.PlayerId, src => src.PlayerId ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Team, src => Text(src.Team))
            .Map(dest => dest.Positions, src => src.Positions == null ? new List<PitchPosition>() : PositionMasks.All(src.Positions.Value))
            .Map(dest => dest.Value, src => Math.Round(src.Value ?? 0, 2))
            // Set by the client from the rows' order.
            .Ignore(dest => dest.Rank)
            .Ignore(dest => dest.RowIndex);

        // A contract without an end (amateur and non-contract terms) is dated 1900; a wage that doesn't apply reads "N/A"
        // with 0, and a club that hasn't set an asking price sends -1.
        config.NewConfig<BridgePlayerContractDto, PlayerContract>()
            .Map(dest => dest.Status, src => src.Status == null ? null : Text(src.Status.DisplayValue))
            .Map(dest => dest.Type, src => src.Type == null ? null : Text(src.Type.DisplayValue))
            .Map(dest => dest.EndDate, src => src.HasEndDate == true && !IsBlankDate(src.EndDate) ? src.EndDate : null)
            .Map(dest => dest.Wage, src => src.Wage != null && src.Wage.PropertyValue > 0 ? Text(src.Wage.DisplayValue) : null)
            .Map(dest => dest.Value, src => src.Value == null ? null : Text(src.Value.TransferValueRangeString))
            .Map(dest => dest.AskingPrice, src => src.Value != null && src.Value.AskingPrice > 0 ? src.Value.AskingPrice : null)
            .Map(dest => dest.TransferStatus, src => src.TransferStatus == null || src.TransferStatus.TransferStatusDescription == null
                ? null
                : Text(src.TransferStatus.TransferStatusDescription.Text));
    }

    // A player's training as his individual training screen shows it: the position and role he trains in (those he plays
    // unless picked otherwise), and the dropdowns' current rows. "---" is the game's unset row; a focus of -1 is none.
    // His role key needs the in-possession role names, passed as RoleKeysParameter. A calendar day holds its sessions
    // and, on a match day, the fixture as an event of its own.
    private static void RegisterTraining(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeOptionDto, TrainingOption>()
            .Map(dest => dest.Id, src => src.PropertyValue ?? 0)
            .Map(dest => dest.Name, src => src.DisplayValue ?? string.Empty);

        config.NewConfig<BridgeOptionDto, TrainingSessionCategory>()
            .Map(dest => dest.Id, src => (int)(src.PropertyValue ?? 0))
            .Map(dest => dest.Name, src => src.DisplayValue ?? string.Empty)
            .Ignore(dest => dest.Sessions);

        config.NewConfig<BridgePlayerTrainingDto, PlayerTraining>()
            .Map(dest => dest.PlayerId, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Position, src => PositionCode(src.Position))
            .Map(dest => dest.RoleKey, src => RoleKey(src.RoleId))
            .Map(dest => dest.RoleName, src => Text(src.RoleName))
            .Map(dest => dest.FollowsMatchRole, src => src.PositionChoice == TrainingConstants.MatchPosition)
            .Map(dest => dest.OutOfPossessionRole, src => Chosen(src.OutOfPossessionRole))
            .Map(dest => dest.Focus, src => src.Focus == null || src.Focus.PropertyValue == null || src.Focus.PropertyValue < 0 ? null : src.Focus)
            .Map(dest => dest.Intensity, src => src.Intensity == null || src.Intensity.PropertyValue == null ? null : src.Intensity)
            .Map(dest => dest.IntensityIsAutomatic, src => src.IntensityAutomatic == true)
            .Map(dest => dest.RestDays, src => src.RestDays > 0 ? src.RestDays : null)
            // The game sends 0 for a player who hasn't trained lately (injured, say).
            .Map(dest => dest.Rating, src => src.Rating > 0 ? (double?)Math.Round(src.Rating.Value, 2) : null)
            .Map(dest => dest.Workload, src => Text(src.Workload))
            .Map(dest => dest.CanEditFocus, src => src.FocusCanEdit == true && src.FocusHumanControlled == true)
            .Map(dest => dest.CanEditIntensity, src => src.IntensityCanEdit == true)
            .Map(dest => dest.FocusChoices, src => TrainingRows.Of(src.FocusChoices).Select(row => row.PropertyValue!.Value).ToList())
            .Map(dest => dest.IntensityChoices, src => TrainingRows.Of(src.IntensityChoices, TrainingConstants.Holidays)
                .Select(row => row.PropertyValue!.Value).ToList());

        config.NewConfig<BridgeTrainingWeekDto, TrainingWeek>()
            .Map(dest => dest.StartDate, src => Text(src.WeekStartDate))
            .Map(dest => dest.Days, src => src.Days().Where(day => day.Day != null).Select(day => day.Day!).ToList());

        config.NewConfig<BridgeTrainingDayDto, TrainingDay>()
            .Map(dest => dest.Date, src => src.Date ?? string.Empty)
            .Map(dest => dest.Sessions, src => (src.Events ?? new List<BridgeCalendarEventDto>())
                .Where(e => e.SessionName != null && e.SessionIndex != null).ToList())
            .Map(dest => dest.Match, src => (src.Events ?? new List<BridgeCalendarEventDto>())
                .Where(e => e.CalendarEvent == TrainingConstants.MatchEvent).Select(e => Text(e.MatchVenue)).FirstOrDefault());

        config.NewConfig<BridgeCalendarEventDto, TrainingSession>()
            .Map(dest => dest.Slot, src => src.SessionIndex ?? 0)
            .Map(dest => dest.Type, src => src.SessionType ?? 0)
            .Map(dest => dest.Name, src => src.SessionName ?? string.Empty)
            .Map(dest => dest.Category, src => src.SessionCategory ?? -1)
            .Map(dest => dest.CanEdit, src => src.CanEdit == true);
    }

    private static string? PositionCode(ulong? mask)
    {
        return mask is { } position ? PositionMasks.ToPosition(position)?.Code : null;
    }

    // A dropdown's current row, unless it's the unset one.
    private static string? Chosen(BridgeOptionDto? row)
    {
        return row?.DisplayValue is { } label && label != TrainingConstants.Unset ? Text(label) : null;
    }

    // What the search shows of each kind: a person's club and nationality, a club's nation and league, a nation's
    // continent. The linked records are the search's own line under the name.
    private static void RegisterDatabase(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeDatabaseRecordDto, DatabaseRecord>()
            .Map(dest => dest.Kind, src => RecordKind(src))
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.LongName, src => RecordKind(src) == DatabaseRecordKind.Club && src.LongName != src.Name ? Text(src.LongName) : null)
            .Map(dest => dest.Description, src => Text(src.Description))
            .Map(dest => dest.Type, src => RecordKind(src) == DatabaseRecordKind.Other || RecordKind(src) == DatabaseRecordKind.NationalTeam
                ? Text(src.TypeLabel)
                : null)
            .Map(dest => dest.Age, src => IsPerson(src) ? src.Age : null)
            .Map(dest => dest.Positions, src => RecordKind(src) == DatabaseRecordKind.Player ? Text(src.Positions) : null)
            .Map(dest => dest.Job, src => RecordKind(src) == DatabaseRecordKind.Staff ? Text(src.Job) : null)
            .Map(dest => dest.Nation, src => RecordNation(src))
            .Map(dest => dest.Club, src => IsPerson(src) ? Text(src.Club) : null)
            .Map(dest => dest.ClubId, src => IsPerson(src) ? src.ClubId : null)
            .Map(dest => dest.ClubNation, src => IsPerson(src) ? Text(src.ClubNation) : null)
            .Map(dest => dest.League, src => RecordLeague(src))
            .Map(dest => dest.Continent, src => RecordContinent(src));
    }

    // Every message attaches something: a forwarded one the article it passes on, any other itself. Only a forwarded
    // message's source and attachment are its own.
    private static void RegisterMessages(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeInboxMessageDto, InboxMessage>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Title, src => src.Title ?? string.Empty)
            .Map(dest => dest.Sender, src => Text(src.Sender))
            .Map(dest => dest.Subtitle, src => Text(src.Subtitle))
            .Map(dest => dest.Body, src => Text(src.Body))
            .Map(dest => dest.IsRead, src => src.IsRead == true)
            .Map(dest => dest.IsImportant, src => src.IsImportant == true)
            .Map(dest => dest.IsPriority, src => src.IsPriority == true)
            .Map(dest => dest.CanReply, src => src.CanReply == true)
            .Map(dest => dest.HasReplied, src => src.HasReplied == true)
            .Map(dest => dest.IsForwarded, src => src.IsForwarded == true)
            .Map(dest => dest.Source, src => src.IsForwarded == true ? Text(src.Source) : null)
            .Map(dest => dest.Article, src => src.IsForwarded == true ? Text(src.Article) : null);

        config.NewConfig<BridgeNewsStoryDto, NewsStory>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Title, src => src.Title ?? string.Empty)
            .Map(dest => dest.Subtitle, src => Text(src.Subtitle))
            .Map(dest => dest.Source, src => Text(src.Source))
            .Map(dest => dest.Body, src => Text(src.Body));

        // A button's fields, from its RowData when the game nests them there. NewsAction -1 opens no panel. Index and
        // the recommendation come from the button's place in the list (InboxBridgeClient).
        config.NewConfig<BridgeSpeakToActionDto, MessageOption>()
            .Map(dest => dest.Title, src => ActionFields(src).ActionTitle ?? string.Empty)
            .Map(dest => dest.Event, src => ActionFields(src).Event ?? 0)
            .Map(dest => dest.ResponseId, src => ActionFields(src).ActionResponseId)
            .Map(dest => dest.ResponseValue, src => ActionFields(src).ActionResponseValue)
            .Map(dest => dest.NewsAction, src => PanelOf(ActionFields(src)))
            .Map(dest => dest.IsDisabled, src => ActionFields(src).IsDisabled == true)
            .Map(dest => dest.IsRecommended, src => ActionFields(src).Recommended == true)
            .Map(dest => dest.Confirmation, src => ConfirmationOf(ActionFields(src)))
            .Ignore(dest => dest.Index)
            .Ignore(dest => dest.Flow!);
    }

    private static BridgeSpeakToActionDto ActionFields(BridgeSpeakToActionDto action)
    {
        return action.ActionTitle is null && action.RowData is not null ? action.RowData : action;
    }

    private static int? PanelOf(BridgeSpeakToActionDto action)
    {
        return action.NewsAction is >= 0 ? action.NewsAction : null;
    }

    private static string? ConfirmationOf(BridgeSpeakToActionDto action)
    {
        return Text(action.ConfirmationDialogData?.Description);
    }

    // What the club's scouting knows: the game sends values it doesn't know as empty text, or a -1 count.
    private static void RegisterScouting(TypeAdapterConfig config)
    {
        // A position is an immutable record: kept as it is rather than copied (it has no default constructor).
        config.NewConfig<TacticalPosition, TacticalPosition>().MapWith(src => src);

        // Role keys need the game's role names (TacticKeys), passed as TacticKeysParameter.
        config.NewConfig<BridgeScoutedPlayerDto, ScoutedPlayer>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Club, src => Text(src.Club))
            .Map(dest => dest.Nation, src => Text(src.Nation))
            .Map(dest => dest.Recommendation, src => Text(src.Recommendation))
            .Map(dest => dest.GradeLabel, src => Text(src.GradeLabel))
            .Map(dest => dest.BestPosition, src => src.BestPosition == null ? null : PositionMasks.ToPosition(src.BestPosition.Value))
            .Map(dest => dest.BestRole, src => ScoutedRoleKey(src.BestRole, true))
            .Map(dest => dest.BestOutOfPossessionRole, src => ScoutedRoleKey(src.BestOutOfPossessionRole, false))
            .Map(dest => dest.KnowledgeLevel, src => src.Knowledge == null ? null : src.Knowledge.ActualValue)
            .Map(dest => dest.KnowledgeMax, src => src.Knowledge == null ? null : src.Knowledge.MaxValue)
            .Map(dest => dest.Knowledge, src => src.Knowledge == null ? null : Text(src.Knowledge.Text))
            .Map(dest => dest.Value, src => Range(src.MinValue, src.MaxValue))
            .Map(dest => dest.Wage, src => Range(Display(src.MinWage), Display(src.MaxWage)))
            .Map(dest => dest.StrongestArea, src => Text(src.StrongestArea))
            .Map(dest => dest.WeakestArea, src => Text(src.WeakestArea))
            .Map(dest => dest.IsShortlisted, src => src.IsShortlisted == true)
            .Map(dest => dest.IsBeingScouted, src => src.IsBeingScouted == true)
            .Map(dest => dest.DaysSinceLastScouted, src => Days(src.DaysSinceLastScouted))
            .Map(dest => dest.Statuses, src => AllStatuses(src.Statuses, src.TeamStatuses))
            .Map(dest => dest.Attributes, src => AttributeBoundsMapper.ToAttributes(src.AttributeBounds))
            .Map(dest => dest.Performance, src => PerformanceMapper.ToPerformance(src.AttributeBounds))
            .Map(dest => dest.League, src => PerformanceMapper.League(src.AttributeBounds))
            .Map(dest => dest.LeagueNation, src => PerformanceMapper.LeagueNation(src.AttributeBounds))
            // The report's date always comes back when the report was asked for; a player never scouted has a blank one.
            .Map(dest => dest.Report, src => IsBlankDate(src.ReportDate) ? null : src);

        config.NewConfig<BridgeScoutedPlayerDto, ScoutReport>()
            .Map(dest => dest.Date, src => src.ReportDate)
            .Map(dest => dest.Scout, src => Text(src.ReportScout))
            .Map(dest => dest.Ability, src => Text(src.ReportAbility))
            .Map(dest => dest.Potential, src => Text(src.ReportPotential))
            .Map(dest => dest.Pros, src => src.ReportProsCons == null ? new List<string>() : Sentences(src.ReportProsCons.ProsList))
            .Map(dest => dest.Cons, src => src.ReportProsCons == null ? new List<string>() : Sentences(src.ReportProsCons.ConsList))
            .Map(dest => dest.TransferFee, src => src.ReportTransfer == null ? null : Short(src.ReportTransfer.ExpectedTransferValue))
            .Map(dest => dest.WageDemands, src => src.ReportTransfer == null ? null : Short(src.ReportTransfer.ExpectedWageDemands))
            .Map(dest => dest.TransferInterest, src => src.ReportTransfer == null ? null : Short(src.ReportTransfer.ExpectedTransferInterest))
            .Map(dest => dest.LoanInterest, src => src.ReportTransfer == null ? null : Short(src.ReportTransfer.ExpectedLoanInterest));

        config.NewConfig<BridgeRecruitmentFocusDto, RecruitmentFocus>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.IsComplete, src => src.IsComplete == true)
            .Map(dest => dest.IsStruggling, src => src.IsStruggling == true)
            .Map(dest => dest.Advice, src => Text(src.Advice))
            .Map(dest => dest.Areas, src => Text(src.Areas))
            .Map(dest => dest.Scouts, src => Text(src.Scouts))
            .Map(dest => dest.MinAge, src => src.MinAge == null ? null : (int?)src.MinAge.PropertyValue)
            .Map(dest => dest.MaxAge, src => src.MaxAge == null ? null : (int?)src.MaxAge.PropertyValue)
            .Map(dest => dest.MinAbilityStars, src => src.MinAbility)
            .Map(dest => dest.MinPotentialStars, src => src.MinPotential)
            .Map(dest => dest.Positions, src => TickedPositions(src.Positions, true))
            .Map(dest => dest.OutOfPossessionPositions, src => TickedPositions(src.Positions, false))
            .Map(dest => dest.RecommendedPlayers, src => src.RecommendedCount)
            .Map(dest => dest.CurrentPlayers, src => src.CurrentCount)
            .Map(dest => dest.NearMissPlayers, src => src.NearMissCount)
            // Set by the client from what the API removed (RemovedFocusLog), not by the game.
            .Ignore(dest => dest.ShowsRemovedFocus);

        config.NewConfig<BridgePlayerStatusDto, PlayerStatus>()
            .Map(dest => dest.Key, src => StatusKey(src.StatusFlag))
            .Map(dest => dest.Label, src => Text(src.StatusTitleFull))
            .Map(dest => dest.Detail, src => OneLine(src.StatusReason));

        config.NewConfig<BridgeSearchStatusDto, PlayerSearchStatus>()
            .Map(dest => dest.Status, src => Text(src.Status))
            .Map(dest => dest.Interest, src => Text(src.Interest))
            .Map(dest => dest.Attributes, src => Text(src.Attributes))
            // Read on their own (IPlayerSearchGameClient).
            .Ignore(dest => dest.Filters!);

        config.NewConfig<BridgeSearchFiltersDto, PlayerSearchFilters>()
            .Map(dest => dest.InterestType, src => InterestType(src.State.Interest))
            .Map(dest => dest.InterestLevel, src => InterestLevel(src.State.Interest))
            .Map(dest => dest.OtherConditions, src => OtherConditions(src));

        config.NewConfig<BridgeSearchRuleDto, SearchRule>()
            .Map(dest => dest.Name, src => Text(src.Name))
            .Map(dest => dest.Comparison, src => src.Comparison == null ? null : Text(src.Comparison.Text))
            .Map(dest => dest.Value, src => RuleValue(src));

        // A rule's values are its options' ids: ages as they are, positions as masks, grades by their letters.
        config.NewConfig<BridgeSearchChoicesDto, PlayerSearchChoices>()
            .Map(dest => dest.Ages, src => (src.Age.MinValues ?? new List<BridgeFilterItemDto>())
                .Where(item => item.Id != null).Select(item => (int)item.Id!.Value).ToList())
            .Map(dest => dest.Positions, src => SearchPositions(src.Position.Values))
            .Map(dest => dest.Familiarities, src => Familiarities(src.Position.Comparisons))
            .Map(dest => dest.Grades, src => (src.Grade.MinValues ?? new List<BridgeFilterItemDto>())
                .Where(item => item.Id != PlayerSearchConstants.AnyGrade && item.Name != null).Select(item => item.Name!).ToList());

        // A top entry carries its own fields, an entry below has them in RowData; both keep their sub-entries in ChildList.
        config.NewConfig<BridgeMenuItemDto, MenuEntry>()
            .Map(dest => dest.Label, src => MenuLabel(src))
            .Map(dest => dest.CanRun, src => MenuRuns(src))
            .Map(dest => dest.Disabled, src => MenuGreyedOut(src))
            .Map(dest => dest.Confirmation, src => MenuConfirmation(src))
            .Map(dest => dest.ConfirmationIsWarning, src => MenuWarns(src))
            .Map(dest => dest.Children, src => src.ChildList ?? new List<BridgeMenuItemDto>());

        // Every box of the position grid, ticked or not, is a position a focus can ask for.
        config.NewConfig<BridgeDraftFocusDto, FocusChoices>()
            .Map(dest => dest.MinAges, src => Ages(src.MinAgeChoices))
            .Map(dest => dest.MaxAges, src => Ages(src.MaxAgeChoices))
            .Map(dest => dest.Positions, src => GridPositions(src.Positions));

        config.NewConfig<BridgeScoutDto, Scout>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty);

        config.NewConfig<BridgeScoutingCountsDto, ScoutingOverview>()
            .Map(dest => dest.RecommendedPlayers, src => src.Centre == null ? null : src.Centre.RecommendedPlayerCount)
            .Map(dest => dest.HighlyRecommendedPlayers, src => src.Centre == null ? null : src.Centre.HighlyRecommendedPlayersCount)
            .Map(dest => dest.RequestedReports, src => src.Centre == null ? null : src.Centre.RequestedReportsCount)
            .Map(dest => dest.FocusesShown, src => src.FocusListType == null ? null : (FocusListFilter?)src.FocusListType.Value)
            .Ignore(dest => dest.Scouts, dest => dest.Focuses, dest => dest.Shortlists);

        // A shortlist's size comes as text ("14 players"); which one the screen shows is set by the client.
        config.NewConfig<BridgeShortlistDto, Shortlist>()
            .Map(dest => dest.Name, src => Text(src.DisplayString) ?? string.Empty)
            .Map(dest => dest.Players, src => LeadingNumber(src.CountString))
            .Map(dest => dest.IsDefault, src => src.IsDefault == true)
            .Ignore(dest => dest.IsSelected);
    }

    private static int? LeadingNumber(string? text)
    {
        var digits = new string((text ?? string.Empty).SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
        return digits.Length > 0 ? int.Parse(digits, CultureInfo.InvariantCulture) : null;
    }

    private static void RegisterMatches(TypeAdapterConfig config)
    {
        // A match's score and numbers only mean something once it's played: before that the game sends -1 and zeros.
        // Which side is the user's, and the shape they played, are set by the client.
        config.NewConfig<BridgeFixtureDto, MatchSummary>()
            .Map(dest => dest.Date, src => DateOnly.ParseExact(src.Date!, MatchConstants.DateFormat, CultureInfo.InvariantCulture))
            .Map(dest => dest.IsFriendly, src => src.IsFriendly == true)
            .Map(dest => dest.IsPlayed, src => src.HasResult == true)
            .Map(dest => dest.Home, src => new MatchSide
            {
                TeamId = src.HomeTeamId ?? 0,
                TeamName = src.HomeTeamName ?? string.Empty,
                Goals = src.HasResult == true ? src.HomeScore : null,
                ExpectedGoals = src.HasResult == true ? src.HomeExpectedGoals : null,
                Possession = src.HasResult == true ? src.HomePossession : null,
                Shots = src.HasResult == true ? src.HomeShots : null,
                ShotsOnTarget = src.HasResult == true ? src.HomeShotsOnTarget : null,
                LeaguePosition = string.IsNullOrEmpty(src.HomeLeaguePosition) ? null : src.HomeLeaguePosition
            })
            .Map(dest => dest.Away, src => new MatchSide
            {
                TeamId = src.AwayTeamId ?? 0,
                TeamName = src.AwayTeamName ?? string.Empty,
                Goals = src.HasResult == true ? src.AwayScore : null,
                ExpectedGoals = src.HasResult == true ? src.AwayExpectedGoals : null,
                Possession = src.HasResult == true ? src.AwayPossession : null,
                Shots = src.HasResult == true ? src.AwayShots : null,
                ShotsOnTarget = src.HasResult == true ? src.AwayShotsOnTarget : null,
                LeaguePosition = string.IsNullOrEmpty(src.AwayLeaguePosition) ? null : src.AwayLeaguePosition
            })
            .Ignore(dest => dest.TeamId, dest => dest.Formation!);

        config.NewConfig<BridgeTeamMatchStatsDto, TeamMatchStats>()
            .Map(dest => dest.Possession, src => src.TotalPossession)
            .Map(dest => dest.Shots, src => src.TotalShots)
            .Map(dest => dest.ShotsHitWoodwork, src => src.ShotHitWoodwork)
            .Map(dest => dest.Crosses, src => src.CrossAttempts)
            .Map(dest => dest.Passes, src => src.PassAttempts)
            .Map(dest => dest.PassesPerDefensiveAction, src => src.OppositionPassesPerDefensiveAction)
            .Map(dest => dest.Tackles, src => src.TackleAttempts)
            .Map(dest => dest.Headers, src => src.HeaderAttempts)
            .Map(dest => dest.Fouls, src => src.FoulsMade)
            .Map(dest => dest.AverageRating, src => src.AverageRatingFloat);

        config.NewConfig<BridgePlayerMatchStatsDto, PlayerMatchStats>()
            .Map(dest => dest.Shots, src => src.TotalShots)
            .Map(dest => dest.Passes, src => src.PassAttempts)
            .Map(dest => dest.Dribbles, src => src.DribblesMade)
            .Map(dest => dest.Crosses, src => src.CrossAttempts)
            .Map(dest => dest.Tackles, src => src.TackleAttempts)
            .Map(dest => dest.TacklesWon, src => src.TackleCompleted)
            .Map(dest => dest.AerialDuels, src => src.AerialDuelsAttempted)
            .Map(dest => dest.AerialDuelsWon, src => src.AerialDuelsCompleted)
            .Map(dest => dest.Saves, src => src.TotalSaves)
            .Map(dest => dest.DistanceRun, src => src.DistanceRan);
    }

    private static void RegisterTactics(TypeAdapterConfig config)
    {
        // The game sends -1 for a score it doesn't know (e.g. a role rating for a player it can't judge); a score not
        // asked for comes as no record at all.
        config.NewConfig<BridgeRatingDto, Rating?>()
            .MapWith(src => src != null && src.MaxValue >= 0
                ? new Rating(src.MinValue >= 0 ? src.MinValue!.Value : src.MaxValue!.Value, src.MaxValue!.Value)
                : null);

        config.NewConfig<BridgeAdviceDto, EffectivenessAdvice>()
            .Map(dest => dest.Text, src => src.Text ?? string.Empty)
            .Map(dest => dest.IsPositive, src => src.IsPro ?? false);

        config.NewConfig<BridgeSlotEffectivenessDto, TacticalEffectiveness>()
            .Map(dest => dest.Warnings, src => src.StatusFlags.Where(flag => flag.Value).Select(flag => flag.Key).ToList());

        config.NewConfig<BridgePlayerAbilityDto, PlayerAbilityScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.RoleSuitability, src => src.RoleSuitabilityScore)
            .Map(dest => dest.Capability, src => src.CapabilityScore)
            .Map(dest => dest.Advice, src => src.AdviceForComponent);

        config.NewConfig<BridgePlayerBuyInDto, PlayerBuyInScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.ManagerSupport, src => src.ManagerSupportScore)
            .Map(dest => dest.Relationships, src => src.TeammateAndStaffRelationshipsScore)
            .Map(dest => dest.Advice, src => src.AdviceForComponent);

        config.NewConfig<BridgeTacticalUnderstandingDto, TacticalUnderstandingScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.Comprehension, src => src.TacticalComprehension)
            .Map(dest => dest.ComprehensionAdvice, src => src.AdviceForTacticalComprehension)
            .Map(dest => dest.AreasAdvice, src => src.AdviceForTacticalAreas)
            .Map(dest => dest.Complexity, src => src.TacticalComplexity);

        config.NewConfig<BridgeTacticalAreaDto, TacticalArea>()
            .Map(dest => dest.Understanding, src => src.OverallEffectiveness);

        config.NewConfig<BridgeTacticalComplexityDto, TacticalComplexity>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.TeamInstructions, src => src.TeamInstructionComplexity)
            .Map(dest => dest.PlayerInstructions, src => src.PlayerInstructionComplexity)
            .Map(dest => dest.OppositionInstructions, src => src.OppositionInstructionComplexity);

        config.NewConfig<BridgeNextMatchDto, NextMatchScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.Advice, src => src.AdviceForComponent);

        // A role's key needs the phase's role names (TacticKeys.Roles), passed as RoleKeysParameter.
        config.NewConfig<BridgeRoleRowDto, RoleRating>()
            .Map(dest => dest.Key, src => RoleKey(src.TacticsRoleId))
            .Map(dest => dest.Role, src => src.TacticsRoleName ?? string.Empty)
            .Map(dest => dest.Rating, src => src.CurrentAbilityStars);

        RegisterTacticEditing(config);

        // Index and position label come from the slot's position list, not the player.
        config.NewConfig<BridgeSlotPlayerDto, SlotPosition>()
            .Map(dest => dest.PlayerId, src => src.Id)
            .Map(dest => dest.PlayerName, src => src.Name)
            .Ignore(dest => dest.Index, dest => dest.InPossession!);
    }

    // The tactics screen's catalogs, as tactic edits name things: the game's identifiers as keys, its labels as labels.
    private static void RegisterTacticEditing(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeInstructionOptionDto, TacticOption>()
            .MapWith(src => new TacticOption(src.Option ?? string.Empty, src.Name));

        // The option the tactic holds comes from the tactic, not the catalog.
        config.NewConfig<BridgeTeamInstructionDto, TeamInstructionOptions>()
            .Map(dest => dest.Key, src => src.Instruction ?? string.Empty)
            .Map(dest => dest.Label, src => src.Name)
            .Ignore(dest => dest.Current!);

        config.NewConfig<BridgePlayerInstructionGroupDto, PlayerInstructionGroup>()
            .Map(dest => dest.Key, src => src.Group ?? string.Empty)
            .Map(dest => dest.Label, src => src.Name)
            .Map(dest => dest.InPossession, src => !TacticConstants.OutOfPossessionInstructionGroups.Contains(src.Group))
            .Map(dest => dest.Options, src => src.Options
                .Where(option => option.Option != null)
                .Select(option => new TacticOption(TacticKeys.PlayerInstructionKey(option.Option!), option.Name))
                .ToList());
    }

    private static string? ScoutedRoleKey(ulong? mask, bool inPossession)
    {
        return mask is { } role
               && MapContext.Current?.Parameters.TryGetValue(TacticKeysParameter, out var names) == true
               && names is TacticKeys keys
            ? keys.Roles(inPossession).GetValueOrDefault(role)
            : null;
    }

    private static DatabaseRecordKind RecordKind(BridgeDatabaseRecordDto record)
    {
        if (IsPerson(record))
        {
            return record.IsPlayer switch
            {
                true => DatabaseRecordKind.Player,
                false => DatabaseRecordKind.Staff,
                null => DatabaseRecordKind.Other
            };
        }

        return record.RecordType is { } type
            ? DatabaseSearchConstants.Kinds.GetValueOrDefault(type, DatabaseRecordKind.Other)
            : DatabaseRecordKind.Other;
    }

    private static bool IsPerson(BridgeDatabaseRecordDto record)
    {
        return record.RecordType == DatabaseSearchConstants.Person;
    }

    private static string? RecordNation(BridgeDatabaseRecordDto record)
    {
        return RecordKind(record) switch
        {
            DatabaseRecordKind.Player or DatabaseRecordKind.Staff => Text(record.Nation),
            DatabaseRecordKind.Club => Text(record.Nation) ?? Text(record.Linked),
            DatabaseRecordKind.Competition or DatabaseRecordKind.NationalTeam => Text(record.Linked),
            _ => null
        };
    }

    private static string? RecordLeague(BridgeDatabaseRecordDto record)
    {
        return RecordKind(record) switch
        {
            DatabaseRecordKind.Player or DatabaseRecordKind.Staff => Text(record.League),
            DatabaseRecordKind.Club => Text(record.SecondaryLinked) ?? Text(record.League),
            _ => null
        };
    }

    private static string? RecordContinent(BridgeDatabaseRecordDto record)
    {
        return RecordKind(record) switch
        {
            DatabaseRecordKind.Nation => Text(record.Linked),
            DatabaseRecordKind.NationalTeam => Text(record.SecondaryLinked),
            _ => null
        };
    }

    // Who can play the next match. Empty texts are the game's blanks (no injury, no reason); a ban's -1 count is the one
    // it isn't measured in.
    private static void RegisterAvailability(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeInjuryDto, PlayerInjury>()
            .Map(dest => dest.PlayerId, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Availability, src => src.Availability ?? 0)
            .Map(dest => dest.Injury, src => Text(src.Description))
            .Map(dest => dest.ExpectedReturn, src => Text(src.ExpectedReturn));

        config.NewConfig<BridgeBanDto, PlayerBan>()
            .Map(dest => dest.PlayerId, src => src.PlayerId ?? 0)
            .Map(dest => dest.Name, src => src.PlayerName ?? string.Empty)
            .Map(dest => dest.Scope, src => Text(src.Scope))
            .Map(dest => dest.MatchesRemaining, src => src.MatchesRemaining == AvailabilityConstants.NotCounted ? null : src.MatchesRemaining)
            .Map(dest => dest.DaysRemaining, src => src.DaysRemaining == AvailabilityConstants.NotCounted ? null : src.DaysRemaining)
            .Map(dest => dest.Reason, src => Text(src.Reason));

        config.NewConfig<BridgeNextMatchRulesDto, NextMatchRules>()
            .Map(dest => dest.Competition, src => Text(src.Competition))
            .Map(dest => dest.EligibilityRules, src => (src.EligibilityRules ?? new List<JsonElement>()).Select(ListItemText).ToList())
            .Map(dest => dest.MaxSquadRule, src => Text(src.MaxSquadRule))
            .Map(dest => dest.MaxSquadRuleStatus, src => Text(src.MaxSquadRuleStatus));
    }

    // The league table. The game's fate for a place with none is a blank name.
    private static void RegisterStandings(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeTableRowDto, LeagueTableRow>()
            .Map(dest => dest.TeamId, src => src.TeamId ?? 0)
            .Map(dest => dest.TeamName, src => src.TeamName ?? string.Empty)
            .Map(dest => dest.Position, src => src.Position ?? 0)
            .Map(dest => dest.Played, src => src.Played ?? 0)
            .Map(dest => dest.Won, src => src.Won ?? 0)
            .Map(dest => dest.Drawn, src => src.Drawn ?? 0)
            .Map(dest => dest.Lost, src => src.Lost ?? 0)
            .Map(dest => dest.GoalsFor, src => src.GoalsFor ?? 0)
            .Map(dest => dest.GoalsAgainst, src => src.GoalsAgainst ?? 0)
            .Map(dest => dest.GoalDifference, src => src.GoalDifference ?? 0)
            .Map(dest => dest.Points, src => src.Points ?? 0)
            .Map(dest => dest.Fate, src => Text(src.Fate))
            .Map(dest => dest.Movement, src => src.Rising == true ? TableMovement.Up : src.Falling == true ? TableMovement.Down : TableMovement.Same)
            .Ignore(dest => dest.IsUser);
    }

    // The club finance screen. Amounts in pounds stay numbers (the service writes them in the screens' currency); texts
    // pass as the game writes them.
    private static void RegisterFinances(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeFinancesDto, FinanceReadout>()
            .Map(dest => dest.FinancialStatus, src => Text(src.FinancialStatus))
            .Map(dest => dest.Wages, src => src.Wages ?? new BridgeWagesDto())
            .Map(dest => dest.WageGroups, src => src.Wages != null && src.Wages.PlayerTypeWageData != null
                ? src.Wages.PlayerTypeWageData
                : new List<BridgeWageGroupDto>())
            .Map(dest => dest.IncomeLines, src => src.IncomeLines ?? new List<BridgeFinanceLineDto>())
            .Map(dest => dest.ExpenditureLines, src => src.ExpenditureLines ?? new List<BridgeFinanceLineDto>())
            .Map(dest => dest.NotableIncome, src => src.NotableIncome ?? new List<BridgeNotableIncomeDto>())
            .Map(dest => dest.BalanceHistory, src => src.BalanceHistory ?? new List<BridgeGraphPointDto>())
            .Map(dest => dest.BalanceProjection, src => SeasonEnds(src.BalanceProjection));

        config.NewConfig<BridgeWagesDto, WageSummary>()
            .Map(dest => dest.Committed, src => Text(src.CommittedWageTotalValueWithShortText))
            .Map(dest => dest.Budget, src => Text(src.CurrentWageBudgetValueWithText))
            .Map(dest => dest.Remaining, src => Text(src.RemainingBudgetValueWithText))
            .Map(dest => dest.NextSeasonBudget, src => Text(src.NextSeasonWageBudgetValueWithText))
            .Map(dest => dest.BudgetUsedPercentage, src => src.WageBudgetPercentage == null ? null : (double?)Math.Round(src.WageBudgetPercentage.Value, 1));

        config.NewConfig<BridgeWageGroupDto, WageGroup>()
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Players, src => src.Quantity ?? 0)
            .Map(dest => dest.TotalWages, src => Text(src.TotalBasicWage))
            .Map(dest => dest.TotalWagesInGameUnit, src => src.TotalBasicWageValue ?? 0)
            .Map(dest => dest.AverageWage, src => Text(src.AverageBasicWage))
            .Map(dest => dest.HighestWage, src => Text(src.HighestBasicWage))
            .Map(dest => dest.LowestWage, src => Text(src.LowestBasicWage))
            .Map(dest => dest.AdvisedMaxWage, src => Text(src.AdvisedMaxBasicWage))
            .Map(dest => dest.IsOverWages, src => src.IsOverWages ?? false);

        config.NewConfig<BridgeFinanceLineDto, FinanceLine>()
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Amount, src => Text(src.DisplayValue))
            .Map(dest => dest.ChangeFromLastMonth, src => src.MonthToMonthPercentage);

        config.NewConfig<BridgeNotableIncomeDto, NotableIncome>()
            .Map(dest => dest.Name, src => src.Name ?? string.Empty);

        config.NewConfig<BridgeGraphPointDto, BalanceAt>()
            .Map(dest => dest.When, src => src.GraphPointX ?? string.Empty)
            .Map(dest => dest.Amount, src => src.GraphPointY ?? 0);
    }

    // This season's end and the next two, in order; a point the game leaves out is skipped.
    private static List<BalanceAt> SeasonEnds(BridgeProjectionDto? projection)
    {
        if (projection is null)
        {
            return [];
        }

        return new[] { projection.ProjectionEndOfSeasonOne, projection.ProjectionEndOfSeasonTwo, projection.ProjectionEndOfSeasonThree }
            .Where(point => point?.PropertyValue is not null)
            .Select(point => new BalanceAt { When = point!.Title ?? string.Empty, Amount = point.PropertyValue!.Value })
            .ToList();
    }

    // The club vision screen. An objective the game leaves unnamed isn't shown; the culture texts hold one item a line.
    private static void RegisterBoard(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeTeamConfidenceDto, BoardConfidence>()
            .Map(dest => dest.Grade, src => src.BoardGrade)
            .Map(dest => dest.Summary, src => Text(src.BoardSummary))
            .Map(dest => dest.Matches, src => Text(src.BoardMatches))
            .Map(dest => dest.SquadSupport, src => Text(src.BoardSquadSupport))
            .Map(dest => dest.Tactics, src => Text(src.BoardTactics))
            .Map(dest => dest.Vision, src => Text(src.BoardVision))
            .Map(dest => dest.Transfers, src => Text(src.BoardTransfers))
            .Map(dest => dest.Highlights, src => src.BoardHighlights ?? new List<string>())
            .Map(dest => dest.Criticism, src => src.BoardCriticism ?? new List<string>());

        config.NewConfig<BridgeTeamConfidenceDto, SupporterConfidence>()
            .Map(dest => dest.Grade, src => src.SupporterGrade)
            .Map(dest => dest.Summary, src => Text(src.SupporterSummary))
            .Map(dest => dest.Matches, src => Text(src.SupporterMatches))
            .Map(dest => dest.Vision, src => Text(src.SupporterVision));

        config.NewConfig<BridgeClubVisionDto, BoardStatus>()
            .Map(dest => dest.ThisYearObjective, src => Objective(src.ThisYearName, src.ThisYearYear, src.ThisYearProgress, src.ThisYearStatus))
            .Map(dest => dest.FinalObjective, src => Objective(src.FinalName, src.FinalYear, src.FinalProgress, src.FinalStatus))
            .Map(dest => dest.SupporterCulture, src => Lines(src.SupporterCulture))
            .Map(dest => dest.BoardCulture, src => Lines(src.BoardCulture))
            .Map(dest => dest.BoardRequests, src => src.BoardRequests ?? 0)
            .Map(dest => dest.ActiveBoardRequests, src => src.ActiveBoardRequests ?? 0)
            // Set by the client from the confidence record and the manager's promises.
            .Ignore(dest => dest.Board)
            .Ignore(dest => dest.Supporters)
            .Ignore(dest => dest.Promises);

        config.NewConfig<BridgeBoardPromisesDto, List<string>>()
            .MapWith(src => (src.Promises ?? new List<JsonElement>()).Select(ListItemText).ToList());
    }

    // The first team's staff as their profiles show them. A contract without an end is dated 1900. The wage stays the
    // game's own text, in the save's currency.
    private static void RegisterStaff(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeStaffAttributeDto, StaffAttribute>()
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Value, src => src.PropertyValue ?? 0)
            .Map(dest => dest.Level, src => Text(src.DisplayString));

        config.NewConfig<BridgeStaffMemberDto, StaffMember>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Job, src => Text(src.Job))
            .Map(dest => dest.CoachingStyle, src => Text(src.CoachingStyle))
            .Map(dest => dest.CoachingAttributes, src => src.CoachingAttributes ?? new List<BridgeStaffAttributeDto>())
            .Map(dest => dest.MedicalAttributes, src => src.MedicalAttributes ?? new List<BridgeStaffAttributeDto>())
            .Map(dest => dest.Wage, src => Text(src.Wage))
            .Map(dest => dest.ContractEnd, src => IsBlankDate(src.ContractEnd) ? null : src.ContractEnd);

        config.NewConfig<BridgeStaffCountsDto, TeamStaff>()
            .Map(dest => dest.TeamName, src => src.TeamName ?? string.Empty)
            .Map(dest => dest.Analyst, src => Text(src.Analyst))
            // Set by the client from their own reads.
            .Ignore(dest => dest.Members)
            .Ignore(dest => dest.Vacancies);
    }

    // The first team's contracts. Money stays as the game writes it; a release clause of 0 ("-") is none, and a contract
    // without an end date (dated 1900) has no end.
    private static void RegisterContracts(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeSquadContractDto, SquadPlayerContract>()
            .Map(dest => dest.PlayerId, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Wage, src => src.Wage == null ? null : Text(src.Wage.DisplayValue))
            .Map(dest => dest.EndDate, src => ContractEnd(src.HasEndDate, src.EndDate))
            .Map(dest => dest.TimeRemaining, src => Text(src.TimeRemaining))
            .Map(dest => dest.Type, src => src.Type == null ? null : Text(src.Type.DisplayValue))
            .Map(dest => dest.AgreedPlayingTime, src => src.AgreedPlayingTime == null ? null : Text(src.AgreedPlayingTime.DisplayValue))
            .Map(dest => dest.ReleaseClause,
                src => src.ReleaseClause != null && src.ReleaseClause.SortValue > 0 ? Text(src.ReleaseClause.DisplayValue) : null)
            .Map(dest => dest.ClausesCount, src => src.ClausesCount ?? 0)
            .Map(dest => dest.HasRenewalOffer, src => src.HasRenewalOffer ?? false)
            .Map(dest => dest.StaffAdvice, src => Text(src.StaffAdvice));
    }

    private static DateOnly? ContractEnd(bool? hasEndDate, string? date)
    {
        if (hasEndDate != true || IsBlankDate(date))
        {
            return null;
        }

        return DateOnly.TryParseExact(date, MatchConstants.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day)
            ? day
            : null;
    }

    // The dressing room. Empty texts are the game's blanks; a happiness reason is the sentence the game shows.
    private static void RegisterSquadDynamics(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeHierarchyMemberDto, HierarchyMember>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty);

        config.NewConfig<BridgeHappinessIssueDto, HappinessIssue>()
            .Map(dest => dest.PlayerId, src => src.PlayerId ?? 0)
            .Map(dest => dest.PlayerName, src => src.PlayerName ?? string.Empty)
            .Map(dest => dest.Severity, src => Text(src.Severity))
            .Map(dest => dest.Description, src => Text(src.Description))
            .Map(dest => dest.PlayersAgreeing, src => src.PlayersAgreeing ?? 0)
            .Map(dest => dest.HasPromises, src => src.LinkedPromises != null && src.LinkedPromises.Count > 0);

        config.NewConfig<BridgeAtmosphereFactorDto, AtmosphereFactor>()
            .Map(dest => dest.Text, src => src.Text ?? string.Empty);

        config.NewConfig<BridgeAtmosphereDto, DressingRoomAtmosphere>()
            .Map(dest => dest.Summary, src => Text(src.Summary))
            .Map(dest => dest.Factors, src => src.Factors ?? new List<BridgeAtmosphereFactorDto>());

        config.NewConfig<BridgePromiseDto, PlayerPromise>()
            .Map(dest => dest.Title, src => Text(src.Title))
            .Map(dest => dest.Description, src => Text(src.Description))
            .Map(dest => dest.Status, src => Text(src.Status))
            .Map(dest => dest.EndDate, src => src.EndDate == null ? null : Text(ListItemText(src.EndDate.Value)));

        config.NewConfig<BridgePlayerDynamicsDto, PlayerDynamics>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.HierarchyLevel, src => Text(src.HierarchyLevel))
            .Map(dest => dest.HappinessPositives, src => Reasons(src.HappinessPositives))
            .Map(dest => dest.HappinessNegatives, src => Reasons(src.HappinessNegatives))
            .Map(dest => dest.Promises, src => src.Promises ?? 0);
    }

    private static List<string> Reasons(List<BridgeHappinessReasonDto>? reasons)
    {
        return (reasons ?? []).Select(reason => Text(reason.Text)).OfType<string>().ToList();
    }

    // The medical centre. Empty texts are the game's blanks (no risk factor, no one returning).
    private static void RegisterInjuries(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeMedicalRecordDto, PlayerMedicalRecord>()
            .Map(dest => dest.PlayerId, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.InjuriesThisSeason, src => src.InjuriesThisSeason ?? 0)
            .Map(dest => dest.RiskOverview, src => Text(src.RiskOverview))
            .Map(dest => dest.RiskReason, src => Text(src.RiskReason))
            .Map(dest => dest.History, src => src.History ?? new List<BridgePastInjuryDto>());

        config.NewConfig<BridgePastInjuryDto, PastInjury>()
            .Map(dest => dest.Date, src => InjuryDate(src.Date))
            .Map(dest => dest.Injury, src => Text(src.Description))
            .Map(dest => dest.Duration, src => Text(src.Duration))
            .Map(dest => dest.Severity, src => Severity(src.Severity))
            .Map(dest => dest.Cause, src => Text(src.Cause))
            .Map(dest => dest.Context, src => Text(src.ReceivedIn))
            .Map(dest => dest.IsActive, src => src.IsActive ?? false);

        config.NewConfig<BridgeSeasonInjuriesDto, SeasonInjurySummary>()
            .Map(dest => dest.ComparedWithLastSeason, src => Text(src.ComparedWithLastSeason))
            .Map(dest => dest.ExpectedOverSeason, src => Text(src.ExpectedOverSeason))
            .Map(dest => dest.LeagueAverage, src => Text(src.LeagueAverage))
            .Map(dest => dest.ExhaustedAndRecurring, src => Text(src.ExhaustedAndRecurring))
            .Map(dest => dest.ExpectedReturnsNextMatch, src => Text(src.ExpectedReturnsNextMatch));
    }

    private static DateOnly? InjuryDate(string? date)
    {
        return DateOnly.TryParseExact(date, InjuryConstants.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    // The game's 0-3 scale; anything else is left out rather than guessed.
    private static InjurySeverity? Severity(int? severity)
    {
        return severity is >= (int)InjurySeverity.Slight and <= (int)InjurySeverity.Major ? (InjurySeverity)severity.Value : null;
    }

    // The youth set-up and loans. Empty texts are the game's blanks; a loan rating of 0 means no appearances yet.
    private static void RegisterYouth(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeYouthSetupDto, YouthSetup>()
            .Map(dest => dest.TeamName, src => Text(src.TeamName))
            .Map(dest => dest.IntakeDate, src => Text(src.IntakeDate))
            .Map(dest => dest.FacilitiesRating, src => src.Facilities == null ? null : src.Facilities.Rating)
            .Map(dest => dest.Facilities, src => src.Facilities == null ? null : Text(src.Facilities.DescriptionShort))
            .Map(dest => dest.FacilitiesDescription, src => src.Facilities == null ? null : Text(src.Facilities.Description));

        config.NewConfig<BridgeYouthPlayerDto, YouthPlayer>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Positions, src => Text(src.Positions))
            .Map(dest => dest.Morale, src => Text(src.Morale))
            .Map(dest => dest.ContractEndDate, src => Text(src.ContractEndDate))
            .Ignore(dest => dest.OnLoan, dest => dest.LoanClub);

        config.NewConfig<BridgeLoanedPlayerDto, LoanedPlayer>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Club, src => Text(src.Club))
            .Map(dest => dest.LoanEndDate, src => Text(src.LoanEndDate))
            .Map(dest => dest.MonthlyFee, src => Text(src.MonthlyFee))
            .Map(dest => dest.AverageRating, src => src.AverageRating > 0 ? (double?)Math.Round(src.AverageRating.Value, 2) : null)
            .Map(dest => dest.Comments, src => (src.Comments ?? new List<BridgeSortedValueDto>())
                .Select(comment => comment.DisplayValue)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(text => text!)
                .ToList());
    }

    private static VisionObjective? Objective(string? name, string? year, int? progress, int? status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return new VisionObjective { Name = name, Year = Text(year), Progress = progress, Status = status };
    }

    private static List<string> Lines(string? text)
    {
        return (text ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    // A list item the game sends as text stays text; anything else is kept as its JSON, since no save has shown one yet.
    private static string ListItemText(JsonElement item)
    {
        return item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : item.GetRawText();
    }

    // The transfer activity screen. A count the game didn't answer is 0; its texts, money included, stay as it shows them.
    private static void RegisterTransferActivity(TypeAdapterConfig config)
    {
        config.NewConfig<BridgeTransferActivityDto, TransferOfferCounts>()
            .Map(dest => dest.All, src => src.OffersAll ?? 0)
            .Map(dest => dest.In, src => src.OffersIn ?? 0)
            .Map(dest => dest.Out, src => src.OffersOut ?? 0)
            .Map(dest => dest.AwaitingResponseAll, src => src.AwaitingResponseAll ?? 0)
            .Map(dest => dest.AwaitingResponseIn, src => src.AwaitingResponseIn ?? 0)
            .Map(dest => dest.AwaitingResponseOut, src => src.AwaitingResponseOut ?? 0)
            .Map(dest => dest.Trades, src => src.Trades ?? 0);

        config.NewConfig<BridgeTransferActivityDto, TransferActivityReadout>()
            .Map(dest => dest.Counts, src => src)
            .Map(dest => dest.IncomingValue, src => Text(src.IncomingValue))
            .Map(dest => dest.OutgoingValue, src => Text(src.OutgoingValue))
            .Ignore(dest => dest.Players);

        config.NewConfig<BridgeTransferActivityPlayerDto, TransferActivityPlayer>()
            .Map(dest => dest.PlayerId, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Status, src => Text(src.Status))
            .Map(dest => dest.Wage, src => Text(src.Wage))
            .Ignore(dest => dest.Direction)
            .Ignore(dest => dest.Offers);

        config.NewConfig<BridgeTransferActivityOfferDto, TransferActivityOffer>()
            .Map(dest => dest.BuyingClub, src => Text(src.BuyingClub))
            .Map(dest => dest.SellingClub, src => Text(src.SellingClub))
            .Map(dest => dest.Type, src => Text(src.Type))
            .Map(dest => dest.Fee, src => Text(src.Fee))
            .Map(dest => dest.Status, src => Text(src.Status))
            .Map(dest => dest.RequiresResponse, src => src.RequiresResponse ?? false);
    }

    private static string? Text(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static SearchInterestType? InterestType(BridgeInterestOptionDto? interest)
    {
        return PlayerSearchConstants.InterestTypeIds
            .Where(pair => pair.Value == interest?.Id)
            .Select(pair => (SearchInterestType?)pair.Key)
            .FirstOrDefault();
    }

    // The selected type's level; none when interest isn't considered.
    private static SearchInterestLevel? InterestLevel(BridgeInterestOptionDto? interest)
    {
        return PlayerSearchConstants.InterestLevels
            .Where(pair => pair.Value == interest?.Level)
            .Select(pair => (SearchInterestLevel?)pair.Key)
            .FirstOrDefault();
    }

    // The game's count of conditions, in brackets at the end of its "edit search" label, beyond those read one by one.
    private static int? OtherConditions(BridgeSearchFiltersDto filters)
    {
        var label = filters.State.EditFilterLabel ?? "";
        var open = label.LastIndexOf('(');
        var close = label.LastIndexOf(')');

        if (open < 0 || close <= open || !int.TryParse(label[(open + 1)..close], NumberStyles.None, CultureInfo.InvariantCulture, out var count))
        {
            return null;
        }

        var other = count - filters.QuickFilters.Count - filters.Rules.Count;
        return other > 0 ? other : null;
    }

    // The rule's value as the comparison uses it: a minimum, a maximum, a range, or the one value.
    private static string? RuleValue(BridgeSearchRuleDto rule)
    {
        var value = rule.Comparison?.Id switch
        {
            PlayerSearchConstants.AtLeast => rule.MinValue?.Name,
            PlayerSearchConstants.AtMost => rule.MaxValue?.Name,
            PlayerSearchConstants.Between or PlayerSearchConstants.NotBetween when rule is { MinValue.Name: { } min, MaxValue.Name: { } max }
                => $"{min}-{max}",
            _ => rule.Value?.Name
        };

        return Text(value) ?? Text(rule.ValueText);
    }

    private static List<PitchPosition> SearchPositions(List<BridgeFilterItemDto>? items)
    {
        return (items ?? [])
            .Where(item => item.Id is > 0)
            .Select(item => PositionMasks.ToPosition((ulong)item.Id!.Value)?.Position)
            .OfType<PitchPosition>()
            .ToList();
    }

    private static List<PositionFamiliarity> Familiarities(List<BridgeCompareFlagDto>? flags)
    {
        return (flags ?? [])
            .Select(flag => PlayerSearchConstants.Familiarities.Where(pair => pair.Value == flag.Id).Select(pair => (PositionFamiliarity?)pair.Key).FirstOrDefault())
            .OfType<PositionFamiliarity>()
            .ToList();
    }

    private static string? Display(BridgeSortedValueDto? value)
    {
        return value is null ? null : Text(value.DisplayValue);
    }

    // "7,76M € - 9,21M €", or the one value when both ends are the same (a fully scouted player).
    private static string? Range(string? min, string? max)
    {
        var low = Amount(min);
        var high = Amount(max);

        return low is null || high is null || low == high ? high ?? low : $"{low} - {high}";
    }

    // The game writes an amount it doesn't know as "-" or "-1 €"; no fee or wage is negative.
    private static string? Amount(string? text)
    {
        return Text(text) is { } amount && !amount.StartsWith(UnknownAmount, StringComparison.Ordinal) ? amount : null;
    }

    private static int? Days(BridgeSortedValueDto? days)
    {
        // Tables sort what they don't know last, as int.MaxValue; a player never scouted reads as NeverScoutedDays.
        return days?.SortValue is { } value && value is >= 0 and < NeverScoutedDays ? (int)value : null;
    }

    // Every status the game has for a player, each once: its full set, then what only the team screens' set holds. The
    // latter carries a placeholder (flag -1, "Invalid"), which isn't a status.
    private static List<BridgePlayerStatusDto> AllStatuses(List<BridgePlayerStatusDto>? all, List<BridgePlayerStatusDto>? team)
    {
        return (all ?? [])
            .Concat(team ?? [])
            .Where(status => status.StatusFlag is >= 0)
            .DistinctBy(status => status.StatusFlag)
            .ToList();
    }

    // A status flag by the game's English name, given as StatusKeysParameter.
    private static string? StatusKey(int? flag)
    {
        return flag is { } value
               && MapContext.Current?.Parameters.TryGetValue(StatusKeysParameter, out var names) == true
               && names is Dictionary<long, string> keys
            ? keys.GetValueOrDefault(value)
            : null;
    }

    // Long details come over several lines (every club watching a player): one line reads as well.
    private static string? OneLine(string? text)
    {
        return Text(text) is { } value
            ? string.Join(' ', value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            : null;
    }

    // The game dates what never happened 1 January 1900, written in the display language's format.
    private static bool IsBlankDate(string? date)
    {
        return string.IsNullOrWhiteSpace(date) || date.Contains(BlankYear, StringComparison.Ordinal);
    }

    private static string? Short(BridgeReportLineDto? line)
    {
        return line is null ? null : Range(line.ShortString, line.ShortString);
    }

    // The scout's sentences read on their own; the headings alone ("Versatility") don't say which way they cut.
    private static List<string> Sentences(List<BridgeProConDto> items)
    {
        return items
            .Select(item => Text(item.ProConsLongString) ?? Text(item.ProConsShortString))
            .OfType<string>()
            .ToList();
    }

    private static List<int> Ages(List<BridgeOptionDto> choices)
    {
        return choices.Where(choice => choice.PropertyValue is not null).Select(choice => (int)choice.PropertyValue!.Value).ToList();
    }

    private static List<TacticalPosition> GridPositions(List<BridgeFocusPositionDto> grid)
    {
        return grid
            .Where(box => box.Position is not null)
            .Select(box => PositionMasks.ToPosition(box.Position!.Value))
            .OfType<TacticalPosition>()
            .Distinct()
            .ToList();
    }

    private static string MenuLabel(BridgeMenuItemDto item)
    {
        return Text(item.Text) ?? Text(item.RowData?.Text) ?? string.Empty;
    }

    private static bool MenuRuns(BridgeMenuItemDto item)
    {
        return MenuFields(item).Event == ScoutingConstants.DoActionEventId && !MenuGreyedOut(item);
    }

    // The game greys out an entry in its RowData or beside it: an advert already placed comes as
    // {"RowData":{..., "Disabled":false}, "Disabled":true, "Hint":"A vaga já foi anunciada."}.
    private static bool MenuGreyedOut(BridgeMenuItemDto item)
    {
        return item.Disabled == true || item.RowData?.Disabled == true;
    }

    private static BridgeMenuItemDto MenuFields(BridgeMenuItemDto item)
    {
        return item.Text is null && item.RowData is not null ? item.RowData : item;
    }

    private static bool MenuWarns(BridgeMenuItemDto item)
    {
        return MenuFields(item).ActionData?.ConfirmationDialogData?.IsErrorDialog == true;
    }

    // The dialog's text on one line, as the game words it ("Ask the scouts to watch X? ... travel costs ...").
    private static string? MenuConfirmation(BridgeMenuItemDto item)
    {
        var description = Text(MenuFields(item).ActionData?.ConfirmationDialogData?.Description);

        return description is null
            ? null
            : string.Join(' ', description.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static List<TacticalPosition> TickedPositions(List<BridgeFocusPositionDto> grid, bool inPossession)
    {
        return grid
            .Where(box => box.Ticked is > 0 && (box.IsInPossession ?? true) == inPossession && box.Position is not null)
            .Select(box => PositionMasks.ToPosition(box.Position!.Value))
            .OfType<TacticalPosition>()
            .Distinct()
            .ToList();
    }

    private static string? RoleKey(ulong? mask)
    {
        return mask is { } role
               && MapContext.Current?.Parameters.TryGetValue(RoleKeysParameter, out var names) == true
               && names is IReadOnlyDictionary<ulong, string> keys
            ? keys.GetValueOrDefault(role)
            : null;
    }
}
