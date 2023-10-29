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
    }
}