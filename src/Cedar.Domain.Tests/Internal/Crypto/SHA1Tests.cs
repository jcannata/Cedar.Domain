namespace Cedar.Domain.Internal.Crypto
{
    using System.Text;
    using FluentAssertions;
    using Xunit;
    using MsCoreLibSha1 = System.Security.Cryptography.SHA1;

    public class SHA1Tests
    {
        [Fact]
        public void Imorted_sha1_algo_should_generate_same_hash_as_mscorelib_version()
        {
            byte[] source = Encoding.UTF8.GetBytes("hashmeplease");
            var mscorlibSha1 = MsCoreLibSha1.Create();
            var importedSha1 = SHA1.Create();

            var mscorelibHash = mscorlibSha1.ComputeHash(source);
            var importedHash = importedSha1.ComputeHash(source);

            mscorelibHash.Should().BeEquivalentTo(importedHash);
        }
    }
}