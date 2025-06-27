using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace KiCData.Models.WebModels
{
    public class VolunteerViewModel : MemberViewModel
    {
        [Required(ErrorMessage = "Please select at least one position.")]
        [Display(Name = "Position", Description = "Select one or more positions you would like to volunteer for.")]
        public List<SelectListItem>? Positions { get; set; }

        [Display(Name = "Additional Information", Prompt = "Please provide any additional information you would like us to know.")]
        public string? Details { get; set; }

        [Required(ErrorMessage = "Please select an event.")]
        [Display(Name = "Upcoming Events", Description = "Choose an upcoming event, leave blank to be added to our roster for upcoming events", Prompt = "Select an event")]
        public int EventId { get; set; }

        public List<SelectListItem>? Events { get; set; }

        public VolunteerViewModel(string firstname, string lastname, string email, DateOnly dateofbirth, string fetname, int clubid, string phonenumber, string additionalinfo, List<SelectListItem> positions, string details, int eventId)
        {
            FirstName = firstname;
            LastName = lastname;
            Email = email;
            DateOfBirth = dateofbirth;
            FetName = fetname;
            ClubId = clubid;
            PhoneNumber = phonenumber;
            AdditionalInfo = additionalinfo;
            Positions = positions;
            Details = details;
            EventId = eventId;

        }

        public VolunteerViewModel()
        {

        }
    }

    



}
