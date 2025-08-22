using System.Threading.Tasks;

namespace Hypesoft.Infrastructure.Persistence.Migrations;

public interface IMongoDbMigration
{
    int Version { get; }
    string Description { get; }
    Task Up(ApplicationDbContext context);
    Task Down(ApplicationDbContext context);
}
