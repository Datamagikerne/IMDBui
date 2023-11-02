using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace IMDBui
{
    public static class DbMethod
    {
        private static string ConnectionString = "";
        public static void Login()
        {
            Console.Clear();
            Console.WriteLine("Welcome to the IMDB Database Console UI");
            Console.Write("Enter Username: ");
            string username = Console.ReadLine();

            Console.Write("Enter Password: ");
            string password = ReadPassword();

            string connectionString = $"server=localhost;database=IMDB;User Id={username};Password={password};";
            ConnectionString = connectionString;

            using SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                Console.WriteLine("\nLogin successful!");
            }
            catch (SqlException ex)
            {
                Console.WriteLine("\nLogin failed. Please check your username and password.");
                Menu(connection);
            }
            Menu(connection);
        }

        public static void Menu(SqlConnection connection)
        {
            

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Menu:");
                Console.WriteLine("0. Back to login");
                Console.WriteLine("1. Search movie by Title Name(Wildcard)");
                Console.WriteLine("2. Search person by Person Name(Wildcard)");
                Console.WriteLine("3. Add movie to DB");
                Console.WriteLine("4. Update movie");
                Console.WriteLine("5. Search person details(Exact)");
                Console.WriteLine("6. Add person to DB");

                Console.Write("Enter your choice: ");

                switch (Console.ReadLine())
                {
                    case "0":
                        connection.Close();
                        Login();
                        break;
                    case "1":
                        SearchByName();
                        Console.WriteLine("Press any key to return to the main menu...");
                        Console.ReadKey();
                        break;
                    case "2":
                        SearchPersonByName();
                        Console.WriteLine("Press any key to return to the main menu...");
                        Console.ReadKey();
                        break;
                    case "3":
                        AddMovie();
                        Console.WriteLine("Press any key to return to the main menu...");
                        Console.ReadKey();
                        break;
                    case "4":
                        UpdateMovie();
                        Console.WriteLine("Press any key to return to the main menu...");
                        Console.ReadKey();
                        break;
                    case "5":
                        SearchPersonByExactName();
                        Console.WriteLine("Press any key to return to the main menu...");
                        Console.ReadKey();
                        break;
                    case "6":
                        AddPerson();
                        Console.WriteLine("Press any key to return to the main menu...");
                        Console.ReadKey();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to try again...");
                        Console.ReadKey();
                        break;
                }
            }
        }
    
        public static void SearchByName()
        {
           


            Console.Clear();
            Console.WriteLine("Enter the title name:");
            string titleName = Console.ReadLine();

            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            using SqlCommand cmd = new SqlCommand("sp_GetTitleByName", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@TitleName", titleName));

            using SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                Console.WriteLine("No results found.");
            }
            else
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Title ID: {reader["TitleID"]}, Title Name: {reader["TitleName"]}");
                    // Add other fields as needed
                }
            }
        }
        public static void SearchPersonByName()
        {

            Console.Clear();
            Console.WriteLine("Enter the persons name:");
            string personName = Console.ReadLine();

            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            using SqlCommand cmd = new SqlCommand("sp_GetPersonByName", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@SearchName", personName));

            using SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                Console.WriteLine("No results found.");
            }
            else
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Person ID: {reader["PersonId"]}, Name: {reader["Name"]}");
                    // Add other fields as needed
                }
            }

        }
        private static string ReadPassword()
        {
            string password = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;
                password += key.KeyChar;
                Console.Write("*");
            }
            return password;
        }
        public static void AddMovie()
        {
            Console.Clear();
            Console.WriteLine("Add a New Movie to the Database");

            Console.Write("Enter tconst (e.g., tt123456): ");
            string tconst = Console.ReadLine();

            Console.Write("Enter title type (e.g., movie, series): ");
            string titleType = Console.ReadLine();

            Console.Write("Enter primary title: ");
            string primaryTitle = Console.ReadLine();

            Console.Write("Enter original title: ");
            string originalTitle = Console.ReadLine();

            Console.Write("Is it adult content? (yes/no): ");
            bool isAdult = Console.ReadLine().ToLower() == "yes";

            Console.Write("Enter start year (or leave blank): ");
            string startYear = Console.ReadLine();
            

            Console.Write("Enter end year (or leave blank): ");
            string endYear = Console.ReadLine();
            

            Console.Write("Enter runtime in minutes: ");
            string runtimeMinutes = Console.ReadLine();
           

            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            using SqlCommand cmd = new SqlCommand("sp_AddMovie", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@tconst", tconst);
            cmd.Parameters.AddWithValue("@titleType", titleType);
            cmd.Parameters.AddWithValue("@primaryTitle", primaryTitle);
            cmd.Parameters.AddWithValue("@originalTitle", originalTitle);
            cmd.Parameters.AddWithValue("@isAdult", isAdult);
            cmd.Parameters.AddWithValue("@startYear", startYear);
            cmd.Parameters.AddWithValue("@endYear", endYear);
            cmd.Parameters.AddWithValue("@runtimeMinutes", runtimeMinutes);

            try
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("Movie added successfully!");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 229) // Permission denied error
                {
                    Console.WriteLine("Sorry, you don't have permission to add movies.");
                }
                else
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }
        public static bool CheckIfTitleExistsByTconst(string tconst)
        {
            bool movieExists = false;

            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            using SqlCommand cmd = new SqlCommand("sp_GetTitleByTconst", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Tconst", tconst);
            cmd.Parameters.Add("@TitleCount", SqlDbType.Int).Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();

            // Check the output parameter to determine if the movie exists
            int titleCount = (int)cmd.Parameters["@TitleCount"].Value;
            if (titleCount > 0)
            {
                movieExists = true;
            }

            return movieExists;
        }
        public static void UpdateMovie()
        {
            Console.Clear();
            Console.WriteLine("Update an Existing Movie in the Database");

            Console.Write("Enter tconst of the movie to update (e.g., tt123456): ");
            string tconst = Console.ReadLine();

            // Call the stored procedure to fetch existing movie details by tconst
            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            using SqlCommand fetchCmd = new SqlCommand("sp_GetTitleByTconst", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            fetchCmd.Parameters.AddWithValue("@Tconst", tconst);

            try
            {
                using SqlDataReader reader = fetchCmd.ExecuteReader();
                if (reader.Read())
                {
                    Console.WriteLine("Existing Movie Details:");
                    Console.WriteLine($"Title Type: {reader["titleType"]}");
                    Console.WriteLine($"Primary Title: {reader["TitleName"]}");
                    Console.WriteLine($"Original Title: {reader["originalTitle"]}");
                    Console.WriteLine($"Is Adult: {reader["isAdult"]}");
                    Console.WriteLine($"Start Year: {reader["startYear"]}");
                    Console.WriteLine($"End Year: {reader["endYear"]}");
                    Console.WriteLine($"Runtime Minutes: {reader["runtimeMinutes"]}");
                }
                else
                {
                    Console.WriteLine("Movie not found with the specified tconst.");
                    return;
                }

                reader.Close();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 229) // Permission denied error
                {
                    Console.WriteLine("Sorry, you don't have permission to add movies.");
                    Console.ReadKey();
                    Menu(connection);
                }
                else
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            

            Console.WriteLine();
            Console.WriteLine("Enter new values (or leave blank to set to null):");

            Console.Write("New title type: ");
            string titleType = Console.ReadLine();

            Console.Write("New primary title: ");
            string primaryTitle = Console.ReadLine();

            Console.Write("New original title: ");
            string originalTitle = Console.ReadLine();

            Console.Write("Is it adult content? (yes/no): ");
            string isAdultInput = Console.ReadLine();
            bool? isAdult = null;
            if (!string.IsNullOrWhiteSpace(isAdultInput))
            {
                isAdult = isAdultInput.ToLower() == "yes";
            }

            Console.Write("New start year: ");
            string startYearInput = Console.ReadLine();
            int? startYear = null;
            if (!string.IsNullOrWhiteSpace(startYearInput))
            {
                if (int.TryParse(startYearInput, out int startYearValue))
                {
                    startYear = startYearValue;
                }
            }

            Console.Write("New end year: ");
            string endYearInput = Console.ReadLine();
            int? endYear = null;
            if (!string.IsNullOrWhiteSpace(endYearInput))
            {
                if (int.TryParse(endYearInput, out int endYearValue))
                {
                    endYear = endYearValue;
                }
            }

            Console.Write("New runtime in minutes: ");
            string runtimeMinutesInput = Console.ReadLine();
            int? runtimeMinutes = null;
            if (!string.IsNullOrWhiteSpace(runtimeMinutesInput))
            {
                if (int.TryParse(runtimeMinutesInput, out int runtimeMinutesValue))
                {
                    runtimeMinutes = runtimeMinutesValue;
                }
            }

            // Call the stored procedure to update the movie
            using SqlCommand updateCmd = new SqlCommand("sp_UpdateMovie", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            updateCmd.Parameters.AddWithValue("@tconst", tconst);
            updateCmd.Parameters.AddWithValue("@titleType", titleType);
            updateCmd.Parameters.AddWithValue("@primaryTitle", primaryTitle);
            updateCmd.Parameters.AddWithValue("@originalTitle", originalTitle);
            updateCmd.Parameters.AddWithValue("@isAdult", isAdult);
            updateCmd.Parameters.AddWithValue("@startYear", startYear);
            updateCmd.Parameters.AddWithValue("@endYear", endYear);
            updateCmd.Parameters.AddWithValue("@runtimeMinutes", runtimeMinutes);

            try
            {
                updateCmd.ExecuteNonQuery();
                Console.WriteLine("Movie updated successfully!");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        public static void SearchPersonByExactName()
        {
            Console.Clear();
            Console.WriteLine("Enter the exact name of the person:");
            string exactName = Console.ReadLine();

            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            using SqlCommand cmd = new SqlCommand("sp_GetPersonDetailsAndMoviesByExactName", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@ExactName", exactName));

            using SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                Console.WriteLine("No results found.");
            }
            else
            {
                Console.WriteLine($"Details for {exactName}:\n");
                bool isFirstResultSet = true;
                do
                {
                    while (reader.Read())
                    {
                        if (isFirstResultSet)
                        {
                            Console.WriteLine($"Person ID: {reader["PersonId"]}, Name: {reader["Name"]}, Birth Year: {reader["birthYear"]}, Death Year: {reader["deathYear"]}\n");
                        }
                        else
                        {
                            Console.WriteLine($"Movie ID: {reader["MovieId"]}, Movie Title: {reader["MovieTitle"]}, Movie Year: {reader["MovieYear"]}, Roles: {reader["Roles"]}");
                        }
                    }
                    isFirstResultSet = false;
                } while (reader.NextResult());
            }
        }
        public static void AddPerson()
        {
            Console.Clear();
            Console.WriteLine("Add a New Person to the Database");

            Console.Write("Enter nconst (e.g., nm123456): ");
            string nconst = Console.ReadLine();

            Console.Write("Enter full name type (e.g., Mads Mikkelsen): ");
            string primaryName = Console.ReadLine();

            Console.Write("Enter birth year: ");
            string birthYear = Console.ReadLine();

            Console.Write("Enter death year (or leave blank): ");
            string deathYear = Console.ReadLine();

            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            using SqlCommand cmd = new SqlCommand("sp_AddPerson", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@nconst", nconst);
            cmd.Parameters.AddWithValue("@primaryName", primaryName);
            cmd.Parameters.AddWithValue("@birthYear", birthYear);
            cmd.Parameters.AddWithValue("@deathYear", deathYear);

            try
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("Person added successfully!");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 229) // Permission denied error
                {
                    Console.WriteLine("Sorry, you don't have permission to add people.");
                }
                else
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}
