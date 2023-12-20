namespace api_s3_aws.Model
{
    public class Archive
    {
        public string? Title { get; set; }
        public string? Path { get; set; }
        public string? Base64String { get; set; }
        public string? ContentType { get; set; }
        public string? Bucket { get; set; }
        public string? Key { get; set; }


        public Archive()
        {

        }

        public Archive(string title, string path)
        {
            Title = title;
            Path = path;
        }

        public Archive(string title, string path, string base64, string contentType)
        {
            Title = title;
            Path = path;
            Base64String = base64;
            ContentType = contentType;
        }
    }
}
