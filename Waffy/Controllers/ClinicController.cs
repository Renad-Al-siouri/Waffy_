using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Xml.Linq;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
namespace Waffy.Controllers
{
    public class ClinicController : Controller
    {
        private readonly IClinicRepository _clinicRepo;
        private readonly IClinicServiceRepository _serviceRepo;
        private readonly IClinicScheduleRepository _scheduleRepo;
        private readonly IClinicImageRepository _imageRepo;
        private readonly IDoctorRepository _doctorRepo;

        public ClinicController(
            IClinicRepository clinicRepo,
            IClinicServiceRepository serviceRepo,
            IClinicScheduleRepository scheduleRepo,
            IClinicImageRepository imageRepo,
            IDoctorRepository doctorRepo)
        {
            _clinicRepo = clinicRepo;
            _serviceRepo = serviceRepo;
            _scheduleRepo = scheduleRepo;
            _imageRepo = imageRepo;
            _doctorRepo = doctorRepo;
        }
        // ══════════════════════════════════════════════════
        // GET /Clinic/Clinics
        // ══════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Clinics(
            string? search, string? city, string? service)
        {
            var all = (await _clinicRepo.GetAllAsync())
                .Where(c => c.VerificationStatus == "Approved" ||
                            c.VerificationStatus == "Under Review")
                .ToList();

            var cards = new List<ClinicCardViewModel>();

            foreach (var clinic in all)
            {
                var services = (await _serviceRepo.GetByClinicIdAsync(clinic.IdClinic))
                    .Where(s => s.DocumentType != null)
                    .Select(s => s.DocumentType!)
                    .ToList();

                if (!string.IsNullOrEmpty(service) && service != "All")
                    if (!services.Contains(service)) continue;

                if (!string.IsNullOrEmpty(city) && city != "All")
                    if (clinic.City != city) continue;

                if (!string.IsNullOrEmpty(search))
                {
                    var q = search.ToLower();
                    if (!clinic.Name.ToLower().Contains(q) &&
                        !(clinic.City ?? "").ToLower().Contains(q) &&
                        !(clinic.Description ?? "").ToLower().Contains(q))
                        continue;
                }

                cards.Add(new ClinicCardViewModel
                {
                    IdClinic = clinic.IdClinic,
                    Name = clinic.Name,
                    ClinicType = clinic.ClinicType,
                    City = clinic.City,
                    Area = clinic.Area,
                    Phone = clinic.Phone,
                    LogoUrl = clinic.LogoUrl,
                    CoverImageUrl = clinic.CoverImageUrl,
                    HasEmergency = clinic.HasEmergency,
                    DefaultOpenTime = clinic.DefaultOpenTime,
                    DefaultCloseTime = clinic.DefaultCloseTime,
                    Services = services,
                    Description = clinic.Description,
                    Rating = clinic.Rating
                });
            }

            // ✅ من الـ all المجلوب مسبقاً
            var cities = all
                .Select(c => c.City)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var vm = new ClinicsListViewModel
            {
                Clinics = cards,
                Cities = cities!,
                AllServices = new List<string>
        {
            "Vaccinations","General checkup","Surgery","Dentistry",
            "Lab Tests","X-Ray","Grooming","Home Visit","Emergency"
        },
                SelectedCity = city ?? "All",
                SearchQuery = search,
                SelectedService = service ?? "All"
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ClinicDetail(int id)
        {
            var clinic = await _clinicRepo.GetByIdAsync(id);

            // ✅ يسمح بـ Under Review أيضاً
            if (clinic == null ||
                (clinic.VerificationStatus != "Approved" &&
                 clinic.VerificationStatus != "Under Review"))
                return NotFound();

            var services = (await _serviceRepo.GetByClinicIdAsync(id))
                .Where(s => s.DocumentType != null)
                .Select(s => s.DocumentType!)
                .ToList();

            var schedule = (await _scheduleRepo.GetByClinicIdAsync(id))
                .OrderBy(s => s.DayOfWeek ?? 0)
                .ToList();

            var gallery = (await _imageRepo.GetByClinicIdAsync(id)).ToList();

            // ✅ فقط Approved للأطباء
            var doctors = (await _doctorRepo.GetByClinicIdAsync(id))
                .Where(d => d.VerificationStatus == "Approved")
                .ToList();

            var vm = new ClinicDetailViewModel
            {
                IdClinic = clinic.IdClinic,
                Name = clinic.Name,
                ClinicType = clinic.ClinicType,
                City = clinic.City,
                Area = clinic.Area,
                Phone = clinic.Phone,
                Email = clinic.Email,
                Description = clinic.Description,
                Address = clinic.Address,
                Latitude = clinic.Latitude,
                Longitude = clinic.Longitude,
                HasEmergency = clinic.HasEmergency,
                DefaultOpenTime = clinic.DefaultOpenTime,
                DefaultCloseTime = clinic.DefaultCloseTime,
                LogoUrl = clinic.LogoUrl,
                CoverImageUrl = clinic.CoverImageUrl,
                Rating = clinic.Rating,
                Services = services,
                Schedule = schedule,
                GalleryImages = gallery,
                Doctors = doctors
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> DoctorDetail(int id)
        {
            var doctor = await _doctorRepo.GetDetailByIdAsync(id);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

    }
}



















