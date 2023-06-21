using Microsoft.AspNetCore.Identity;

namespace HarmonyHub.Models
{
    public class UserModel : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
