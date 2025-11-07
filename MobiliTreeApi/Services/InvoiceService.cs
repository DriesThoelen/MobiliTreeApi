using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MobiliTreeApi.Domain;
using MobiliTreeApi.Repositories;

namespace MobiliTreeApi.Services
{
    public interface IInvoiceService
    {
        List<Invoice> GetInvoices(string parkingFacilityId);
        Invoice GetInvoice(string parkingFacilityId, string customerId);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly ISessionsRepository _sessionsRepository;
        private readonly IParkingFacilityRepository _parkingFacilityRepository;
        private readonly ICustomerRepository _customerRepository;

        public InvoiceService(ISessionsRepository sessionsRepository, IParkingFacilityRepository parkingFacilityRepository, ICustomerRepository customerRepository)
        {
            _sessionsRepository = sessionsRepository;
            _parkingFacilityRepository = parkingFacilityRepository;
            _customerRepository = customerRepository;
        }

        public List<Invoice> GetInvoices(string parkingFacilityId)
        {
            var serviceProfile = _parkingFacilityRepository.GetServiceProfile(parkingFacilityId);
            if (serviceProfile == null)
            {
                throw new ArgumentException($"Invalid parking facility id '{parkingFacilityId}'");
            }

            var sessions = _sessionsRepository.GetSessions(parkingFacilityId);

            return sessions.GroupBy(x => x.CustomerId).Select(x => new Invoice
            {
                ParkingFacilityId = parkingFacilityId,
                CustomerId = x.Key,
                Amount = CalculatePrice(serviceProfile, [.. x])
            }).ToList();
        }

        public Invoice GetInvoice(string parkingFacilityId, string customerId)
        {
            var serviceProfile = _parkingFacilityRepository.GetServiceProfile(parkingFacilityId);
            if (serviceProfile == null)
            {
                throw new ArgumentException($"Invalid parking facility id '{parkingFacilityId}'");
            }

            var sessions = _sessionsRepository.GetSessions(parkingFacilityId);

            return sessions.Where(x => x.CustomerId == customerId).Select(x => new Invoice
            {
                ParkingFacilityId = parkingFacilityId,
                CustomerId = x.CustomerId,
                Amount = CalculatePrice(serviceProfile, [x])
            }).FirstOrDefault();
        }

        private decimal CalculatePrice(ServiceProfile serviceProfile, List<Session> sessions)
        {
            var amount = 0M;

            if (serviceProfile == null)
            {
                throw new ArgumentException($"Invalid parking facility id");
            }

            foreach (var session in sessions)
            {
                if (session == null)
                {
                    throw new ArgumentNullException($"Invalid session");
                }

                var customer = _customerRepository.GetCustomer(session.CustomerId);

                amount += CalculateTimeslotCost(serviceProfile, session.StartDateTime, session.EndDateTime, customer, session.ParkingFacilityId);
            }

            return amount;
        }

        private decimal CalculateTimeslotCost(ServiceProfile serviceProfile, DateTime startDate, DateTime endDate, Customer customer, string parkingFacilityId)
        {
            var amount = 0M;
            var actualTimeSlots = new List<ActualTimeSlot>();

            //logic to iterate timeslots starting from startdate until enddate, generating a list of actual timeslots
            DateTime startDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, startDate.Minute, 0); //We need to set the seconds to zero because we only invoice hours and minutes
            DateTime endTimeTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, endDate.Hour, endDate.Minute, 0); //We need to set the seconds to zero because we only invoice hours and minutes
            bool isBetweenTimeslot = false;
            int dayIndex = 0; //To look beyond one day if the duration between start and endDate is equal or greater than 24 hours

            do
            {
                DateOnly date = new DateOnly(startDate.Year, startDate.Month, startDate.Day + dayIndex);
                var timeslots = GetAppropriateTimeslotsFromServiceProfile(serviceProfile, date, customer, parkingFacilityId);
                for (int i = 0; i < timeslots.Count; i++)
                {
                    DateTime startTimeSlotDateTime = new DateTime(startDate.Year, startDate.Month, timeslots[i].StartHour == 24 ? startDate.Day + 1 + dayIndex : startDate.Day + dayIndex, timeslots[i].StartHour == 24 ? 0 : timeslots[i].StartHour, 0, 0);
                    DateTime endTimeSlotDateTime = new DateTime(startDate.Year, startDate.Month, timeslots[i].EndHour == 24 ? startDate.Day + 1 + dayIndex : startDate.Day + dayIndex, timeslots[i].EndHour == 24 ? 0 : timeslots[i].EndHour, 0, 0);
                    isBetweenTimeslot = IsBetweenTimeSlot(startDate, endDate, startTimeSlotDateTime, endTimeSlotDateTime);
                    if (isBetweenTimeslot)
                    {
                        actualTimeSlots.Add(new ActualTimeSlot
                        {
                            StartTimeSlotDateTime = startDate < startTimeSlotDateTime ? startTimeSlotDateTime : startDate,
                            EndTimeSlotDateTime = endTimeSlotDateTime < endDate ? endTimeSlotDateTime : endDate,
                            PricePerHour = timeslots[i].PricePerHour
                        });
                    }
                }

                dayIndex++;
            }
            while (isBetweenTimeslot);

            foreach (var actualTimeSlot in actualTimeSlots)
            {
                var timeInHours = (decimal)actualTimeSlot.EndTimeSlotDateTime.Subtract(actualTimeSlot.StartTimeSlotDateTime).TotalHours;

                amount += timeInHours * actualTimeSlot.PricePerHour;
            }

            return amount;
        }

        private bool IsWeekend(DateOnly date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        private bool IsBetweenTimeSlot(DateTime startDate, DateTime endDate, DateTime startTimeSlotDateTime, DateTime endTimeSlotDateTime)
        {
            return (startDate < endTimeSlotDateTime && startTimeSlotDateTime < endDate);
        }

        private IList<TimeslotPrice> GetAppropriateTimeslotsFromServiceProfile(ServiceProfile serviceProfile, DateOnly date, Customer customer, string parkingFacilityId)
        {
            bool isWeekend = IsWeekend(date);
            if (customer.ContractedParkingFacilityIds?.Contains(parkingFacilityId) ?? false)
            {
                if (isWeekend)
                {
                    return serviceProfile.WeekendPrices;
                }
                else
                {
                    return serviceProfile.WeekDaysPrices;
                }
            }
            else
            {
                if (isWeekend)
                {
                    return serviceProfile.OverrunWeekendPrices;
                }
                else
                {
                    return serviceProfile.OverrunWeekDaysPrices;
                }
            }
        }

        private class ActualTimeSlot
        {
            public DateTime StartTimeSlotDateTime { get; set; }
            public DateTime EndTimeSlotDateTime { get; set; }
            public decimal PricePerHour { get; set; }
        }
    }
}
