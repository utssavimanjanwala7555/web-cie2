using HospitalAppointmentManagement.Extensions;
using HospitalAppointmentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace HospitalAppointmentManagement.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;

        public DoctorController(
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
        }

       
        // DOCTOR LIST
        
        public IActionResult Index()
        {
            const string cacheKey = "doctor-list";

            if (!_cache.TryGetValue(
                cacheKey,
                out List<Doctor>? doctors))
            {
                doctors = GetDoctors();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(3)
                };

                _cache.Set(
                    cacheKey,
                    doctors,
                    cacheOptions);
            }

            return View(doctors);
        }

        // DOCTOR DATA
    
        private List<Doctor> GetDoctors()
        {
            return new List<Doctor>
            {
                new Doctor
                {
                    DoctorId = 1,
                    DoctorName = "Dr. Rajesh Patel",
                    Specialization = "Cardiology",
                    Experience = 12,
                    ConsultationFee = 800
                },

                new Doctor
                {
                    DoctorId = 2,
                    DoctorName = "Dr. Priya Shah",
                    Specialization = "Dermatology",
                    Experience = 8,
                    ConsultationFee = 600
                },

                new Doctor
                {
                    DoctorId = 3,
                    DoctorName = "Dr. Amit Mehta",
                    Specialization = "Orthopedics",
                    Experience = 15,
                    ConsultationFee = 1000
                },

                new Doctor
                {
                    DoctorId = 4,
                    DoctorName = "Dr. Neha Desai",
                    Specialization = "Pediatrics",
                    Experience = 10,
                    ConsultationFee = 700
                }
            };
        }

        // DOCTOR DETAILS

        [ResponseCache(Duration = 180)]
        public IActionResult Details(
            int DoctorId,
            string? refresh)
        {
            var doctors = GetDoctors();

            var doctor = doctors.FirstOrDefault(
                d => d.DoctorId == DoctorId);

            if (doctor == null)
            {
                return NotFound();
            }

          
            // SESSION
           
            HttpContext.Session.SetString(
                "SelectedSpecialization",
                doctor.Specialization);

            
            // Read patient name
          
            string? patientName =
                Request.Cookies["PatientName"];

            ViewBag.PatientName = patientName;

         
            // Consultation Fee + ₹100
            

            ViewBag.TotalCharge =
                doctor.CalculateTotalCharge();

            return View(doctor);
        }

        // BOOK APPOINTMENT

        [HttpPost]
        public IActionResult BookAppointment(
            int DoctorId,
            string PatientName)
        {
            var doctors = GetDoctors();

            var doctor = doctors.FirstOrDefault(
                d => d.DoctorId == DoctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            // Store patient name
 
            Response.Cookies.Append(
                "PatientName",
                PatientName,
                new CookieOptions
                {
                    Expires =
                        DateTimeOffset.Now.AddDays(1),

                    HttpOnly = true,
                    IsEssential = true
                });

          
            // Store specialization
          
            HttpContext.Session.SetString(
                "SelectedSpecialization",
                doctor.Specialization);

            // SUCCESS MESSAGE

            TempData["Message"] =
                $"Appointment request submitted successfully for {doctor.DoctorName}.";


            // REDIRECT
      
            return RedirectToAction(
                nameof(Details),
                new
                {
                    DoctorId = DoctorId,
                    refresh = DateTimeOffset.UtcNow
                        .ToUnixTimeMilliseconds()
                });
        }

        // ASYNCHRONOUS WEB REQUEST
  
        public async Task<IActionResult> Posts()
        {
            var client =
                _httpClientFactory.CreateClient();

            var posts =
                await client.GetFromJsonAsync<List<Post>>(
                    "https://jsonplaceholder.typicode.com/posts");

            if (posts == null)
            {
                posts = new List<Post>();
            }

            return View(posts);
        }
    }
}



