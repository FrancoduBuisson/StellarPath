using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StelarPath.API.Infrastructure.Data.Repositories
{
    public class BookingStatusRepository(IUnitOfWork unitOfWork)
        : Repository<BookingStatus>(unitOfWork, "booking_statuses", "booking_status_id"), IBookingStatusRepository
    {
        public override async Task<int> AddAsync(BookingStatus entity)
        {
            const string query = @"
                INSERT INTO booking_statuses (
                    status_name, 
                    is_active
                )
                VALUES (
                    @StatusName, 
                    @IsActive
                )
                RETURNING booking_status_id";

            return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
        }

        public override async Task<bool> UpdateAsync(BookingStatus entity)
        {
            const string query = @"
                UPDATE booking_statuses
                SET 
                    status_name = @StatusName,
                    is_active = @IsActive
                WHERE booking_status_id = @BookingStatusId";

            var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
            return result > 0;
        }

        public override async Task<BookingStatus?> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM booking_statuses WHERE booking_status_id = @Id";
            return await UnitOfWork.Connection.QueryFirstOrDefaultAsync<BookingStatus>(
                query, new { Id = id });
        }

        public override async Task<IEnumerable<BookingStatus>> GetAllAsync()
        {
            const string query = "SELECT * FROM booking_statuses";
            return await UnitOfWork.Connection.QueryAsync<BookingStatus>(query);
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM booking_statuses WHERE booking_status_id = @Id";
            var result = await UnitOfWork.Connection.ExecuteAsync(query, new { Id = id });
            return result > 0;
        }

        public async Task<BookingStatus?> GetByStatusNameAsync(string statusName)
        {
            const string query = "SELECT * FROM booking_statuses WHERE status_name = @StatusName";
            return await UnitOfWork.Connection.QueryFirstOrDefaultAsync<BookingStatus>(
                query, new { StatusName = statusName });
        }

        public async Task<IEnumerable<BookingStatus>> GetActiveStatusesAsync()
        {
            const string query = "SELECT * FROM booking_statuses WHERE is_active = true";
            return await UnitOfWork.Connection.QueryAsync<BookingStatus>(query);
        }

        public async Task<IEnumerable<BookingStatus>> GetInactiveStatusesAsync()
        {
            const string query = "SELECT * FROM booking_statuses WHERE is_active = false";
            return await UnitOfWork.Connection.QueryAsync<BookingStatus>(query);
        }

        public async Task<IEnumerable<BookingStatus>> GetByIdsAsync(IEnumerable<int> ids)
        {
            const string query = "SELECT * FROM booking_statuses WHERE booking_status_id = ANY(@Ids)";
            return await UnitOfWork.Connection.QueryAsync<BookingStatus>(query, new { Ids = ids.ToArray() });
        }

        public async Task<IEnumerable<BookingStatus>> SearchStatusesAsync(string? name, bool? isActive)
        {
            var conditions = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(name))
            {
                conditions.Add("status_name ILIKE @Name");
                parameters.Add("Name", $"%{name}%");
            }

            if (isActive.HasValue)
            {
                conditions.Add("is_active = @IsActive");
                parameters.Add("IsActive", isActive.Value);
            }

            var query = "SELECT * FROM booking_statuses";
            if (conditions.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", conditions);
            }

            return await UnitOfWork.Connection.QueryAsync<BookingStatus>(query, parameters);
        }
    }
}