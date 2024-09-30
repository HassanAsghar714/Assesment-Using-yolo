using Assesment.Models;
using Assesment.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Diagnostics;
using System.Text.Json;

namespace Assesment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FileUtility _fileUtility;
        private readonly ImageProcessing _imageProcessing;

        public HomeController(ILogger<HomeController> logger, FileUtility fileUtility, ImageProcessing imageProcessing)
        {
            _logger = logger;
            _fileUtility = fileUtility;
            _imageProcessing = imageProcessing;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile formFile)
        {
            try
            {
                string path = await _fileUtility.UploadFileAsync(formFile);
                var res = await _imageProcessing.PerformObjectDetection(path);
                TempData["Response"] = JsonSerializer.Serialize(res);
                //return Ok($"File Uploaded Successfully.\n Response :: \n  {JsonSerializer.Serialize(res)}");
                return View("~/Views/Home/Index.cshtml");
            }
            catch (Exception ex)
            {
                return BadRequest("There has been an error while uploading the file");

            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
