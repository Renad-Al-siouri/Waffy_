namespace Waffy.Models.Enums
{
    public enum EnergyLevel
    {
        Low,
        Medium,
        High
    }

    public enum PostStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum AdoptionStatus
    {
        Available,
        Reserved,
        Adopted,
        Closed
    }

    public enum LostPetStatus
    {
        Active,
        Found,
        Closed
    }

    public enum VaccinationStatus
    {
        UpToDate,
        Partial,
        None
    }

    public enum RewardStatus
    {
        None,
        Reserved,
        Released
    }
}