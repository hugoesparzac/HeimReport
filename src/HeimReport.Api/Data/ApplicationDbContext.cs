using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SurveyInstance> SurveyInstances => Set<SurveyInstance>();
    public DbSet<SurveyTemplate> SurveyTemplates => Set<SurveyTemplate>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}