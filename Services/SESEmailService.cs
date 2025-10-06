using Amazon;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using KiCData.Models.WebModels;
using Microsoft.Extensions.Configuration;

namespace KiCData.Services
{
    public class SESEmailService : IEmailService
    {
        private readonly IConfigurationRoot _config;
        private readonly IKiCLogger _logger;
        private readonly AmazonSimpleEmailServiceV2Client _client;

        public SESEmailService(IConfigurationRoot config, IKiCLogger logger)
        {
            this._config = config;
            this._logger = logger;
            var awsRegion = RegionEndpoint.GetBySystemName(config["AWS:Region"]);
            this._client = new AmazonSimpleEmailServiceV2Client(config["AWS:AccessKey"], config["AWS:SecretKey"], awsRegion);
        }

        /// <summary>
        /// Creates a form message with the necessary details to be serialized into an email.
        /// </summary>
        /// <param name="rep">The member of staff to whom the email should be sent.</param>
        /// <returns>FormMessage</returns>
        public FormMessage FormSubmissionEmailFactory(string rep)
        {
            FormMessage message = new FormMessage();

            message.To.Add(_config["Email Addresses:" + rep]);
            message.Cc.Add(_config["Email Addresses:Admin"]);
            message.Subject = "Web Form Submission | " + rep + " | " + DateTime.Now.ToString();
            message.From = _config["Email Addresses:From"];


            return message;
        }

        /// <summary>
        /// Sends the given message as an email.
        /// </summary>
        /// <param name="message">The FormMessage to be sent.</param>
        /// <returns>Task</returns>
        /// <exception cref="Exception"></exception>
        public void SendEmail(FormMessage message)
        {
            if(message.Html is null && message.HtmlBuilder is null)
            {
                throw new Exception("Empty FormMessage");
            }

            if (message.Html is null)
            {
                message.BuildHtml();
            }

            _logger.LogText("Sending email. ");
            
            // message fields can be null, so coalesce to an empty list if they are null
            List<string> toAddresses = message.To ?? [];
            List<string> ccAddresses = message.Cc ?? [];
            List<string> bccAddresses = message.Bcc ?? [];

            var dest = new Destination
            {
                BccAddresses = bccAddresses,
                CcAddresses = ccAddresses,
                ToAddresses = toAddresses
            };

            Content? htmlContent = null;
            if (message.Html != null)
            {
                htmlContent = new Content
                {
                    Charset = "UTF-8",
                    Data = message.Html
                };
            }
            Content? plaintextContent = null;
            if (message.Text != null)
            {
                plaintextContent = new Content
                {
                    Charset = "UTF-8",
                    Data = message.Text
                };
            }

            var content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content
                    {
                        Charset = "UTF-8",
                        Data = message.Subject
                    },
                    Body = new Body
                    {
                        Html = htmlContent,
                        Text = plaintextContent
                    }
                }
            };
            var req = new SendEmailRequest
            {
                Destination = dest,
                Content = content,
                FromEmailAddress = message.From
            };
            try
            {
                _client.SendEmailAsync(req, CancellationToken.None).Wait();
                _logger.LogText("Message sent...");
            }
            catch (AmazonSimpleEmailServiceV2Exception e)
            {
                _logger.LogText("An error occured while sending email. See the exception below for details.");
                _logger.LogText(e.Message);
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
