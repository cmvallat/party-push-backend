using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Common.TextMessagingHelpers
{
    public class TextMessagingHelpers
    {
        public static string SendSMSMessage(string phoneNum, string messageBody)
        {
            return "Implement later";
            //validate input - make sure they passed a value for phoneNum
            // if(!Validators.Validators.ValidateStringParameters(new List<string>(){phoneNum}))
            // {
            //     return Constants.Constants.PhoneNumberValidationMessage;
            // }

            // //Todo: store these in secrets manager or environment variables
            // string accountSid = "AC204481289afbc1ff5711342416e181fb";
            // string authToken = "d4d59b8880574c388a79e2cece63e005";

            // TwilioClient.Init(accountSid, authToken);

            // var messageOptions = new CreateMessageOptions(new Twilio.Types.PhoneNumber(phoneNum));

            // messageOptions.From = new Twilio.Types.PhoneNumber(Constants.Constants.TwilioPhoneNumber);
            // messageOptions.Body = messageBody;

            // //send the message
            // var text_message = MessageResource.Create(messageOptions);

            // if(!String.IsNullOrWhiteSpace(text_message))
            // {
            //     return "Success! Message sent: " + text_message;
            // }
            // else
            // {
            //     return Constants.Constants.UserNotTextedMessage;
            // }
        }
    }
}