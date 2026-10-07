namespace CashStack.Services;

public class LoggerService
{
    private readonly string _folder = "logs";

    public LoggerService(){
        
    if (!Directory.Exists(_folder)){
        Directory.CreateDirectory(_folder);
    }
}
    }