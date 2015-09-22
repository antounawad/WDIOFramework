using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Eulg.Tools.MailTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TextBoxSmtpServer.Text = "ks-software.de";
            TextBoxSmtpPort.Text = "25";
            TextBoxSmtpUsername.Text = "klaus.seiler@eulg.de";
            TextBoxSmtpPassword.Text = "a8Qw43mx";
            CheckBoxSsl.IsChecked = false;
            TextBoxFromMail.Text = "klaus.seiler@eulg.de";
            TextBoxFromName.Text = "Klaus Seiler";
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            var fromName = TextBoxFromName.Text.Trim();
            var fromMail = TextBoxFromMail.Text.Trim();

            var subject = TextBoxSubject.Text.Trim();
            var content = TextBoxContent.Text;

            var smtpClient = new SmtpClient(TextBoxSmtpServer.Text, Int32.Parse(TextBoxSmtpPort.Text))
                             {
                                 Credentials = new NetworkCredential(TextBoxSmtpUsername.Text, TextBoxSmtpPassword.Text),
                                 EnableSsl = CheckBoxSsl.IsChecked.GetValueOrDefault(false)
                             };

            var mailToList = TextBoxMailTo.Text.Split(new[] {Environment.NewLine, "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);

            var from = new MailAddress(fromMail, fromName);

            ProgressBar.Maximum = mailToList.Length;

            ExpanderLog.IsExpanded = true;
            TextBoxLog.Text = String.Empty;

            var t = new Task(() => DoIt(smtpClient, mailToList, from, subject, content));
            t.Start();
        }

        private void DoIt(SmtpClient smtpClient, IEnumerable<string> mailToList, MailAddress from, string subject, string content)
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
                    var to = new MailAddress(mailTo);
                    var mailMessage = new MailMessage(from, to)
                                      {
                                          Subject = subject,
                                          IsBodyHtml = true,
                                          Body = content
                                      };
                    smtpClient.Send(mailMessage);
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

        private void ButtonTest_OnClick(object sender, RoutedEventArgs e)
        {
            var from = new MailQueueAddress("gnulp@eulg.de", "From Name");
            var to1 = new MailQueueAddress("gnulp@eulg.de", "To Name 1");
            var att1 = new MailQueueAttachment(@"C:\Users\holger.mueller\Downloads\logo-eulg.png", MediaTypeNames.Application.Octet);
            var att2 = new MailQueueAttachment(@"C:\Users\holger.mueller\Downloads\ks-logo.png", MediaTypeNames.Application.Octet);

            var msg = new MailQueueMessage
                      {
                          From = @from,
                          Subject = "Betreff öäüÖÄÜßâéù",
                          IsBodyHtml = true,
                          Body = "<strong>Body öäüÖÄÜßâúè</strong>"
                      };

            msg.To.Add(to1);
            msg.Attachments.Add(att1);
            msg.Attachments.Add(att2);

            Console.WriteLine("==============================");
            Console.WriteLine(msg.GetWithoutAttachmentsJson());
            Console.WriteLine("==============================");
            Console.WriteLine(BitConverter.ToString(msg.GetAttachmentsBson(), 0, 100));

            var smtpClient = new SmtpClient("mail.eulg.de", 25)
                             {
                                 Credentials = new NetworkCredential("gnulp@eulg.de", "leeloo"),
                                 EnableSsl = false
                             };

            smtpClient.Send(msg.ToMailMessage());
        }
    }
}
