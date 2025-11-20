namespace MiniHR.Application.POCOs.VOs
{
    public class UserVO
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
