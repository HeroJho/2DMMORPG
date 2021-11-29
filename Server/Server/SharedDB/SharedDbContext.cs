using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedDB
{
    public class SharedDbContext : DbContext
    {
        public DbSet<TokenDb> Tokens { get; set; }
        public DbSet<ServerDb> Servers { get; set; }

        // GameServer >> API를 만들 때 생성자가 묻혀서 만들어 줬다고 이해
        public SharedDbContext()
        {

        }

        // ASP.NET(API버전)
        public SharedDbContext(DbContextOptions<SharedDbContext> options): base(options)
        {
            
        }

        // static으로 해서 주소를 웹 서버에서 공통적으로 사용할 수 있게 함
        public static string ConnectionString { get; set; } = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SharedDbPractice;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // API가 먼저 사용중이면 
            if(options.IsConfigured == false)
            {
                options
                    .UseSqlServer(ConnectionString);
            }

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TokenDb>()
                .HasIndex(t => t.AccountDbId)
                .IsUnique();

            builder.Entity<ServerDb>()
                .HasIndex(s => s.Name)
                .IsUnique();
        }

    }
}
