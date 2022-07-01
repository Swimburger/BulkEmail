using Bogus;
using Microsoft.Extensions.Configuration;

namespace BulkEmail;

// pretend this repository uses a database
public class SubscriberRepository
{
    private readonly int subscriberCount;
    private readonly string toEmailAddressTemplate;

    public SubscriberRepository(IConfiguration configuration)
    {
        subscriberCount = int.Parse(configuration["SubscriberCount"] 
            ?? throw new Exception("ToEmailTemplate is not configured."));
        
        toEmailAddressTemplate = configuration["ToEmailTemplate"] 
            ?? throw new Exception("ToEmailTemplate is not configured.");
    }
    
    public int Count() => subscriberCount;

    public IEnumerable<Person> GetAll() => GenerateSubscribers();
    
    public IEnumerable<Person> GetByPage(int pageSize, int pageIndex) 
        => GenerateSubscribers().Skip(pageSize * pageIndex).Take(pageSize);

    private IEnumerable<Person> GenerateSubscribers()
    {
        return new Faker<Person>()
            .CustomInstantiator(f => new Person())
            .RuleFor(p => p.Email, e => string.Format(toEmailAddressTemplate, e.IndexFaker))
            .GenerateLazy(subscriberCount);
    }
}