using HeimReport.Api.Security;

namespace HeimReport.Api.UnitTests.Security;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _sut = new();

    [Fact]
    public void Hash_ShouldProduceAHashThatVerifiesSuccessfully_AgainstTheOriginalPassword()
    {
        // Arrange
        const string password = "P@ssw0rd123!";

        // Act
        var hash = _sut.Hash(password);

        // Assert
        Assert.True(_sut.Verify(password, hash));
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        const string correctPassword = "CorrectPassword1!";
        const string wrongPassword = "WrongPassword1!";
        var hash = _sut.Hash(correctPassword);

        // Act
        var result = _sut.Verify(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Hash_ShouldProduceDifferentHashes_ForTheSamePasswordOnDifferentCalls()
    {
        // Arrange
        const string password = "P@ssw0rd123!";

        // Act
        var hash1 = _sut.Hash(password);
        var hash2 = _sut.Hash(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.True(_sut.Verify(password, hash1));
        Assert.True(_sut.Verify(password, hash2));
    }

    [Fact]
    public void Hash_ShouldProduceA60CharacterHash()
    {
        // Arrange
        const string password = "AnyPassword1!";

        // Act
        var hash = _sut.Hash(password);

        // Assert
        Assert.Equal(60, hash.Length);
    }

    [Fact]
    public void GenerateTemporaryPassword_ShouldMeetComplexityRequirements()
    {
        // Act
        var password = _sut.GenerateTemporaryPassword();

        // Assert
        Assert.True(password.Length >= 8);
        Assert.Contains(password, c => char.IsUpper(c));
        Assert.Contains(password, c => char.IsLower(c));
        Assert.Contains(password, c => char.IsDigit(c));
        Assert.Contains(password, c => !char.IsLetterOrDigit(c));
    }

    [Fact]
    public void GenerateTemporaryPassword_ShouldProduceDifferentValues_OnEachCall()
    {
        // Act
        var password1 = _sut.GenerateTemporaryPassword();
        var password2 = _sut.GenerateTemporaryPassword();

        // Assert
        Assert.NotEqual(password1, password2);
    }

    [Fact]
    public void GenerateTemporaryPassword_ShouldProduceAPasswordThatHashesAndVerifiesCorrectly()
    {
        // Arrange
        var password = _sut.GenerateTemporaryPassword();

        // Act
        var hash = _sut.Hash(password);

        // Assert
        Assert.True(_sut.Verify(password, hash));
    }
}