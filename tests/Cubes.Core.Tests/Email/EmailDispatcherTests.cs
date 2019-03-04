using System;
using System.Collections.Generic;
using System.Net.Mail;
using Cubes.Core.Email;
using Moq;
using Xunit;

namespace Cubes.Core.Tests.Email
{
    public class EmailDispatcherTests : IDisposable
    {
        private MockRepository mockRepository;
        public EmailDispatcherTests() => mockRepository = new MockRepository(MockBehavior.Strict);
        public void Dispose() => mockRepository.VerifyAll();

        [Fact]
        public void When_MessageDispatched_SendIsCalledOnClient()
        {
            //Given
            var smtpClientMock = mockRepository.Create<ISmtpClient>();
            var sut = new EmailDispatcher(smtpClientMock.Object);
            var emailContent = new EmailContent
            {
                Body        = "This is a body",
                Subject     = "This is a subject",
                ToAddresses = new List<string> { "receiver@nowhere.com"}
            };
            var smtpSettings = new SmtpSettings
            {
                Host = "localhost",
                Port = 25,
                UseSsl = false,

            };
            smtpClientMock.SetupAllProperties();
            smtpClientMock.Setup(m => m.Send(It.IsAny<MailMessage>())).Verifiable();

            //When
            sut.DispatchEmail(emailContent, smtpSettings);

            //Then
            smtpClientMock.Verify(m => m.Send(It.IsAny<MailMessage>()), Times.Once());
        }
    }
}