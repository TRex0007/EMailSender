using EMailSender.Email;
using EMailSender.Email;
using EMailSender.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EMailSender.Controllers
{
    public class HomeController : Controller
    {
        private EMailSenderEntities db = new EMailSenderEntities();

        public ActionResult Index()
        {
            return View(db.Tasks.ToList());
        }

        public async System.Threading.Tasks.Task<ActionResult> SendEmailsAsync(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Task task = db.Tasks.Find(id);
            if (task == null)
            {
                return HttpNotFound();
            }
            
            MailSender sender = new MailSender();

            var to = task.Users.Select(i => i.EMailAddress).ToList();
            var subject = "";
            if (!string.IsNullOrEmpty(task.Mail))
            {
                subject = task.Mail.Substring(0, task.Mail.Length > 100 ? 100 : task.Mail.Length);
            }

            Email.Email email = new Email.Email(subject, to, task.Mail);

            await sender.SendEmails(email, task);

            task.Status = "running";
            db.Entry(task).State = EntityState.Modified;
            db.SaveChanges();

            return View("Index", db.Tasks.ToList());
        }
    }
}