// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using HarmonyHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HarmonyHub.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Username")]
            public string Username { get; set; }

            [Display(Name = "Profile Photo")]
            public IFormFile ProfilePhoto { get; set; }

            public string PathToPhoto { get; set; }
        }

        private async Task LoadAsync(UserModel user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);


            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                PathToPhoto = user.PathToPhoto,
                Username = userName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User) as UserModel;
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User) as UserModel;
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.ProfilePhoto != null)
            {
                if (user.PathToPhoto != null)
                {
                    var previousImagePath = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot", user.PathToPhoto.TrimStart('/'));
                    if (System.IO.File.Exists(previousImagePath))
                    {
                        System.IO.File.Delete(previousImagePath);
                    }
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.ProfilePhoto.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePhoto.CopyToAsync(fileStream);
                }

                Input.PathToPhoto = "/images/profiles/" + fileName;

                user.PathToPhoto = Input.PathToPhoto;
                await _userManager.UpdateAsync(user);

                StatusMessage = "Your profile photo has been uploaded.";
            }

            if (Input.Username != user.UserName)
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(user, Input.Username);
                if (!setUsernameResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set username.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
