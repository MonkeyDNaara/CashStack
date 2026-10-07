namespace CashStack.Services;

public class StorageService
{
    private readonly string _folder = "data";

    public StorageService(){
        
    if (!Directory.Exists(_folder)){
        Directory.CreateDirectory(_folder);
    }
}
    }