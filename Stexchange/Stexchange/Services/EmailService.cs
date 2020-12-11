using MimeKit;
using MimeKit.Text;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using MailKit.Net.Smtp;
using System; 

namespace Stexchange.Services
{
	/// <summary>
	/// Manages the sending of emails on a background thread. This class cannot be inherited.
	/// </summary>
	public sealed class EmailService : IDisposable
	{
		/// <summary>
		/// Thread-safe blocking queue from which the background worker thread reads.
		/// </summary>
		private readonly BlockingCollection<MimeMessage> queue = new BlockingCollection<MimeMessage>();

		/// <summary>
		/// Background worker thread that handles the email messages fron <see cref="queue"/>.
		/// </summary>
		private Thread workerThread;

		/// <summary>
		/// Gets the email configuration section of this application.
		/// </summary>
		private IConfiguration Config { get; }

		/// <summary>
		/// Gets the logger for this service.
		/// </summary>
		private ILogger Log { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="EmailService"/>.
		/// </summary>
		/// <param name="config">The configuration instance of this application.</param>
		/// <param name="logger">A new ILogger instance for this service.</param>
		public EmailService(IConfiguration config, ILogger<EmailService> logger)
		{
			Config = config.GetSection("MailSettings");
			Log = logger;

			// Create and start a new thread that runs the main task
			workerThread = new Thread(() =>
			{
				var task = Run();
				task.Wait();
			})
			{ Name = "EmailSender" };
			workerThread.Start();
		}

		/// <summary>
		/// Main loop for the background thread. 
		/// </summary>
		private async Task Run()
		{
			MimeMessage message;
			// Loop when the queue is not yet completed
			while (!queue.IsCompleted)
			{
				try
				{
					// Wait for a new item in the queue (may throw an exception)
					message = queue.Take();
				}
				catch (Exception e) when (e is ObjectDisposedException || e is InvalidOperationException || e is OperationCanceledException)
				{
					// Do nothing (queue is done)
					break;
				}

				Log.LogTrace($"Processing email message {message.Subject}");

				// send email
				using var smtp = new SmtpClient();
				await smtp.ConnectAsync(Config["Host"], Config.GetValue<int>("Port"), SecureSocketOptions.StartTls);
				await smtp.AuthenticateAsync(Config["Email"], Config["Password"]);
				await smtp.SendAsync(message);
				await smtp.DisconnectAsync(true);

				Log.LogTrace($"Send email message {message.Subject}");
			}
			Log.LogTrace($"Exiting thread {workerThread.Name}");
		}

		/// <summary>
        /// Queues a new email message for this service.
        /// </summary>
        /// <param name="address">The email to send a message to.</param>
        /// <param name="body">The contents of the email.</param>
		public void QueueMessage(string address, string body)
		{
			// Create email message
			var email = new MimeMessage();
			email.From.Add(new MailboxAddress("StexChange", Config["Email"]));
			email.To.Add(MailboxAddress.Parse(address));
			email.Subject = "E-mail Verificatie";
			email.Body = new TextPart(TextFormat.Plain) { Text = body };

			// Place the email in the queue so that it can be handled in a background thread.
			Log.LogTrace($"Queuing message for address {address}");
			queue.Add(email);
		}

		public void Dispose()
		{
			if (workerThread != null)
			{
				// Finish the queue and wait for the background thread to exit
				queue.CompleteAdding();
				workerThread.Join();
				workerThread = null;
			}
			queue.Dispose();
		}
	}
}