using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.Enum
{
    public enum PatientStatusEnum
    {
        NewIntake,
        InProgress,
        Waitlisted,
        PendingAssessment,
        OnAssessment,
        PendingApproval,
        PendingAdmission,
        Admitted,
        InTreatment,
        Closed,
        Relapsed,
        Discharged
    }
}

