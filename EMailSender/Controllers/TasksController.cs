using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EMailSender.Models;
using EMailSender.Email;

namespace EMailSender.Controllers
{
    public class TasksController : Controller
    {
        private EMailSenderEntities db = new EMailSenderEntities();
        public List<string> GroupOfRecipients { get; set; }
        public List<string> GroupOfRecipientsSelected { get; set; }

        public TasksController()
        {
            GroupOfRecipientsSelected = new List<string>();
            GroupOfRecipients = new List<string>();
            foreach (var user in db.Users)
            {
                if (!GroupOfRecipients.Contains(user.GroupOfRecipients))
                    GroupOfRecipients.Add(user.GroupOfRecipients);
            }

            ViewBag.GroupOfRecipients = GroupOfRecipients;
        }

        public ActionResult Index()
        {
            return View(db.Tasks.ToList());
        }


        public ActionResult Details(int? id)
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
            return View(task);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async System.Threading.Tasks.Task<ActionResult> Create([Bind(Include = "TaskId,Mail,QuantityOfUsers,QuantityOfSentMails")] Task task, string action, IEnumerable<string> GroupOfRecipients)
        {

            if (ModelState.IsValid)
            {
                var users = db.Users;

                foreach (var group in GroupOfRecipients)
                {
                    task.SelectedGroupsOfUsers.Add(new SelectedGroupsOfUsers { Name = group, TasksId = task.TaskId });
                }

                foreach (var user in users)
                {
                    foreach (var group in GroupOfRecipients)
                    {
                        if (user.GroupOfRecipients == group)
                            task.Users.Add(user);
                    }
                }

                if (action == "Save")
                {
                    task.QuantityOfUsers = task.Users.Count;
                    task.Status = "stopped";
                    db.Tasks.Add(task);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                if (action == "Send")
                {
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

                    task.QuantityOfUsers = task.Users.Count;
                    task.Status = "running";
                    db.Tasks.Add(task);
                    db.SaveChanges();

                    await sender.SendEmails(email, task);

                    return RedirectToAction("Index");
                }
            }

            return View(task);
        }

        public ActionResult Edit(int? id)
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
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> Edit([Bind(Include = "TaskId,Mail,QuantityOfUsers,QuantityOfSentMails")] Task task, string action, IEnumerable<string> GroupOfRecipients)
        {
            if (ModelState.IsValid)
            {
                var users = db.Users;

                task.SelectedGroupsOfUsers.Clear();
                foreach (var group in GroupOfRecipients)
                {
                    task.SelectedGroupsOfUsers.Add(new SelectedGroupsOfUsers { Name = group, TasksId = task.TaskId });
                }

                foreach (var user in users)
                {
                    foreach (var group in GroupOfRecipients)
                    {
                        if (user.GroupOfRecipients == group)
                            task.Users.Add(user);
                    }
                }

                if (action != null && action == "Save")
                {
                    task.Status = "stopped";
                    db.Entry(task).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                if (action == "Send")
                {
                    if (task == null)
                    {
                        return HttpNotFound();
                    }

                    MailSender sender = new MailSender();

                    var to = task.Users.Select(i => i.EMailAddress).ToList();
                    var subject = "";
                    if (!string.IsNullOrEmpty(task.Mail))
                    {
                        subject = task.Mail.Substring(0, task.Mail.Length > 100 ? 100 : task.Mail.Length - 1);
                    }

                    Email.Email email = new Email.Email(subject, to, task.Mail);

                    task.QuantityOfUsers = task.Users.Count;
                    task.Status = "running";
                    db.Entry(task).State = EntityState.Modified;
                    db.SaveChanges();

                    await sender.SendEmails(email, task);

                    return RedirectToAction("Index");
                }
            }
            return View(task);
        }

        public ActionResult Delete(int? id)
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
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Task task = db.Tasks.Find(id);
            db.Tasks.Remove(task);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
