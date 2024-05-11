using CwiczeniaKolNauka.Model;

namespace CwiczeniaKolNauka.Repositories;

public interface IPrescriptionRepository
{
    public Task<IEnumerable<PrescriptionWithNames>> GetPrescriptions(string? doctorName = "none");
    public Task<PrescriptionWithKey> AddPrescription(PrescriptionWithoutKey prescriptionWithoutKey);
}