// File: ViewModels/UserRolesViewModel.cs
using System.Collections.Generic;

namespace ArianNovinWeb.ViewModels
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }
}
