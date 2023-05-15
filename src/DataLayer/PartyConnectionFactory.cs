using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;



namespace DataLayer
{
    public interface IDatabaseConnectionFactory
    {
        Task<MySqlConnection> GetConnection();
    }

    public class PartyConnectionFactory : IDatabaseConnectionFactory
    {
        //Todo: setup Secrets Manager here (TourLiveConnectionFactory)

        public async Task<MySqlConnection> GetConnection()
        {
            var conn = new MySqlConnection("server=party-resources.crurrv9mzw4i.us-west-1.rds.amazonaws.com;port=3306;database=Party;user=cmvallat;password=Gdtbath21");
            conn.Open();
            return conn;
        }
    }
}