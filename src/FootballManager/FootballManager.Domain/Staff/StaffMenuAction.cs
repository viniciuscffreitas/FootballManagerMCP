namespace FootballManager.Domain.Staff;

/// <summary>The game's menu actions for staff: on a staff member, or on the club for its job adverts.</summary>
public enum StaffMenuAction
{
    /// <summary>Sack the staff member, paying off his contract (PersonTerminateContract).</summary>
    Dismiss,

    /// <summary>End his contract by mutual agreement (PersonMutualTerminationContract).</summary>
    MutualTermination,

    /// <summary>Send him on a coaching course, for his coaching badges (PersonCoachingCourse).</summary>
    CoachingCourse,

    /// <summary>Send him on a leadership course (PersonLeadershipCourse).</summary>
    LeadershipCourse,

    /// <summary>Offer him a new contract (PersonOfferContract): opens the game's contract offer.</summary>
    OfferContract,

    /// <summary>Advertise a staff job, any department (PlaceJobAdvert), on the club.</summary>
    AdvertiseJob,

    /// <summary>Advertise a coaching job (PlaceCoachingJobAdvert), on the club.</summary>
    AdvertiseCoachingJob,

    /// <summary>Advertise a scouting job (PlaceScoutingJobAdvert), on the club.</summary>
    AdvertiseScoutingJob,

    /// <summary>Advertise a medical job (PlaceMedicalJobAdvert), on the club.</summary>
    AdvertiseMedicalJob
}
