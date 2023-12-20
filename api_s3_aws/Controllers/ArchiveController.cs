using api_s3_aws.Model;
using api_s3_aws.Service;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace api_s3_aws.Controllers
{
    [Route("api/[controller]")]    
    [ApiController]
    public class ArchiveController : Controller
    {
        [HttpGet]
        [Route("ListArchive")]
        public async Task<IActionResult> ListArchive(string bucket)
        {
            try
            {
                AmazomS3Service amazom = new AmazomS3Service();

                var list = await amazom.ListFile(bucket);
                return Ok(list);

            }catch (Exception ex)
            {
                return BadRequest($"Erro ao listar objetos no bucket: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("UploadForm")]
        [ProducesResponseType(typeof(ArquivoVw), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> UploadForm([FromForm] ArquivoVw arquivoForm /*[FromForm] IFormFile arquivo*/)
        {
            try
            {
                //arquivoForm.Title = arquivo.FileName.ToString();
                var amazom = new AmazomS3Service();
                var key = "midias/" + Guid.NewGuid();

                //fazendo o upload do arquivo usando o serviço s3
                var uploadFile = await amazom.UploadForm("api-salao-cliente", key, arquivoForm.Path);

                //lançar
                if (!uploadFile) throw new Exception("Falha ao add arquivo");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("UploadBase64")]
        [ProducesResponseType(typeof(Archive), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UploadBase64([FromForm] Archive model)
        {
            try
            {
                var amazom = new AmazomS3Service();

                //vamos definir a chave que é o nome do arquivo e do bucket
                var key = "midias/" + Guid.NewGuid();
                var bucket = "api-salao-cliente";

                //vamos fazer o upLoad do arquivo
                var upload = await amazom.UploadBase64(bucket, key, model.Base64String, model.ContentType);

                if (!upload)
                {
                    return BadRequest("Falha ao fazer o upload do arquivo em Base64");
                }

                return Ok();
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("UploadBinary")]
        public async Task<IActionResult>UploadBinary(Archive model)
        {
            try
            {
                AmazomS3Service amazom = new AmazomS3Service();

                var bucket = "api-salao-cliente";
                var key = "midias/" + Guid.NewGuid();

                byte[] data = Convert.FromBase64String(model.Base64String);

                //converte 

                var upload = await amazom.UploadBinary(bucket, key, data, model.ContentType);

                if (upload)
                {
                    return Ok("Upload bem-sucedido para o Amazon S3.");
                }
                else
                {
                    return BadRequest("Falha ao fazer o upload para o Amazon S3.");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("DownloadFile")]
        public async Task<IActionResult> DownloadFile(string key)
        {
            try
            {
                var amazom = new AmazomS3Service();

                var bucket = "api-salao-cliente";

                var download = await amazom.DownloadFileAsync(bucket, key);

                // Verifique se o arquivo foi encontrado no S3
                if (download == null || download.Length == 0)
                {
                    return NotFound("Arquivo não encontrado no S3");
                }

                // Defina o tipo de conteúdo apropriado para a resposta
                var contentType = "application/octet-stream"; // Pode variar dependendo do tipo de arquivo
                var fileName = "nome-do-arquivo-no-download"; // Substitua pelo nome desejado


                return File(download, contentType, fileName);

            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao baixar o arquivo: {ex.Message}");
            }

            
        }
    }

    public class ArquivoVw
    {
        public string? Title { get; set; }
        public IFormFile? Path { get; set; }
    }
}
