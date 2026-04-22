using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.Extensions.Logging;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils;

namespace ProjectManagementApi.Controllers
{
    /// <summary>
    /// Controlador para la gestión de descarga de archivos adjuntos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentRepository<Attachment> _attachmentRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<AttachmentsController> _logger;

        public AttachmentsController(
            IAttachmentRepository<Models.Attachment> attachmentRepository,
            IFileService fileService,
            ILogger<AttachmentsController> logger)
        {
            _attachmentRepository = attachmentRepository;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Method to download an attachment by id
        /// </summary>
        /// <param name="id">Attachment id to download</param>
        /// <returns>File stream if found, 404 otherwise</returns>
        [HttpGet("{id}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var attachment = await _attachmentRepository.GetByIdAsync(id);
                if (attachment == null)
                {
                    return NotFound(new { message = $"Attachment with id '{id}' not found" });
                }

                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    attachment.FilePath
                );

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound(new { message = "El archivo solicitado no existe" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(fileBytes, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment with id {Id}", id);
                return ApiErrorResponse.BadRequest(this, ex, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Method to delete an attachment by id
        /// </summary>
        /// <param name="id">Attachment id to delete</param>
        /// <returns>200 if deleted, 404 if not found</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var attachment = await _attachmentRepository.GetByIdAsync(id);
                if (attachment == null)
                {
                    return NotFound(new { message = $"Attachment with id '{id}' not found" });
                }

                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    attachment.FilePath
                );

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                await _attachmentRepository.DeleteAttachmentAsync(id);

                return Ok(new { message = "Archivo eliminado satisfactoriamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment with id {Id}", id);
                return ApiErrorResponse.BadRequest(this, ex, "Ocurrió un error al procesar la solicitud.");
            }
        }
    }
}
