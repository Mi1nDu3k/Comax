
public class AppSettings
{
    public string SecretKey { get; set; } = "THIS_IS_A_SECRET_KEY_CHANGE_IT"; //
    public int ExpireMinutes { get; set; } = 60; //


    public string MinioBaseUrl { get; set; } = "http://localhost:9000/comics-bucket";
}