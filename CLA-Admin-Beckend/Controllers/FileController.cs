using Microsoft.AspNetCore.Mvc;
using Shared.Lib.Services.FileStorage;
using CLA.Admin.Beckend;
using static CLA.Admin.Beckend.ProcessService;

namespace Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {

        private readonly ILogger<FileController> _logger;
        private readonly ProcessService _processService;
        private readonly InputStorageService _storeService;

        public FileController(ILogger<FileController> logger, ProcessService processService, InputStorageService storeService)
        {
            _logger = logger;
            _processService = processService;
            _storeService = storeService;
        }

        [HttpPost(Name = "UploadFile")]
        public async Task<IActionResult> Upload([FromForm] FormFileModel model, CancellationToken cancellationToken)
        {
            var result = await _storeService.PutFileAsync(model.File.OpenReadStream(), model.File.FileName, new(), null, cancellationToken);

            if (result)
            {
                var versionId = await _storeService.GetVersionId(model.File.FileName);
                var processModel = new ProcessModel
                {
                    FileName = model.File.FileName,
                    Status = ProcessStatusType.Processing,
                    VersionId = versionId!
                };

                _processService.Add(processModel);
                return Ok(processModel);
            }
            return StatusCode(500);
        }

        [HttpGet(Name = "FileStatus")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return Ok(_processService.List);
        }

        public class FormFileModel
        {
            [System.ComponentModel.DataAnnotations.Required] public IFormFile File { get; set; }
        }
    }
}