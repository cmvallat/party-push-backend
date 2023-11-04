namespace Common.Validators
{
    public class Validators
    {

        //previously developed to validate API parameters but went with Required data annotation instead
        //keeping it here in case it needs to be used in future
        public static bool ValidateStringParameters(List<string> parameters)
        {
            foreach(var param in parameters)
            {
                if(String.IsNullOrWhiteSpace(param))
                {
                    return false;
                }
            }
            return true;
        }
        // public async Task<bool> ValidateUserMatchesHost(string party_code, string username)
        // {
        //     Models.Host host = await _mediator.Send(new GetHost.Query { Party_code = party_code });
        //     if(host.username == username)
        //     {
        //         return true;
        //     }
        //     return false;
        // }

        // public async Task<bool> ValidateHostExists(string party_code)
        // {
        //     Models.Host host = await _mediator.Send(new GetHost.Query { Party_code = party_code });
        //     if(host != null)
        //     {
        //         return true;
        //     }
        //     return false;
        // }
    }
}