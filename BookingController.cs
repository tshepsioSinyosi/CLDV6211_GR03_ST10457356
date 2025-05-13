using EventSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly EventBookingDbContext _context;

        public BookingController(EventBookingDbContext context)
        {
            _context = context;
        }

        // GET: Booking
        public async Task<IActionResult> Index(string searchString)
        {
            var bookings = _context.Booking
                .Include(b => b.Event)
                    .ThenInclude(e => e.Venue) // 👈 Include Venue through Event
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.CustomerName.Contains(searchString) ||
                    b.Event.EventName.Contains(searchString) ||
                    b.Event.Venue.VenueName.Contains(searchString)); // 👈 Access Venue through Event
            }

            return View(await bookings.ToListAsync());
        }


        // GET: Booking/Create
        public IActionResult Create()
        {
            // Fetch events to display in the dropdown
            ViewBag.EventId = new SelectList(_context.Event, "EventID", "EventName"); // Assuming EventName is available
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            // STEP 1: Fetch the event being booked
            var selectedEvent = await _context.Event.FirstOrDefaultAsync(e => e.EventID == booking.EventID);

            if (selectedEvent == null)
            {
                ModelState.AddModelError("EventID", "Selected event not found.");
            }
            else
            {
                // STEP 2: Check if this event has already been booked
                bool isAlreadyBooked = await _context.Booking
                    .AnyAsync(b => b.EventID == booking.EventID);

                if (isAlreadyBooked)
                {
                    ModelState.AddModelError("EventID", "This event has already been booked.");
                }
            }

            // STEP 3: If model is invalid, return with event dropdown repopulated
            if (!ModelState.IsValid)
            {
                ViewBag.EventId = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
                return View(booking);
            }

            // STEP 4: Save booking
            _context.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        // GET: Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Booking booking)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Booking.Any(b => b.BookingID == booking.BookingID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // GET: Booking/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking.FirstOrDefaultAsync(b => b.BookingID == id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var eventObj = await _context.Event
                .FirstOrDefaultAsync(e => e.EventID == id); // This looks up the event

            if (eventObj == null)
                return NotFound();

            return View(eventObj);
        }


    }
}

