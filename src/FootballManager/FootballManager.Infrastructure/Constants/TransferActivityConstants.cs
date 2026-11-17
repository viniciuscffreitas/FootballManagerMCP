namespace FootballManager.Infrastructure.Constants;

// The game's transfer activity screen, as its record on the user's club binds it (root Human, base Club.TransferActivity,
// read with the user's manager and team as contexts). Read on 17/11/2026 in the investigation save: every count 0, the
// lists [], both values "R$0 (R$0)". No list item has been seen yet: the item paths are the game's schema for
// TransferActivityPerson and TransferActivityTransferOffer.
internal static class TransferActivityConstants
{
    public const string Activity = "Club.TransferActivity";

    public const string OffersAll = "TransferOffersPlayerCountAll";
    public const string OffersIn = "TransferOffersPlayerCountIn";
    public const string OffersOut = "TransferOffersPlayerCountOut";
    public const string AwaitingResponseAll = "TransferOffersRequireResponseCountAll";
    public const string AwaitingResponseIn = "TransferOffersRequireResponseCountIn";
    public const string AwaitingResponseOut = "TransferOffersRequireResponseCountOut";
    public const string Trades = "TradeOffersCount";

    // Guaranteed then potential, already in the game's currency text: "R$0 (R$0)".
    public const string IncomingValue = "TransfersInPotentialValueString";
    public const string OutgoingValue = "TransfersOutPotentialValueString";

    public static readonly string[] SummaryProperties =
    [
        OffersAll, OffersIn, OffersOut, AwaitingResponseAll, AwaitingResponseIn, AwaitingResponseOut, Trades, IncomingValue, OutgoingValue
    ];

    // The screen's player lists, one per direction (the "All" list is both).
    public const string PlayersIn = Activity + ".TransferActivityPlayerListIn";
    public const string PlayersOut = Activity + ".TransferActivityPlayerListOut";

    // A player's row.
    public const string PlayerId = "Person.UniqueId";
    public const string PlayerName = "Person.Name";
    public const string PlayerStatus = "TransferOfferStatusString";
    public const string PlayerWage = "Wage";

    public static readonly string[] PlayerProperties = [PlayerId, PlayerName, PlayerStatus, PlayerWage];

    // The offers made for the player at the list's index.
    public static string Offers(string players, int index) => $"{players}[{index}].TransferOffers";

    public const string BuyingClub = "BuyingClub.Name";
    public const string SellingClub = "SellingClub.Name";
    public const string OfferType = "TransferOfferTypeString";
    public const string OfferFee = "TransferActivityTableFeeString1";
    public const string OfferStatus = "TransferOfferStatusString";
    public const string OfferRequiresResponse = "OfferRequiresResponse";

    public static readonly string[] OfferProperties = [BuyingClub, SellingClub, OfferType, OfferFee, OfferStatus, OfferRequiresResponse];
}
