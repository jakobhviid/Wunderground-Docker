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

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("IntervalSeconds")
                        .HasColumnType("INTEGER");

                    b.HasKey("SubscriptionId");

                    b.ToTable("Subscriptions");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Subscription");
                });

            modelBuilder.Entity("Server.Data.Models.CurrentConditionSubscription", b =>
                {
                    b.HasBaseType("Server.Data.Models.Subscription");

                    b.Property<string>("StationId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue("CurrentConditionSubscription");
                });

            modelBuilder.Entity("Server.Data.Models.ForecastSubscription", b =>
                {
                    b.HasBaseType("Server.Data.Models.Subscription");

                    b.Property<string>("GeoCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue("ForecastSubscription");
                });
#pragma warning restore 612, 618
        }
    }
}
