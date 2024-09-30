namespace Assesment.Util
{
    public class FileUtility
    {
        private readonly string basePath;
        public FileUtility(IConfiguration config)
        {
            basePath = config.GetValue<string>("baseUploadPath");
        }

        public async Task<string> UploadFileAsync(IFormFile document)
        {
            string path = Path.Combine(basePath, document.FileName);
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            using Stream stream = new FileStream(path, FileMode.Create);
            await document.CopyToAsync(stream);

            return path;
        }
    }
}
