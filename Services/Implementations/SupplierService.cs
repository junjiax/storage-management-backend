using Microsoft.EntityFrameworkCore;
using dotnet_backend.Data;
using dotnet_backend.DTOs.Supplier;
using dotnet_backend.Models;

namespace dotnet_backend.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly StoreDbContext _context;

        public SupplierService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<SupplierResponse>> GetSupplierListAsync()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new SupplierResponse
                {
                    SupplierId = s.SupplierId,
                    Name = s.Name,
                    Address = s.Address,
                    Phone = s.Phone,
                    Email = s.Email
                })
                .ToListAsync();

            return suppliers;
        }

        public async Task<SupplierResponse> AddSupplierItemAsync(SupplierRequest request)
        {
            var newSupplier = new Models.Supplier
            {
                Name = request.Name,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email
            };

            _context.Suppliers.Add(newSupplier);
            await _context.SaveChangesAsync();

            return new SupplierResponse
            {
                SupplierId = newSupplier.SupplierId,
                Name = newSupplier.Name,
                Address = newSupplier.Address,
                Phone = newSupplier.Phone,
                Email = newSupplier.Email
            };
        }

        public async Task<SupplierResponse?> UpdateSupplierItemAsync(int supplierId, SupplierRequest request)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null)
            {
                return null;
            }

            supplier.Name = request.Name;
            supplier.Address = request.Address;
            supplier.Phone = request.Phone;
            supplier.Email = request.Email;

            await _context.SaveChangesAsync();

            return new SupplierResponse
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Address = supplier.Address,
                Phone = supplier.Phone,
                Email = supplier.Email
            };
        }

        public async Task<bool> DeleteSupplierItemAsync(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null)
            {
                return false;
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SupplierResponse?> GetSupplierByIdAsync(int supplierId)
        {
            var supplier = await _context.Suppliers
                .Where(s => s.SupplierId == supplierId)
                .Select(s => new SupplierResponse
                {
                    SupplierId = s.SupplierId,
                    Name = s.Name,
                    Address = s.Address,
                    Phone = s.Phone,
                    Email = s.Email
                })
                .FirstOrDefaultAsync();

            return supplier;
        }

        public async Task<bool> SupplierExistsAsync(int supplierId)
        {
            return await _context.Suppliers.AnyAsync(s => s.SupplierId == supplierId);
        }
    }
}
