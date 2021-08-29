using BookShop.DataAccess.Repositories.IRepositories;
using BookShop.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace BookShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id==null)
            {
                return View(coverType);
            }
            else
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Id", id);
                coverType = _unitOfWork.SP_Call.OneRecord<CoverType>(StaticDetails.Proc_CoverType_Get, parameter);
                return View(coverType);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Name", coverType.Name);
                if (coverType.Id != 0)
                {
                    parameter.Add("@Id", coverType.Id);
                    _unitOfWork.SP_Call.Execute(StaticDetails.Proc_CoverType_Update, parameter);
                }
                else
                {
                    _unitOfWork.SP_Call.Execute(StaticDetails.Proc_CoverType_Create, parameter);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(coverType);
            }
        }

        #region API Call
        [HttpGet]
        public IActionResult GetAll()
        {
            var value = _unitOfWork.SP_Call.List<CoverType>(StaticDetails.Proc_CoverType_GetAll);
            return Json(new { data = value });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var param = new DynamicParameters();
            param.Add("@Id", id);
            _unitOfWork.SP_Call.Execute(StaticDetails.Proc_CoverType_Delete, param);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete successfull!" });
        }
        #endregion
    }
}

