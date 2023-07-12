using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Common.TextMessagingHelpers
{
    public class TextMessagingHelpers
    {
        public static string SendSMSMessage(string phoneNum, string messageBody)
        {
            //validate input - make sure they passed a value for phoneNum
            if(String.IsNullOrWhiteSpace(phoneNum))
            {
                return "Failed; Phone Number was invalid";
            }

            //Todo: store these in secrets manager or environment variables
            string accountSid = "AC204481289afbc1ff5711342416e181fb";
            string authToken = "d4d59b8880574c388a79e2cece63e005";

            TwilioClient.Init(accountSid, authToken);

            var messageOptions = new CreateMessageOptions(
                 new Twilio.Types.PhoneNumber(phoneNum));
                messageOptions.From = new Twilio.Types.PhoneNumber("+18449453925");
                messageOptions.Body = messageBody;

            var text_message = MessageResource.Create(messageOptions);

            //Todo: don't hardcode return success or failure
            return "Success! Message sent: " + text_message;
        }
    }
}