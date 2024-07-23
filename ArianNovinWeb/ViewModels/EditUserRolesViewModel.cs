using System.Collections.Generic;

namespace ArianNovinWeb.ViewModels
{
    public class EditUserRolesViewModel
    {
        public string UserId { get; set; }
        public List<string> AvailableRoles { get; set; }
        public IList<string> SelectedRoles { get; set; }
    }
}
