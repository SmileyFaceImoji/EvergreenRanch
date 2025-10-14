using EvergreenRanch.Utilities;

namespace EvergreenRanch.Models.Common
{
    public enum TypeAnimal
    {
        [EnumDisplayName("Cattle")] Cattle,
        [EnumDisplayName("Sheep")] Sheep,
        [EnumDisplayName("Goat")] Goat,
        [EnumDisplayName("Horse")] Horse,
        [EnumDisplayName("Pig")] Pig,
        [EnumDisplayName("Chicken")] Chicken
    }

    public enum StatusHealth
    {
        [EnumDisplayName("Healthy")] Healthy,
        [EnumDisplayName("Sick")] Sick,
        [EnumDisplayName("Injured")] Injured,
        [EnumDisplayName("Recovering")] Recovering
    }

    public enum StatusAnimal
    {
        [EnumDisplayName("Available")] ForSale,
        [EnumDisplayName("Sold")] Sold,
        [EnumDisplayName("Not for Sale")] NotForSale
    }

    public enum TypeGender
    {
        Male,
        Female
    }

    public enum RandomCharType
    {
        None = 0,
        Uppercase = 1,
        Lowercase = 2,
        Digit = 4,
        Special = 8
    }

    public enum ApplicationStatus
    { 
        Pending = 0, 
        Approved = 1, 
        Rejected = 2 
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }


    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected
    }

}
