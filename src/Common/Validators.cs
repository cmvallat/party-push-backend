using System;
using System.Collections.Generic;

namespace Common.Validators

{
    public class Validators
    {
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
        //Todo: add more validators shared across projects as needed
    }
}