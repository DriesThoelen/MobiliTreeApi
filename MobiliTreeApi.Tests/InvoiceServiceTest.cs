using System;
using System.Linq;
using MobiliTreeApi.Repositories;
using MobiliTreeApi.Services;
using Xunit;

namespace MobiliTreeApi.Tests
{
    public class InvoiceServiceTest
    {
        private readonly ISessionsRepository _sessionsRepository;
        private readonly IParkingFacilityRepository _parkingFacilityRepository;
        private readonly ICustomerRepository _customerRepository;

        public InvoiceServiceTest()
        {
            _sessionsRepository = new SessionsRepositoryFake(FakeData.GetSeedSessions());
            _parkingFacilityRepository = new ParkingFacilityRepositoryFake(FakeData.GetSeedServiceProfiles());
            _customerRepository = new CustomerRepositoryFake(FakeData.GetSeedCustomers());
        }

        [Fact]
        public void GivenSessionsService_WhenQueriedForInexistentParkingFacility_ThenThrowException()
        {
            var ex = Assert.Throws<ArgumentException>(() => GetSut().GetInvoices("nonExistingParkingFacilityId"));
            Assert.Equal("Invalid parking facility id 'nonExistingParkingFacilityId'", ex.Message);
        }

        [Fact]
        public void GivenEmptySessionsStore_WhenQueriedForUnknownParkingFacility_ThenReturnEmptyInvoiceList()
        {
            var result = GetSut().GetInvoices("pf001");

            Assert.Empty(result);
        }

