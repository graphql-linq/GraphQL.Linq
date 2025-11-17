// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace TechMart.EfCore2;

public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}
