using Microsoft.AspNetCore.Mvc;
using WarEntiGox.Models;
using WarEntiGox.Services;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace WarEntiGox.Controllers.MVC
{
    [Route("users")]
    public class UserControllerMvc : Controller
    {
        private readonly UserService _userService;
        private readonly CompanyService _companyService;
        private readonly ILogger<UserControllerMvc> _logger;

        public UserControllerMvc(UserService userService, CompanyService companyService, ILogger<UserControllerMvc> logger)
        {
            _userService = userService;
            _companyService = companyService;
            _logger = logger;
        }

        // Kullanıcı listesi
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm)
        {
            try
            {
                ViewData["CurrentFilter"] = searchTerm;

                var users = await _userService.GetAllUsersAsync() ?? new List<User>();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    users = users.Where(u => u.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return View("~/Views/User/Index.cshtml", users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı listesi alınırken hata oluştu.");
                ModelState.AddModelError(string.Empty, "Kullanıcı listesi alınırken hata oluştu.");
                return View("~/Views/User/Index.cshtml", new List<User>());
            }
        }

        // Kullanıcı oluşturma sayfası (GET)
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Şirketleri alıyoruz
                var companies = await _companyService.GetAllCompaniesAsync();
                ViewBag.Companies = new SelectList(companies, "CompanyId", "Name");

                // Roller için seçim kutusu (ComboBox)
                ViewBag.Roles = new SelectList(new[]
                {
                    new { Value = 1, Text = "User" },
                    new { Value = 2, Text = "Owner" },
                    new { Value = 3, Text = "Admin" }
                }, "Value", "Text");

                return View("~/Views/User/Create.cshtml", new User());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şirketler alınırken hata oluştu.");
                return RedirectToAction(nameof(Index)); // Hata durumunda kullanıcılar listesine yönlendir
            }
        }

        // Kullanıcı oluşturma işlemi (POST)
        [HttpPost("Create")]
        public async Task<IActionResult> Create(User user)
        {
            if (!ModelState.IsValid)
            {
                await LoadCompaniesAsync();
                return View("~/Views/User/Create.cshtml", user);
            }

            try
            {
                // Kullanıcı oluşturulurken tarih bilgileri ekleniyor
                user.CreateDate = DateTime.Now;
                user.UpdateDate = DateTime.Now;

                // Şifreyi hash'leyelim
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    user.PasswordHash = HashPassword(user.PasswordHash); // Şifreyi hash'le
                }

                await _userService.CreateUserAsync(user); // Kullanıcıyı veritabanına ekliyoruz
                return RedirectToAction(nameof(Index)); // Listeye yönlendiriyoruz
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yeni kullanıcı oluşturulurken hata oluştu.");
                ModelState.AddModelError(string.Empty, $"Bir hata oluştu: {ex.Message}");
                await LoadCompaniesAsync();
                return View("~/Views/User/Create.cshtml", user);
            }
        }

        // Kullanıcı düzenleme sayfası (GET)
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                // Şirketleri alıyoruz
                var companies = await _companyService.GetAllCompaniesAsync();
                ViewBag.Companies = new SelectList(companies, "CompanyId", "Name");

                // Roller için seçim kutusu (ComboBox)
                ViewBag.Roles = new SelectList(new[]
                {
                    new { Value = 1, Text = "User" },
                    new { Value = 2, Text = "Owner" },
                    new { Value = 3, Text = "Admin" }
                }, "Value", "Text");

                // Kullanıcıyı id'ye göre alıyoruz
                // 'id' parametresi string, ama 'ObjectId' ile karşılaştırmamız gerekiyor.
                if (ObjectId.TryParse(id, out ObjectId objectId))
                {
                    var user = await _userService.GetUserByIdAsync(objectId);
                    if (user == null) return NotFound(); // Kullanıcı bulunamazsa 404 döndür

                    return View("~/Views/User/Edit.cshtml", user);
                }
                else
                {
                    return BadRequest("Geçersiz kullanıcı ID.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı düzenleme sayfası alınırken hata oluştu.");
                return RedirectToAction(nameof(Index));
            }
        }

        // Kullanıcı düzenleme işlemi (POST)
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(string id, User user)
        {
            if (!ModelState.IsValid)
            {
                await LoadCompaniesAsync(user.CompanyId);
                return View("~/Views/User/Edit.cshtml", user);
            }

            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    return BadRequest("Geçersiz kullanıcı ID.");
                }

                if (user.Id != objectId) // 'Id' karşılaştırması
                {
                    return NotFound(); // Kullanıcı ID'si uyuşmazsa
                }

                // Şifreyi hash'leyelim
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    user.PasswordHash = HashPassword(user.PasswordHash); // Şifreyi hash'le
                }

                user.UpdateDate = DateTime.Now;
                await _userService.UpdateUserAsync(objectId, user); // Kullanıcıyı güncelle
                return RedirectToAction(nameof(Index)); // Listeye yönlendiriyoruz
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı düzenlenirken hata oluştu.");
                ModelState.AddModelError(string.Empty, $"Bir hata oluştu: {ex.Message}");
                await LoadCompaniesAsync(user.CompanyId);
                return View("~/Views/User/Edit.cshtml", user);
            }
        }

        // Şifreyi hash'leme fonksiyonu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Şirket verilerini yükleyen yardımcı fonksiyon
        private async Task LoadCompaniesAsync(int? selectedCompanyId = null)
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            ViewBag.Companies = new SelectList(companies, "CompanyId", "Name", selectedCompanyId);

            // Roller için seçim kutusunu tekrar ekliyoruz
            ViewBag.Roles = new SelectList(new[]
            {
                new { Value = 1, Text = "User" },
                new { Value = 2, Text = "Owner" },
                new { Value = 3, Text = "Admin" }
            }, "Value", "Text");
        }
    }
}
