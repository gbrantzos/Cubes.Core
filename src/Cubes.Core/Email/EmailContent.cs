using System.Collections.Generic;

namespace Cubes.Core.Email
{
    public class EmailContent
    {
        public string Subject { get; set; }
        public List<string> ToAddresses { get; set; }
        public string Body { get; set; }
        public IEnumerable<EmailAttachment> Attachments { get; set; }

        public class EmailAttachment
        {
            public string FileName { get; set; }
            public byte[] Content { get; set; }
            public string ContentType { get; set; }
        }
    }
}