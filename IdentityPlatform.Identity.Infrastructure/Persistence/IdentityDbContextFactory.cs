using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Persistence
{
    //public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
    //{
    //    public IdentityDbContext CreateDbContext(string[] args)
    //    {
    //        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();

    //        // ضع هنا نص الاتصال الخاص بقاعدة البيانات لديك (Connection String)
    //        var connectionString = "Server=DESKTOP-SARA\\SQLEXPRESS;Database=DDDIdentityPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true;Trusted_Connection=True;TrustServerCertificate=True;";

    //        // (تأكد من مطابقة UseSqlServer مع مزود قاعدة البيانات الذي تستخدمه)
    //        optionsBuilder.UseSqlServer(connectionString);

    //        return new IdentityDbContext(optionsBuilder.Options);
    //    }
    //}
}
