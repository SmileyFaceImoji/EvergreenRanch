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
}
