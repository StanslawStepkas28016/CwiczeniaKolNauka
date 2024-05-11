using CwiczeniaKolNauka.Model;
using CwiczeniaKolNauka.Services;
using Microsoft.AspNetCore.Mvc;

namespace CwiczeniaKolNauka.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PrescriptionController : ControllerBase
{
    private IPrescriptionService _prescriptionService;

    public PrescriptionController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpGet("GetPrescriptions")]
    public async Task<IActionResult> GetPrescriptions(string? doctorName = "none")
    {
        var res = await _prescriptionService.GetPrescriptions(doctorName);

        if (res.ToList()[0].IdPrescription == -1)
        {
            return BadRequest("Doctor with the provided \'doctorName\' does not exist in the Database!");
        }

        return Ok(res);
    }

    [HttpPost("AddPrescription")]
    public async Task<IActionResult> AddPrescription(PrescriptionWithoutKey prescriptionWithoutKey)
    {
        var res = await _prescriptionService.AddPrescription(prescriptionWithoutKey);

        if (res.IdDoctor == -1)
        {
            return BadRequest("Doctor with the provided \'IdDoctor\' does not exist in the Database!");
        }

        if (res.IdPatient == -1)
        {
            return BadRequest("Patient with the provided \'IdDoctor\' does not exist in the Database!");
        }

        if (res.Date == null)
        {
            return BadRequest("\'Date\' is not earlier than \'DueDate\'!");
        }

        return Ok(res);
    }
}