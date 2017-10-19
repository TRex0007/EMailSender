using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EMailSender.Email
{
    public class Email
    {
        public string Subject { get; set; }

        public string From { get; set; }

        public List<string> To { get; set; }

        public string MailText { get; set; }

        public string Password { get; set; }

        public Email()
        {
            From = "somerandomname123123@gmail.com";
            Password = "somerandompassword123123";
            To = new List<string>();
        }

        public Email(string subject, List<string> to, string mailtext, string from = "somerandomname123123@gmail.com")
        {
            this.Subject = subject;
            this.From = from;
            this.To = to;
            this.MailText = mailtext;
            this.Password = "somerandompassword123123";
        }
    }
}