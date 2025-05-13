using Azure.Storage.Blobs;
using EventSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
 

namespace EventSystem.Controllers
{
    public class VenueController : Controller
    {
        private readonly BlobService _blobService;
        private readonly EventBookingDbContext _context;

        public VenueController(EventBookingDbContext context , IConfiguration config)
        {
          
            _context = context;
            _blobService = new BlobService(config);
        }

        // GET: Venue
        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venues.ToListAsync();
            return View(venues);
        }
        public IActionResult create ()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Upload the file to Blob Storage and get the URL
                    venue.ImageURL = await _blobService.UploadFileAsync(imageFile);
                }

                // Add the venue to the database
                _context.Add(venue);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(venue); // Return the view with errors if model is invalid
        }


        // GET: Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue, IFormFile imageFile)
        {
            if (id != venue.VenueID)
                return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                // Upload the new image to Blob Storage and update the URL
                venue.ImageURL = await _blobService.UploadFileAsync(imageFile);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Venue updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Venues.Any(v => v.VenueID == venue.VenueID))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(venue); // Return the view with errors if model is invalid
        }
        // GET: Venue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueID == id);
            if (venue == null) return NotFound();

            return View(venue);
        }
        // GET: Venue/Details/5
        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.VenueID == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Step 1: Fetch the venue by its ID
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound(); // If the venue doesn't exist
            }

            // Step 2: Check if there are any events that reference this venue
            var relatedEvents = await _context.Event
                .AnyAsync(e => e.VenueID == venue.VenueID);

            // Step 3: If there are events associated with the venue, notify the user and prevent deletion
            if (relatedEvents)
            {
                TempData["ErrorMessage"] = "This venue cannot be deleted because there are events associated with it.";
                return RedirectToAction(nameof(Index)); // Redirect back to the index page
            }

            // Step 4: If no events are found, proceed with the deletion of the venue
            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();

            // Step 5: Notify the user that the venue has been successfully deleted
            TempData["SuccessMessage"] = "The venue has been successfully deleted.";

            return RedirectToAction(nameof(Index)); // Redirect to the index page after successful deletion
        }
        public async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
        {
            // Check if the file is not null and has content
            if (imageFile != null && imageFile.Length > 0)
            {
                // Use var for type inference
                var connectionString = "your_connection_string_here"; // Replace with your actual connection string
                var containerName = "your_container_name_here"; // Replace with your actual container name

                // Create a BlobServiceClient to interact with the Blob service
                var blobServiceClient = new BlobServiceClient(connectionString);

                // Get a reference to the container and create it if it doesn't exist
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                // Generate a unique file name using a GUID
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                // Get a reference to the blob client and upload the file
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = imageFile.OpenReadStream())
                {
                    // Upload the file to Blob storage
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                // Return the URL of the uploaded file
                return blobClient.Uri.ToString();
            }
            else
            {
                // If the file is null or empty, return an empty string or handle error accordingly
                return string.Empty;
            }
        }

    }
}
