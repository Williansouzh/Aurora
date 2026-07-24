using Aurora.API.Files;
using FluentAssertions;
using Xunit;

namespace Aurora.Tests.Security;

public class ImageContentValidatorTests
{
    private static byte[] Pad(params byte[] prefix)
    {
        var buffer = new byte[ImageContentValidator.HeaderSize];
        prefix.CopyTo(buffer, 0);
        return buffer;
    }

    [Fact]
    public void Aceita_assinaturas_de_imagem_validas()
    {
        ImageContentValidator.IsAllowedImage(Pad(0xFF, 0xD8, 0xFF)).Should().BeTrue(); // JPEG
        ImageContentValidator.IsAllowedImage(Pad(0x89, 0x50, 0x4E, 0x47)).Should().BeTrue(); // PNG
        ImageContentValidator.IsAllowedImage(Pad(0x47, 0x49, 0x46, 0x38)).Should().BeTrue(); // GIF
        ImageContentValidator.IsAllowedImage(
            [0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50]).Should().BeTrue(); // WebP
    }

    [Fact]
    public void Rejeita_conteudo_que_nao_e_imagem()
    {
        // "<script>" / HTML — an executable payload renamed as an image.
        ImageContentValidator.IsAllowedImage(Pad(0x3C, 0x73, 0x63, 0x72, 0x69, 0x70, 0x74)).Should().BeFalse();
        // "%PDF"
        ImageContentValidator.IsAllowedImage(Pad(0x25, 0x50, 0x44, 0x46)).Should().BeFalse();
    }

    [Fact]
    public void Rejeita_cabecalho_muito_curto()
    {
        ImageContentValidator.IsAllowedImage(new byte[] { 0xFF, 0xD8, 0xFF }).Should().BeFalse();
        ImageContentValidator.IsAllowedImage(System.Array.Empty<byte>()).Should().BeFalse();
    }
}
