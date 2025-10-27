using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.Enum
{
    public enum MedicationOrderStatus
    {
        NotStarted,
        Active,
        InProgress,
        Completed,
        Discontinued
    }
}

