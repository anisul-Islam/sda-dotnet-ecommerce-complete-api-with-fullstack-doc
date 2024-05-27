namespace api.Helpers
{
    public class Helper
    {
        public static string GenerateSlug(string name)
        {
            return name.ToLower().Replace(" ", "-");
        }
    }
}