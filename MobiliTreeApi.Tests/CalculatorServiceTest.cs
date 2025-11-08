using MobiliTreeApi.Repositories;
using MobiliTreeApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MobiliTreeApi.Tests
{
    public class CalculatorServiceTest
    {
        private readonly ISessionsRepository _sessionsRepository;
        private readonly IParkingFacilityRepository _parkingFacilityRepository;
        private readonly ICustomerRepository _customerRepository;

        public CalculatorServiceTest()
        {
            _sessionsRepository = new SessionsRepositoryFake(FakeData.GetSeedSessions());
            _parkingFacilityRepository = new ParkingFacilityRepositoryFake(FakeData.GetSeedServiceProfiles());
            _customerRepository = new CustomerRepositoryFake(FakeData.GetSeedCustomers());
        }

        [Theory]
        [ClassData(typeof(InvoiceTheoryData))]
        public void CalculatePrice_Should_Return_Correct_Price(DateTime startDateTime, double durationInHours, string customerId, string parkingFacilityId, decimal expectedPrice)
        {
            _sessionsRepository.AddSession(new Domain.Session
            {
                CustomerId = customerId,
                ParkingFacilityId = parkingFacilityId,
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddHours(durationInHours)
            });

            var serviceProfile = _parkingFacilityRepository.GetServiceProfile(parkingFacilityId);
            var sessions = _sessionsRepository.GetSessions(parkingFacilityId);
            var customer = _customerRepository.GetCustomer(customerId);

            var result = GetSut().CalculatePrice(serviceProfile, sessions, customer);

            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
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

        private ICalculatorService GetSut()
        {
            return new CalculatorService();
        }
    }
}
