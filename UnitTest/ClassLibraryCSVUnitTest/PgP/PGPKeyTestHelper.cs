using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Security;

namespace CsvTools.Tests
{
  public static class PGPKeyTestHelper
  {
    public static readonly long KeyID = -2973606277962788727;
    public static readonly string ClearPassphrase = "UGotMe";
    public static System.Security.SecureString Passphrase => ClearPassphrase.ToSecureString();

    public static PgpSecretKeyRingBundle PrivateKey
    {
      get => PgpHelper.ParsePrivateKey(cPrivate);
    }

    public static PgpPublicKey PublicKey
    {
      get => PgpHelper.ParsePublicKey(cPublic);
    }

    #region const

    // Generated these keys on https://www.igolder.com/pgp/generate-key/
    public const string cPublic = @"-----BEGIN PGP PUBLIC KEY BLOCK-----
Version: BCPG C# v1.6.1.0

mQENBFoLFdgBCACLNKAwnyLa2wyW2MEFtgJpxlXoXBhzhVfOl3zf6f6JL2ok4lbn
Rcl8SPAjjh3hwp8H1IoI8tP4y+Qq+x/QEYZzVnp/XQiLexL6lfyMiU8v2lswie0S
LXFYJ5wNcvTOL75sPAYtKDMy9RtNBjD0C+wAjeD8I01+AH/VntSgPcQNrKCkXsEV
2bKiOLYL1jO8VKIFVYvOZ0ywBh0udoyj8TWvzt9r/jXcYMLfQE6stE4/ERg5rVii
o93KEOgPh6whfCYW8CRhsEby2gqa4QBKWMOXktgd7azmxkOsguAWlHwAV3OkDVn4
D0O0N45+cotiZnyDvDVTAWtK1C97DBzOBiopABEBAAG0DXRlc3RAYWNtZS5jb22J
ARwEEAECAAYFAloLFdgACgkQ1rug2FQoEIlO9wf/S9SeORm5uhLBqh4Yu9NEfGj0
R0YuJv13LINZQ7Yqdfvanz13hpFeWpTy3q5bmI4Tbzw6yHmCXrwkmshENlJTWnIQ
Yt8F9lkJZ5uYPV2/H2UT/nBPhKqfZNEkLxVSePHScG9gpwyptuA6l4qFqVPtJgVs
Y05HdjllSD4qR09UnOz7u1/VxwYp8uuLLrWG00b3PiQPftX1UcLMlSrj0uFcKGja
XaLKMIAfWiFzht3a7HN0oeUacZNpsOMp0zWEEI1Yak0S5uVgTqon0zS+qVv81npZ
rIl4ZI0lO9GACgiKjJa0AC1AQfZ+s1bH6UcOUoayqpu1EUExwpLky12H+o7hiA==
=FLjK
-----END PGP PUBLIC KEY BLOCK-----";

    public const string cPrivate = @"-----BEGIN PGP PRIVATE KEY BLOCK-----
Version: BCPG C# v1.6.1.0

lQOsBFoLFdgBCACLNKAwnyLa2wyW2MEFtgJpxlXoXBhzhVfOl3zf6f6JL2ok4lbn
Rcl8SPAjjh3hwp8H1IoI8tP4y+Qq+x/QEYZzVnp/XQiLexL6lfyMiU8v2lswie0S
LXFYJ5wNcvTOL75sPAYtKDMy9RtNBjD0C+wAjeD8I01+AH/VntSgPcQNrKCkXsEV
2bKiOLYL1jO8VKIFVYvOZ0ywBh0udoyj8TWvzt9r/jXcYMLfQE6stE4/ERg5rVii
o93KEOgPh6whfCYW8CRhsEby2gqa4QBKWMOXktgd7azmxkOsguAWlHwAV3OkDVn4
D0O0N45+cotiZnyDvDVTAWtK1C97DBzOBiopABEBAAH/AwMCEbyFWRQd9oBgixKj
xzs218z+dQAEA9eU4sHkeXgA/4XMUKjtl4vxdf6LVNpZ3BGPpsF+kD7DFkQCNEnA
b/MSffC4tAgRL0WkiZ6ONoTWlLuWf5pEHh4t9on0A3AFx3gEI2RoYLS3pQjThhyH
PZXzFez8anuNKvoT7aRQuG12ROPn+P5CPiRC/jdeJldRDwr2qfIgute3P7KQRyW0
DGhSOZsHgis8UepYyYUUsUR0YjsULWJ9+ysI9vQ0QIrQitVcytNGwDA1kZZigCI3
VGekkVq3OUID5yZ7Q7MmQbFJuKJ+9B546x+NY65rkVDD4UHqf7FxqnHk3FOlw4Mx
93S2JncKi7YBcHoZjLWke8NzGH+i4qPMG8+YNOLI5wl5j9HtL4s+lIjj+w3yD22C
QyNbxtibNkClj1laYfgxwWxwTxkIr8DJSPzYLUYbXInaa5DDRXAl0bm/MrajUqqF
3XE/fCJMZZllv8d3+tPhR9Sao4ezTGbFZlOPJWl9XOG0arLDXrPX2PhU5Vbs7NKR
HJdmxBYFpSahQVaVizFcMIgiMVtrNhx9uJ18i7VGkTtC5vCzFoQN77S2e/CF171F
y7UFq8B9mFO7hoVexgCZqs3ei2ReSXQMTEkekr66eJApVI3lgBTrxpQCJVHNF0HR
blz11Dxx54ew2tyOKET8LDYAhVzByL24qg6BYfmEYd5lGnjcTC90ashMs5Hg+AWX
y+VGEF6aN14s8j4grNqsC41FuXLz6uU/yhjUNOY2Y8Udn+bOOIjnN2lYoCys+xPQ
msUfUOQoBzco8SSwepC0IDqGDrDwamqXIgDI538kM2r6hyi0u+bmlgRZ9H73a03F
W24zZsPLw7frImkQQ85LCvLID3JzrYs7FQey3aTTqLQNdGVzdEBhY21lLmNvbYkB
HAQQAQIABgUCWgsV2AAKCRDWu6DYVCgQiU73B/9L1J45Gbm6EsGqHhi700R8aPRH
Ri4m/Xcsg1lDtip1+9qfPXeGkV5alPLerluYjhNvPDrIeYJevCSayEQ2UlNachBi
3wX2WQlnm5g9Xb8fZRP+cE+Eqp9k0SQvFVJ48dJwb2CnDKm24DqXioWpU+0mBWxj
Tkd2OWVIPipHT1Sc7Pu7X9XHBiny64sutYbTRvc+JA9+1fVRwsyVKuPS4VwoaNpd
osowgB9aIXOG3drsc3Sh5Rpxk2mw4ynTNYQQjVhqTRLm5WBOqifTNL6pW/zWelms
iXhkjSU70YAKCIqMlrQALUBB9n6zVsfpRw5ShrKqm7URQTHCkuTLXYf6juGI
=nF4x
-----END PGP PRIVATE KEY BLOCK-----";

    #endregion const
  }
}
