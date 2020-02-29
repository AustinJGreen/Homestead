using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Net;

namespace Homestead.Services
{
    public class UsersDatabase : IDisposable
    {
        private ILogger log;

        private SqlConnection con;
        private string ConnectionString { get; set; }

        public bool UserExists(User user)
        {
            const string cmdText = "SELECT * FROM requests WHERE email = @email";
            using (SqlCommand command = new SqlCommand(cmdText, con))
            {
                command.Parameters.AddWithValue("email", user.Email);

                try
                {
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    log.LogCritical(ex.Message);
                }
            }

            return false;
        }

        public User GetUser(Guid uid)
        {
            const string cmdText = "SELECT email, name, phone, ip FROM requests WHERE logid = @uid";
            using (SqlCommand command = new SqlCommand(cmdText, con))
            {
                command.Parameters.AddWithValue("uid", uid);

                try
                {
                    var result = command.ExecuteReader();
                    if (result.Read())
                    {
                        string email = result.GetString(0);
                        string name = result.GetString(1);
                        string phone = result.GetString(2);
                        string ip = result.GetString(3);
                        return new User(email, name, phone) { IP = ip };
                    }
                }
                catch (Exception ex)
                {
                    log.LogCritical(ex.Message);
                }
            }

            return null;
        }

        public Guid CreateUser(User user, IPAddress address)
        {
            Guid guid = Guid.NewGuid();
            const string cmdText = "INSERT INTO requests VALUES (@id, @email, @name, @phone, @ip)";
            using (SqlCommand command = new SqlCommand(cmdText, con))
            {
                command.Parameters.AddWithValue("id", guid);
                command.Parameters.AddWithValue("email", user.Email);
                command.Parameters.AddWithValue("name", user.Name);
                command.Parameters.AddWithValue("phone", user.Phone);
                command.Parameters.AddWithValue("ip", address.ToString());

                try
                {
                    command.ExecuteNonQuery();
                    return guid;
                }
                catch (Exception ex)
                {
                    log.LogCritical(ex.Message);
                    return Guid.Empty;
                }
            }
        }

        public Guid LogLookup(Guid userid, string address)
        {
            Guid guid = Guid.NewGuid();
            const string cmdText = "INSERT INTO lookups VALUES (@uid, @reqid, @address)";
            using (SqlCommand command = new SqlCommand(cmdText, con))
            {
                command.Parameters.AddWithValue("uid", userid);
                command.Parameters.AddWithValue("reqid", guid);
                command.Parameters.AddWithValue("address", address);

                try
                {
                    command.ExecuteNonQuery();
                    return guid;
                }
                catch (Exception ex)
                {
                    log.LogCritical(ex.Message);
                    return Guid.Empty;
                }
            }
        }

        public void LogEstimate(Guid reqid, int estimateLow, int estimateHigh)
        {
            const string cmdText = "INSERT INTO estimates (reqid, est_low, est_high) VALUES (@reqid, @est_low, @est_high)";
            using (SqlCommand command = new SqlCommand(cmdText, con))
            {
                command.Parameters.AddWithValue("reqid", reqid);
                command.Parameters.AddWithValue("est_low", estimateLow);
                command.Parameters.AddWithValue("est_high", estimateHigh);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    log.LogCritical(ex.Message);
                }
            }
        }

        public void LogExpectedRent(Guid reqid, int expected)
        {
            const string cmdText = "UPDATE estimates Set expected = @expected WHERE reqid = @reqid";
            using (SqlCommand command = new SqlCommand(cmdText, con))
            {
                command.Parameters.AddWithValue("reqid", reqid);
                command.Parameters.AddWithValue("expected", expected);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    log.LogCritical(ex.Message);
                }
            }
        }

        public bool Open()
        {
            if (con == null)
            {
                con = new SqlConnection(ConnectionString);

                try
                {
                    con.Open();
                }
                catch (Exception ex)
                {
                    log.LogCritical(ex, "Failed to connect to users DB");
                    return false;
                }
            }

            return true;
        }

        public void Dispose()
        {
            if (con != null)
            {
                con.Dispose();
            }
        }

        public UsersDatabase(string connectionString, ILoggerFactory factory)
        {
            ConnectionString = connectionString;
            log = factory.CreateLogger<UsersDatabase>();
        }
    }
}
