using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=bd-kip.fa.ru;Initial Catalog=Prokofiev_PR7;Persist Security Info=True;User ID=sa;Password=1qaz!QAZ;Encrypt=False";

        using (var connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                Console.WriteLine("Подключение успешно!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка подключения:");
                Console.WriteLine(ex.Message);
            }
        }

        Console.ReadKey();
    }
}