﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okurdostu.Data;
using Okurdostu.Web.Base;
using Okurdostu.Web.Models;
using Okurdostu.Web.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Okurdostu.Web.Controllers.Api.Me
{
    [Route("api/me/educationdocuments")]
    public class EducationDocumentsController : SecureApiController
    {
        private readonly ILocalStorageService LocalStorage;
        private readonly IHostingEnvironment Environment;

        public EducationDocumentsController(IHostingEnvironment hostingEnvironment, ILocalStorageService localStorageService)
        {
            LocalStorage = localStorageService;
            Environment = hostingEnvironment;
        }

        [HttpGet("")]
        public ActionResult Index()
        {
            return NotFound();
        }

        [HttpPost("")] //api/me/educationdocuments
        public async Task<IActionResult> Post(Guid Id, IFormFile File)
        {
            ReturnModel rm = new ReturnModel();

            if (File != null && File.Length <= 10485767 / 2 && File.Length > 0)
            {
                if (File.ContentType != "application/pdf" && File.ContentType != "image/png" && File.ContentType != "image/jpg" && File.ContentType != "image/jpeg")
                {
                    rm.Code = 200;
                    rm.Message = "PDF, PNG, JPG veya JPEG türünde dosya yollayabilirsin";
                    return Error(rm);
                }
            }
            else if (File != null && File.Length > 10485767 / 2)
            {
                rm.Code = 200;
                rm.Message = "En fazla 500 kilobyte boyutunda dosya yollayabilirsin";
                return Error(rm);
            }
            else if (File != null && File.Length! > 0)
            {
                rm.Code = 200;
                rm.Message = "Yolladığınız dosya görüntülenemez";
                return Error(rm);
            }
            else // file is null
            {
                rm.Code = 200;
                rm.Message = "Dosya yollamadınız";
                return Error(rm);
            }

            var AuthenticatedUserId = Guid.Parse(User.Identity.GetUserId());

            var Education = await Context.UserEducation.FirstOrDefaultAsync(
               x => x.Id == Id
            && x.UserId == AuthenticatedUserId
            && !x.IsRemoved
            && !x.IsSentToConfirmation);

            if (Education != null)
            {
                string DocumentName = Guid.NewGuid().ToString() + Path.GetExtension(File.FileName);
                string FileFullPath = Environment.WebRootPath + "/documents/" + DocumentName;

                using (var Stream = System.IO.File.Create(FileFullPath))
                {
                    await File.CopyToAsync(Stream);
                };

                if (System.IO.File.Exists(FileFullPath))
                {
                    var EducationDocument = new UserEducationDoc
                    {
                        CreatedOn = DateTime.Now,
                        UserEducationId = Id,
                        FullPath = FileFullPath, // C:/application/wwwroot/documents/{guid}.extensiontype
                        PathAfterRoot = "/documents/" + DocumentName, // /documents/{guid}.extensiontype
                    };

                    Education.IsSentToConfirmation = true;

                    await Context.AddAsync(EducationDocument);

                    var result = await Context.SaveChangesAsync();
                    if (result > 0)
                    {
                        rm.Message = "Eğitim dökümanınız yollandı";
                        TempData["ProfileMessage"] = "Eğitim dökümanınız yollandı, en geç 48 saat içinde geri dönüş yapılacak";
                        return Succes(rm);
                    }
                    else
                    {
                        LocalStorage.DeleteIfExists(EducationDocument.FullPath);
                        return Error(rm);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Logger.LogCritical("A user is trying to send education document but file wasn't create on server");
                    }

                    rm.Code = 500;
                    return Error(rm);
                }
            }

            rm.Code = 404;
            return Error(rm);
        }
    }
}