using Discord;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stupify.Data;
using Discord.OAuth2;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication.Cookies;
using Stupify.Data.Encryption;
using Stupify.Data.Models;

namespace StupifyWebsite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSqlDatabase(Configuration["SQL:ConnectionString"]);
            services.AddRepositories();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = DiscordDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie()
                .AddDiscord(options =>
            {
                options.AppId = Configuration["Discord:AppId"];
                options.AppSecret = Configuration["Discord:AppSecret"];
                options.Scope.Add("guilds");
            });

            services.AddSingleton<IDiscordClient>(sp =>
            {
                var client = new DiscordRestClient();
                client.LoginAsync(TokenType.Bot, Configuration["Discord:BotUserToken"]).ConfigureAwait(false).GetAwaiter().GetResult();
                return client;
            });

            services.AddSingleton((sp) => new AesCryptography(Configuration["SQL:EncryptionPassword"]));

            services.AddOptions();

            services.Configure<SpotifyOptions>(options =>
            {
                options.AppId = Configuration["Spotify:AppId"];
                options.AppSecret = Configuration["Spotify:AppSecret"];
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
