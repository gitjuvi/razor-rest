using Microsoft.Data.Sqlite;
using MinimalAPIs.Models;
namespace RazorAPI.Services
{
    public interface IVitsiService
    {
        void TallennaVitsi(Vitsi vitsi);
        List<Vitsi> HaeVitsit();
    }
    public class VitsiService : IVitsiService
    {
        public void TallennaVitsi(Vitsi vitsi)
        {
            // tallenna kantaan vitsi
            using (var connection = new SqliteConnection("Data Source=razor.db"))
            {
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = @"INSERT INTO Vitsit (Otsikko, Vitsiteksti) VALUES($otsikko,$teksti)";
                command.Parameters.AddWithValue("$otsikko", vitsi.Otsikko);
                command.Parameters.AddWithValue("$teksti", vitsi.Vitsiteksti);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public List<Vitsi> HaeVitsit()
        {
            List<Vitsi> vitsit = new List<Vitsi>();

            // hae vitsit tietokannasta
            using (var connection = new SqliteConnection("Data Source=razor.db"))
            {
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = "SELECT Otsikko, Vitsiteksti FROM Vitsit ORDER BY Otsikko ASC";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string otsikko = reader.GetString(0);
                        string teksti = reader.GetString(1);
                        Vitsi vitsi = new Vitsi();
                        vitsi.Otsikko = otsikko;
                        vitsi.Vitsiteksti = teksti;

                        vitsit.Add(vitsi);
                    }
                }
                connection.Close();
            }

            return vitsit;
        }
    }
}
