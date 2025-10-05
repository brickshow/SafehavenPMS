using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Models
{
[Authorize]
    public class Goal
    {
        [Key]
        public int GoalId { get; set; }
        public string Description { get; set; }
        public DateTime? TargetDate { get; set; }

        public string Status { get; set; } = "In Progress"; // e.g. In Progress, Completed, Discontinued

        public string? NotedBy { get; set; }

        // Foreign Key to ProblemList
        public int? PsyProblemListId { get; set; } // Make nullable
        public PsyProblemList? ProblemList { get; set; }

        //Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

    }
}
