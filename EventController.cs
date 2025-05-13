using EventSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventSystem.Controllers
{
    public class EventController : Controller
    {
        private readonly EventBookingDbContext _context;

        public EventController(EventBookingDbContext context)
        {
            _context = context;
        }

        // GET: Event
        public async Task<IActionResult> Index()
        {
            var events = await _context.Event.ToListAsync(); // Asynchronous call to fetch all events
            return View(events);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventObj)
        {
            if (!ModelState.IsValid)
                return View(eventObj);

            // Get all events on the same date and venue
            var existingEvents = await _context.Event
                .Where(e => e.EventDate.Date == eventObj.EventDate.Date && e.VenueID == eventObj.VenueID)
                .ToListAsync();

            foreach (var existing in existingEvents)
            {
                // Only check if both events have valid times
                if (existing.StartTime.HasValue && existing.EndTime.HasValue &&
                    eventObj.StartTime.HasValue && eventObj.EndTime.HasValue)
                {
                    bool isOverlap =
                        eventObj.StartTime < existing.EndTime &&
                        eventObj.EndTime > existing.StartTime;

                    if (isOverlap)
                    {
                        ModelState.AddModelError("", "There is already an event scheduled at this venue that overlaps with your selected time.");
                        return View(eventObj);
                    }
                }
            }

            try
            {
                _context.Add(eventObj);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the event: " + ex.Message);
                return View(eventObj);
            }
        }


        // GET: Event/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var eventObj = await _context.Event.FindAsync(id); // Find event by ID asynchronously
            if (eventObj == null)
                return NotFound();

            return View(eventObj); // Return the event to the Edit view
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventObj)
        {
            if (id != eventObj.EventID)
                return NotFound();

            if (!ModelState.IsValid)
                return View(eventObj);

            // Get all events at the same venue on the same date, excluding the one being edited
            var existingEvents = await _context.Event
                .Where(e => e.EventID != eventObj.EventID &&
                            e.EventDate.Date == eventObj.EventDate.Date &&
                            e.VenueID == eventObj.VenueID)
                .ToListAsync();

            foreach (var existing in existingEvents)
            {
                if (existing.StartTime.HasValue && existing.EndTime.HasValue &&
                    eventObj.StartTime.HasValue && eventObj.EndTime.HasValue)
                {
                    bool isOverlap =
                        eventObj.StartTime < existing.EndTime &&
                        eventObj.EndTime > existing.StartTime;

                    if (isOverlap)
                    {
                        ModelState.AddModelError("", "This event update causes a time conflict with another event at the same venue.");
                        return View(eventObj);
                    }
                }
            }

            try
            {
                _context.Update(eventObj);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Event.Any(e => e.EventID == eventObj.EventID))
                    return NotFound();
                else
                    throw;
            }
        }


        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var eventObj = await _context.Event
                .FirstOrDefaultAsync(e => e.EventID == id); // Find the event to delete asynchronously
            if (eventObj == null)
                return NotFound();

            return View(eventObj); // Return the event to the Delete view
        }

        // POST: Event/Delete/5
        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventObj = await _context.Event.FindAsync(id);

            if (eventObj == null)
                return NotFound();

            // ✅ Check for bookings before deleting
            bool hasBookings = await _context.Booking.AnyAsync(b => b.EventID == id);

            if (hasBookings)
            {
                TempData["DeleteError"] = "❌ This event cannot be deleted because it has existing bookings.";
                return RedirectToAction(nameof(Delete), new { id }); // Redirect back to Delete view
            }

            _context.Event.Remove(eventObj);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }



    }
}
