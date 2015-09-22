using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Eulg.Tools.MailTool
{
    public class MailQueueAddress
    {
        public string Address { get; set; }
        public string DisplayName { get; set; }

        public MailQueueAddress()
            : this(null, null)
        {
        }

        public MailQueueAddress(string address)
            : this(address, null)
        {
        }

        public MailQueueAddress(string address, string displayName)
        {
            Address = address;
            DisplayName = displayName;
        }

        #region Konverter

        public MailQueueAddress(MailAddress mailAddress)
        {
            Address = mailAddress.Address;
            DisplayName = mailAddress.DisplayName;
        }

        public MailAddress ToMailAddress()
        {
            return new MailAddress(Address, DisplayName);
        }

        #endregion
    }

    public class MailQueueAttachment
    {
        public ContentType ContentType { get; private set; }
        public byte[] ContentBytes { get; set; }

        public MailQueueAttachment()
        {
            ContentType = new ContentType();
        }

        public MailQueueAttachment(string fileName)
            : this()
        {
            ContentType.Name = Path.GetFileName(fileName);
            ContentBytes = File.ReadAllBytes(fileName);
        }

        public MailQueueAttachment(string fileName, string mediaType)
            : this(fileName)
        {
            ContentType.MediaType = mediaType;
        }

        public MailQueueAttachment(string fileName, ContentType contentType)
            : this(fileName)
        {
            ContentType = contentType;
        }

        private MailQueueAttachment(Stream contentStream)
            : this()
        {
            using (var ms = new MemoryStream())
            {
                contentStream.CopyTo(ms);
                ContentBytes = ms.ToArray();
            }
        }

        public MailQueueAttachment(Stream contentStream, string name)
            : this(contentStream)
        {
            ContentType.Name = name;
        }

        public MailQueueAttachment(Stream contentStream, string name, string mediaType)
            : this(contentStream, name)
        {
            ContentType.MediaType = mediaType;
        }

        public MailQueueAttachment(Stream contentStream, ContentType contentType)
            : this(contentStream)
        {
            ContentType = contentType;
        }

        private MailQueueAttachment(byte[] contentBytes)
        {
            ContentBytes = contentBytes;
        }

        public MailQueueAttachment(byte[] contentBytes, string name)
            : this(contentBytes)
        {
            ContentType.Name = name;
        }

        public MailQueueAttachment(byte[] contentBytes, string name, string mediaType)
            : this(contentBytes, name)
        {
            ContentType = new ContentType {MediaType = mediaType};
        }

        public MailQueueAttachment(byte[] contentBytes, ContentType contentType)
            : this(contentBytes)
        {
            ContentType = contentType;
        }

        #region Konverter

        public MailQueueAttachment(AttachmentBase attachment)
            : this(attachment.ContentStream, attachment.ContentType)
        {
        }

        public Attachment ToAttachment()
        {
            return new Attachment(new MemoryStream(ContentBytes), ContentType);
        }

        #endregion
    }

    public class MailQueueMessage
    {
        public MailQueueAddress From { get; set; }
        public Collection<MailQueueAddress> To { get; private set; }
        // ReSharper disable once InconsistentNaming
        public Collection<MailQueueAddress> CC { get; private set; }
        public Collection<MailQueueAddress> Bcc { get; private set; }
        public string Subject { get; set; }
        public bool IsBodyHtml { get; set; }
        public string Body { get; set; }
        public Collection<MailQueueAddress> ReplyToList { get; private set; }
        public MailPriority Priority { get; set; }
        public DeliveryNotificationOptions DeliveryNotificationOptions { get; set; }

        [JsonIgnore]
        public Collection<MailQueueAttachment> Attachments { get; private set; }

        public MailQueueMessage()
        {
            To = new Collection<MailQueueAddress>();
            CC = new Collection<MailQueueAddress>();
            Bcc = new Collection<MailQueueAddress>();
            ReplyToList = new Collection<MailQueueAddress>();
            Attachments = new Collection<MailQueueAttachment>();
        }

        public MailQueueMessage(string from, string to)
            : this()
        {
            From = new MailQueueAddress(from);
            To.Add(new MailQueueAddress(to));
        }

        public MailQueueMessage(MailQueueAddress from, MailQueueAddress to)
            : this()
        {
            From = from;
            To.Add(to);
        }

        public string GetWithoutAttachmentsJson()
        {
            //var tempAttachments = new Collection<MailQueueAttachment>();
            //foreach (var a in Attachments)
            //{
            //    tempAttachments.Add(a);
            //}
            //Attachments.Clear();
            //try
            //{
            using (var s = new StringWriter())
            {
                using (var writer = new JsonTextWriter(s))
                {
                    var serializer = new JsonSerializer
                                     {
                                         NullValueHandling = NullValueHandling.Ignore,
                                         DefaultValueHandling = DefaultValueHandling.Ignore,
                                         Formatting = Formatting.None
                                     };
                    serializer.Serialize(writer, this);
                    return s.ToString();
                }
            }
            //}
            //finally
            //{
            //    foreach (var a in tempAttachments)
            //    {
            //        Attachments.Add(a);
            //    }
            //}
        }

        public byte[] GetAttachmentsBson()
        {
            if (Attachments.Count == 0)
            {
                return null;
            }
            using (var ms = new MemoryStream())
            {
                using (var writer = new BsonWriter(ms))
                {
                    var serializer = new JsonSerializer
                                     {
                                         NullValueHandling = NullValueHandling.Ignore,
                                         DefaultValueHandling = DefaultValueHandling.Ignore,
                                         Formatting = Formatting.None
                                     };
                    serializer.Serialize(writer, Attachments);
                    return ms.ToArray();
                }
            }
        }

        #region Konverter

        public MailQueueMessage(MailMessage mailMessage)
        {
            From = new MailQueueAddress(mailMessage.From);
            Subject = mailMessage.Subject;
            IsBodyHtml = mailMessage.IsBodyHtml;
            Body = mailMessage.Body;
            Priority = mailMessage.Priority;
            DeliveryNotificationOptions = mailMessage.DeliveryNotificationOptions;
        }

        public MailMessage ToMailMessage()
        {
            var m = new MailMessage
                    {
                        From = From.ToMailAddress(),
                        Subject = Subject,
                        IsBodyHtml = IsBodyHtml,
                        Body = Body,
                        Priority = Priority,
                        DeliveryNotificationOptions = DeliveryNotificationOptions
                    };
            foreach (var adr in To)
            {
                m.To.Add(adr.ToMailAddress());
            }
            foreach (var adr in CC)
            {
                m.CC.Add(adr.ToMailAddress());
            }
            foreach (var adr in Bcc)
            {
                m.Bcc.Add(adr.ToMailAddress());
            }
            foreach (var adr in ReplyToList)
            {
                m.ReplyToList.Add(adr.ToMailAddress());
            }
            foreach (var att in Attachments)
            {
                m.Attachments.Add(att.ToAttachment());
            }
            return m;
        }

        #endregion
    }
}
