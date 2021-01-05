﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Stexchange.Data;

namespace Stexchange.Migrations
{
    [DbContext(typeof(Database))]
    partial class DatabaseModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Stexchange.Data.Models.Block", b =>
                {
                    b.Property<long>("BlockedId")
                        .HasColumnName("blocked_id")
                        .HasColumnType("bigint(20) unsigned");

                    b.Property<long>("BlockerId")
                        .HasColumnName("blocker_id")
                        .HasColumnType("bigint(20) unsigned");

                    b.HasKey("BlockedId", "BlockerId");

                    b.ToTable("Blocks");
                });

            modelBuilder.Entity("Stexchange.Data.Models.Chat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("serial");

                    b.Property<long>("AdId")
                        .HasColumnName("ad_id")
                        .HasColumnType("bigint(20) unsigned");

                    b.Property<long>("ResponderId")
                        .HasColumnName("responder_id")
                        .HasColumnType("bigint(20) unsigned");

                    b.HasKey("Id");

                    b.HasAlternateKey("AdId", "ResponderId");

                    b.HasIndex("ResponderId");

                    b.ToTable("Chats");
                });

            modelBuilder.Entity("Stexchange.Data.Models.Filter", b =>
                {
                    b.Property<string>("Value")
                        .HasColumnName("value")
                        .HasColumnType("varchar(30)");

                    b.HasKey("Value");

                    b.ToTable("Filters");
                });

            modelBuilder.Entity("Stexchange.Data.Models.FilterListing", b =>
                {
                    b.Property<long>("ListingId")
                        .HasColumnName("listing_id")
                        .HasColumnType("bigint(20) unsigned");

                    b.Property<string>("Value")
                        .HasColumnName("filter_value")
                        .HasColumnType("varchar(30)");

                    b.HasKey("ListingId", "Value");

                    b.HasIndex("Value");

                    b.ToTable("FilterListings");
                });

            modelBuilder.Entity("Stexchange.Data.Models.ImageData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("serial");

                    b.Property<byte[]>("Image")
                        .IsRequired()
                        .HasColumnName("image")
                        .HasColumnType("LONGBLOB");

                    b.Property<long>("ListingId")
                        .HasColumnName("listing_id")
                        .HasColumnType("bigint(20) unsigned");

                    b.HasKey("Id");

                    b.HasIndex("ListingId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("Stexchange.Data.Models.Listing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("serial");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnName("description")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("last_modified")
                        .HasColumnType("timestamp");

                    b.Property<string>("NameLatin")
                        .HasColumnName("name_lt")
                        .HasColumnType("varchar(50)");

                    b.Property<string>("NameNl")
                        .IsRequired()
                        .HasColumnName("name_nl")
                        .HasColumnType("varchar(50)");

                    b.Property<int>("Quantity")
                        .HasColumnName("quantity")
                        .HasColumnType("int unsigned");

                    b.Property<bool>("Renewed")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("renewed")
                        .HasColumnType("bit(1)")
                        .HasDefaultValue(false);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasColumnType("varchar(80)");

                    b.Property<long>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("bigint(20) unsigned");

                    b.Property<bool>("Visible")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("visible")
                        .HasColumnType("bit(1)")
                        .HasDefaultValue(true);

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Listings");
                });

            modelBuilder.Entity("Stexchange.Data.Models.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("serial");

                    b.Property<long>("ChatId")
                        .HasColumnName("chat_id")
                        .HasColumnType("bigint(20) unsigned");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnName("content")
                        .HasColumnType("varchar(1024)");

                    b.Property<long>("SenderId")
                        .HasColumnName("sender")
                        .HasColumnType("bigint(20) unsigned");

                    b.Property<DateTime>("Timestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.HasIndex("SenderId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Stexchange.Data.Models.Rating", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("serial");

                    b.Property<byte>("Communication")
                        .HasColumnName("quantity")
                        .HasColumnType("tinyint unsigned");

                    b.Property<byte?>("Quality")
                        .HasColumnName("quality")
                        .HasColumnType("tinyint unsigned");

                    b.Property<long>("RevieweeId")
                        .HasColumnName("reviewee")
                        .HasColumnType("bigint(20) unsigned");

                    b.Property<long>("ReviewerId")
                        .HasColumnName("reviewer")
                        .HasColumnType("bigint(20) unsigned");

                    b.HasKey("Id");

                    b.HasIndex("RevieweeId");

                    b.HasIndex("ReviewerId");

                    b.ToTable("Ratings");
                });

            modelBuilder.Entity("Stexchange.Data.Models.RatingRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("serial");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasColumnType("datetime");

                    b.Property<long>("RevieweeId")
                        .HasColumnName("reviewee")
                        .HasColumnType("bigint(20) unsigned");

                    b.Property<long>("ReviewerId")
                        .HasColumnName("reviewer")
                        .HasColumnType("bigint(20) unsigned");

                    b.HasKey("Id");

                    b.HasIndex("RevieweeId");

                    b.HasIndex("ReviewerId");

                    b.ToTable("RatingRequests");
                });

            modelBuilder.Entity("Stexchange.Data.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("serial");

                    b.Property<DateTime>("Created_At")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasColumnType("datetime");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnName("email")
                        .HasColumnType("varchar(254)");

                    b.Property<bool>("IsVerified")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("verified")
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<byte[]>("Password")
                        .IsRequired()
                        .HasColumnName("password")
                        .HasColumnType("varbinary(64)");

                    b.Property<string>("Postal_Code")
                        .IsRequired()
                        .HasColumnName("postal_code")
                        .HasColumnType("char(6)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnName("username")
                        .HasColumnType("varchar(15)");

                    b.HasKey("Id");

                    b.HasAlternateKey("Email");

                    b.HasAlternateKey("Username");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Stexchange.Data.Models.UserVerification", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("user_id")
                        .HasColumnType("serial");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp");

                    b.Property<byte[]>("Guid")
                        .IsRequired()
                        .HasColumnName("verification_code")
                        .HasColumnType("varbinary(16)");

                    b.HasKey("Id");

                    b.HasIndex("Guid")
                        .IsUnique();

                    b.ToTable("UserVerifications");
                });

            modelBuilder.Entity("Stexchange.Data.Models.Block", b =>
                {
                    b.HasOne("Stexchange.Data.Models.User", "Blocked")
                        .WithMany()
                        .HasForeignKey("BlockedId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Stexchange.Data.Models.Chat", b =>
                {
                    b.HasOne("Stexchange.Data.Models.Listing", "Listing")
                        .WithMany()
                        .HasForeignKey("AdId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Stexchange.Data.Models.User", "Responder")
                        .WithMany()
                        .HasForeignKey("ResponderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Stexchange.Data.Models.FilterListing", b =>
                {
                    b.HasOne("Stexchange.Data.Models.Listing", "Listing")
                        .WithMany()
                        .HasForeignKey("ListingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Stexchange.Data.Models.Filter", "Filter")
                        .WithMany()
                        .HasForeignKey("Value")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Stexchange.Data.Models.ImageData", b =>
                {
                    b.HasOne("Stexchange.Data.Models.Listing", "Listing")
                        .WithMany("Pictures")
                        .HasForeignKey("ListingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Stexchange.Data.Models.Listing", b =>
                {
                    b.HasOne("Stexchange.Data.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Stexchange.Data.Models.Message", b =>
                {
                    b.HasOne("Stexchange.Data.Models.Chat", null)
                        .WithMany("Messages")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Stexchange.Data.Models.User", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Stexchange.Data.Models.Rating", b =>
                {
                    b.HasOne("Stexchange.Data.Models.User", "Reviewee")
                        .WithMany()
                        .HasForeignKey("RevieweeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Stexchange.Data.Models.User", "Reviewer")
                        .WithMany()
                        .HasForeignKey("ReviewerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Stexchange.Data.Models.RatingRequest", b =>
                {
                    b.HasOne("Stexchange.Data.Models.User", "Reviewee")
                        .WithMany()
                        .HasForeignKey("RevieweeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Stexchange.Data.Models.User", "Reviewer")
                        .WithMany()
                        .HasForeignKey("ReviewerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Stexchange.Data.Models.UserVerification", b =>
                {
                    b.HasOne("Stexchange.Data.Models.User", "User")
                        .WithOne("Verification")
                        .HasForeignKey("Stexchange.Data.Models.UserVerification", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
