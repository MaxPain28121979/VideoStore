using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoStore.Data;
using VideoStore.Models;

namespace VideoStore.Controllers
{
    public class VideosController : Controller
    {
        private readonly AppDbContext _db;

        public VideosController(AppDbContext db)
        {
            _db = db;
        }

        // GET: Videos
        public async Task<IActionResult> Index()
        {
            var videos = await _db.Videos.ToListAsync();
            return View(videos);
        }

        // GET: Details
        public async Task<IActionResult> Details(int id)
        {
            var video = await _db.Videos.FindAsync(id);
            if (video == null) return NotFound();
            return View(video);
        }

        // GET: Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Video model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Videos.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var video = await _db.Videos.FindAsync(id);
            if (video == null) return NotFound();
            return View(video);
        }

        // POST: Edit
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Video model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            _db.Videos.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var video = await _db.Videos.FindAsync(id);
            if (video == null) return NotFound();
            return View(video);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var video = await _db.Videos.FindAsync(id);
            if (video == null) return NotFound();
            _db.Videos.Remove(video);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
