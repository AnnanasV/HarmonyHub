using Microsoft.AspNetCore.Identity;

namespace HarmonyHub.Models
{
    public class UserModel : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string PathToPhoto { get; set; }
    }
}
