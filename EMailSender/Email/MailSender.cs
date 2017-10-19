using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.ComponentModel;
using System.Text;
using System.Data.Entity;
using EMailSender.Models;

namespace EMailSender.Email
{
    public class MailSender
    {
        private EMailSender.Models.Task task;
        private EMailSenderEntities db = new EMailSenderEntities();

        public static void Send(Email email)
        {
            if (email.To == null)
                throw new ArgumentException("recipients");

            var client = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(email.From, email.Password)
            };

            foreach (var receiver in email.To)
            {

                using (var msg = new System.Net.Mail.MailMessage(email.From, receiver, email.Subject, email.MailText))
                {

                    msg.To.Add(receiver);

                    try
                    {
                        client.Send(msg);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Message wasnt sent");
                    }
                }
            }


        }

        public async System.Threading.Tasks.Task SendEmails(Email email, EMailSender.Models.Task task)
        {
            this.task = task;
            using (MailMessage mail = new MailMessage())
            {
                foreach (var to in email.To)
                {
                    mail.To.Add(new MailAddress(to));
                }
                UTF8Encoding utf8 = new UTF8Encoding();
                mail.SubjectEncoding = utf8;
                mail.Subject = email.Subject ?? "";
                mail.Body = email.MailText;
                mail.From = new MailAddress(email.From);


                using (SmtpClient smtp = new SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(email.From, email.Password)
                })
                {
                    smtp.SendCompleted += new SendCompletedEventHandler(SmtpClient_SendCompleted);
                    await smtp.SendMailAsync(mail);
                }
            }
        }

        private void SmtpClient_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // Get the message we sent
            //MailMessage msg = (MailMessage)e.UserState;

            if (e.Cancelled)
            {
                // prompt user with "send cancelled" message 
            }
            if (e.Error != null)
            {
                // prompt user with error message 
            }
            else
            {
                //this.task.QuantityOfSentMails = this.task.QuantityOfSentMails != null ? this.task.QuantityOfSentMails++ : 0;
                //db.Entry(task).State = EntityState.Modified;
                //db.SaveChanges();
                // prompt user with message sent!
                // as we have the message object we can also display who the message
                // was sent to etc 
            }

            // finally dispose of the message
            //if (msg != null)
            //    msg.Dispose();
        }
    }
    
}