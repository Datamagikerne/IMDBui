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

        public static void Menu()
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
                Menu();
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Search movie by Title Name");
                Console.WriteLine("2. Search person by Person Name");
                Console.WriteLine("3. Add movie to DB");
                Console.WriteLine("4. Search person detailed");
                Console.WriteLine("5. Back to login");
                Console.Write("Enter your choice: ");

                switch (Console.ReadLine())
                {
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
                        SearchPersonByExactName();
                        Console.ReadKey();
                        break;
                    case "5":
                        connection.Close();
                        Menu();
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




    }
}
