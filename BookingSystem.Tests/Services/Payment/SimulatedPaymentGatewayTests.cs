using BookingSystem.Services.Payment;
using FluentAssertions;
using Xunit;

namespace BookingSystem.Tests.Services.Payment
{
    public class SimulatedPaymentGatewayTests
    {
        private readonly SimulatedPaymentGateway _gateway;

        public SimulatedPaymentGatewayTests()
        {
            _gateway = new SimulatedPaymentGateway();
        }

        [Theory]
        [InlineData("4111111111111111")] // Visa
        [InlineData("5555555555554444")] // MasterCard
        [InlineData("4532111111111111")] // Visa
        [InlineData("1234567890123456")] // 16 digits
        [InlineData("1234567890123")]    // 13 digits (minimum)
        [InlineData("1234567890123456789")] // 19 digits (maximum)
        public async Task ProcessPaymentAsync_ShouldReturnValid_WhenCardNumberIsValid(string cardNumber)
        {
            // Arrange
            var request = new PaymentRequest(cardNumber);

            // Act
            var result = await _gateway.ProcessPaymentAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.PaymentId.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("0000111122223333")] // Starts with 0000
        [InlineData("0000")] // Too short and starts with 0000
        [InlineData("12345")] // Too short
        [InlineData("123")] // Too short
        [InlineData("12345678901234567890")] // Too long (20 digits)
        public async Task ProcessPaymentAsync_ShouldReturnInvalid_WhenCardNumberIsInvalid(string cardNumber)
        {
            // Arrange
            var request = new PaymentRequest(cardNumber);

            // Act
            var result = await _gateway.ProcessPaymentAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.PaymentId.Should().BeNull();
        }

        [Theory]
        [InlineData("4111-1111-1111-1111")] // With dashes
        [InlineData("4111 1111 1111 1111")] // With spaces
        public async Task ProcessPaymentAsync_ShouldHandleFormattedCardNumbers(string cardNumber)
        {
            // Arrange
            var request = new PaymentRequest(cardNumber);

            // Act
            var result = await _gateway.ProcessPaymentAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldThrowException_WhenRequestIsNull()
        {
            // Arrange
            PaymentRequest request = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _gateway.ProcessPaymentAsync(request));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ProcessPaymentAsync_ShouldThrowException_WhenCardNumberIsNullOrEmpty(string cardNumber)
        {
            // Arrange
            var request = new PaymentRequest(cardNumber);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _gateway.ProcessPaymentAsync(request));
        }

        [Theory]
        [InlineData("abcd1234efgh5678")] // Contains letters
        [InlineData("1234-abcd-5678-efgh")] // Contains letters with dashes
        public async Task ProcessPaymentAsync_ShouldReturnInvalid_WhenCardNumberContainsNonDigits(string cardNumber)
        {
            // Arrange
            var request = new PaymentRequest(cardNumber);

            // Act
            var result = await _gateway.ProcessPaymentAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldGenerateUniquePaymentIds()
        {
            // Arrange
            var request1 = new PaymentRequest("4111111111111111");
            var request2 = new PaymentRequest("4111111111111111");

            // Act
            var result1 = await _gateway.ProcessPaymentAsync(request1);
            var result2 = await _gateway.ProcessPaymentAsync(request2);

            // Assert
            result1.PaymentId.Should().NotBe(result2.PaymentId);
        }
    }
}
