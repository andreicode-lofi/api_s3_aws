using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using api_s3_aws.Model;

namespace api_s3_aws.Service
{
    public class AmazomS3Service
    {
        // Aqui estão as credenciais de acesso à sua conta AWS.
        public string AwsKeyId { get; set; }
        public string AwsKeySecret { get; set; }

        // Essa variável armazenará as credenciais que serão usadas para autenticar sua aplicação na AWS.
        public BasicAWSCredentials AwsCredentials { get; private set; }

        // Essa variável é o cliente para o serviço AWS S3(com credenciais e configuração).
        private readonly IAmazonS3 _awsS3Client;

        // O construtor é chamado quando você cria um objeto AmazonS3Service.
        public AmazomS3Service()
        {
            // Configure as credenciais da AWS.
            AwsKeyId = "***************";//aqui vc colocara a sua key
            AwsKeySecret = "**********************************************";//aqui vc colocara a sua secret key
            AwsCredentials = new BasicAWSCredentials(AwsKeyId, AwsKeySecret);

            // Crie um objeto BasicAWSCredentials com as credenciais.
            var config = new AmazonS3Config
            {
                // Configure a região da AWS para a América do Sul (São Paulo).
                RegionEndpoint = RegionEndpoint.SAEast1
            };

            //Crie o cliente aws s3 com as credenciais e configuração.
            _awsS3Client = new AmazonS3Client(AwsCredentials, config);
        }

        //base64
        public async Task<bool>UploadBase64(string bucket, string key, string stringBase64, string contentType)
        {
            //preciso converter a string base64 em um arrayu de bytes
            byte[] bytes = Convert.FromBase64String(stringBase64);

            //vamos criar  um fluxo de memoria a parti dos bytes
            using var Memoria = new MemoryStream(bytes);

            //agora vamos cria um utilitario de transferencia para fazer o upload
            var trasnferir = new TransferUtility(_awsS3Client);

            //Enviar os arquivos para o s3
            await trasnferir.UploadAsync(new TransferUtilityUploadRequest
            {
                //passando informações do meu bucket
                InputStream = Memoria,
                Key = key,
                BucketName = bucket,
                ContentType = contentType
            });

            return true;
        }

        //binary
        public async Task<bool> UploadBinary(string bucket, string key, byte[] data, string contentType)
        {
            // Cria um fluxo de memória para armazenar os dados binários a serem enviados
            using var memoria = new MemoryStream(data);

            // Cria uma instância de TransferUtility para fazer o upload de arquivos para o Amazon S3.
            var transferir = new TransferUtility(_awsS3Client);

            // Faz o upload dos dados para o Amazon S3
            await transferir.UploadAsync(new TransferUtilityUploadRequest
            {
                InputStream = memoria,
                Key = key,
                BucketName = bucket,
                ContentType = contentType
            });

            return true;
        }

        //form
        public async Task<bool> UploadForm(string bucket, string key, IFormFile file)
        {
            // Crie um fluxo de memória para armazenar o conteúdo do arquivo.
            using var newMemoryStream = new MemoryStream();
            file.CopyTo(newMemoryStream);

            // Crie um utilitário de transferência para fazer o upload do arquivo.
            var fileTransferUtility = new TransferUtility(_awsS3Client);

            // Envie o arquivo para o AWS S3 com as informações necessárias. 
            await fileTransferUtility.UploadAsync(new TransferUtilityUploadRequest
            { 
                InputStream = newMemoryStream,
                Key = key,
                BucketName = bucket,
                ContentType = file.ContentType
            });

            return true;
        }

        public async Task<byte[]> DownloadFileAsync(string bucket, string key)
        {
            // Use a instância do cliente S3 para fazer o download do arquivo
            var getObjectResponse = await _awsS3Client.GetObjectAsync(bucket, key);

            // Verifique se o objeto foi encontrado.
            if (getObjectResponse != null && getObjectResponse.ResponseStream != null)
            {
                // Crie um MemoryStream para armazenar o conteúdo do arquivo baixado.
                using var memoryStream = new MemoryStream();

                // Copie o conteúdo do objeto S3 para o MemoryStream.
                await getObjectResponse.ResponseStream.CopyToAsync(memoryStream);

                // Converta o MemoryStream em um array de bytes e retorne.
                return memoryStream.ToArray();
            }
            else
            {
                throw new Exception("Arquivo não encontrado no S3");
            }
        }

        public async Task<List<Archive>> ListFile(string bucket)
        {
            // Cria uma lista para armazenar objetos Archive.
            List<Archive> list = new List<Archive>();

            // Cria uma solicitação para listar objetos em um bucket do Amazon S3.
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = bucket 
            };

            // Declara uma resposta para armazenar os resultados da solicitação.
            ListObjectsV2Response response;

            // Inicia um loop para lidar com resultados paginados.
            do
            {
                // Faz uma chamada assíncrona para listar objetos no bucket.
                response = await _awsS3Client.ListObjectsV2Async(request);

                // Itera pelos objetos encontrados na resposta
                foreach (var entrey in response.S3Objects)
                {
                    Archive archive = new Archive
                    {
                        Title = entrey.Key,
                        Path = entrey.Key,
                        Bucket = bucket,
                        Key = entrey.Key,
                    };

                    // Adiciona o objeto Archive à lista.
                    list.Add(archive);  
                }

                // Define o token de continuação para a próxima página (se houver).
                request.ContinuationToken = response.NextContinuationToken;

            } while (response.IsTruncated);

            return list;
        }




    }


}
