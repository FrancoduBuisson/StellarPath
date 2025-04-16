using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using StellarPath.API.Core.Interfaces.Repositories;
using StelarPath.API.Infrastructure.Services;
using StellarPath.API.Core.Models;

namespace StellarPath.API.Tests.Services
{
    public class BookingStatusServiceTests
    {
        private readonly Mock<IBookingStatusRepository> _mockRepo;
        private readonly BookingStatusService _service;


        public BookingStatusServiceTests()
        {
            _mockRepo = new Mock<IBookingStatusRepository>();
            _service = new BookingStatusService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetReservedStatusIdAsync_ReturnsCorrectId()
        {
            var expectedId = 1;
            _mockRepo.Setup(repo => repo.GetByNameAsync("Reserved"))
                     .ReturnsAsync(new BookingStatus { StatusName = "Reserved", BookingStatusId = expectedId });

            var result = await _service.GetReservedStatusIdAsync();

            Assert.Equal(expectedId, result);

            var result2 = await _service.GetReservedStatusIdAsync();
            Assert.Equal(expectedId, result2);

            _mockRepo.Verify(r => r.GetByNameAsync("Reserved"), Times.Once);
        }

        [Fact]
        public async Task GetReservedStatusIdAsync_ThrowsException_WhenNotFound()
        {
            _mockRepo.Setup(repo => repo.GetByNameAsync("Reserved"))
                     .ReturnsAsync((BookingStatus?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.GetReservedStatusIdAsync());
        }

        [Fact]
        public async Task GetPaidStatusIdAsync_ReturnsCorrectId()
        {
            var expectedId = 2;
            _mockRepo.Setup(repo => repo.GetByNameAsync("Paid"))
                     .ReturnsAsync(new BookingStatus { StatusName = "Paid", BookingStatusId = expectedId });

            var result = await _service.GetPaidStatusIdAsync();

            Assert.Equal(expectedId, result);
        }

        [Fact]
        public async Task GetCompletedStatusIdAsync_ReturnsCorrectId()
        {
            var expectedId = 3;
            _mockRepo.Setup(repo => repo.GetByNameAsync("Completed"))
                     .ReturnsAsync(new BookingStatus {StatusName = "Completed", BookingStatusId = expectedId });

            var result = await _service.GetCompletedStatusIdAsync();

            Assert.Equal(expectedId, result);
        }

        [Fact]
        public async Task GetCancelledStatusIdAsync_ReturnsCorrectId()
        {
            var expectedId = 4;
            _mockRepo.Setup(repo => repo.GetByNameAsync("Cancelled"))
                     .ReturnsAsync(new BookingStatus {StatusName = "Cancelled", BookingStatusId = expectedId });

            var result = await _service.GetCancelledStatusIdAsync();

            Assert.Equal(expectedId, result);
        }

        [Fact]
        public async Task GetExpiredStatusIdAsync_ReturnsCorrectId()
        {
            var expectedId = 5;
            _mockRepo.Setup(repo => repo.GetByNameAsync("Expired"))
                     .ReturnsAsync(new BookingStatus {StatusName = "Expired", BookingStatusId = expectedId });

            var result = await _service.GetExpiredStatusIdAsync();

            Assert.Equal(expectedId, result);
        }

        [Fact]
        public async Task GetStatusNameByIdAsync_ReturnsCorrectName()
        {
            int statusId = 10;
            string expectedName = "SomeStatus";
            _mockRepo.Setup(repo => repo.GetByIdAsync(statusId))
                     .ReturnsAsync(new BookingStatus { BookingStatusId = statusId, StatusName = expectedName });

            var result = await _service.GetStatusNameByIdAsync(statusId);

            Assert.Equal(expectedName, result);
        }

        [Fact]
        public async Task GetStatusNameByIdAsync_ReturnsUnknown_WhenNotFound()
        {
            int statusId = 999;
            _mockRepo.Setup(repo => repo.GetByIdAsync(statusId))
                     .ReturnsAsync((BookingStatus?)null);

            var result = await _service.GetStatusNameByIdAsync(statusId);

            Assert.Equal("Unknown", result);
        }
    }
}
