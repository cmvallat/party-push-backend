using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using Amazon;
// using Amazon.RDSDataService;
// using Amazon.RDSDataService.Model;
using Microsoft.AspNetCore.Mvc;

namespace Api.DemoController
{
    [ApiController]
    [Route("[controller]")]

    public class DemoController : ControllerBase
    {
        // private readonly string _databaseName = "my-db-name";
        // private readonly string _resourceArn = "arn:aws:rds:us-east-1:123456789012:cluster:my-cluster-name";
        // private readonly string _secretArn = "arn:aws:secretsmanager:us-east-1:123456789012:secret:my-db-secret-name";

        [HttpPost("post-guest-to-db")]
        public async Task<IActionResult> PostData(int id)
        {
            //try{
                if(id == 1)
                {
                    // Return a success response
                    return Ok(new { message = "User created successfully" });
                }
                 
                
            //}
            // catch (Exception ex){
            //     // Log the exception
            //     Logger.Error(ex);

            //     // Return an error response
                return StatusCode(500, new { message = "Id wasn't 1" });
            // } 
        }
        // public class MyModel
        // {
        //     public string Property1 { get; set; }
        //     public string Property2 { get; set; }
        //     public string Property3 { get; set; }
        // }
    
    }
}
