﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Portal.Infrastructure;

#nullable disable

namespace Portal.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240123101951_AddAvatar168")]
    partial class AddAvatar168
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Portal.Domain.AggregatesModel.AlbumAggregate.Album", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AlbumAlertMessageId")
                        .HasColumnType("int");

                    b.Property<int>("AlbumStatus")
                        .HasColumnType("int");

                    b.Property<string>("AlternativeName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ArtitstNames")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AuthorNames")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CdnOriginalUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CdnThumbnailUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FriendlyName")
                        .HasColumnType("varchar(350)");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<string>("OriginalUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReleaseYear")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Tags")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ThumbnailUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedOnUtc")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("AlbumAlertMessageId");

                    b.HasIndex("FriendlyName")
                        .IsUnique()
                        .HasFilter("[FriendlyName] IS NOT NULL");

                    b.ToTable("Album", (string)null);
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.AlbumAggregate.AlbumAlertMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedOnUtc")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("AlbumAlertMessage", (string)null);
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.AlbumAggregate.AlbumContentType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlbumId")
                        .HasColumnType("int");

                    b.Property<int>("ContentTypeId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("UpdatedOnUtc")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("AlbumId");

                    b.HasIndex("ContentTypeId");

                    b.ToTable("AlbumContentType", (string)null);
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.AlbumAggregate.ContentType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedOnUtc")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("ContentType", (string)null);
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.CollectionAggregate.Collection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlbumId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExtendName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FriendlyName")
                        .HasColumnType("varchar(350)");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Volume")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlbumId", "FriendlyName")
                        .IsUnique()
                        .HasDatabaseName("UC_AlbumId_FriendlyName")
                        .HasFilter("[FriendlyName] IS NOT NULL");

                    b.ToTable("Collection", (string)null);
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.CollectionAggregate.Comment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlbumId")
                        .HasColumnType("int");

                    b.Property<int?>("CollectionId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int?>("ParentCommentId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlbumId");

                    b.HasIndex("CollectionId");

                    b.HasIndex("ParentCommentId");

                    b.HasIndex("UserId");

                    b.ToTable("Comment", (string)null);
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.CollectionAggregate.ContentItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CollectionId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("DisplayUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("OrderBy")
                        .HasColumnType("int");

                    b.Property<string>("OriginalUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RelativeUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedOnUtc")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.ToTable("ContentItem", (string)null);
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.UserAggregate.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Avatar")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IdentityUserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("UpdatedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("IdentityUserId")
                        .IsUnique();

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("User", (string)null);
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.AlbumAggregate.Album", b =>
                {
                    b.HasOne("Portal.Domain.AggregatesModel.AlbumAggregate.AlbumAlertMessage", "AlbumAlertMessage")
                        .WithMany("Albums")
                        .HasForeignKey("AlbumAlertMessageId");

                    b.Navigation("AlbumAlertMessage");
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.AlbumAggregate.AlbumContentType", b =>
                {
                    b.HasOne("Portal.Domain.AggregatesModel.AlbumAggregate.Album", "Album")
                        .WithMany("AlbumContentTypes")
                        .HasForeignKey("AlbumId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Portal.Domain.AggregatesModel.AlbumAggregate.ContentType", "ContentType")
                        .WithMany("AlbumContentTypes")
                        .HasForeignKey("ContentTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Album");

                    b.Navigation("ContentType");
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.CollectionAggregate.Collection", b =>
                {
                    b.HasOne("Portal.Domain.AggregatesModel.AlbumAggregate.Album", "Album")
                        .WithMany("Collections")
                        .HasForeignKey("AlbumId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Album");
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.CollectionAggregate.Comment", b =>
                {
                    b.HasOne("Portal.Domain.AggregatesModel.AlbumAggregate.Album", "Album")
                        .WithMany("Comments")
                        .HasForeignKey("AlbumId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Portal.Domain.AggregatesModel.CollectionAggregate.Collection", "Collection")
                        .WithMany("Comments")
                        .HasForeignKey("CollectionId");

                    b.HasOne("Portal.Domain.AggregatesModel.CollectionAggregate.Comment", "ParentComment")
                        .WithMany("Replies")
                        .HasForeignKey("ParentCommentId");

                    b.HasOne("Portal.Domain.AggregatesModel.UserAggregate.User", "User")
                        .WithMany("Comments")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Album");

                    b.Navigation("Collection");

                    b.Navigation("ParentComment");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.CollectionAggregate.ContentItem", b =>
                {
                    b.HasOne("Portal.Domain.AggregatesModel.CollectionAggregate.Collection", "Collection")
                        .WithMany("ContentItems")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Collection");
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.AlbumAggregate.Album", b =>
                {
                    b.Navigation("AlbumContentTypes");

                    b.Navigation("Collections");

                    b.Navigation("Comments");
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.AlbumAggregate.AlbumAlertMessage", b =>
                {
                    b.Navigation("Albums");
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.AlbumAggregate.ContentType", b =>
                {
                    b.Navigation("AlbumContentTypes");
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.CollectionAggregate.Collection", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("ContentItems");
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.CollectionAggregate.Comment", b =>
                {
                    b.Navigation("Replies");
                });

            modelBuilder.Entity("Portal.Domain.AggregatesModel.UserAggregate.User", b =>
                {
                    b.Navigation("Comments");
                });
#pragma warning restore 612, 618
        }
    }
}
