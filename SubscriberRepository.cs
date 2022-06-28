using Bogus;
using Microsoft.Extensions.Configuration;

namespace BulkEmail;

// pretend this repository uses a database
public class SubscriberRepository
{
    private const int SubscriberCount = 2345;
    private readonly List<Person> subscribers;

    public SubscriberRepository(IConfiguration configuration)
    {
        var toEmailAddressTemplate = configuration["ToEmailTemplate"] 
                                     ?? throw new Exception("ToEmailTemplate is not configured.");
        
        subscribers = new Faker<Person>()
            .CustomInstantiator(f => new Person())
            .RuleFor(p => p.Email, e => string.Format(toEmailAddressTemplate, e.IndexFaker))
            .Generate(SubscriberCount);
    }
    
    public int Count() => SubscriberCount;

    public IEnumerable<Person> GetAll() => subscribers;
    
    public IEnumerable<Person> GetByPage(int pageSize, int pageIndex) 
        => subscribers.Skip(pageSize * pageIndex).Take(pageSize);
}