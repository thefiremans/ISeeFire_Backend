﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NASATest2018;

namespace NASATest2018.Migrations
{
    [DbContext(typeof(IsfContext))]
    [Migration("20181021093040_addedDistance")]
    partial class addedDistance
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("NASATest2018.NasaFireReport", b =>
                {
                    b.Property<int>("NasaFireReportId")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Confidence");

                    b.Property<decimal>("Latitude");

                    b.Property<decimal>("Longitude");

                    b.Property<DateTime>("Timestamp");

                    b.HasKey("NasaFireReportId");

                    b.ToTable("NasaFireReports");
                });

            modelBuilder.Entity("NASATest2018.Report", b =>
                {
                    b.Property<int>("ReportId")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Distance");

                    b.Property<string>("ImagePath");

                    b.Property<decimal>("Latitude");

                    b.Property<decimal>("Longitude");

                    b.Property<string>("SecretUserId");

                    b.Property<string>("TextOfComment");

                    b.Property<DateTime>("Timestamp");

                    b.HasKey("ReportId");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("NASATest2018.User", b =>
                {
                    b.Property<string>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Phone");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
