using Employee.Application.Common.Interfaces;
using Employee.Application.Common.Services;
using Employee.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Employee.UnitTests.Common
{
    public class CustomDataValidatorTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly CustomDataValidator _validator;
        private readonly Guid _tenantId = Guid.NewGuid();

        public CustomDataValidatorTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _validator = new CustomDataValidator(_mockContext.Object);
        }

        private void SetupDefinitions(List<CustomFieldDefinition> definitions)
        {
            var mockDbSet = definitions.BuildMockDbSet();
            _mockContext.Setup(c => c.CustomFieldDefinitions).Returns(mockDbSet.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("{}")]
        public async Task ValidateAsync_WhenCustomDataIsEmptyAndNoRequiredFields_ShouldReturnValid(string? json)
        {
            // Arrange
            SetupDefinitions(new List<CustomFieldDefinition>());

            // Act
            var (isValid, errorMessage) = await _validator.ValidateAsync(json);

            // Assert
            isValid.Should().BeTrue();
            errorMessage.Should().BeNull();
        }

        [Fact]
        public async Task ValidateAsync_WhenJsonIsMalformed_ShouldReturnInvalid()
        {
            // Arrange
            SetupDefinitions(new List<CustomFieldDefinition>());

            // Act
            var (isValid, errorMessage) = await _validator.ValidateAsync("invalid-json{");

            // Assert
            isValid.Should().BeFalse();
            errorMessage.Should().Contain("not valid JSON");
        }

        [Fact]
        public async Task ValidateAsync_WhenJsonIsNotObject_ShouldReturnInvalid()
        {
            // Arrange
            SetupDefinitions(new List<CustomFieldDefinition>());

            // Act
            var (isValid, errorMessage) = await _validator.ValidateAsync("[\"item1\", \"item2\"]");

            // Assert
            isValid.Should().BeFalse();
            errorMessage.Should().Contain("must be a valid JSON object");
        }

        [Fact]
        public async Task ValidateAsync_WhenRequiredFieldIsMissing_ShouldReturnInvalid()
        {
            // Arrange
            var definitions = new List<CustomFieldDefinition>
            {
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantId,
                    FieldName = "BadgeNumber",
                    DataType = "string",
                    IsRequired = true
                }
            };
            SetupDefinitions(definitions);

            // Act
            var (isValid, errorMessage) = await _validator.ValidateAsync("{\"DepartmentCode\": \"D1\"}");

            // Assert
            isValid.Should().BeFalse();
            errorMessage.Should().Be("Required custom field 'BadgeNumber' is missing.");
        }

        [Fact]
        public async Task ValidateAsync_WhenTypeDoesNotMatchNumber_ShouldReturnInvalid()
        {
            // Arrange
            var definitions = new List<CustomFieldDefinition>
            {
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantId,
                    FieldName = "ExperienceYears",
                    DataType = "number",
                    IsRequired = false
                }
            };
            SetupDefinitions(definitions);

            // Act
            var (isValid, errorMessage) = await _validator.ValidateAsync("{\"ExperienceYears\": \"five\"}");

            // Assert
            isValid.Should().BeFalse();
            errorMessage.Should().Be("Custom field 'ExperienceYears' must be a numeric value.");
        }

        [Fact]
        public async Task ValidateAsync_WhenTypeDoesNotMatchBoolean_ShouldReturnInvalid()
        {
            // Arrange
            var definitions = new List<CustomFieldDefinition>
            {
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantId,
                    FieldName = "HasClearance",
                    DataType = "boolean",
                    IsRequired = false
                }
            };
            SetupDefinitions(definitions);

            // Act
            var (isValid, errorMessage) = await _validator.ValidateAsync("{\"HasClearance\": \"yes\"}");

            // Assert
            isValid.Should().BeFalse();
            errorMessage.Should().Be("Custom field 'HasClearance' must be a boolean (true/false).");
        }

        [Fact]
        public async Task ValidateAsync_WhenTypeDoesNotMatchDate_ShouldReturnInvalid()
        {
            // Arrange
            var definitions = new List<CustomFieldDefinition>
            {
                new CustomFieldDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantId,
                    FieldName = "CertificationDate",
                    DataType = "date",
                    IsRequired = false
                }
            };
            SetupDefinitions(definitions);

            // Act
            var (isValid, errorMessage) = await _validator.ValidateAsync("{\"CertificationDate\": \"not-a-date\"}");

            // Assert
            isValid.Should().BeFalse();
            errorMessage.Should().Contain("must be a valid ISO-8601 date");
        }

        [Fact]
        public async Task ValidateAsync_WhenAllFieldsAreValid_ShouldReturnValid()
        {
            // Arrange
            var definitions = new List<CustomFieldDefinition>
            {
                new CustomFieldDefinition { Id = Guid.NewGuid(), TenantId = _tenantId, FieldName = "BadgeNumber", DataType = "string", IsRequired = true },
                new CustomFieldDefinition { Id = Guid.NewGuid(), TenantId = _tenantId, FieldName = "ExperienceYears", DataType = "number", IsRequired = false },
                new CustomFieldDefinition { Id = Guid.NewGuid(), TenantId = _tenantId, FieldName = "HasClearance", DataType = "boolean", IsRequired = true },
                new CustomFieldDefinition { Id = Guid.NewGuid(), TenantId = _tenantId, FieldName = "HireDate", DataType = "date", IsRequired = false }
            };
            SetupDefinitions(definitions);

            var validJson = "{\"BadgeNumber\": \"BN-9921\", \"ExperienceYears\": 4, \"HasClearance\": true, \"HireDate\": \"2024-05-15\"}";

            // Act
            var (isValid, errorMessage) = await _validator.ValidateAsync(validJson);

            // Assert
            isValid.Should().BeTrue();
            errorMessage.Should().BeNull();
        }
    }
}
