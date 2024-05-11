namespace CwiczeniaKolNauka.Model;

public class PrescriptionWithNames
{
    public int IdPrescription { get; set; }
    public string? Date { get; set; }
    public string? DueDate { get; set; }
    public string? PatientLastName { get; set; }
    public string? DoctorLastName { get; set; }
}