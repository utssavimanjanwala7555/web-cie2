using HospitalAppointmentManagement.Models;

namespace HospitalAppointmentManagement.Extensions
{
    public static class DoctorExtensions
    {
        public static decimal CalculateTotalCharge(this Doctor doctor)
        {
            return doctor.ConsultationFee + 100;
        }
    }
}
