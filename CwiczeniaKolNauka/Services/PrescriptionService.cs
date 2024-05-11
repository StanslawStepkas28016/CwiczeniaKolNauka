using CwiczeniaKolNauka.Model;
using CwiczeniaKolNauka.Repositories;

namespace CwiczeniaKolNauka.Services;

public class PrescriptionService : IPrescriptionService
{
    private IPrescriptionRepository _prescriptionRepository;

    public PrescriptionService(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<IEnumerable<PrescriptionWithNames>> GetPrescriptions(string? doctorName = "none")
    {
        return await _prescriptionRepository.GetPrescriptions(doctorName);
    }

    public async Task<PrescriptionWithKey> AddPrescription(PrescriptionWithoutKey prescriptionWithoutKey)
    {
        return await _prescriptionRepository.AddPrescription(prescriptionWithoutKey);
    }
}