using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Common.TextMessagingHelpers
{
    public class TextMessagingHelpers
    {
        public static string SendSMSMessage(string phoneNum, string messageBody)
        {
            return "Implement later";
        //     //validate input - make sure they passed a value for phoneNum
        //     if(!Validators.Validators.ValidateStringParameters(new List<string>(){phoneNum}))
        //     {
        //         return Constants.Constants.PhoneNumberValidationMessage;
        //     }

        //     // //Todo: store these in secrets manager or environment variables
        //     string accountSid = "AC204481289afbc1ff5711342416e181fb";
        //     string authToken = "65adf9957aef2cc4e88598d80dcbf70c";

        //     TwilioClient.Init(accountSid, authToken);

        //     //send the message
        //     var message = MessageResource.Create(
        //         body: messageBody,
        //         from: new Twilio.Types.PhoneNumber(Constants.Constants.TwilioPhoneNumber),
        //         to: new Twilio.Types.PhoneNumber(phoneNum)
        //     );
        //     // if(!String.IsNullOrWhiteSpace(text_message))
        //     // {
        //         return "Success! Message sent: " + message;
        //     //}
        //     // else
        //     // {
        //     //     return Constants.Constants.UserNotTextedMessage;
        //     // }
        }
    }
}