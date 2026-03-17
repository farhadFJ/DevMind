using DevMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevMind.Api.Data;

public class AppDbContext : DbContext
{
    // The constructor receives DbContext options from Program.cs
    // and passes them to the base DbContext class.

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
        // Each DbSet<T> usually represents one table in the database.
    // If you create a new model and want EF Core to map it as a table,
    // you usually add a new DbSet here.
    public DbSet<ChatConversation> ChatConversations { get; set;  }  // This creates/maps the ChatConversations table.
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
         // Configure the relationship between ChatConversation and ChatMessage:
        // One ChatConversation has many Messages.
        modelBuilder.Entity<ChatConversation>()
            .HasMany(c => c.Messages)// A single conversation can contain many messages.
            .WithOne(m => m.ChatConversation)// Each message belongs to exactly one conversation.
            .HasForeignKey(m => m.ChatConversationId)// The foreign key is stored in ChatMessage table.
            .OnDelete(DeleteBehavior.Cascade);// If a conversation is deleted, all related messages are deleted too.

         // Configure the Title property of ChatConversation.
        modelBuilder.Entity<ChatConversation>()
            .Property(c => c.Title) // Select the Title column/property.
            .HasMaxLength(200); // Limit the maximum length of Title to 200 characters.


        modelBuilder.Entity<ChatMessage>()
            .Property(m => m.Role)
            .HasMaxLength(50);
    }
}