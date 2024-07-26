using System.Collections.Generic;

namespace ArianNovinWeb.ViewModels
{
    public class EditUserRoleVM
    {
        public string UserId { get; set; }
        public List<string> AvailableRoles { get; set; } = new List<string>();
        public IList<string> SelectedRoles { get; set; } = new List<string>(); 
    }
}
