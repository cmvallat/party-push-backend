namespace Common.Constants
{
    public class Constants
    {
        public const string TwilioPhoneNumber = "+18449453925";
        public const string UserNotTextedMessage = "Host created but user was not texted";
        //Status Code Messages (returned by PartyController endpoints)
        public const string SuccessMessage = "Success!";
        public const string FailedToInsertMessage = "Failed to insert Object into database";
        public const string UserNotValidatedMessage = "User could not be validated";
        public const string ParameterValidationMessage = "One or more parameters was missing";
        public const string PhoneNumberValidationMessage = "Failed; Phone Number was invalid";

        //Exception Messages (thrown in PartyService)
        public const string DuplicateEntryMessage = "SQL Exception 1062: duplicate entry. Could not create object.";
        public const string GenericSqlExceptionMessage = "SQL Exception: Something went wrong";
        public const string GenericDatabaseErrorMessage = "General Database Exception: Something went wrong";
        public const string GenericSystemExceptionMessage = "General System Exception in PartyService";

        //Specific exception messages

        public const string FailedToGetHost = "Failed to get Host from database";



        public const string OpenInviteMessage = "Your party is open invite, no need to add guests! Just give them your party_code and tell them to check-in";
        public const string InvalidInviteOnlyValueMessage = "Something went wrong with the invite_only parameter not being either 0 or 1.";
        public const string CouldntFindHostMessage = "Couldn't find a party with this party code.";
        public const string GuestAlreadyAtPartyCheckInMessage = "There is already someone at this party with this name. Please try checking in again with a new name.";
        public const string NotInvitedMessage = "Oops, this is awkward! Doesn't seem like you're on the invite list. Please check that you spelled your name and the party code right, or try joining another party.";
        public const string OkToUpdateGuestMessage = "Ok to update guest with at_party = 1";
        public const string HostNotValidatedMessage = "The user trying to add this guest to the party is not the party host";
        public const string HostDoesntExistForGuestMessage = "Could not get guest list, as that party does not exist";
        public const string HostDoesntExistForFoodMessage = "Could not update/report food for this party, as party does not exist in database";
        public const string CouldntGetGuestListMessage = "Something went wrong, failed to get Guest list from database";
        public const string FailedToLeavePartyMessage = "Failed to remove guest from party in database";
        public const string MismatchedPartyToUserMessage = "This is not your party to manage";
        public const string FailedToGetPartyObjectsMessage = "Failed to get Host and Guest objects from db";
        public const string FailedToGetPartyInfoMessage = "Failed to get Guest and Food objects from db for this party";
        public const string FailedToEndPartyMessage = "Failed to get delete party from db";
        public const string InvalidFoodStatusMessage = "Status was invalid b/c it was not 'full', 'low', or 'out' ";
        //Todo: make error messages prettier to return for FE
        public const string GuestAlreadyInvitedOrAtPartyMessage = "You already have a guest invited to or currently at your party with this name. Please check your current and invited guest list or add a new guest.";
        public const string DuplicateUserMessage = "You already have a user with this username. Please add a new user.";

    }
}