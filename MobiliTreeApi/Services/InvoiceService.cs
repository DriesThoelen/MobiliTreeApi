using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MobiliTreeApi.Domain;
using MobiliTreeApi.Helper;
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
        private readonly ICalculatorService _calculatorService;
        private readonly ICustomerRepository _customerRepository;

        public InvoiceService(ISessionsRepository sessionsRepository, IParkingFacilityRepository parkingFacilityRepository, ICalculatorService calculatorService, ICustomerRepository customerRepository)
        {
            _sessionsRepository = sessionsRepository;
            _parkingFacilityRepository = parkingFacilityRepository;
            _calculatorService = calculatorService;
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
                Amount = _calculatorService.CalculatePrice(serviceProfile, [.. x], _customerRepository.GetCustomer(x.Key))
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
                Amount = _calculatorService.CalculatePrice(serviceProfile, [x], _customerRepository.GetCustomer(x.CustomerId))
            }).FirstOrDefault();
        }
    }
}
