﻿using MySql.Data.MySqlClient;
using System;

namespace ProjectUnionFreeroam.Database
{
    public class MySQL
    {
        public static MySqlConnection connection = null;

        public void Connect()
        {
            //TODO: Place connection string into config file



            string connectionString = "SERVER=localhost;PASSWORD=;UID=root;DATABASE=projectunionfreeroam;";
            connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                Main.Logger.Log("Connected SQL!");
            }
            catch (Exception e)
            {
                Main.Logger.LogError(e.ToString());
            }
        }
    }
}
