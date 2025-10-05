using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel.Billing
{
[Authorize]
    public class MiscellaneousItemViewModel
    {
        [Required]
        public int PatientId { get; set; }


        [Required]
        public List<string> ItemDescriptions { get; set; } = new List<string>();

        [Required]
        public List<decimal> Amounts { get; set; } = new List<decimal>();

        public decimal Total
        {
            get
            {
                decimal t = 0;
                for (int i = 0; i < Amounts?.Count; i++)
                    t += Amounts[i];
                return t;
            }
        }
    }
}

