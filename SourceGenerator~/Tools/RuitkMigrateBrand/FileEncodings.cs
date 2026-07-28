using System;
using System.Text;

namespace Ruitk.SourceGenerator.Tools
{
    /// <summary>
    /// Byte-order-mark aware read/write helpers for the rebrand codemod (audit finding M3).
    ///
    /// <para><c>File.ReadAllText</c> + <c>File.WriteAllText</c> do NOT round-trip: the read
    /// consumes a BOM, the write never emits one. Every user <c>.cs</c> saved by Visual Studio
    /// (which writes a UTF-8 BOM by default) would come back BOM-less — a whole-file VCS diff on
    /// top of the intended rename, which defeats review of a codemod's output.</para>
    ///
    /// <para>Worse, <c>File.ReadAllText</c> decodes with a replacing fallback: a file saved in an
    /// OS ANSI codepage (Shift-JIS, Windows-1252 — normal in non-English studios) turns every
    /// invalid byte into U+FFFD and is written back as UTF-8, irreversibly mojibaking the user's
    /// own comments and strings. So decoding here is STRICT and the caller SKIPS what it cannot
    /// decode.</para>
    /// </summary>
    public static class FileEncodings
    {
        /// <summary>Which byte-order mark (if any) a file carried, so it can be written back.</summary>
        public enum BomKind
        {
            Utf8NoBom,
            Utf8Bom,
            Utf16Le,
            Utf16Be,
        }

        private static readonly byte[] s_bomUtf8 = { 0xEF, 0xBB, 0xBF };
        private static readonly byte[] s_bomUtf16Le = { 0xFF, 0xFE };
        private static readonly byte[] s_bomUtf16Be = { 0xFE, 0xFF };

        private static readonly Encoding s_utf8 =
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private static readonly Encoding s_utf16Le =
            new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true);

        private static readonly Encoding s_utf16Be =
            new UnicodeEncoding(bigEndian: true, byteOrderMark: false, throwOnInvalidBytes: true);

        /// <summary>
        /// Decodes file bytes, detecting and remembering the BOM. Returns <c>false</c> when the
        /// file has no BOM and is not valid UTF-8 — the caller must then SKIP the file rather than
        /// rewrite it, because decoding it as UTF-8 would destroy the user's text.
        /// </summary>
        public static bool TryDecode(byte[] bytes, out string? text, out BomKind bom)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes));

            if (StartsWith(bytes, s_bomUtf8))
            {
                bom = BomKind.Utf8Bom;
                return TryGetString(s_utf8, bytes, s_bomUtf8.Length, out text);
            }

            // UTF-32 LE also opens FF FE (then 00 00) — not a text encoding this tool handles;
            // fall through so it fails the strict UTF-8 decode and gets skipped.
            if (StartsWith(bytes, s_bomUtf16Le)
                && !(bytes.Length >= 4 && bytes[2] == 0x00 && bytes[3] == 0x00))
            {
                bom = BomKind.Utf16Le;
                return TryGetString(s_utf16Le, bytes, s_bomUtf16Le.Length, out text);
            }

            if (StartsWith(bytes, s_bomUtf16Be))
            {
                bom = BomKind.Utf16Be;
                return TryGetString(s_utf16Be, bytes, s_bomUtf16Be.Length, out text);
            }

            bom = BomKind.Utf8NoBom;
            return TryGetString(s_utf8, bytes, 0, out text);
        }

        /// <summary>Re-encodes migrated text with the exact BOM the file arrived with.</summary>
        public static byte[] Encode(string text, BomKind bom)
        {
            if (text is null) throw new ArgumentNullException(nameof(text));

            return bom switch
            {
                BomKind.Utf8Bom => Concat(s_bomUtf8, s_utf8.GetBytes(text)),
                BomKind.Utf16Le => Concat(s_bomUtf16Le, s_utf16Le.GetBytes(text)),
                BomKind.Utf16Be => Concat(s_bomUtf16Be, s_utf16Be.GetBytes(text)),
                _ => s_utf8.GetBytes(text),
            };
        }

        private static bool TryGetString(Encoding enc, byte[] bytes, int offset, out string? text)
        {
            try
            {
                text = enc.GetString(bytes, offset, bytes.Length - offset);
                return true;
            }
            catch (DecoderFallbackException)
            {
                text = null;
                return false;
            }
        }

        private static bool StartsWith(byte[] bytes, byte[] prefix)
        {
            if (bytes.Length < prefix.Length) return false;
            for (int i = 0; i < prefix.Length; i++)
                if (bytes[i] != prefix[i])
                    return false;
            return true;
        }

        private static byte[] Concat(byte[] a, byte[] b)
        {
            var r = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, r, 0, a.Length);
            Buffer.BlockCopy(b, 0, r, a.Length, b.Length);
            return r;
        }
    }
}
