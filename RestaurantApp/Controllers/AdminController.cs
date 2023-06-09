using RestaurantApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantApp.Controllers
{
  [Authorize(Roles = "Admin")]
  public class AdminController : Controller
  {
    private UserManager<ApplicationUser> userManager;
    private IPasswordHasher<ApplicationUser> passwordHasher;


    public AdminController(UserManager<ApplicationUser> usrMgr, IPasswordHasher<ApplicationUser> passwordHash)
    {
      userManager = usrMgr;
      passwordHasher = passwordHash;

    }

    public IActionResult Index()
    {
      return View(userManager.Users);
    }
    public ViewResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(UserTest user)
    {
      if (ModelState.IsValid)
      {
        ApplicationUser ApplicationUser = new ApplicationUser
        {
          UserName = user.Name,
          Email = user.Email
        };

        IdentityResult result = await userManager.CreateAsync(ApplicationUser, user.Password);

        if (result.Succeeded)
          return RedirectToAction("Index");
        else
        {
          foreach (IdentityError error in result.Errors)
            ModelState.AddModelError("", error.Description);
        }
      }
      return View(user);
    }
    public async Task<IActionResult> Update(string id)
    {
      ApplicationUser user = await userManager.FindByIdAsync(id);
      if (user != null)
        return View(user);
      else
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Update(string id, string email, string password)
    {
      ApplicationUser user = await userManager.FindByIdAsync(id);
      if (user != null)
      {
        if (!string.IsNullOrEmpty(email))
          user.Email = email;
        else
          ModelState.AddModelError("", "Email cannot be empty");

        if (!string.IsNullOrEmpty(password))
          user.PasswordHash = passwordHasher.HashPassword(user, password);
        else
          ModelState.AddModelError("", "Password cannot be empty");

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
          IdentityResult result = await userManager.UpdateAsync(user);
          if (result.Succeeded)
            return RedirectToAction("Index");
          else
            Errors(result);
        }
      }
      else
        ModelState.AddModelError("", "User Not Found");
      return View(user);
    }

    private void Errors(IdentityResult result)
    {
      foreach (IdentityError error in result.Errors)
        ModelState.AddModelError("", error.Description);
    }
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
      ApplicationUser user = await userManager.FindByIdAsync(id);
      if (user != null)
      {
        IdentityResult result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
          return RedirectToAction("Index");
        else
          Errors(result);
      }
      else
        ModelState.AddModelError("", "User Not Found");
      return View("Index", userManager.Users);
    }
  }
}