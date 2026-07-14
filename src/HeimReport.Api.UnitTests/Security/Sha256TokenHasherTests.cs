using HeimReport.Api.Security;

namespace HeimReport.Api.UnitTests.Security;

public class Sha256TokenHasherTests
{
    private readonly Sha256TokenHasher _sut = new();

    [Fact]
    public void Hash_ShouldReturnConsistentHash_ForSameInput()
    {
        // Arrange
        var rawToken = "some-raw-token-value";

        // Act
        var hash1 = _sut.Hash(rawToken);
        var hash2 = _sut.Hash(rawToken);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Hash_ShouldReturnDifferentHashes_ForDifferentInputs()
    {
        // Arrange
        var token1 = "token-one";
        var token2 = "token-two";

        // Act
        var hash1 = _sut.Hash(token1);
        var hash2 = _sut.Hash(token2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GenerateRawToken_ShouldReturnDifferentValues_OnEachCall()
    {
        // Act
        var token1 = _sut.GenerateRawToken();
        var token2 = _sut.GenerateRawToken();

        // Assert
        Assert.NotEqual(token1, token2);
        Assert.False(string.IsNullOrWhiteSpace(token1));
    }
}
