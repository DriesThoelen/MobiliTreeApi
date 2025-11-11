using MobiliTreeApi.Domain;
using MobiliTreeApi.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobiliTreeApi.Services
{
    public interface ICalculatorService
    {
        decimal CalculatePrice(ServiceProfile serviceProfile, List<Session> sessions, Customer customer);
        decimal CalculateTimeslotCost(ServiceProfile serviceProfile, DateTime startDate, DateTime endDate, Customer customer, string parkingFacilityId);
    }

    public class CalculatorService : ICalculatorService
    {
        public decimal CalculatePrice(ServiceProfile serviceProfile, List<Session> sessions, Customer customer)
        {
            var amount = 0M;

            if (serviceProfile == null)
            {
                throw new ArgumentNullException($"Invalid parking facility id");
            }

            if (sessions == null) {
                throw new ArgumentNullException($"Invalid sessions");
            }

            if (customer == null)
            {
                throw new ArgumentNullException($"Invalid customer");
            }

            foreach (var session in sessions.Where(s => s != null && !string.IsNullOrEmpty(s.CustomerId) && !string.IsNullOrEmpty(s.ParkingFacilityId)))
            {
                amount += CalculateTimeslotCost(serviceProfile, session.StartDateTime, session.EndDateTime, customer, session.ParkingFacilityId);
            }

            return amount;
        }

        public decimal CalculateTimeslotCost(ServiceProfile serviceProfile, DateTime startDate, DateTime endDate, Customer customer, string parkingFacilityId) //Facade pattern
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
                    isBetweenTimeslot = DateHelper.IsBetweenTimeSlot(startDate, endDate, startTimeSlotDateTime, endTimeSlotDateTime);
                    if (isBetweenTimeslot)
                    {
                        actualTimeSlots.Add(new ActualTimeSlot
                        {
                            StartTimeSlotDateTime = startDate < startTimeSlotDateTime ? startTimeSlotDateTime : startDate,
                            EndTimeSlotDateTime = endTimeSlotDateTime < endDate ? endTimeSlotDateTime : endDate,
                            PricePerHour = timeslots[i].PricePerHour
                        }); //Adapter pattern
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

        private IList<TimeslotPrice> GetAppropriateTimeslotsFromServiceProfile(ServiceProfile serviceProfile, DateOnly date, Customer customer, string parkingFacilityId) //Strategy pattern
        {
            bool isWeekend = DateHelper.IsWeekend(date); //Refactor idea: use state pattern
            if (customer.ContractedParkingFacilityIds?.Contains(parkingFacilityId) ?? false) //Refactor idea: use state pattern
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
    }
}
