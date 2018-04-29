﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Stupify.Data.SQL;
using System;

namespace Stupify.Data.Migrations
{
    [DbContext(typeof(BotContext))]
    [Migration("20180422164553_adding-some-settings-for-guilds")]
    partial class addingsomesettingsforguilds
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Stupify.Data.SQL.Models.CustomCommand", b =>
                {
                    b.Property<int>("CustomCommandId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Command");

                    b.Property<string>("CommandTag");

                    b.Property<int?>("ServerUserId");

                    b.HasKey("CustomCommandId");

                    b.HasIndex("ServerUserId");

                    b.ToTable("CustomCommands");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.Quote", b =>
                {
                    b.Property<int>("QuoteId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("QuoteBody");

                    b.Property<int>("ServerUserId");

                    b.HasKey("QuoteId");

                    b.HasIndex("ServerUserId");

                    b.ToTable("Quotes");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.Segment", b =>
                {
                    b.Property<int>("SegmentId")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("EnergyPerTick");

                    b.Property<decimal>("UnitsPerTick");

                    b.Property<int?>("UserId");

                    b.HasKey("SegmentId");

                    b.HasIndex("UserId");

                    b.ToTable("Segments");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.SegmentTemplate", b =>
                {
                    b.Property<int>("SegmentTemplateId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int?>("UserId");

                    b.HasKey("SegmentTemplateId");

                    b.HasIndex("UserId");

                    b.ToTable("SegmentTemplates");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.Server", b =>
                {
                    b.Property<int>("ServerId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("DiscordGuildId");

                    b.Property<bool>("StoryInProgress");

                    b.Property<long?>("TwitchUpdateChannel");

                    b.HasKey("ServerId");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.ServerSettings", b =>
                {
                    b.Property<int>("ServerSettingsId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CommandPrefix");

                    b.Property<string>("CustomCommandPrefix");

                    b.Property<int?>("ServerId");

                    b.HasKey("ServerSettingsId");

                    b.HasIndex("ServerId");

                    b.ToTable("ServerSettings");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.ServerStory", b =>
                {
                    b.Property<int>("ServerStoryId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("EndTime");

                    b.Property<int?>("ServerId");

                    b.Property<DateTime>("StartTime");

                    b.Property<int?>("StoryInitiatedByServerUserId");

                    b.HasKey("ServerStoryId");

                    b.HasIndex("ServerId");

                    b.HasIndex("StoryInitiatedByServerUserId");

                    b.ToTable("ServerStories");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.ServerStoryPart", b =>
                {
                    b.Property<int>("ServerStoryPartId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Part");

                    b.Property<int?>("PartAuthorServerUserId");

                    b.Property<int>("PartNumber");

                    b.Property<int?>("ServerStoryId");

                    b.Property<DateTime>("TimeOfAddition");

                    b.HasKey("ServerStoryPartId");

                    b.HasIndex("PartAuthorServerUserId");

                    b.HasIndex("ServerStoryId");

                    b.ToTable("ServerStoryParts");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.ServerTwitchChannel", b =>
                {
                    b.Property<int>("ServerTwitchChannelId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("LastStatus");

                    b.Property<int?>("ServerId");

                    b.Property<string>("TwitchLoginName");

                    b.HasKey("ServerTwitchChannelId");

                    b.HasIndex("ServerId");

                    b.ToTable("ServerTwitchChannels");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.ServerUser", b =>
                {
                    b.Property<int>("ServerUserId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Muted");

                    b.Property<int>("ServerId");

                    b.Property<int>("UserId");

                    b.HasKey("ServerUserId");

                    b.HasIndex("ServerId");

                    b.HasIndex("UserId");

                    b.ToTable("ServerUsers");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Balance");

                    b.Property<long>("DiscordUserId");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.UserSettings", b =>
                {
                    b.Property<int>("UserSettingsId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("UserId");

                    b.HasKey("UserSettingsId");

                    b.HasIndex("UserId");

                    b.ToTable("UserSettings");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.CustomCommand", b =>
                {
                    b.HasOne("Stupify.Data.SQL.Models.ServerUser", "ServerUser")
                        .WithMany()
                        .HasForeignKey("ServerUserId");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.Quote", b =>
                {
                    b.HasOne("Stupify.Data.SQL.Models.ServerUser", "ServerUser")
                        .WithMany("Quotes")
                        .HasForeignKey("ServerUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.Segment", b =>
                {
                    b.HasOne("Stupify.Data.SQL.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.SegmentTemplate", b =>
                {
                    b.HasOne("Stupify.Data.SQL.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.ServerSettings", b =>
                {
                    b.HasOne("Stupify.Data.SQL.Models.Server", "Server")
                        .WithMany()
                        .HasForeignKey("ServerId");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.ServerStory", b =>
                {
                    b.HasOne("Stupify.Data.SQL.Models.Server", "Server")
                        .WithMany("ServerStories")
                        .HasForeignKey("ServerId");

                    b.HasOne("Stupify.Data.SQL.Models.ServerUser", "StoryInitiatedBy")
                        .WithMany()
                        .HasForeignKey("StoryInitiatedByServerUserId");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.ServerStoryPart", b =>
                {
                    b.HasOne("Stupify.Data.SQL.Models.ServerUser", "PartAuthor")
                        .WithMany()
                        .HasForeignKey("PartAuthorServerUserId");

                    b.HasOne("Stupify.Data.SQL.Models.ServerStory", "ServerStory")
                        .WithMany("ServerStoryParts")
                        .HasForeignKey("ServerStoryId");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.ServerTwitchChannel", b =>
                {
                    b.HasOne("Stupify.Data.SQL.Models.Server", "Server")
                        .WithMany()
                        .HasForeignKey("ServerId");
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.ServerUser", b =>
                {
                    b.HasOne("Stupify.Data.SQL.Models.Server", "Server")
                        .WithMany("ServerUsers")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Stupify.Data.SQL.Models.User", "User")
                        .WithMany("ServerUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Stupify.Data.SQL.Models.UserSettings", b =>
                {
                    b.HasOne("Stupify.Data.SQL.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });
#pragma warning restore 612, 618
        }
    }
}
