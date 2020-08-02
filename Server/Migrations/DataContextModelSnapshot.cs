﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.Data;

namespace Server.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.6");

            modelBuilder.Entity("Server.Data.Models.Subscription", b =>
                {
                    b.Property<int>("SubscriptionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("IntervalSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StationId")
                        .HasColumnType("TEXT");

                    b.HasKey("SubscriptionId");

                    b.ToTable("Subscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
