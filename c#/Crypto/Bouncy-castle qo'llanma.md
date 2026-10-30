# BouncyCastle bilan kriptografiya — amaliy qo'llanma

C# / .NET 10 · BouncyCastle.Cryptography 2.6.1

Barcha API nomlari `BouncyCastle.Cryptography.dll` 2.6.1 dan reflection orqali
tasdiqlangan.

---

## Mundarija

1. [AES-GCM ichki tuzilishi — H, GHASH, GMAC](#1-aes-gcm-ichki-tuzilishi)
2. [AAD — amaliyotda qanday ishlatiladi](#2-aad--amaliyotda)
3. [BouncyCastle AES-GCM API — Init / ProcessBytes / DoFinal](#3-bouncycastle-aes-gcm-api)
4. [RandomNumberGenerator va SecureRandom](#4-randomnumbergenerator-va-securerandom)
5. [Elliptik egri chiziq nuqtalari — to'liq](#5-elliptik-egri-chiziq-nuqtalari)

---

# 1. AES-GCM ichki tuzilishi

## 1.1. GCM ikki qismdan iborat

```
GCM  =  CTR (maxfiylik)  +  GHASH (butunlik)
                              ↑
                         GMAC ham shu
```

- **CTR** — faqat ochiq matnni shifrlaydi
- **GHASH** — AAD ham, ciphertext ham o'tadigan polinomial hash

## 1.2. Bitta kalitdan uchta qiymat

AES-GCM ga **faqat bitta kalit** `K` beriladi. Qolgani ichida hosil bo'ladi:

```
K  ──►  H  = AES-ECB_K(00000000000000000000000000000000)   ← GHASH/GMAC kaliti
   ──►  J0 = nonce ‖ 00000001                              ← hisoblagich (96-bit nonce uchun)
   ──►  AES_K(J0)                                          ← tegni niqoblovchi blok
```

`H` — bu KDF emas, shunchaki **16 ta nol baytni `K` bilan shifrlash**. Bitta AES bloki.

```csharp
static byte[] DeriveHashSubkey(byte[] key)
{
    using var aes = Aes.Create();
    aes.Key = key;
    return aes.EncryptEcb(new byte[16], PaddingMode.None);
}
```

## 1.3. To'liq oqim

```
C   = CTR_K(J0+1, plaintext)                       ← faqat ochiq matn shifrlanadi
S   = GHASH_H(AAD ‖ pad ‖ C ‖ pad ‖ len(AAD) ‖ len(C))
Tag = S ⊕ AES_K(J0)
```

GHASH — GF(2^128) maydonidagi polinom:

```
S = A₁·H^m ⊕ A₂·H^(m-1) ⊕ … ⊕ C₁·H^k ⊕ … ⊕ len·H
```

AAD bloklari va ciphertext bloklari **aynan bir xil polinomga** koeffitsient
bo'lib kiradi. Farqi shundaki, AAD shifrlanmaydi — u faqat shu yerga tushadi.

## 1.4. GHASH — GF(2^128) ko'paytmasi

NIST SP 800-38D, 6.3-bo'lim. GCM da bitlar "teskari" tartibda sanaladi:
0-bit — birinchi baytning eng katta razryadi.

```csharp
// DIQQAT: bu o'quv maqsadidagi implementatsiya — DOIMIY VAQTDA ISHLAMAYDI.
// Real kodda AesGcm (AES-NI + CLMUL) ishlatiladi.
public static byte[] Multiply(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
{
    const byte R = 0xE1;              // x^128 + x^7 + x^2 + x + 1
    var z = new byte[16];
    var v = y.ToArray();

    for (var i = 0; i < 128; i++)
    {
        var bit = (x[i >> 3] >> (7 - (i & 7))) & 1;
        if (bit == 1)
            for (var j = 0; j < 16; j++)
                z[j] ^= v[j];

        var lsbSet = (v[15] & 1) == 1;
        ShiftRight(v);
        if (lsbSet)
            v[0] ^= R;                // kamaytirish (modul olish)
    }

    return z;
}

private static void ShiftRight(byte[] v)
{
    for (var i = 15; i > 0; i--)
        v[i] = (byte)((v[i] >> 1) | ((v[i - 1] & 1) << 7));

    v[0] >>= 1;
}
```

## 1.5. GMAC nima

**GMAC = GCM, plaintext bo'sh bo'lgan holda.**

| | plaintext | AAD | natija |
|---|---|---|---|
| GCM | bor | ixtiyoriy | ciphertext + tag |
| **GMAC** | **bo'sh** | bor | **faqat tag** |

Alohida algoritm emas — GCM ning xususiy holati. NIST SP 800-38D da shunday
ta'riflangan.

```csharp
// GMAC: hech narsa shifrlanmaydi, faqat AAD autentifikatsiya qilinadi
var tag = new byte[16];
using var aesGcm = new AesGcm(key, 16);
aesGcm.Encrypt(nonce, ReadOnlySpan<byte>.Empty, Span<byte>.Empty, tag, aad);
```

## 1.6. Nega nonce takrorlanishi halokatli

`H` **faqat `K` ga bog'liq, nonce ga emas** — bitta kalit ostida hech qachon
o'zgarmaydi. Bir xil `K` + bir xil nonce bilan ikkita xabar chiqsa:

```
Tag₁ ⊕ Tag₂  =  S₁ ⊕ S₂        ← AES_K(J0) niqobi qisqarib ketadi
→ H bo'yicha polinom tenglamasi → H TOPILADI
→ hujumchi istalgan xabarga to'g'ri teg yasay oladi
```

Bu — **"forbidden attack"**. 2016 yilda minglab HTTPS serverlarda topilgan.

**Yechim:** har bir xabarga alohida salt → alohida kalit → alohida `H`.
Nonce to'qnashuvi zararsiz bo'lib qoladi.

```csharp
// Ikki bosqichli kalit hosil qilish
// MASTER KEY --HKDF-SHA256(salt = har xabarga random 16B)--> DATA KEY
var salt = RandomNumberGenerator.GetBytes(16);
var dataKey = new byte[32];
HKDF.DeriveKey(HashAlgorithmName.SHA256, masterKey, dataKey, salt, keyContext);
```

Va teg **hech qachon qisqartirilmaydi** — 128 bitdan kam teg soxtalashtirish
ehtimolini keskin oshiradi.

---

# 2. AAD — amaliyotda

## 2.1. Real protokollarda

AAD doim ishlatiladi, lekin protokol uni o'zi to'ldiradi.

| Protokol | AAD nima |
|---|---|
| **TLS 1.3** | yozuv sarlavhasi: `type ‖ version ‖ length` (5 bayt) |
| **TLS 1.2 GCM** | `seq_num ‖ type ‖ version ‖ length` |
| **QUIC** | butun paket sarlavhasi (connection ID, packet number) |
| **IPsec ESP** | `SPI ‖ sequence number` |
| **SSH (RFC 5647)** | paket uzunligi maydoni |
| **JWE** | base64url(protected header) |
| **AWS/GCP/Azure KMS** | "encryption context" |
| **Signal** | ikkala tomonning identity kalitlari |

**Umumiy qonuniyat:** ochiq ketishi shart bo'lgan, lekin o'zgartirilishi mumkin
bo'lmagan hamma narsa AAD ga tushadi.

Nega ochiq ketadi? Qabul qiluvchi paketni **parse qilishi** uchun uzunlik, tur,
ketma-ketlik raqami deshifrlashdan **oldin** kerak. Shifrlab bo'lmaydi. Lekin
himoyasiz qoldirsa, hujumchi ularni o'zgartiradi.

## 2.2. Konvert sarlavhasini AAD ga qo'shish

```csharp
// AAD = konvert sarlavhasi ‖ foydalanuvchi AAD si
private static byte[] BuildAssociatedData(ReadOnlySpan<byte> header, string? associatedData)
{
    if (string.IsNullOrEmpty(associatedData))
        return header.ToArray();

    var extra = Encoding.UTF8.GetBytes(associatedData);
    var result = new byte[header.Length + extra.Length];
    header.CopyTo(result);
    extra.CopyTo(result, header.Length);
    return result;
}
```

Sarlavha (magic + versiya + kdfId + salt + IV) har doim tegga bog'lanadi. Buni
qilmasangiz, hujumchi ikkita shifrmatnning IV larini almashtira oladi.

## 2.3. Record swapping hujumi

Bazadagi `users.passport` ustuni bitta kalit bilan shifrlangan:

```
id=1001  Alisher   passport_enc = QUdDMQEB....(A)
id=2002  Bobur     passport_enc = QUdDMQEB....(B)
```

Hujumchi `A` ni o'z qatoriga ko'chiradi:

```sql
UPDATE users SET passport_enc = '....(A)' WHERE id = 2002;
```

Endi ilova Boburning profilida **Alisherning pasportini** ko'rsatadi. Kalit
bitta, IV to'g'ri, teg to'g'ri — kriptografiya buzilmadi, lekin ma'lumot oqdi.

AAD bu hujumni yopadi:

```csharp
crypto.Encrypt(passport, $"v1/users/{userId}/passport");
```

## 2.4. Eng muhim qoida

```
❌ NOTO'G'RI:  {"cipher": "...", "aad": "users/1001/passport"}   ← bitta blobda
```

Hujumchi blobni ko'chirganda AAD ham u bilan ko'chadi → himoya nolga teng.

```csharp
// ✅ TO'G'RI: AAD ni O'QILGAN JOYDAN qayta quring
var aad = $"v1/users/{row.Id}/passport";   // row.Id — bazadagi haqiqiy qator
```

AAD **manzildan** kelishi kerak, blobdan emas. Shuning uchun uni saqlash shart emas.

## 2.5. AAD — generatsiya emas, qayta qurish

| | Kalit | IV / nonce | **AAD** |
|---|---|---|---|
| Qayerdan | KDF | CSPRNG (tasodifiy) | **kontekstdan, pattern bo'yicha** |
| Tasodifiymi | — | ha | **yo'q, determinlashgan** |
| Saqlanadimi | yo'q | ha, konvertda | **yo'q — qayta hosil qilinadi** |
| Maxfiymi | ha | yo'q | yo'q |

Ikkita amaliy tuzoq:

**1. Bayt-ma-bayt bir xil bo'lishi shart.** Bitta probel, `Users` vs `users`,
yoki madaniyatga bog'liq formatlash — teg mos kelmaydi.

```csharp
static string PassportAad(long userId) =>
    string.Create(CultureInfo.InvariantCulture, $"v1/users/{userId}/passport");
```

**2. Pattern — versiyaga tegishli.** Keyinchalik formatni o'zgartirsangiz, eski
shifrmatnlar ochilmay qoladi. Shuning uchun boshidanoq `v1/` qo'ying.

## 2.6. Qayerga nima yozish

| Holat | AAD |
|---|---|
| Baza ustuni | `"v1/{jadval}/{birlamchi kalit}/{ustun}"` |
| Multi-tenant | `"v1/tenant:{id}/..."` |
| Fayl | fayl yo'li yoki `fileId` |
| Sessiya token | `"v1/session"` yoki foydalanuvchi id |
| Xabar navbati | `"v1/queue:{nom}"` |
| Backup | `backupId ‖ sana` |

---

# 3. BouncyCastle AES-GCM API

## 3.1. Obyekt modeli

```
new AesEngine()                        // xom AES bloki (16 → 16 bayt), rejimsiz
   ↓ o'raladi
new GcmBlockCipher(new AesEngine())    // GCM rejimi = CTR + GHASH
   ↓ sozlanadi
new AeadParameters(key, macBits, nonce, aad)
```

`AesEngine` — faqat `E_K(block)`. U na IV, na teg, na uzunlik haqida biladi.
Barcha "aqlli" qism `GcmBlockCipher` da.

## 3.2. Eng oddiy misol

```csharp
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

byte[] key   = RandomNumberGenerator.GetBytes(32);  // AES-256
byte[] nonce = RandomNumberGenerator.GetBytes(12);  // 96-bit IV
byte[] aad   = "v1/users/1001/passport"u8.ToArray();
byte[] plain = "AA1234567"u8.ToArray();

// ---------- SHIFRLASH ----------
var enc = new GcmBlockCipher(new AesEngine());
enc.Init(true, new AeadParameters(new KeyParameter(key), 128, nonce, aad));

byte[] output = new byte[enc.GetOutputSize(plain.Length)];   // ciphertext + tag
int len = enc.ProcessBytes(plain, 0, plain.Length, output, 0);
len += enc.DoFinal(output, len);
// output = ciphertext ‖ tag(16)

// ---------- DESHIFRLASH ----------
var dec = new GcmBlockCipher(new AesEngine());
dec.Init(false, new AeadParameters(new KeyParameter(key), 128, nonce, aad));

byte[] result = new byte[dec.GetOutputSize(output.Length)];
int rlen = dec.ProcessBytes(output, 0, output.Length, result, 0);
rlen += dec.DoFinal(result, rlen);   // ← teg AYNAN shu yerda tekshiriladi
```

Naqsh har doim bir xil: **Init → (ProcessAadBytes) → ProcessBytes → DoFinal**.

## 3.3. `Init(bool forEncryption, ICipherParameters)`

```csharp
cipher.Init(
    forEncryption: true,          // true = shifrlash, false = deshifrlash
    new AeadParameters(
        new KeyParameter(key),    // AES kaliti (16/24/32 bayt)
        128,                      // teg uzunligi BITLARDA (bayt emas!)
        nonce,                    // IV, 12 bayt tavsiya etiladi
        aad));                    // ixtiyoriy, null bo'lishi mumkin
```

Ichida nima bo'ladi:

```
1. AesEngine kalit bilan sozlanadi   → kalit jadvali (key expansion)
2. H  = E_K(0^128)                   ← GHASH/GMAC kaliti
3. J0 = nonce ‖ 00000001             ← 96-bit nonce uchun
   (boshqa uzunlikda: J0 = GHASH(nonce ‖ pad ‖ len))
4. GHASH holati nolga tushadi
5. AAD darhol GHASH ga yutiladi
6. Ichki bufer tozalanadi
```

**Uchta muhim xususiyat:**

**a) `macSize` bitlarda.** `128` yozasiz, `16` emas. Chalkashtirsangiz — 16-bitli
teg, ya'ni soxtalashtirish ehtimoli 1/65536.

**b) `Init` holatni to'liq tozalaydi.** Bir obyektni qayta ishlatish mumkin,
lekin har safar `Init` shart.

**c) BC nonce takrorlanishini o'zi to'sadi:**

```csharp
enc.Init(true, new AeadParameters(new KeyParameter(key), 128, nonce, aad));
enc.Init(true, new AeadParameters(new KeyParameter(key), 128, nonce, aad));
// → ArgumentException: cannot reuse nonce for GCM encryption
```

Bu foydali, lekin faqat **oldingi** `Init` ni eslaydi — butun tarixni emas.
Nonce ni siz boshqarishingiz kerak.

## 3.4. AAD ni berishning ikki yo'li

```csharp
// A) AeadParameters ichida — qulay, bir martalik
cipher.Init(true, new AeadParameters(new KeyParameter(key), 128, nonce, aad));

// B) alohida — bo'lak-bo'lak berish kerak bo'lsa
cipher.Init(true, new ParametersWithIV(new KeyParameter(key), nonce));
cipher.ProcessAadBytes(header, 0, header.Length);
cipher.ProcessAadBytes(extra,  0, extra.Length);
```

**Qoida:** barcha `ProcessAadBytes` `ProcessBytes` dan **oldin** chaqirilishi
kerak. GHASH polinomida AAD bloklari ciphertext bloklaridan oldin keladi.

## 3.5. `ProcessBytes` — eng katta tuzoq

```csharp
int written = cipher.ProcessBytes(input, inOff, len, output, outOff);
```

Qaytargan qiymat — **shu chaqiruvda yozilgan baytlar soni**, siz bergan `len` emas.

```csharp
// ❌ NOTO'G'RI
cipher.ProcessBytes(plain, 0, plain.Length, output, 0);
cipher.DoFinal(output, plain.Length);          // ofset xato!

// ✅ TO'G'RI — qaytgan qiymatni yig'ib boring
int len = cipher.ProcessBytes(plain, 0, plain.Length, output, 0);
len += cipher.DoFinal(output, len);
```

Nega farq? `GcmBlockCipher` ichida bufer bor:

- **Shifrlashda** — bir blok (16 bayt) buferlanadi
- **Deshifrlashda** — blok + teg o'lchami (16 + 16 = 32 bayt)

Deshifrlashdagi sabab muhim: oxirgi 16 bayt **teg**, lekin oqim tugaganini faqat
`DoFinal` da bilasiz. Shuning uchun GCM oxirgi 32 baytni ushlab turishi kerak.

Amalda: kichik matnda `ProcessBytes` ko'pincha **0** qaytaradi, hamma narsa
`DoFinal` da chiqadi.

## 3.6. `DoFinal(byte[] output, int outOff)`

| | Shifrlashda | Deshifrlashda |
|---|---|---|
| Nima qiladi | buferdagi qoldiqni chiqaradi, `S = GHASH(...)` yakunlaydi, `Tag = S ⊕ E_K(J0)` hisoblab **oxiriga qo'shadi** | qoldiqni chiqaradi, tegni qayta hisoblab **doimiy vaqtda solishtiradi** |
| Qaytaradi | qoldiq + 16 (teg) | qoldiq (teg chiqishga kirmaydi) |
| Xato | — | `InvalidCipherTextException("mac check in GCM failed")` |

```csharp
try
{
    len += cipher.DoFinal(output, len);
}
catch (InvalidCipherTextException)
{
    // Sababni oshkor qilmang — bitta umumiy javob.
    // MUHIM: bu paytda `output` ichida DESHIFRLANGAN, LEKIN TASDIQLANMAGAN
    // ma'lumot yotgan bo'lishi mumkin. Uni ISHLATMANG va tozalang.
    throw new CryptoIntegrityException("Butunlik tasdiqlanmadi.");
}
finally
{
    Arrays.Fill(dataKey, 0);
    if (plainBytes is not null) Arrays.Fill(plainBytes, 0);
}
```

Bu — GCM ni qo'lda ishlatishdagi asosiy xavf. `.NET` ning `AesGcm.Decrypt` i teg
mos kelmasa chiqish buferini o'zi nolga to'ldiradi; BC da buni **siz** qilasiz.

`DoFinal` dan keyin obyekt `Init` holatiga qaytadi — qayta ishlatsa bo'ladi,
lekin shifrlash uchun **yangi nonce bilan qayta `Init`** shart.

## 3.7. Bufer o'lchamini hisoblash

```csharp
int GetOutputSize(int len)        // ProcessBytes(len) + DoFinal uchun JAMI
int GetUpdateOutputSize(int len)  // faqat ProcessBytes(len) uchun
```

```
Shifrlashda:    GetOutputSize(n) = n + buferdagi + 16      ← teg qo'shiladi
Deshifrlashda:  GetOutputSize(n) = n + buferdagi − 16      ← teg ayriladi
```

## 3.8. `GetMac()` va `Reset()`

```csharp
byte[] tag = cipher.GetMac();   // DoFinal dan KEYIN — tegning nusxasi
cipher.Reset();                 // Init dagi holatga qaytaradi (kalit/nonce O'ZGARMAYDI)
```

`GetMac()` teg va ciphertextni alohida saqlaydigan formatlarda foydali.

⚠️ `Reset()` shifrlashda xavfli: kalit va nonce o'zgarmaydi, ya'ni yana
shifrlasangiz — **nonce takrorlanishi**. Uni faqat bekor qilingan amaldan keyin
tozalash uchun ishlating.

## 3.9. Oqim (streaming)

```csharp
var cipher = new GcmBlockCipher(new AesEngine());
cipher.Init(true, new AeadParameters(new KeyParameter(key), 128, nonce, aad));

var inBuf  = new byte[64 * 1024];
var outBuf = new byte[cipher.GetUpdateOutputSize(inBuf.Length)];

int read;
while ((read = source.Read(inBuf, 0, inBuf.Length)) > 0)
{
    int n = cipher.ProcessBytes(inBuf, 0, read, outBuf, 0);
    if (n > 0) destination.Write(outBuf, 0, n);
}

var tail = new byte[cipher.GetOutputSize(0)];
int f = cipher.DoFinal(tail, 0);
destination.Write(tail, 0, f);      // qoldiq + teg
```

⚠️ Oqimli **deshifrlash** da printsipial muammo: teg faqat oxirida tekshiriladi,
ya'ni tasdiqlanmagan ma'lumotni allaqachon yozib bo'lgan bo'lasiz. Katta
fayllarni vaqtinchalik joyga yozing va faqat `DoFinal` o'tgach ko'chiring — yoki
faylni bo'laklarga bo'lib, har biriga o'z tegini qo'ying.

## 3.10. Boshqa KDF lar — BC dagi ekvivalentlar

```csharp
// PBKDF2-HMAC-SHA512 — .NET ning Rfc2898DeriveBytes i bilan BIT-MA-BIT bir xil
var generator = new Pkcs5S2ParametersGenerator(new Sha512Digest());
generator.Init(passwordBytes, salt, iterations);
var key = ((KeyParameter)generator.GenerateDerivedMacParameters(256)).GetKey();

// Argon2id — .NET da YO'Q, faqat BC da. RFC 9106.
var parameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
    .WithVersion(Argon2Parameters.Version13)
    .WithSalt(salt)
    .WithMemoryAsKB(65536)      // 64 MiB  (OWASP minimumi: 19456)
    .WithIterations(3)          // OWASP minimumi: 2
    .WithParallelism(4)
    .Build();

var argon = new Argon2BytesGenerator();
argon.Init(parameters);
argon.GenerateBytes(passwordBytes, destination);

// HKDF-SHA256 — RFC 5869, .NET ning HKDF si bilan bir xil
var hkdf = new HkdfBytesGenerator(new Sha256Digest());
hkdf.Init(new HkdfParameters(masterKey, salt, info));
hkdf.GenerateBytes(destination, 0, destination.Length);
```

**Nega Argon2id:** PBKDF2 faqat protsessor vaqtini talab qiladi va GPU/ASIC da
juda yaxshi parallellashadi. Argon2id **xotira-qattiq**: har urinishga 64 MiB
kerak, shuning uchun million urinishni parallel qilish apparat jihatdan
qimmatlashadi.

⚠️ KDF ni almashtirish master kalitni **butunlay o'zgartiradi** — eski
shifrmatnlar ochilmay qoladi. Bu migratsiya, oddiy sozlama emas.

## 3.11. .NET bilan solishtirish

| | .NET `AesGcm` | BC `GcmBlockCipher` |
|---|---|---|
| Uslub | bir zarbali (one-shot) | bosqichli (Init/Process/DoFinal) |
| Oqim | yo'q | bor |
| Teg | alohida `Span` | chiqishning oxirida |
| Tezlik | AES-NI + CLMUL | dasturiy |
| Xato | `CryptographicException`, bufer tozalanadi | `InvalidCipherTextException`, **tozalash sizda** |
| Nonce nazorati | yo'q | oldingi `Init` bilan solishtiradi |
| Argon2 | yo'q | bor |

---

# 4. RandomNumberGenerator va SecureRandom

## 4.1. Bir xil emas, lekin ikkalasi ham xavfsiz

```
RandomNumberGenerator          SecureRandom
        │                            │
        ▼                            ▼
  OS CSPRNG                   DigestRandomGenerator (SHA-256)
  (BCryptGenRandom /                 │
   getrandom(2))                     ▼
                              OS entropiyasidan urug'lanadi
```

| | `RandomNumberGenerator` | `SecureRandom` |
|---|---|---|
| Joyi | `System.Security.Cryptography` | `Org.BouncyCastle.Security` |
| Ichi | OS syscall | SHA-256 DRBG + OS urug'i |
| Holat | yadroda | jarayon xotirasida |
| Urug'lash | **mumkin emas** | `SetSeed()` bor, lekin **qo'shimcha** (reset emas) |
| Bazaviy sinf | `IDisposable` | ⚠️ **`System.Random`** |

## 4.2. `SecureRandom : System.Random` — ehtiyot bo'ling

Reflection natijasi: `SecureRandom` `System.Random` dan meros oladi. Barcha
metodlar xavfsiz implementatsiya bilan qayta yozilgan, lekin kodda chalkashish oson:

```csharp
Random rng = new SecureRandom();      // ✅ xavfsiz
Random rng = new Random();            // ❌ xuddi shu tip, MUTLAQO xavfsiz emas
```

Metod imzosi `Random` qabul qilsa, ikkalasi ham o'tadi. Kalit yasaydigan joyda
tipni har doim `SecureRandom` deb aniq yozing.

## 4.3. `SetSeed` — qo'shadi, almashtirmaydi

```csharp
var a = new SecureRandom(); a.SetSeed(seed);
var b = new SecureRandom(); b.SetSeed(seed);
// a va b BIR XIL ketma-ketlik bermaydi
```

Ataylab shunday — Java ning `new Random(seed)` kabi tasodifan determinlashgan
generator yasab qo'yish imkonsiz.

## 4.4. Qachon qaysinisi

```csharp
// RandomNumberGenerator — o'zingiz chaqirayotgan bo'lsangiz
var nonce = RandomNumberGenerator.GetBytes(12);
RandomNumberGenerator.Fill(salt);

// SecureRandom — BC API si TALAB QILSA
var gen = new ECKeyPairGenerator();
gen.Init(new ECKeyGenerationParameters(domain, new SecureRandom()));   // majbur
```

Bunday joylar: `ECKeyPairGenerator`, `RsaKeyPairGenerator`,
`CipherKeyGenerator`, imzo generatorlari.

## 4.5. Ikkalasini ulash

```csharp
using Org.BouncyCastle.Crypto.Prng;

// BC endi foydalanuvchi makonidagi DRBG ni emas, OS ni chaqiradi
var random = new SecureRandom(new CryptoApiRandomGenerator());
```

Auditda "bitta entropiya manbasi bo'lsin" talabi bo'lsa — shu yo'l.

⚠️ `System.Random` ning o'zi bashorat qilinadi va kalit/IV uchun **hech qachon**
yaramaydi.

---

# 5. Elliptik egri chiziq nuqtalari

## 5.1. Nuqta nima

Egri chiziq: `y² = x³ + ax + b` (mod p). Nuqta — shu tenglamani qanoatlantiruvchi
`(x, y)` juftligi.

Nuqtalar ustida **guruh amali** aniqlangan:

```
P + Q   →  ikkita nuqtadan uchinchisi (chiziq o'tkazib, uchinchi kesishuvni aks ettirish)
2P      →  P + P (urinma chiziq orqali)
O       →  cheksizlikdagi nuqta = guruhning "nol"i
-P      →  (x, -y)
k·P     →  P ni k marta qo'shish (skalyar ko'paytma)
```

Butun xavfsizlik bitta faktda: `k` va `P` dan `k·P` ni hisoblash **oson**, lekin
`P` va `k·P` dan `k` ni topish **imkonsiz**. Bu — ECDLP.

## 5.2. BC obyekt ierarxiyasi

```
X9ECParameters          ← egri chiziqning standart ta'rifi (secp256r1 va h.k.)
├── Curve : ECCurve     ← p, a, b — maydon va tenglama
├── G     : ECPoint     ← GENERATOR (bazaviy nuqta)
├── N     : BigInteger  ← G ning tartibi (n·G = O)
└── H     : BigInteger  ← kofaktor

ECDomainParameters      ← yuqoridagini kriptografik API lar uchun o'raydi
ECPrivateKeyParameters  ← D : BigInteger      (yopiq kalit — SON)
ECPublicKeyParameters   ← Q : ECPoint         (ochiq kalit — NUQTA)
```

**Yopiq kalit — son, ochiq kalit — nuqta.** Butun farq shu.

## 5.3. Egri chiziqni olish

```csharp
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Parameters;

// CustomNamedCurves — optimallashtirilgan (tezroq)
X9ECParameters x9 = CustomNamedCurves.GetByName("secp256r1");   // = NIST P-256
// yoki: ECNamedCurveTable.GetByName("secp256r1")  — umumiy jadval

var domain = new ECDomainParameters(x9);

Console.WriteLine($"p bit  : {domain.Curve.FieldSize}");   // 256
Console.WriteLine($"n      : {domain.N.ToString(16)}");    // tartib
Console.WriteLine($"h      : {domain.H}");                 // P-curve larda 1
Console.WriteLine($"G      : {Hex(domain.G.GetEncoded(false))}");
```

`n` — nechta nuqta bor. Barcha skalyarlar `mod n` bo'yicha hisoblanadi.

## 5.4. Ochiq kalit = d·G

```csharp
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

var random = new SecureRandom();

// 1) Yopiq kalit — shunchaki [1, n-1] oralig'idagi tasodifiy SON
BigInteger d = BigIntegers.CreateRandomInRange(
    BigInteger.One, domain.N.Subtract(BigInteger.One), random);

// 2) Ochiq kalit — G ni d marta ko'paytirish. HAMMASI SHU.
ECPoint Q = domain.G.Multiply(d).Normalize();

var priv = new ECPrivateKeyParameters(d, domain);
var pub  = new ECPublicKeyParameters(Q, domain);
```

Amalda generator ishlatiladi:

```csharp
using Org.BouncyCastle.Crypto.Generators;

var gen = new ECKeyPairGenerator();
gen.Init(new ECKeyGenerationParameters(domain, random));
var pair = gen.GenerateKeyPair();

var priv = (ECPrivateKeyParameters)pair.Private;   // priv.D  → BigInteger
var pub  = (ECPublicKeyParameters)pair.Public;     // pub.Q   → ECPoint

// Tekshirish: Q haqiqatan d·G mi?
Console.WriteLine(pub.Q.Equals(domain.G.Multiply(priv.D)));   // True
```

## 5.5. Nuqta amallari — to'liq ro'yxat

```csharp
ECPoint P = domain.G;
ECPoint Q = domain.G.Multiply(BigInteger.ValueOf(7));

ECPoint sum    = P.Add(Q);              // P + Q
ECPoint diff   = P.Subtract(Q);         // P − Q
ECPoint dbl    = P.Twice();             // 2P  (Add dan tezroq — maxsus formula)
ECPoint neg    = P.Negate();            // −P  (y ni almashtirish, juda arzon)
ECPoint mul    = P.Multiply(k);         // k·P
ECPoint tp     = P.TwicePlus(Q);        // 2P + Q  (bitta amalda, tezroq)

bool isInf     = P.IsInfinity;          // P == O ?
bool ok        = P.IsValid();           // egri chiziq ustidami + tartib to'g'rimi
bool norm      = P.IsNormalized();
```

Ikkita nuqtaning kombinatsiyasi (ECDSA tekshiruvida kerak) — **Shamir hiylasi**:

```csharp
using Org.BouncyCastle.Math.EC;

// u1·G + u2·Q — alohida ikki marta ko'paytirishdan ~1.4x tez
ECPoint R = ECAlgorithms.SumOfTwoMultiplies(G, u1, Q, u2);
// yoki: ECAlgorithms.ShamirsTrick(G, u1, Q, u2)
// ko'p had uchun: ECAlgorithms.SumOfMultiplies(points, scalars)
```

## 5.6. `Normalize()` — eng katta amaliy tuzoq

BC nuqtalarni ichida **proyektiv koordinatalarda** saqlaydi (`X:Y:Z`), chunki har
bir qo'shishda modul bo'yicha teskari qiymat hisoblash juda qimmat. Affin
`(x, y)` ga o'tish uchun `Z` ga bo'lish kerak — bu faqat oxirida bir marta.

```csharp
ECPoint Q = domain.G.Multiply(d);

var x = Q.AffineXCoord;               // ❌ InvalidOperationException: point not in normal form
var x = Q.Normalize().AffineXCoord;   // ✅

// Xom (proyektiv) koordinatalar — bular AFFIN EMAS, ishlatmang:
var raw = Q.XCoord;                   // Z ga bo'linmagan qiymat
```

| Metod | Normalize kerakmi |
|---|---|
| `AffineXCoord` / `AffineYCoord` | **ha** |
| `GetEncoded(...)` | yo'q — ichida o'zi qiladi |
| `Equals(...)` | yo'q |
| `Add` / `Multiply` zanjiri | yo'q — oxirida bir marta |

```csharp
// ✅ To'g'ri naqsh: zanjir oxirida bitta Normalize
ECPoint result = G.Multiply(a).Add(Q.Multiply(b)).Normalize();
BigInteger x = result.AffineXCoord.ToBigInteger();
```

## 5.7. Nuqtani kodlash — 04 / 02 / 03

```csharp
byte[] uncompressed = Q.GetEncoded(false);   // 0x04 ‖ X ‖ Y   → P-256 da 65 bayt
byte[] compressed   = Q.GetEncoded(true);    // 0x02|0x03 ‖ X  → P-256 da 33 bayt

ECPoint back = domain.Curve.DecodePoint(compressed);
int size = Q.GetEncodedLength(true);
```

| Bayt | Ma'no |
|---|---|
| `0x00` | cheksizlikdagi nuqta (O) |
| `0x04` | siqilmagan: X ham, Y ham yozilgan |
| `0x02` | siqilgan, Y **juft** |
| `0x03` | siqilgan, Y **toq** |

Siqilgan shakl nega ishlaydi: `x` ma'lum bo'lsa, `y² = x³ + ax + b` dan `y` ni
kvadrat ildiz orqali topish mumkin. Ikkita yechim bor (`y` va `−y`), prefiks
qaysinisi ekanini aytadi. Joyning yarmi tejaladi, evaziga dekodlashda bitta
modul kvadrat ildiz hisoblanadi.

## 5.8. ⚠️ BigInteger va nol baytlar

```csharp
// X koordinatani MAYDON O'LCHAMIGA to'ldirib olish — MAJBURIY
int fieldBytes = (domain.Curve.FieldSize + 7) / 8;   // P-256 → 32
byte[] xBytes = BigIntegers.AsUnsignedByteArray(fieldBytes, Q.AffineXCoord.ToBigInteger());
```

`BigInteger` boshidagi nol baytlarni tashlab yuboradi. To'ldirmasangiz, `x`
**1/256 ehtimol bilan** 31 baytli chiqadi — va kod "ba'zan" ishlamay qoladi.
Bunday xatoni topish juda qiyin.

## 5.9. Begona nuqtani tekshirish — majburiy

```csharp
ECPoint peer = domain.Curve.DecodePoint(peerBytes);

if (peer.IsInfinity)
    throw new SecurityException("Cheksizlikdagi nuqta");

if (!peer.IsValid())                    // egri chiziq ustidami + n·Q == O mi
    throw new SecurityException("Nuqta egri chiziqda emas");

// Kofaktor 1 dan katta bo'lsa (masalan h=8), qo'shimcha:
if (domain.H.CompareTo(BigInteger.One) > 0 && !peer.Multiply(domain.N).IsInfinity)
    throw new SecurityException("Kichik guruhdagi nuqta");
```

**Invalid curve attack:** hujumchi egri chiziqda yotmaydigan nuqta yuboradi. U
nuqta boshqa, zaif tartibli guruhga tegishli bo'ladi. Siz `d·Q` hisoblab
natijani qaytarganingizda, hujumchi `d` ning kichik bo'lagini bilib oladi. Bir
necha o'nlab so'rovdan keyin — Xitoy qoldiqlar teoremasi orqali butun `d`.
2015 yilda ko'plab TLS va JOSE kutubxonalari shu bilan buzilgan.

```csharp
// Yaratish paytida darhol tekshirish
ECPoint safe = domain.Curve.ValidatePoint(xBig, yBig);   // yaroqsiz bo'lsa — throw
// yoki
ECPoint safe = ECAlgorithms.ValidatePoint(point);
```

## 5.10. ECDH — nuqta ko'paytmasi

```csharp
// Alisa tomonida
ECPoint shared = peerPublic.Q.Multiply(myPrivate.D).Normalize();

// Bob tomonida
ECPoint shared = alicePublic.Q.Multiply(bobPrivate.D).Normalize();

// Ikkalasi bir xil:  d_A·(d_B·G) = d_B·(d_A·G) = (d_A·d_B)·G
```

Umumiy sir — shu nuqtaning **X koordinatasi**:

```csharp
BigInteger z = shared.AffineXCoord.ToBigInteger();
byte[] Z = BigIntegers.AsUnsignedByteArray(fieldBytes, z);
```

⚠️ `Z` ni **to'g'ridan-to'g'ri kalit sifatida ishlatmang** — u bir tekis
taqsimlanmagan (egri chiziq nuqtasining X i barcha 2²⁵⁶ qiymatni teng ehtimol
bilan olmaydi). HKDF orqali "tekislash" shart.

```csharp
// BC dagi tayyor agreement
var agreement = new ECDHBasicAgreement();
agreement.Init(myPrivate);
BigInteger z = agreement.CalculateAgreement(peerPublic);

// SO'NG — majburiy HKDF
var sharedSecret = BigIntegers.AsUnsignedByteArray(fieldBytes, z);
var hkdf = new HkdfBytesGenerator(new Sha384Digest());
hkdf.Init(new HkdfParameters(sharedSecret, salt, info));
hkdf.GenerateBytes(masterKey, 0, 32);
```

## 5.11. ECDSA imzolash

```
k  ← tasodifiy [1, n−1]          ← BIR MARTALIK, HECH QACHON TAKRORLANMAYDI
R  = k·G                          ← NUQTA
r  = R.x mod n                    ← nuqtaning X i → son
e  = H(xabar) ning yuqori bitlari
s  = k⁻¹·(e + r·d) mod n

Imzo = (r, s)
```

Qo'lda:

```csharp
BigInteger n = domain.N;
BigInteger e = new BigInteger(1, hash);   // SHA-256 natijasi

BigInteger k = BigIntegers.CreateRandomInRange(
    BigInteger.One, n.Subtract(BigInteger.One), random);

ECPoint R = domain.G.Multiply(k).Normalize();

BigInteger r = R.AffineXCoord.ToBigInteger().Mod(n);
BigInteger s = k.ModInverse(n).Multiply(e.Add(r.Multiply(d))).Mod(n);
```

BC bilan:

```csharp
using Org.BouncyCastle.Crypto.Signers;

var signer = new ECDsaSigner();
signer.Init(true, new ParametersWithRandom(priv, random));
BigInteger[] sig = signer.GenerateSignature(hash);   // {r, s}
```

`GenerateSignature` **xesh** kutadi, xabarni emas — siz oldindan `SHA256` qilasiz.

## 5.12. ECDSA tekshirish

```
w  = s⁻¹ mod n
u1 = e·w mod n
u2 = r·w mod n
R' = u1·G + u2·Q          ← OCHIQ KALIT NUQTA SIFATIDA ISHLATILADI
To'g'ri ⟺ R'.x mod n == r
```

Nega ishlaydi:

```
R' = u1·G + u2·Q
   = (e·w)G + (r·w)(d·G)
   = w·(e + r·d)·G
   = s⁻¹·(e + r·d)·G
s = k⁻¹(e + rd)  ⟹  s⁻¹(e + rd) = k
R' = k·G = R ✓
```

```csharp
BigInteger w  = s.ModInverse(n);
BigInteger u1 = e.Multiply(w).Mod(n);
BigInteger u2 = r.Multiply(w).Mod(n);

ECPoint Rp = ECAlgorithms.SumOfTwoMultiplies(domain.G, u1, pub.Q, u2).Normalize();

bool valid = !Rp.IsInfinity && Rp.AffineXCoord.ToBigInteger().Mod(n).Equals(r);
```

BC bilan:

```csharp
var verifier = new ECDsaSigner();
verifier.Init(false, pub);
bool valid = verifier.VerifySignature(hash, sig[0], sig[1]);
```

## 5.13. `k` takrorlanishi — halokat

Ikkita imzo bir xil `k` bilan yasalsa, ikkalasida `r` bir xil chiqadi (chunki
`R = k·G` bir xil). Shundan:

```
s₁ = k⁻¹(e₁ + rd)
s₂ = k⁻¹(e₂ + rd)
─────────────────────────
s₁ − s₂ = k⁻¹(e₁ − e₂)
k = (e₁ − e₂) / (s₁ − s₂)  mod n
d = (s₁·k − e₁) / r        mod n     ← YOPIQ KALIT TOPILDI
```

Real hodisalar: **PlayStation 3** (Sony `k` ni doimiy qilib qo'ygan, 2010),
**Android Bitcoin hamyonlari** (2013, zaif RNG).

**Yechim — RFC 6979:** `k` ni tasodifiy emas, xabar va yopiq kalitdan
determinlashgan hosil qilish.

```csharp
var signer = new ECDsaSigner(new HMacDsaKCalculator(new Sha256Digest()));
signer.Init(true, priv);              // random KERAK EMAS
BigInteger[] sig = signer.GenerateSignature(hash);
```

Bir xil xabar → bir xil imzo, turli xabar → turli `k`. RNG ga umuman bog'liq
emas. **Buni default qilib qo'ying.**

## 5.14. Imzodan ochiq kalitni tiklash

`r` — bu `R` nuqtasining X koordinatasi. Demak `R` ni qayta qurish mumkin
(ikkita nomzod: `y` juft yoki toq), keyin:

```
Q = r⁻¹·(s·R − e·G)
```

```csharp
// recId: 0..3 — qaysi nomzod ekanini bildiradi (imzo bilan birga saqlanadi)
static ECPoint RecoverPublicKey(
    ECDomainParameters dp, byte[] hash, BigInteger r, BigInteger s, int recId)
{
    BigInteger n = dp.N;
    int fieldBytes = (dp.Curve.FieldSize + 7) / 8;

    // 1) x = r + (recId / 2)·n
    BigInteger x = r.Add(BigInteger.ValueOf(recId / 2).Multiply(n));

    // 2) x dan R nuqtasini tiklash — prefiks y ning juft/toqligini beradi
    byte[] encoded = new byte[fieldBytes + 1];
    encoded[0] = (byte)(0x02 | (recId & 1));
    BigIntegers.AsUnsignedByteArray(x, encoded, 1, fieldBytes);
    ECPoint R = dp.Curve.DecodePoint(encoded);

    if (!R.Multiply(n).IsInfinity)
        throw new InvalidOperationException("nR != O — nomzod noto'g'ri");

    // 3) Q = r⁻¹(s·R − e·G)
    BigInteger e     = new BigInteger(1, hash);
    BigInteger rInv  = r.ModInverse(n);
    BigInteger srInv = rInv.Multiply(s).Mod(n);
    BigInteger eInv  = rInv.Multiply(n.Subtract(e)).Mod(n);

    return ECAlgorithms.SumOfTwoMultiplies(dp.G, eInv, R, srInv).Normalize();
}
```

Isbot:

```
r⁻¹(s·R − e·G) = r⁻¹(s·k·G − e·G) = r⁻¹(s·k − e)·G
s·k = e + rd  ⟹  s·k − e = rd
= r⁻¹·r·d·G = d·G = Q ✓
```

Ethereum imzosi `(r, s, v)` — `v` aynan o'sha `recId`. Ochiq kalitni
tranzaksiyaga qo'shish shart emas, u imzodan tiklanadi.

## 5.15. Kalitlarni import/eksport

```csharp
using Org.BouncyCastle.Security;

// Import
var priv = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(pkcs8Der);
var pub  = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(spkiDer);

// Eksport
byte[] pkcs8 = PrivateKeyInfoFactory.CreatePrivateKeyInfo(priv).GetDerEncoded();
byte[] spki  = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pub).GetDerEncoded();
```

.NET tomoni bilan mos:

```csharp
using var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP384);
byte[] pkcs8 = ecdh.ExportPkcs8PrivateKey();
byte[] spki  = ecdh.ExportSubjectPublicKeyInfo();
// ← shularni PrivateKeyFactory/PublicKeyFactory ga berish mumkin
```

## 5.16. Yig'ma jadval

| Tushuncha | BC turi | Nima |
|---|---|---|
| Yopiq kalit | `BigInteger D` | son |
| Ochiq kalit | `ECPoint Q` | **nuqta**, `Q = d·G` |
| Generator | `ECPoint G` | egri chiziqning bazaviy nuqtasi |
| Tartib | `BigInteger N` | `n·G = O` |
| Kofaktor | `BigInteger H` | P-curve larda 1 |
| ECDH siri | `ECPoint` → `AffineXCoord` | `d_A·Q_B` |
| Imzo `r` | `BigInteger` | `(k·G).x mod n` |
| Tekshirish | `SumOfTwoMultiplies` | `u1·G + u2·Q` |

## 5.17. Uchta oltin qoida

1. **Koordinata o'qishdan oldin `Normalize()`** — aks holda
   `InvalidOperationException` yoki (xom koordinata orqali) jimgina noto'g'ri
   qiymat.
2. **Begona nuqtani `IsValid()` bilan tekshiring** — invalid curve attack real.
3. **`k` uchun RFC 6979 (`HMacDsaKCalculator`)** — RNG ga ishonmang.

---

## Ilova: eng ko'p uchraydigan xatolar

| Xato | Oqibati |
|---|---|
| `macSize` ni baytlarda berish (`16` o'rniga `128` kerak) | 16-bitli teg, soxtalashtirish 1/65536 |
| `ProcessBytes` qaytargan qiymatni e'tiborsiz qoldirish | ofset xato, buzilgan chiqish |
| `DoFinal` xato bergach chiqish buferini tozalamaslik | tasdiqlanmagan ma'lumot oqib ketadi |
| Bir xil kalit+nonce bilan ikki marta shifrlash | GCM da `H` oshkor bo'ladi |
| ECDH xom sirini to'g'ridan-to'g'ri kalit qilish | bir tekis taqsimlanmagan kalit |
| `AsUnsignedByteArray` siz `BigInteger.ToByteArray()` | 1/256 holatda bir bayt kalta |
| `Normalize()` siz `AffineXCoord` | exception yoki noto'g'ri qiymat |
| Begona nuqtani tekshirmaslik | invalid curve attack → yopiq kalit oqadi |
| ECDSA `k` ni takrorlash | yopiq kalit to'liq tiklanadi |
| AAD ni shifrmatn bilan birga saqlash | record swapping himoyasi nolga teng |