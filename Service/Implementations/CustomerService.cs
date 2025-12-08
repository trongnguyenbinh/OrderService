using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Model.Models;
using Model.RequestModels;
using Repository.Interfaces;
using Service.Interfaces;
using System.Text.RegularExpressions;

namespace Service.Implementations;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerService> _logger;
    private readonly IMapper _mapper;

    public CustomerService(ICustomerRepository customerRepository, ILogger<CustomerService> logger, IMapper mapper)
    {
        _customerRepository = customerRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Getting customer by ID: {CustomerId}", id);

        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Service: Customer not found with ID: {CustomerId}", id);
            return null;
        }

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Getting all customers");

        var customers = await _customerRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Creating new customer with Email: {Email}", request.Email);
        
        // Validation
        ValidateCreateRequest(request);
        
        // Check if Email already exists
        var existingCustomer = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingCustomer != null)
        {
            _logger.LogWarning("Service: Customer with Email {Email} already exists", request.Email);
            throw new InvalidOperationException($"Customer with Email '{request.Email}' already exists");
        }
        
        var entity = new CustomerEntity
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CustomerType = request.CustomerType,
            IsActive = request.IsActive
        };
        
        var createdCustomer = await _customerRepository.AddAsync(entity, cancellationToken);

        _logger.LogInformation("Service: Successfully created customer with ID: {CustomerId}", createdCustomer.Id);

        return _mapper.Map<CustomerDto>(createdCustomer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Updating customer with ID: {CustomerId}", id);
        
        // Validation
        ValidateUpdateRequest(request);
        
        // Check if customer exists
        var existingCustomer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (existingCustomer == null)
        {
            _logger.LogWarning("Service: Customer not found for update with ID: {CustomerId}", id);
            throw new KeyNotFoundException($"Customer with ID {id} not found");
        }
        
        // Check if Email is being changed and if new Email already exists
        if (existingCustomer.Email != request.Email)
        {
            var customerWithSameEmail = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (customerWithSameEmail != null && customerWithSameEmail.Id != id)
            {
                _logger.LogWarning("Service: Email {Email} is already used by another customer", request.Email);
                throw new InvalidOperationException($"Email '{request.Email}' is already used by another customer");
            }
        }
        
        var entity = new CustomerEntity
        {
            Id = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CustomerType = request.CustomerType,
            IsActive = request.IsActive,
            CreatedAt = existingCustomer.CreatedAt
        };
        
        var updatedCustomer = await _customerRepository.UpdateAsync(entity, cancellationToken);

        _logger.LogInformation("Service: Successfully updated customer with ID: {CustomerId}", id);

        return _mapper.Map<CustomerDto>(updatedCustomer);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Deleting customer with ID: {CustomerId}", id);
        
        var result = await _customerRepository.DeleteAsync(id, cancellationToken);
        
        if (result)
        {
            _logger.LogInformation("Service: Successfully deleted customer with ID: {CustomerId}", id);
        }
        else
        {
            _logger.LogWarning("Service: Customer not found for deletion with ID: {CustomerId}", id);
        }
        
        return result;
    }

    public async Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Getting customer by Email: {Email}", email);

        var customer = await _customerRepository.GetByEmailAsync(email, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Service: Customer not found with Email: {Email}", email);
            return null;
        }

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<PagedResult<CustomerDto>> SearchAsync(CustomerSearchRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Service: Searching customers - FirstName: {FirstName}, LastName: {LastName}, Email: {Email}, PhoneNumber: {PhoneNumber}, CustomerType: {CustomerType}, Page: {PageNumber}, PageSize: {PageSize}",
            request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.CustomerType, request.PageNumber, request.PageSize);

        // Validate pagination parameters
        ValidateSearchRequest(request);
        var pagedResult = await _customerRepository.SearchAsync(request, cancellationToken);

        return new PagedResult<CustomerDto>
        {
            Items = _mapper.Map<IEnumerable<CustomerDto>>(pagedResult.Items),
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages
        };
    }

    private void ValidateCreateRequest(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            _logger.LogWarning("Service: Validation failed - Customer first name is required");
            throw new ArgumentException("Customer first name is required", nameof(request.FirstName));
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            _logger.LogWarning("Service: Validation failed - Customer last name is required");
            throw new ArgumentException("Customer last name is required", nameof(request.LastName));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Service: Validation failed - Customer email is required");
            throw new ArgumentException("Customer email is required", nameof(request.Email));
        }

        if (!IsValidEmail(request.Email))
        {
            _logger.LogWarning("Service: Validation failed - Invalid email format: {Email}", request.Email);
            throw new ArgumentException("Invalid email format", nameof(request.Email));
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !IsValidPhoneNumber(request.PhoneNumber))
        {
            _logger.LogWarning("Service: Validation failed - Invalid phone number format: {PhoneNumber}", request.PhoneNumber);
            throw new ArgumentException("Invalid phone number format", nameof(request.PhoneNumber));
        }
    }

    private void ValidateUpdateRequest(UpdateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            _logger.LogWarning("Service: Validation failed - Customer first name is required");
            throw new ArgumentException("Customer first name is required", nameof(request.FirstName));
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            _logger.LogWarning("Service: Validation failed - Customer last name is required");
            throw new ArgumentException("Customer last name is required", nameof(request.LastName));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Service: Validation failed - Customer email is required");
            throw new ArgumentException("Customer email is required", nameof(request.Email));
        }

        if (!IsValidEmail(request.Email))
        {
            _logger.LogWarning("Service: Validation failed - Invalid email format: {Email}", request.Email);
            throw new ArgumentException("Invalid email format", nameof(request.Email));
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !IsValidPhoneNumber(request.PhoneNumber))
        {
            _logger.LogWarning("Service: Validation failed - Invalid phone number format: {PhoneNumber}", request.PhoneNumber);
            throw new ArgumentException("Invalid phone number format", nameof(request.PhoneNumber));
        }
    }

    private void ValidateSearchRequest(CustomerSearchRequest request)
    {
        const int maxPageSize = 100;

        if (request.PageNumber < 1)
        {
            _logger.LogWarning("Service: Validation failed - Page number must be greater than or equal to 1");
            throw new ArgumentException("Page number must be greater than or equal to 1", nameof(request.PageNumber));
        }

        if (request.PageSize < 1)
        {
            _logger.LogWarning("Service: Validation failed - Page size must be greater than 0");
            throw new ArgumentException("Page size must be greater than 0", nameof(request.PageSize));
        }

        if (request.PageSize > maxPageSize)
        {
            _logger.LogWarning("Service: Validation failed - Page size cannot exceed {MaxPageSize}", maxPageSize);
            throw new ArgumentException($"Page size cannot exceed {maxPageSize}", nameof(request.PageSize));
        }

        // Validate sort direction
        if (!string.IsNullOrWhiteSpace(request.SortDirection) &&
            request.SortDirection.ToLower() != "asc" &&
            request.SortDirection.ToLower() != "desc")
        {
            _logger.LogWarning("Service: Validation failed - Sort direction must be 'asc' or 'desc'");
            throw new ArgumentException("Sort direction must be 'asc' or 'desc'", nameof(request.SortDirection));
        }

        // Validate sort field
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var validSortFields = new[] { "firstname", "lastname", "email", "customertype" };
            if (!validSortFields.Contains(request.SortBy.ToLower()))
            {
                _logger.LogWarning("Service: Validation failed - Invalid sort field: {SortBy}", request.SortBy);
                throw new ArgumentException("Sort by must be either 'FirstName', 'LastName', 'Email', or 'CustomerType'", nameof(request.SortBy));
            }
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Simple email validation regex
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        try
        {
            // Simple phone number validation - allows digits, spaces, dashes, parentheses, and plus sign
            var phoneRegex = new Regex(@"^[\d\s\-\(\)\+]+$");
            return phoneRegex.IsMatch(phoneNumber) && phoneNumber.Length >= 10 && phoneNumber.Length <= 20;
        }
        catch
        {
            return false;
        }
    }
}
