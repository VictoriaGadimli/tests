namespace ProjectX.Model;

public class Response
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
}

public class Response<T> : Response
{
    public bool isSuccess;
    public T Data { get; set; }
}

public class UserProfileDto
{
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string Gender { get; set; }
    public string CountryKey { get; set; }
}