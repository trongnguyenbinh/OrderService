using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Models;
using Model.RequestModels;
using Repository.Interfaces;

namespace Repository.Implementations;

public class CustomerRepository : ICustomerRepository
{
    private readonly DataContext _context;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(DataContext context, ILogger<CustomerRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting customer by ID: {CustomerId}", id);
        
        try
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer not found with ID: {CustomerId}", id);
            }
            
            return customer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by ID: {CustomerId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CustomerEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all customers");
        
        try
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} customers", customers.Count);
            return customers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all customers");
            throw;
        }
    }

    public async Task<CustomerEntity> AddAsync(CustomerEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new customer with Email: {Email}", entity.Email);
        
        try
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await _context.Customers.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully added customer with ID: {CustomerId}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding customer with Email: {Email}", entity.Email);
            throw;
        }
    }

    public async Task<CustomerEntity> UpdateAsync(CustomerEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating customer with ID: {CustomerId}", entity.Id);
        
        try
        {
            var existingCustomer = await _context.Customers.FindAsync(new object[] { entity.Id }, cancellationToken);
            
            if (existingCustomer == null)
            {
                _logger.LogWarning("Customer not found for update with ID: {CustomerId}", entity.Id);
                throw new KeyNotFoundException($"Customer with ID {entity.Id} not found");
            }
            
            existingCustomer.FirstName = entity.FirstName;
            existingCustomer.LastName = entity.LastName;
            existingCustomer.Email = entity.Email;
            existingCustomer.PhoneNumber = entity.PhoneNumber;
            existingCustomer.CustomerType = entity.CustomerType;
            existingCustomer.IsActive = entity.IsActive;
            existingCustomer.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully updated customer with ID: {CustomerId}", entity.Id);
            return existingCustomer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", entity.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);
        
        try
        {
            var customer = await _context.Customers.FindAsync(new object[] { id }, cancellationToken);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for deletion with ID: {CustomerId}", id);
                return false;
            }
            
            // Soft delete by setting IsActive to false
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully soft deleted customer with ID: {CustomerId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", id);
            throw;
        }
    }

    public async Task<CustomerEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting customer by Email: {Email}", email);

        try
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);

            if (customer == null)
            {
                _logger.LogWarning("Customer not found with Email: {Email}", email);
            }

            return customer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by Email: {Email}", email);
            throw;
        }
    }

    public async Task<PagedResult<CustomerEntity>> SearchAsync(CustomerSearchRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Searching customers - FirstName: {FirstName}, LastName: {LastName}, Email: {Email}, PhoneNumber: {PhoneNumber}, CustomerType: {CustomerType}, Page: {PageNumber}, PageSize: {PageSize}, SortBy: {SortBy}, SortDirection: {SortDirection}",
            request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.CustomerType, request.PageNumber, request.PageSize, request.SortBy, request.SortDirection);

        try
        {
            // Start with base query - filter by IsActive = true
            var query = _context.Customers
                .AsNoTracking()
                .Where(c => c.IsActive == true);

            // Apply search filters
            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                var firstNameLower = request.FirstName.ToLower();
                query = query.Where(c => c.FirstName.ToLower().Contains(firstNameLower));
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                var lastNameLower = request.LastName.ToLower();
                query = query.Where(c => c.LastName.ToLower().Contains(lastNameLower));
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var emailLower = request.Email.ToLower();
                query = query.Where(c => c.Email.ToLower().Contains(emailLower));
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                query = query.Where(c => c.PhoneNumber != null && c.PhoneNumber.Contains(request.PhoneNumber));
            }

            if (request.CustomerType.HasValue)
            {
                query = query.Where(c => c.CustomerType == request.CustomerType.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDirection);

            // Apply pagination
            var skip = (request.PageNumber - 1) * request.PageSize;
            var items = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            _logger.LogInformation("Found {TotalCount} customers matching search criteria, returning page {PageNumber} of {TotalPages}",
                totalCount, request.PageNumber, totalPages);

            return new PagedResult<CustomerEntity>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers");
            throw;
        }
    }

    private static IQueryable<CustomerEntity> ApplySorting(IQueryable<CustomerEntity> query, string? sortBy, string sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "firstname" => isDescending
                ? query.OrderByDescending(c => c.FirstName)
                : query.OrderBy(c => c.FirstName),
            "lastname" => isDescending
                ? query.OrderByDescending(c => c.LastName)
                : query.OrderBy(c => c.LastName),
            "email" => isDescending
                ? query.OrderByDescending(c => c.Email)
                : query.OrderBy(c => c.Email),
            "customertype" => isDescending
                ? query.OrderByDescending(c => c.CustomerType)
                : query.OrderBy(c => c.CustomerType),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };
    }
}

