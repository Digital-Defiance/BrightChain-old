using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using FileContextCore;
using FileContextCore.FileManager;
using FileContextCore.Serializer;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BrightChain.Contexts
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            //Default: JSON-Serializer
            optionsBuilder.UseFileContextDatabase<JSONSerializer, DefaultFileManager>();// optionsBuilder.UseFileContextDatabase<JSONSerializer, DefaultFileManager>();// optionsBuilder.UseFileContextDatabase<BSONSerializer, DefaultFileManager>();//JSON-Serialize + simple Encryption// optionsBuilder.UseFileContextDatabase<JSONSerializer, EncryptedFileManager>();//XML// optionsBuilder.UseFileContextDatabase<XMLSerializer, DefaultFileManager>();// optionsBuilder.UseFileContextDatabase<XMLSerializer, PrivateFileManager>();//CSV// optionsBuilder.UseFileContextDatabase<CSVSerializer, DefaultFileManager>();//Custom location// optionsBuilder.UseFileContextDatabase(location: @"D:\t");//Excel// ExcelPackage.LicenseContext = LicenseContext.NonCommercial;// optionsBuilder.UseFileContextDatabase<EXCELStoreManager>(databaseName: "test");

        public DbSet<Block> Blocks { get; set; }
        public IDbConnection Connection => this.Database.GetDbConnection();
    }
}
