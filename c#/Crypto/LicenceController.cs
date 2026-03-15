using LicenceApi.Data;
using LicenceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;
using System.Security.Cryptography;
using System.Text;

namespace LicenceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicenceController : ControllerBase
{
    private readonly LicenceDbContext _context;

    public LicenceController(LicenceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Litsenziya yaratish
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Licence>> CreateLicence([FromBody] CreateLicenceRequest request, CancellationToken cancellationToken = default)
    {
        var licenseViewModel = new LicenceViewModel
        {
            LicenceId = Guid.NewGuid(),
            MechineId = request.MechineId,
            UserCount = request.UserCount,
            DeadLine = request.DeadLine
        };

        var privateKey = GetLicencePrivateKey();
        var data = GetLicenceHashData(licenseViewModel);

        var signer = new RsaDigestSigner(new Sha256Digest());
        signer.Init(forSigning: true, privateKey);
        signer.BlockUpdate(data, 0, data.Length);

        var licence = new Licence
        {
            LicenceId = licenseViewModel.LicenceId,
            MechineId = request.MechineId,
            UserCount = request.UserCount,
            DeadLine = request.DeadLine,
            SignData = Convert.ToBase64String(signer.GenerateSignature())
        };

        _context.Licences.Add(licence);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetLicenceById), new { id = licence.LicenceId }, licence);
    }

    /// <summary>
    /// Barcha litsenziyalarni olish
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Licence>>> GetLicences(CancellationToken cancellationToken = default)
    {
        var licences = await _context.Licences.ToListAsync(cancellationToken);
        return Ok(licences);
    }

    /// <summary>
    /// Litsenziyani ID bo'yicha olish
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Licence>> GetLicenceById(Guid id, CancellationToken cancellationToken = default)
    {
        var licence = await _context.Licences.FindAsync([id], cancellationToken);
        if (licence == null)
            return NotFound();

        var public_key = GetLicencePublicKey();

        var data = GetLicenceHashData(licence);

        var signer = new RsaDigestSigner(new Sha256Digest());
        signer.Init(false, public_key);
        signer.BlockUpdate(data, 0, data.Length);
        bool verify = signer.VerifySignature(Convert.FromBase64String(licence.SignData));

        return Ok(licence);
    }

    private AsymmetricKeyParameter GetLicencePublicKey()
    {
        using (var reader = new StreamReader(Path.Combine("Certificates", "public_key.pem")))
        {
            var pemReader = new PemReader(reader);
            var publicKey = (AsymmetricKeyParameter)pemReader.ReadObject();
            return publicKey;
        }
    }

    private AsymmetricKeyParameter GetLicencePrivateKey()
    {
        using (var reader = new StreamReader(Path.Combine("Certificates", "private_key.pem")))
        {
            var pemReader = new PemReader(reader);
            var pair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            return pair.Private;
        }
    }

    private byte[] GetLicenceHashData(Licence licence)
    {
        var canonical = string.Join("|",
             licence.LicenceId,
             licence.MechineId,
             licence.UserCount,
             licence.DeadLine.ToUniversalTime().ToString("O"));

        return SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
    }

    private byte[] GetLicenceHashData(LicenceViewModel licence)
    {
        var canonical = string.Join("|",
             licence.LicenceId,
             licence.MechineId,
             licence.UserCount,
             licence.DeadLine.ToUniversalTime().ToString("O"));

        return SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
    }
}
