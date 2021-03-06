﻿#if SUDOKU_RECOGNIZING
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Sudoku.Data;
using Sudoku.Data.Extensions;
using Field = Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>;

namespace Sudoku.Recognitions
{
	/// <summary>
	/// Define a recognizer.
	/// </summary>
	/// <remarks>
	/// During the recognizing, the <b>field</b> indicates the whole outline of a grid.
	/// </remarks>
	internal sealed class InternalServiceProvider : IDisposable
	{
		/// <summary>
		/// The internal <see cref="Tesseract"/> instance.
		/// </summary>
		private Tesseract? _ocr;


		/// <summary>
		/// Indicates whether the current recognizer has already initialized.
		/// </summary>
		public bool Initialized => !(_ocr is null);


		/// <inheritdoc/>
		public void Dispose() => _ocr?.Dispose();

		/// <summary>
		/// Recognizes digits.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <returns>The grid.</returns>
		/// <exception cref="RecognizingException">
		/// Throws when the processing is wrong or unhandleable.
		/// </exception>
		public Grid RecognizeDigits(Field field)
		{
			var result = Grid.Empty.Clone();
			int w = field.Width / 9;
			int o = w / 6;
			for (int x = 0; x < 9; x++)
			{
				for (int y = 0; y < 9; y++)
				{
					// Recognize digit from cell.
					int recognition =
						RecognizeCellNumber(field.GetSubRect(new Rectangle(o + w * x, o + w * y, w - o * 2, w - o * 2)));
					switch (recognition)
					{
						case 0:
						{
							continue;
						}
						case -1:
						{
							throw new RecognizingException(
								$"Recognition error. Cannot fill the cell r{x + 1}c{y + 1} because the current value " +
								$"is not a valid number.");
						}
					}

					int cell = x * 9 + y, digit = recognition - 1;
					if (result[cell, digit])
					{
						throw new RecognizingException(
							$"Recognition error. Cannot fill the cell r{x + 1}c{y + 1} with the digit {digit + 1}.");
					}

					result[cell] = digit;
				}
			}

			// The result will be transposed.
			return result.Transpose();
		}

		/// <summary>
		/// Recognize the number of a cell.
		/// </summary>
		/// <param name="cellImg">The image of a cell.</param>
		/// <returns>
		/// The result value (must be between 1 and 9). If the recognition is failed,
		/// the value will be <c>0</c>.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Throws when the OCR engine error.
		/// </exception>
		private int RecognizeCellNumber(Field cellImg)
		{
			// Convert the image to gray-scale and filter out the noise
			var imgGray = new Mat();
			CvInvoke.CvtColor(cellImg, imgGray, ColorConversion.Bgr2Gray);

			// TODO: can be problem with values for some image.
			// Another methods to process image, but worse. Use only one!
			var imgThresholded = new Mat();
			CvInvoke.Threshold(
				imgGray, imgThresholded, InternalSettings.ThOcrMin, InternalSettings.ThOcrMax,
				ThresholdType.Binary);

			_ocr!.SetImage(imgThresholded);
			if (_ocr.Recognize() != 0)
			{
				throw new InvalidOperationException("Tessaract Error. Cannot to recognize cell image.");
			}

			var characters = _ocr.GetCharacters();
			string numberText = string.Empty;
			foreach (var c in characters)
			{
				if (!(c.Text is " "))
				{
					numberText += c.Text;
				}
			}

			return numberText.Length > 1 ? -1 : int.TryParse(numberText, out int resultValue) ? resultValue : -1;
		}

		/// <summary>
		/// Initializes <see cref="Tesseract"/> instance.
		/// </summary>
		/// <param name="dir">The directory.</param>
		/// <param name="lang">The language. The default value is <c>"eng"</c>.</param>
		/// <returns>The task.</returns>
		public async Task<bool> InitTesseractAsync(string dir, string lang = "eng")
		{
			try
			{
				if (!File.Exists($@"{dir}\{lang}.traineddata"))
				{
					return await TesseractDownloadLangFileAsync(dir, lang);
				}

				_ocr = new Tesseract(dir, lang, OcrEngineMode.TesseractOnly, "123456789");
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// When the trained data is failed to find in the local machine, this method will download
		/// the file online.
		/// </summary>
		/// <param name="dir">The directory to find.</param>
		/// <param name="lang">The language.</param>
		/// <returns>The result.</returns>
		private async Task<bool> TesseractDownloadLangFileAsync(string dir, string lang)
		{
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			HttpClient? client = null;
			try
			{
				client = new HttpClient();
				File.WriteAllText(
					$@"{dir}\{lang}.traineddata",
					await client.GetStringAsync(
						new Uri($"https://github.com/tesseract-ocr/tessdata/raw/master/{lang}.traineddata")));

				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				client?.Dispose();
			}
		}
	}
}
#endif