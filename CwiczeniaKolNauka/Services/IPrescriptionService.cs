using CwiczeniaKolNauka.Model;

namespace CwiczeniaKolNauka.Services;

public interface IPrescriptionService
{
    public Task<IEnumerable<PrescriptionWithNames>> GetPrescriptions(string? doctorName = "none");
    public Task<PrescriptionWithKey> AddPrescription(PrescriptionWithoutKey prescriptionWithoutKey);
}