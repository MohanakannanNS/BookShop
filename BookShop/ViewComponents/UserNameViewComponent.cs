using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.ViewComponents
{
    public class UserNameViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext db;

        public UserNameViewComponent(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var user = db.ApplicationUsers.FirstOrDefault(i => i.Id == claim.Value);
            return View(user);
            
        }

    }
}
