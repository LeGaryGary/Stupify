using FluentAssertions;
using Stupify.Data.Encryption;
using Xunit;

namespace Stupify.Data.Tests
{
    public class AesEncryptionTests
    {
        [Theory(DisplayName = "AES Encryption Tests")]
        [InlineData("Just a normal string ya know!", "Password1")]
        [InlineData("", "Password1")]
        [InlineData("😁😁😁҂֍⌇", "Password1")]
        [InlineData("Just a normal string ya know!", "😁😁😁҂֍⌇")]
        [InlineData("Just a normal string ya know!", "fBbaxzHxg2kZdKYqF%#&!5mSZaASq=yeeSFfT6m$5@Fhmq5TJuCSR*FP8?gQsssL+KRybS&-HPf@JJca3XM^$Rkf@?eW$ALGMDMX@w$YkGWP#%4T9Hus@LdjHwwcDR7kh+-2X^_=DSc6SuhgT8attV-Ya5G_QcK!mFQ73dZZuq6$QY+-427Av$Qm9xb6H3j6HQ?5p3SA3Y^xmzaAV5UhdJK2EDKdC!4+6rrEdUkG2nN5jhwmJddTE?%tDdd&g^p_")]
        public void AesTests(string plainText, string password)
        {
            var aes = new AesCryptography(password);

            var encrypted = aes.Encrypt(plainText);
            var decrypted = aes.Decrypt(encrypted);

            decrypted.Should().Be(plainText, "the decrypted string should be unchanged from the original string");
        }
    }
}
