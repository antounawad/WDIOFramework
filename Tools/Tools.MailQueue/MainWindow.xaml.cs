using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Eulg.Server.Common;

namespace Eulg.Tools.MailTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TextBoxSmtpServer.Text = "smtp.office365.com";
            TextBoxSmtpPort.Text = "587";
            TextBoxSmtpUsername.Text = "kontakt@eulg.de";
            TextBoxSmtpPassword.Password = "Fh3479asdl%sa";
            CheckBoxSsl.IsChecked = true;
            TextBoxFromMail.Text = "kontakt@eulg.de";
            TextBoxFromName.Text = "EULG - Entgeltumwandlung leicht gemacht";

            //TextBoxMailTo.Text = "gnulpus@gmail.com";
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            var message = new EulgMailMessage(true) { Subject = TextBoxSubject.Text.Trim() };

            // Ersetze Grußformel, Signatur und Disclaimer
            var greeting = Mailer.GetMailContent(message.MailGreeting);
            var signature = Mailer.GetMailContent(message.MailSignature);

            var footerCustom = (message.NewsletterLogout != null) ? message.NewsletterLogout.GetMailBody() : string.Empty;
            footerCustom = footerCustom.Replace("~/", Settings.Current?.Urls?.UrlVerwaltung ?? "" + "/");
            var footerHeadOffice = Mailer.GetMailContent(message.FooterHeadOffice);
            var footerDisclaimer = Mailer.GetMailContent(message.FooterDisclaimer);

            var mailLayout = Mailer.GetMailLayout();
            mailLayout = mailLayout.Replace("[@Body]", TextBoxContent.Text);
            mailLayout = mailLayout.Replace("[@Greeting]", greeting);
            mailLayout = mailLayout.Replace("[@Signature]", signature);

            mailLayout = mailLayout.Replace("[@Footer]", footerCustom);
            mailLayout = mailLayout.Replace("[@FooterHeadOffice]", footerHeadOffice);
            mailLayout = mailLayout.Replace("[@FooterDisclaimer]", footerDisclaimer);

            message.Body = PreMailer.Net.PreMailer.MoveCssInline(mailLayout, false, null, Mailer.GetMailResource("EulgMailStyles", ".css"), false, true).Html;

            var fromName = TextBoxFromName.Text.Trim();
            var fromMail = TextBoxFromMail.Text.Trim();

            var smtpClient = new SmtpClient(TextBoxSmtpServer.Text, int.Parse(TextBoxSmtpPort.Text))
            {
                Credentials = new NetworkCredential(TextBoxSmtpUsername.Text, TextBoxSmtpPassword.Password),
                EnableSsl = CheckBoxSsl.IsChecked.GetValueOrDefault(false)
            };

            var mailToList = TextBoxMailTo.Text.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            message.From = new MailQueueAddress(fromMail, fromName);

            ProgressBar.Maximum = mailToList.Length;

            ExpanderLog.IsExpanded = true;
            TextBoxLog.Text = string.Empty;

            var t = new Task(() => DoIt(smtpClient, mailToList, message));
            t.Start();
        }

        private void DoIt(SmtpClient smtpClient, IEnumerable<string> mailToList, EulgMailMessage message)
        {
            var i = 0;
            foreach (var mailTo in mailToList)
            {
                i++;
                Dispatcher.Invoke(() =>
                {
                    ProgressBar.Value = i;
                    LabelStatus.Content = mailTo;
                });
                var log = mailTo;
                try
                {
                    message.To.Clear();
                    message.To.Add(new MailQueueAddress(mailTo));
                    smtpClient.Send(message.ToMailMessage());
                    log += " OK.";
                }
                catch (Exception exception)
                {
                    log += " ERROR: " + exception.Message;
                }
                log += Environment.NewLine;
                Dispatcher.Invoke(() => { TextBoxLog.Text += log; });
                Thread.Sleep(250);
            }
        }

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            var message = new EulgMailMessage(true) { Subject = TextBoxSubject.Text.Trim() };

            // Ersetze Grußformel, Signatur und Disclaimer
            var greeting = Mailer.GetMailContent(message.MailGreeting);
            var signature = Mailer.GetMailContent(message.MailSignature);

            var footerCustom = (message.NewsletterLogout != null) ? message.NewsletterLogout.GetMailBody() : string.Empty;
            footerCustom = footerCustom.Replace("~/", Settings.Current?.Urls?.UrlVerwaltung ?? "" + "/");
            var footerHeadOffice = Mailer.GetMailContent(message.FooterHeadOffice);
            var footerDisclaimer = Mailer.GetMailContent(message.FooterDisclaimer);

            var mailLayout = Mailer.GetMailLayout();
            mailLayout = mailLayout.Replace("[@Body]", TextBoxContent.Text);
            mailLayout = mailLayout.Replace("[@Greeting]", greeting);
            mailLayout = mailLayout.Replace("[@Signature]", signature);

            mailLayout = mailLayout.Replace("[@Footer]", footerCustom);
            mailLayout = mailLayout.Replace("[@FooterHeadOffice]", footerHeadOffice);
            mailLayout = mailLayout.Replace("[@FooterDisclaimer]", footerDisclaimer);

            message.Body = PreMailer.Net.PreMailer.MoveCssInline(mailLayout, false, null, Mailer.GetMailResource("EulgMailStyles", ".css"), false, true).Html;

            TabItemHtml.IsSelected = true;
            Preview.NavigateToString(message.Body);

        }

    }
}
