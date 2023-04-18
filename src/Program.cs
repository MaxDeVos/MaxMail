using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MaxMail.Models;

var personalGmail = new ImapClient ();
personalGmail.Connect("imap.gmail.com", 993, true);
personalGmail.Authenticate("devosmaxwell@gmail.com", Environment.GetEnvironmentVariable("gmail_app_password"));

// var gmailAllMail = personalGmail.GetFolder("RealPeople");
// gmailAllMail.Open(FolderAccess.ReadWrite);

var gmailRealPeople = personalGmail.GetFolder("RealPeople");
gmailRealPeople.Open(FolderAccess.ReadWrite);

var maxdevosEmail = new ImapClient ();
maxdevosEmail.Connect("imap.mailfence.com", 993, true);
maxdevosEmail.Authenticate("maxdevos@mailfence.com", Environment.GetEnvironmentVariable("maxdevos_email_app_password"));

foreach (var uid in gmailRealPeople.Search(SearchQuery.All))
{

    var rawMessage = gmailRealPeople.GetMessage(uid);
    if (rawMessage == null)
        continue;

    var message = new Message(rawMessage);

    if (message.Contains("Unsubscribe") || message.Contains("unsubscribe") || message.Contains("email preferences"))
    {
        Console.WriteLine($"Found Unsubscribe Email: {message.SanitizedSubject(maxLength: 100), 100}");
        gmailRealPeople.AddFlags(uid, MessageFlags.Flagged, true);
    }

}

maxdevosEmail.Disconnect(true);
personalGmail.Disconnect(true);