using System.Data.SqlClient;
using CwiczeniaKolNauka.Model;

namespace CwiczeniaKolNauka.Repositories;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly string? _connectionString;

    public PrescriptionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<PrescriptionWithNames>> GetPrescriptions(string? doctorName = "none")
    {
        if (doctorName.Equals("none"))
        {
            const string query = "SELECT IdPrescription, Date, DueDate, pa.LastName PN, doc.LastName DN  " +
                                 "FROM Prescription pr " +
                                 "JOIN Patient pa ON pa.IdPatient = pr.IdPatient " +
                                 "JOIN Doctor doc ON doc.IdDoctor = pr.IdDoctor " +
                                 "ORDER BY Date DESC;";

            var list = new List<PrescriptionWithNames>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var data = new PrescriptionWithNames
                {
                    IdPrescription = (int)reader["IdPrescription"],
                    Date = reader["Date"].ToString(),
                    DueDate = reader["DueDate"].ToString(),
                    PatientLastName = reader["PN"].ToString(),
                    DoctorLastName = reader["DN"].ToString(),
                };

                list.Add(data);
            }

            return list;
        }
        else
        {
            if (await DoesDoctorNameExistInDataBase(doctorName) == false)
            {
                var doctorDoesNotExistElement = new PrescriptionWithNames
                {
                    IdPrescription = -1,
                    Date = null,
                    DoctorLastName = null,
                    DueDate = null,
                    PatientLastName = null,
                };

                return new List<PrescriptionWithNames> { doctorDoesNotExistElement };
            }

            const string query = "SELECT IdPrescription, Date, DueDate, pa.LastName PN, doc.LastName DN  " +
                                 "FROM Prescription pr " +
                                 "JOIN Patient pa ON pa.IdPatient = pr.IdPatient " +
                                 "JOIN Doctor doc ON doc.IdDoctor = pr.IdDoctor " +
                                 "WHERE doc.LastName = @doctorName " +
                                 "ORDER BY Date DESC;";

            var list = new List<PrescriptionWithNames>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@doctorName", doctorName);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var data = new PrescriptionWithNames
                {
                    IdPrescription = (int)reader["IdPrescription"],
                    Date = reader["Date"].ToString(),
                    DueDate = reader["DueDate"].ToString(),
                    PatientLastName = reader["PN"].ToString(),
                    DoctorLastName = reader["DN"].ToString(),
                };

                list.Add(data);
            }

            return list;
        }
    }

    private async Task<bool> DoesDoctorNameExistInDataBase(string? doctorName)
    {
        const string query = "SELECT 1 FROM Doctor WHERE LastName = @doctorName;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@doctorName", doctorName);

        var res = await command.ExecuteScalarAsync();

        return res != null;
    }

    public async Task<PrescriptionWithKey> AddPrescription(PrescriptionWithoutKey prescriptionWithoutKey)
    {
        if (await DoesDoctorIdExistInDataBase(prescriptionWithoutKey.IdDoctor) == false)
        {
            return new PrescriptionWithKey { IdDoctor = -1 };
        }

        if (await DoesPatientExistInDatabase(prescriptionWithoutKey.IdPatient) == false)
        {
            return new PrescriptionWithKey { IdPatient = -1 };
        }

        /*if (await DoesPrescriptionMedicamentExistInDatabase(prescription.IdPrescription) == false)
        {
            return new Prescription { Date = null, DueDate = null, IdDoctor = -3, IdPrescription = -3 };
        }*/

        if (await IsDueDateRightWithinDate(prescriptionWithoutKey.Date, prescriptionWithoutKey.DueDate) == false)
        {
            return new PrescriptionWithKey { Date = null };
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Dodanie obiektu do bazy.
        const string insertQuery =
            "INSERT INTO Prescription " +
            "VALUES (@Date, @DueDate, @IdPatient, @IdDoctor);" +
            "SELECT SCOPE_IDENTITY()";

        await using var insertCommand = new SqlCommand(insertQuery, connection);
        insertCommand.Parameters.AddWithValue("@Date", prescriptionWithoutKey.Date);
        insertCommand.Parameters.AddWithValue("@DueDate", prescriptionWithoutKey.DueDate);
        insertCommand.Parameters.AddWithValue("@IdPatient", prescriptionWithoutKey.IdPatient);
        insertCommand.Parameters.AddWithValue("@IdDoctor", prescriptionWithoutKey.IdDoctor);

        var resId = Convert.ToInt32(await insertCommand.ExecuteScalarAsync());

        // Pobranie obiektu z bazy.
        const string selectQuery =
            "SELECT * FROM Prescription " +
            "WHERE IdPrescription = @IdPrescription;";

        await using var selectCommand = new SqlCommand(selectQuery, connection);
        selectCommand.Parameters.AddWithValue("@IdPrescription", resId);

        var reader = await selectCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var data = new PrescriptionWithKey
            {
                IdPrescription = (int)reader["IdPrescription"],
                Date = reader["Date"].ToString(),
                DueDate = reader["DueDate"].ToString(),
                IdDoctor = (int)reader["IdDoctor"],
                IdPatient = (int)reader["IdPatient"],
            };

            return data;
        }

        return null;
    }

    private async Task<bool> IsDueDateRightWithinDate(string? date, string? dueDate)
    {
        const string query = "SELECT 1 WHERE @Date < @DueDate;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Date", date);
        command.Parameters.AddWithValue("@DueDate", dueDate);

        var res = await command.ExecuteScalarAsync();

        return res != null;
    }

    private async Task<bool> DoesPrescriptionMedicamentExistInDatabase(int prescriptionId)
    {
        const string query = "SELECT 1 FROM Prescription_Medicament WHERE IdPrescription = @IdPrescription;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdPrescription", prescriptionId);

        var res = await command.ExecuteScalarAsync();

        return res != null;
    }

    private async Task<bool> DoesPatientExistInDatabase(int patientId)
    {
        const string query = "SELECT 1 FROM Patient WHERE IdPatient = @PatientId;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@PatientId", patientId);

        var res = await command.ExecuteScalarAsync();

        return res != null;
    }

    private async Task<bool> DoesDoctorIdExistInDataBase(int doctorId)
    {
        const string query = "SELECT 1 FROM Doctor WHERE IdDoctor = @DoctorId;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@DoctorId", doctorId);

        var res = await command.ExecuteScalarAsync();

        return res != null;
    }
}