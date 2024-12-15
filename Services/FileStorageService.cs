
namespace AonFreelancing.Services
{
    public class FileStorageService
    {
        public static readonly string ROOT = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        public async Task<string> SaveAsync(IFormFile formFile)
        {
            string fileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(formFile.FileName)}";
            using Stream stream = File.Create(Path.Combine(ROOT, fileName));
            await formFile.CopyToAsync(stream);
            return fileName;
        }
        public void Delete(string fileName)
        {
            File.Delete(Path.Combine(ROOT, fileName));
        }
    }
}