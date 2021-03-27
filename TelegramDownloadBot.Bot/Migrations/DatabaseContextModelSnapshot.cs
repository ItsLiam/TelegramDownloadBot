﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TelegramDownloadBot.Bot.Data;

namespace TelegramDownloadBot.Bot.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.4");

            modelBuilder.Entity("TelegramDownloadBot.Bot.Data.CachedSearchResponse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OptionNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SearchResponseId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SearchResponseId");

                    b.ToTable("CachedSearchResponses");
                });

            modelBuilder.Entity("TelegramDownloadBot.Bot.Data.SearchResponse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("MagnetUrl")
                        .HasColumnType("TEXT");

                    b.Property<uint>("NumOfFiles")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Peers")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Seeders")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Uploaded")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SearchResponses");
                });

            modelBuilder.Entity("TelegramDownloadBot.Bot.Data.CachedSearchResponse", b =>
                {
                    b.HasOne("TelegramDownloadBot.Bot.Data.SearchResponse", "SearchResponse")
                        .WithMany()
                        .HasForeignKey("SearchResponseId");

                    b.Navigation("SearchResponse");
                });
#pragma warning restore 612, 618
        }
    }
}
