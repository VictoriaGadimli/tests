namespace ProjectX.Model;

public class TokenDto
{
    public string AccessToken { get; set; }
    public long ExpiresIn { get; set; }
    public string RefreshToken { get; set; }
}