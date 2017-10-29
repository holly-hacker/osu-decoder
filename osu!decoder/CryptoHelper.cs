using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace osu_decoder_dnlib
{
	// Token: 0x02000005 RID: 5
	internal class CryptoHelper
	{
		// Token: 0x04000012 RID: 18
		private readonly SymmetricAlgorithm _symmetricAlgorithm;

		// Token: 0x06000025 RID: 37 RVA: 0x00002518 File Offset: 0x00000718
		public CryptoHelper(string password)
		{
			Program.Verbose("Creating new decoder with password " + password);
			this._symmetricAlgorithm = new RijndaelManaged
			{
				KeySize = 256,
				BlockSize = 128,
				IV = new Rfc2898DeriveBytes(password, new byte[]
				{
					28,
					136,
					27,
					216,
					83,
					147,
					140,
					207,
					60,
					153,
					41,
					107,
					117,
					164,
					37,
					157,
					94,
					233,
					51,
					48,
					146,
					108,
					127,
					191,
					30,
					226,
					250,
					88,
					109,
					7,
					132,
					15
				}).GetBytes(16),
				Key = new Rfc2898DeriveBytes(password, new byte[]
				{
					167,
					126,
					112,
					16,
					4,
					244,
					15,
					120,
					135,
					116,
					123,
					212,
					157,
					48,
					5,
					194,
					12,
					179,
					153,
					201,
					204,
					249,
					248,
					212,
					86,
					20,
					215,
					55,
					105,
					157,
					111,
					11
				}).GetBytes(32)
			};
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000025A8 File Offset: 0x000007A8
		internal string Decrypt(string input)
		{
			if (!input.StartsWith("#="))
			{
				throw new NotImplementedException("I don't support this encryption type");
			}
			string text = input.Substring(2);
			if (text.StartsWith("q"))
			{
				text = text.Substring(1);
				text = text.Replace('_', '+').Replace('$', '/');
				string text2 = this.DecryptWithXor(Convert.FromBase64String(text));
				Program.Debug(string.Format("Decrypted {0} to {1}", input, text2));
				return text2;
			}
			throw new NotImplementedException("I don't support this encryption type (yet), poke me about it");
		}

		// Token: 0x06000027 RID: 39 RVA: 0x0000262C File Offset: 0x0000082C
		private string DecryptWithXor(byte[] toDecrypt)
		{
			MemoryStream memoryStream = new MemoryStream();
			using (ICryptoTransform cryptoTransform = this._symmetricAlgorithm.CreateDecryptor())
			{
				CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
				cryptoStream.Write(toDecrypt, 0, toDecrypt.Length);
				cryptoStream.FlushFinalBlock();
				cryptoStream.Close();
			}
			toDecrypt = memoryStream.ToArray();
			byte b = toDecrypt[toDecrypt.Length - 1];
			Array.Resize<byte>(ref toDecrypt, toDecrypt.Length - 1);
			for (int i = 0; i < toDecrypt.Length; i++)
			{
				byte[] array = toDecrypt;
				int num = i;
				array[num] ^= b;
			}
			return Encoding.UTF8.GetString(toDecrypt);
		}
	}
}