        [Fact]
        public void GivenOneSessionInTheStore_WhenQueriedForExistingParkingFacility_ThenReturnInvoiceListWithOneElement()
        {
            var startDateTime = new DateTime(2018, 12, 15, 12, 25, 0);
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = "c001",
                ParkingFacilityId = "pf001",
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1)
            });

            var result = GetSut().GetInvoices("pf001");
            
            var invoice = Assert.Single(result);
            Assert.NotNull(invoice);
            Assert.Equal("pf001", invoice.ParkingFacilityId);
            Assert.Equal("c001", invoice.CustomerId);
        }

        [Fact]
        public void GivenMultipleSessionsInTheStore_WhenQueriedForExistingParkingFacility_ThenReturnOneInvoicePerCustomer()
        {
            var startDateTime = new DateTime(2018, 12, 15, 12, 25, 0);
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = "c001",
                ParkingFacilityId = "pf001",
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1)
            });
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = "c001",
                ParkingFacilityId = "pf001",
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1)
            });
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = "c002",
                ParkingFacilityId = "pf001",
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1)
            });

            var result = GetSut().GetInvoices("pf001");

            Assert.Equal(2, result.Count);
            var invoiceCust1 = result.SingleOrDefault(x => x.CustomerId == "c001");
            var invoiceCust2 = result.SingleOrDefault(x => x.CustomerId == "c002");
            Assert.NotNull(invoiceCust1);
            Assert.NotNull(invoiceCust2);
            Assert.Equal("pf001", invoiceCust1.ParkingFacilityId);
            Assert.Equal("pf001", invoiceCust2.ParkingFacilityId);
            Assert.Equal("c001", invoiceCust1.CustomerId);
            Assert.Equal("c002", invoiceCust2.CustomerId);
        }

        [Fact]
        public void GivenMultipleSessionsForMultipleFacilitiesInTheStore_WhenQueriedForExistingParkingFacility_ThenReturnInvoicesOnlyForQueriedFacility()
        {
            var startDateTime = new DateTime(2018, 12, 15, 12, 25, 0);
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = "c001",
                ParkingFacilityId = "pf001",
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1)
            });
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = "c001",
                ParkingFacilityId = "pf002",
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1)
            });
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = "c002",
                ParkingFacilityId = "pf001",
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1)
            });

            var result = GetSut().GetInvoices("pf001");

            Assert.Equal(2, result.Count);
            var invoiceCust1 = result.SingleOrDefault(x => x.CustomerId == "c001");
            var invoiceCust2 = result.SingleOrDefault(x => x.CustomerId == "c002");
            Assert.NotNull(invoiceCust1);
            Assert.NotNull(invoiceCust2);
            Assert.Equal("pf001", invoiceCust1.ParkingFacilityId);
            Assert.Equal("pf001", invoiceCust2.ParkingFacilityId);
            Assert.Equal("c001", invoiceCust1.CustomerId);
            Assert.Equal("c002", invoiceCust2.CustomerId);
        }

        [Fact]
        public void GetInvoice_Should_Return_Correct_Invoice()
        {
            var startDateTime = new DateTime(2018, 12, 15, 12, 25, 0);
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = "c001",
                ParkingFacilityId = "pf001",
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1)
            });
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = "c001",
                ParkingFacilityId = "pf002",
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1)
            });
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = "c002",
                ParkingFacilityId = "pf001",
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(1)
            });

            var result = GetSut().GetInvoice("pf001", "c002");

            Assert.NotNull(result);
            Assert.Equal("pf001", result.ParkingFacilityId);
            Assert.Equal("c002", result.CustomerId);
        }

        [Theory]
        [ClassData(typeof(InvoiceTheoryData))]
        public void GetInvoices_Should_Return_Correct_Price(DateTime startDateTime, double durationInHours, string customerId, string parkingFacilityId, decimal expectedPrice)
        {
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = customerId,
                ParkingFacilityId = parkingFacilityId,
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(durationInHours)
            });

            var result = GetSut().GetInvoices(parkingFacilityId);

            Assert.NotNull(result);
            var invoiceCust = result.SingleOrDefault(x => x.CustomerId == customerId);
            Assert.NotNull(invoiceCust);
            Assert.Equal(parkingFacilityId, invoiceCust.ParkingFacilityId);
            Assert.Equal(customerId, invoiceCust.CustomerId);
            Assert.Equal(expectedPrice, invoiceCust.Amount);
        }

        [Theory]
        [ClassData(typeof(InvoiceTheoryData))]
        public void GetInvoice_Should_Return_Correct_Price(DateTime startDateTime, double durationInHours, string customerId, string parkingFacilityId, decimal expectedPrice)
        {
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = customerId,
                ParkingFacilityId = parkingFacilityId,
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(durationInHours)
            });

            var result = GetSut().GetInvoice(parkingFacilityId, customerId);

            Assert.NotNull(result);
            Assert.Equal(parkingFacilityId, result.ParkingFacilityId);
            Assert.Equal(customerId, result.CustomerId);
            Assert.Equal(expectedPrice, result.Amount);
        }


        private class InvoiceTheoryData : TheoryData<DateTime, double, string, string, decimal>
        {
            public InvoiceTheoryData()
            {
                //Within one timeslot
                //1 Hour
                //WeekDays abo
                Add(new DateTime(2018, 12, 14, 0, 0, 0), 1, "c002", "pf001", 0.5M);
                Add(new DateTime(2018, 12, 14, 0, 0, 0), 1, "c002", "pf002", 0.5M);
                Add(new DateTime(2018, 12, 14, 12, 0, 0), 1, "c002", "pf001", 2.5M);
                Add(new DateTime(2018, 12, 14, 12, 0, 0), 1, "c002", "pf002", 2.5M);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 1, "c002", "pf001", 1.5M);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 1, "c002", "pf002", 1.5M);

                //Weekend abo
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 1, "c002", "pf001", 0.8M);
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 1, "c002", "pf002", 0.8M);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 1, "c002", "pf001", 2.8M);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 1, "c002", "pf002", 2.8M);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 1, "c002", "pf001", 1.8M);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 1, "c002", "pf002", 1.8M);

                //WeekDays non-abo
                Add(new DateTime(2018, 12, 14, 0, 0, 0), 1, "c004", "pf001", 1.5M);
                Add(new DateTime(2018, 12, 14, 0, 0, 0), 1, "c004", "pf002", 1.5M);
                Add(new DateTime(2018, 12, 14, 12, 0, 0), 1, "c004", "pf001", 3.5M);
                Add(new DateTime(2018, 12, 14, 12, 0, 0), 1, "c004", "pf002", 3.5M);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 1, "c004", "pf001", 2.5M);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 1, "c004", "pf002", 2.5M);

                //Weekend non-abo
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 1, "c004", "pf001", 1.8M);
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 1, "c004", "pf002", 1.8M);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 1, "c004", "pf001", 3.8M);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 1, "c004", "pf002", 3.8M);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 1, "c004", "pf001", 2.8M);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 1, "c004", "pf002", 2.8M);

                //2 Hour
                //WeekDays abo
                Add(new DateTime(2018, 12, 14, 0, 0, 0), 2, "c002", "pf001", 1M);
                Add(new DateTime(2018, 12, 14, 0, 0, 0), 2, "c002", "pf002", 1M);
                Add(new DateTime(2018, 12, 14, 12, 0, 0), 2, "c002", "pf001", 5M);
                Add(new DateTime(2018, 12, 14, 12, 0, 0), 2, "c002", "pf002", 5M);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 2, "c002", "pf001", 3M);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 2, "c002", "pf002", 3M);

                //Weekend abo
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 2, "c002", "pf001", 1.6M);
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 2, "c002", "pf002", 1.6M);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 2, "c002", "pf001", 5.6M);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 2, "c002", "pf002", 5.6M);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 2, "c002", "pf001", 3.6M);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 2, "c002", "pf002", 3.6M);

                //WeekDays non-abo
                Add(new DateTime(2018, 12, 14, 0, 0, 0), 2, "c004", "pf001", 3M);
                Add(new DateTime(2018, 12, 14, 0, 0, 0), 2, "c004", "pf002", 3M);
                Add(new DateTime(2018, 12, 14, 12, 0, 0), 2, "c004", "pf001", 7M);
                Add(new DateTime(2018, 12, 14, 12, 0, 0), 2, "c004", "pf002", 7M);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 2, "c004", "pf001", 5M);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 2, "c004", "pf002", 5M);

                //Weekend non-abo
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 2, "c004", "pf001", 3.6M);
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 2, "c004", "pf002", 3.6M);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 2, "c004", "pf001", 7.6M);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 2, "c004", "pf002", 7.6M);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 2, "c004", "pf001", 5.6M);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 2, "c004", "pf002", 5.6M);


                //Within two or more timeslots
                //10 Hour
                //WeekDays abo
                Add(new DateTime(2018, 12, 13, 6, 30, 0), 12, "c002", "pf001", 28.5M);

                Add(new DateTime(2018, 12, 13, 0, 0, 0), 10, "c002", "pf001", 0.5M * 7 + 2.5M * 3);
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 10, "c002", "pf002", 0.5M * 8 + 2.5M * 2);
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 10, "c002", "pf001", 2.5M * 6 + 1.5M * 4);
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 10, "c002", "pf002", 2.5M * 5 + 1.5M * 5);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 10, "c002", "pf001", 1.5M * 6 + 0.5M * 4);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 10, "c002", "pf002", 1.5M * 6 + 0.5M * 4);

                //Weekend abo
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 10, "c002", "pf001", 0.8M * 7 + 2.8M * 3);
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 10, "c002", "pf002", 0.8M * 8 + 2.8M * 2);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 10, "c002", "pf001", 2.8M * 6 + 1.8M * 4);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 10, "c002", "pf002", 2.8M * 5 + 1.8M * 5);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 10, "c002", "pf001", 1.8M * 6 + 0.8M * 4);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 10, "c002", "pf002", 1.8M * 6 + 0.8M * 4);

                //WeekDays non-abo
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 10, "c004", "pf001", 1.5M * 7 + 3.5M * 3);
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 10, "c004", "pf002", 1.5M * 8 + 3.5M * 2);
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 10, "c004", "pf001", 3.5M * 6 + 2.5M * 4);
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 10, "c004", "pf002", 3.5M * 5 + 2.5M * 5);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 10, "c004", "pf001", 2.5M * 6 + 1.5M * 4);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 10, "c004", "pf002", 2.5M * 6 + 1.5M * 4);

                //Weekend non-abo
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 10, "c004", "pf001", 1.8M * 7 + 3.8M * 3);
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 10, "c004", "pf002", 1.8M * 8 + 3.8M * 2);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 10, "c004", "pf001", 3.8M * 6 + 2.8M * 4);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 10, "c004", "pf002", 3.8M * 5 + 2.8M * 5);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 10, "c004", "pf001", 2.8M * 6 + 1.8M * 4);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 10, "c004", "pf002", 2.8M * 6 + 1.8M * 4);

                //Over multiple days
                //24 Hour
                //WeekDays abo
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 24, "c002", "pf001", 0.5M * 7 + 2.5M * 11 + 1.5M * 6);
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 24, "c002", "pf002", 0.5M * 8 + 2.5M * 9 + 1.5M * 7);
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 24, "c002", "pf001", 2.5M * 6 + 1.5M * 6 + 0.5M * 7 + 2.5M * 5);
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 24, "c002", "pf002", 2.5M * 5 + 1.5M * 7 + 0.5M * 8 + 2.5M * 4);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 24, "c002", "pf001", 1.5M * 6 + 0.5M * 7 + 2.5M * 11);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 24, "c002", "pf002", 1.5M * 6 + 0.5M * 8 + 2.5M * 9 + 1.5M * 1);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 24, "c002", "pf001", 1.5M * 6 + 0.8M * 7 + 2.8M * 11); //weekday to weekend
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 24, "c002", "pf002", 1.5M * 6 + 0.8M * 8 + 2.8M * 9 + 1.8M * 1); //weekday to weekend

                //Weekend abo
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 24, "c002", "pf001", 0.8M * 7 + 2.8M * 11 + 1.8M * 6);
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 24, "c002", "pf002", 0.8M * 8 + 2.8M * 9 + 1.8M * 7);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 24, "c002", "pf001", 2.8M * 6 + 1.8M * 6 + 0.8M * 7 + 2.8M * 5);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 24, "c002", "pf002", 2.8M * 5 + 1.8M * 7 + 0.8M * 8 + 2.8M * 4);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 24, "c002", "pf001", 1.8M * 6 + 0.8M * 7 + 2.8M * 11);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 24, "c002", "pf002", 1.8M * 6 + 0.8M * 8 + 2.8M * 9 + 1.8M * 1);
                Add(new DateTime(2018, 12, 16, 18, 0, 0), 24, "c002", "pf001", 1.8M * 6 + 0.5M * 7 + 2.5M * 11); //weekend to weekday
                Add(new DateTime(2018, 12, 16, 18, 0, 0), 24, "c002", "pf002", 1.8M * 6 + 0.5M * 8 + 2.5M * 9 + 1.5M * 1); //weekend to weekday

                //WeekDays non-abo
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 24, "c004", "pf001", 1.5M * 7 + 3.5M * 11 + 2.5M * 6);
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 24, "c004", "pf002", 1.5M * 8 + 3.5M * 9 + 2.5M * 7);
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 24, "c004", "pf001", 3.5M * 6 + 2.5M * 6 + 1.5M * 7 + 3.5M * 5);
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 24, "c004", "pf002", 3.5M * 5 + 2.5M * 7 + 1.5M * 8 + 3.5M * 4);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 24, "c004", "pf001", 2.5M * 6 + 1.5M * 7 + 3.5M * 11);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 24, "c004", "pf002", 2.5M * 6 + 1.5M * 8 + 3.5M * 9 + 2.5M * 1);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 24, "c004", "pf001", 2.5M * 6 + 1.8M * 7 + 3.8M * 11); //weekday to weekend
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 24, "c004", "pf002", 2.5M * 6 + 1.8M * 8 + 3.8M * 9 + 2.8M * 1); //weekday to weekend

                //Weekend non-abo
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 24, "c004", "pf001", 1.8M * 7 + 3.8M * 11 + 2.8M * 6);
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 24, "c004", "pf002", 1.8M * 8 + 3.8M * 9 + 2.8M * 7);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 24, "c004", "pf001", 3.8M * 6 + 2.8M * 6 + 1.8M * 7 + 3.8M * 5);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 24, "c004", "pf002", 3.8M * 5 + 2.8M * 7 + 1.8M * 8 + 3.8M * 4);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 24, "c004", "pf001", 2.8M * 6 + 1.8M * 7 + 3.8M * 11);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 24, "c004", "pf002", 2.8M * 6 + 1.8M * 8 + 3.8M * 9 + 2.8M * 1);
                Add(new DateTime(2018, 12, 16, 18, 0, 0), 24, "c004", "pf001", 2.8M * 6 + 1.5M * 7 + 3.5M * 11); //weekend to weekday
                Add(new DateTime(2018, 12, 16, 18, 0, 0), 24, "c004", "pf002", 2.8M * 6 + 1.5M * 8 + 3.5M * 9 + 2.5M * 1); //weekend to weekday


                //An entire week
                //168 Hours
                //WeekDays abo
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 168, "c002", "pf001", 5 * (0.5M * 7 + 2.5M * 11 + 1.5M * 6) + 2 * (0.8M * 7 + 2.8M * 11 + 1.8M * 6));
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 168, "c002", "pf002", 5 * (0.5M * 8 + 2.5M * 9 + 1.5M * 7) + 2 * (0.8M * 8 + 2.8M * 9 + 1.8M * 7));
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 168, "c002", "pf001", 2.5M * 6 + 1.5M * 6 + 4 * (0.5M * 7 + 2.5M * 11 + 1.5M * 6) + 2 * (0.8M * 7 + 2.8M * 11 + 1.8M * 6) + 0.5M * 7 + 2.5M * 5);
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 168, "c002", "pf002", 2.5M * 5 + 1.5M * 7 + 4 * (0.5M * 8 + 2.5M * 9 + 1.5M * 7) + 2 * (0.8M * 8 + 2.8M * 9 + 1.8M * 7) + 0.5M * 8 + 2.5M * 4);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 168, "c002", "pf001", 1.5M * 6 + 4 * (0.5M * 7 + 2.5M * 11 + 1.5M * 6) + 2 * (0.8M * 7 + 2.8M * 11 + 1.8M * 6) + 0.5M * 7 + 2.5M * 11);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 168, "c002", "pf002", 1.5M * 6 + 4 * (0.5M * 8 + 2.5M * 9 + 1.5M * 7) + 2 * (0.8M * 8 + 2.8M * 9 + 1.8M * 7) + 0.5M * 8 + 2.5M * 9 + 1.5M * 1);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 168, "c002", "pf001", 1.5M * 6 + 4 * (0.5M * 7 + 2.5M * 11 + 1.5M * 6) + 2 * (0.8M * 7 + 2.8M * 11 + 1.8M * 6) + 0.5M * 7 + 2.5M * 11); //weekday to weekend
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 168, "c002", "pf002", 1.5M * 6 + 4 * (0.5M * 8 + 2.5M * 9 + 1.5M * 7) + 2 * (0.8M * 8 + 2.8M * 9 + 1.8M * 7) + 0.5M * 8 + 2.5M * 9 + 1.5M * 1); //weekday to weekend

                //Weekend abo
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 168, "c002", "pf001", 2 * (0.8M * 7 + 2.8M * 11 + 1.8M * 6) + 5 * (0.5M * 7 + 2.5M * 11 + 1.5M * 6));
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 168, "c002", "pf002", 2 * (0.8M * 8 + 2.8M * 9 + 1.8M * 7) + 5 * (0.5M * 8 + 2.5M * 9 + 1.5M * 7));
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 168, "c002", "pf001", 2.8M * 6 + 1.8M * 6 + 5 * (0.5M * 7 + 2.5M * 11 + 1.5M * 6) + 1 * (0.8M * 7 + 2.8M * 11 + 1.8M * 6) + 0.8M * 7 + 2.8M * 5);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 168, "c002", "pf002", 2.8M * 5 + 1.8M * 7 + 5 * (0.5M * 8 + 2.5M * 9 + 1.5M * 7) + 1 * (0.8M * 8 + 2.8M * 9 + 1.8M * 7) + 0.8M * 8 + 2.8M * 4);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 168, "c002", "pf001", 1.8M * 6 + 5 * (0.5M * 7 + 2.5M * 11 + 1.5M * 6) + 1 * (0.8M * 7 + 2.8M * 11 + 1.8M * 6) + 0.8M * 7 + 2.8M * 11);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 168, "c002", "pf002", 1.8M * 6 + 5 * (0.5M * 8 + 2.5M * 9 + 1.5M * 7) + 1 * (0.8M * 8 + 2.8M * 9 + 1.8M * 7) + 0.8M * 8 + 2.8M * 9 + 1.8M * 1);
                Add(new DateTime(2018, 12, 16, 18, 0, 0), 168, "c002", "pf001", 1.8M * 6 + 5 * (0.5M * 7 + 2.5M * 11 + 1.5M * 6) + 1 * (0.8M * 7 + 2.8M * 11 + 1.8M * 6) + 0.8M * 7 + 2.8M * 11); //weekend to weekday
                Add(new DateTime(2018, 12, 16, 18, 0, 0), 168, "c002", "pf002", 1.8M * 6 + 5 * (0.5M * 8 + 2.5M * 9 + 1.5M * 7) + 1 * (0.8M * 8 + 2.8M * 9 + 1.8M * 7) + 0.8M * 8 + 2.8M * 9 + 1.8M * 1); //weekend to weekday

                ////WeekDays non-abo
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 168, "c004", "pf001", 5 * (1.5M * 7 + 3.5M * 11 + 2.5M * 6) + 2 * (1.8M * 7 + 3.8M * 11 + 2.8M * 6));
                Add(new DateTime(2018, 12, 13, 0, 0, 0), 168, "c004", "pf002", 5 * (1.5M * 8 + 3.5M * 9 + 2.5M * 7) + 2 * (1.8M * 8 + 3.8M * 9 + 2.8M * 7));
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 168, "c004", "pf001", 3.5M * 6 + 2.5M * 6 + 4 * (1.5M * 7 + 3.5M * 11 + 2.5M * 6) + 2 * (1.8M * 7 + 3.8M * 11 + 2.8M * 6) + 1.5M * 7 + 3.5M * 5);
                Add(new DateTime(2018, 12, 13, 12, 0, 0), 168, "c004", "pf002", 3.5M * 5 + 2.5M * 7 + 4 * (1.5M * 8 + 3.5M * 9 + 2.5M * 7) + 2 * (1.8M * 8 + 3.8M * 9 + 2.8M * 7) + 1.5M * 8 + 3.5M * 4);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 168, "c004", "pf001", 2.5M * 6 + 4 * (1.5M * 7 + 3.5M * 11 + 2.5M * 6) + 2 * (1.8M * 7 + 3.8M * 11 + 2.8M * 6) + 1.5M * 7 + 3.5M * 11);
                Add(new DateTime(2018, 12, 13, 18, 0, 0), 168, "c004", "pf002", 2.5M * 6 + 4 * (1.5M * 8 + 3.5M * 9 + 2.5M * 7) + 2 * (1.8M * 8 + 3.8M * 9 + 2.8M * 7) + 1.5M * 8 + 3.5M * 9 + 2.5M * 1);
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 168, "c004", "pf001", 2.5M * 6 + 4 * (1.5M * 7 + 3.5M * 11 + 2.5M * 6) + 2 * (1.8M * 7 + 3.8M * 11 + 2.8M * 6) + 1.5M * 7 + 3.5M * 11); //weekday to weekend
                Add(new DateTime(2018, 12, 14, 18, 0, 0), 168, "c004", "pf002", 2.5M * 6 + 4 * (1.5M * 8 + 3.5M * 9 + 2.5M * 7) + 2 * (1.8M * 8 + 3.8M * 9 + 2.8M * 7) + 1.5M * 8 + 3.5M * 9 + 2.5M * 1); //weekday to weekend

                //Weekend non-abo
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 168, "c004", "pf001", 2 * (1.8M * 7 + 3.8M * 11 + 2.8M * 6) + 5 * (1.5M * 7 + 3.5M * 11 + 2.5M * 6));
                Add(new DateTime(2018, 12, 15, 0, 0, 0), 168, "c004", "pf002", 2 * (1.8M * 8 + 3.8M * 9 + 2.8M * 7) + 5 * (1.5M * 8 + 3.5M * 9 + 2.5M * 7));
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 168, "c004", "pf001", 3.8M * 6 + 2.8M * 6 + 5 * (1.5M * 7 + 3.5M * 11 + 2.5M * 6) + 1 * (1.8M * 7 + 3.8M * 11 + 2.8M * 6) + 1.8M * 7 + 3.8M * 5);
                Add(new DateTime(2018, 12, 15, 12, 0, 0), 168, "c004", "pf002", 3.8M * 5 + 2.8M * 7 + 5 * (1.5M * 8 + 3.5M * 9 + 2.5M * 7) + 1 * (1.8M * 8 + 3.8M * 9 + 2.8M * 7) + 1.8M * 8 + 3.8M * 4);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 168, "c004", "pf001", 2.8M * 6 + 5 * (1.5M * 7 + 3.5M * 11 + 2.5M * 6) + 1 * (1.8M * 7 + 3.8M * 11 + 2.8M * 6) + 1.8M * 7 + 3.8M * 11);
                Add(new DateTime(2018, 12, 15, 18, 0, 0), 168, "c004", "pf002", 2.8M * 6 + 5 * (1.5M * 8 + 3.5M * 9 + 2.5M * 7) + 1 * (1.8M * 8 + 3.8M * 9 + 2.8M * 7) + 1.8M * 8 + 3.8M * 9 + 2.8M * 1);
                Add(new DateTime(2018, 12, 16, 18, 0, 0), 168, "c004", "pf001", 2.8M * 6 + 5 * (1.5M * 7 + 3.5M * 11 + 2.5M * 6) + 1 * (1.8M * 7 + 3.8M * 11 + 2.8M * 6) + 1.8M * 7 + 3.8M * 11); //weekend to weekday
                Add(new DateTime(2018, 12, 16, 18, 0, 0), 168, "c004", "pf002", 2.8M * 6 + 5 * (1.5M * 8 + 3.5M * 9 + 2.5M * 7) + 1 * (1.8M * 8 + 3.8M * 9 + 2.8M * 7) + 1.8M * 8 + 3.8M * 9 + 2.8M * 1); //weekend to weekday}
            }
        }

        private IInvoiceService GetSut()
        {
            return new InvoiceService(
                _sessionsRepository, 
                _parkingFacilityRepository,
                _customerRepository);
        }
    }
}
