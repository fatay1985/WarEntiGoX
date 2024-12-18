using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WarEntiGox.Models;
using WarEntiGox.Services;
using Microsoft.AspNetCore.Authorization; // Authorize namespace
using System.Threading.Tasks;

namespace WarEntiGox.Controllers.Aut
{
    [Authorize] // Bu sınıfın tüm aksiyonlarına yalnızca giriş yapmış kullanıcılar erişebilir.
    public class SentenceController : Controller
    {
        private readonly SentenceService _sentenceService;

        public SentenceController(SentenceService sentenceService)
        {
            _sentenceService = sentenceService;
        }

        // GET: Sentence/Update/{id}
        [HttpGet]
        public async Task<IActionResult> Update(ObjectId id)
        {
            var sentence = await _sentenceService.GetSentenceByIdAsync(id);
            if (sentence == null)
                return NotFound();

            return View(sentence);
        }

        // POST: Sentence/Update/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ObjectId id, string newContent)
        {
            if (ModelState.IsValid)
            {
                await _sentenceService.UpdateSentenceAsync(id, newContent);
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}