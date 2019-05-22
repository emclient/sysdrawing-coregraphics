//
// System.Drawing.Color.cs
//
// Authors:
// 	Dennis Hayes (dennish@raytek.com)
// 	Ben Houston  (ben@exocortex.org)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
// 	Juraj Skripsky (juraj@hotfeet.ch)
//	Sebastien Pouliot  <sebastien@ximian.com>
//      Jiri Volejnik <aconcagua21@volny.cz>
//      Filip Navara <filip.navara@gmail.com>
//
// (C) 2002 Dennis Hayes
// (c) 2002 Ximian, Inc. (http://www.ximiam.com)
// (C) 2005 HotFeet GmbH (http://www.hotfeet.ch)
// Copyright (C) 2004,2006-2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Drawing 
{
	[Serializable]
	[TypeConverter (typeof (ColorConverter))]
	public struct Color {
		private const short StateKnownColorValid = 0x0001;
		private const short StateARGBValueValid = 0x0002;
		private const short StateValueMask = StateARGBValueValid;
		private const short StateNameValid = 0x0008;
		private const long NotDefinedValue = 0;

		 // Do not rename (binary serialization)
		private readonly string name;
		private readonly long value;
		private readonly short knownColor;
		private readonly short state;

		internal Color (KnownColor knownColor)
		{
			value = 0;
			state = StateKnownColorValid;
			name = null;
			this.knownColor = unchecked((short)knownColor);
		}

		private Color (long value, short state, string name, KnownColor knownColor)
		{
			this.value = value;
			this.state = state;
			this.name = name;
			this.knownColor = unchecked((short)knownColor);
		}

		public string Name {
			get {
				if ((state & StateNameValid) != 0) {
					return name;
				}

				if (IsKnownColor) {
					return KnownColors.GetName ((KnownColor)knownColor);
				}

				return Convert.ToString(value, 16);
			}
		}

		internal long Value {
			get {
				if ((state & StateValueMask) != 0) {
					return value;
				}

				if (IsKnownColor) {
					return KnownColors.GetValue ((KnownColor)knownColor);
				}

				return NotDefinedValue;
			}
		}

		public bool IsKnownColor {
			get{
				return (state & StateKnownColorValid) != 0;
			}
		}

		public bool IsSystemColor {
			get{
				return IsKnownColor && (((KnownColor)knownColor <= KnownColor.WindowText) || ((KnownColor)knownColor > KnownColor.YellowGreen));
			}
		}

		public bool IsNamedColor {
			get {
				return ((state & StateNameValid) != 0) || IsKnownColor;
			}
		}
	
		public static Color FromArgb (int red, int green, int blue)
		{
			return FromArgb (255, red, green, blue);
		}
		
		public static Color FromArgb (int alpha, int red, int green, int blue)
		{
			if ((red > 255) || (red < 0))
				throw CreateColorArgumentException(red, nameof(red));
			if ((green > 255) || (green < 0))
				throw CreateColorArgumentException (green, nameof(green));
			if ((blue > 255) || (blue < 0))
				throw CreateColorArgumentException (blue, nameof(blue));
			if ((alpha > 255) || (alpha < 0))
				throw CreateColorArgumentException (alpha, nameof(alpha));

			return new Color (((uint) alpha << 24) + (red << 16) + (green << 8) + blue, StateARGBValueValid, null, (KnownColor)0);
		}

		public int ToArgb()
		{
			return (int) Value;
		} 

		public static Color FromArgb (int alpha, Color baseColor)
		{
			return FromArgb (alpha, baseColor.R, baseColor.G, baseColor.B);
		}

		public static Color FromArgb (int argb)
		{
			return new Color (unchecked((uint)argb), StateARGBValueValid, null, (KnownColor)0);
		}

		public static Color FromKnownColor (KnownColor color)
		{
			if (color <= 0 || color > KnownColor.MenuHighlight) {
				return FromName (color.ToString());
			}

			return new Color (color);
		}

		public static Color FromName (string name)
		{
			if (ColorTable.TryGetNamedColor (name, out Color color)) {
				return color;
			}

			return new Color (NotDefinedValue, StateNameValid, name, (KnownColor)0);
		}

	
		public static readonly Color Empty;
		
		public static bool operator == (Color left, Color right)
		{
			return
				left.value == right.value &&
				left.state == right.state &&
				left.knownColor == right.knownColor &&
				left.name == right.name;
		}
		
		public static bool operator != (Color left, Color right)
		{
			return !(left == right);
		}

		private void GetRgbValues (out int r, out int g, out int b)
		{
			uint value = (uint)Value;
			r = (byte) (value >> 16);
			g = (byte) (value >> 8);
			b = (byte) (value);
		}

		public float GetBrightness ()
		{
			GetRgbValues (out int r, out int g, out int b);

			int minval = Math.Min (r, Math.Min (g, b));
			int maxval = Math.Max (r, Math.Max (g, b));
	
			return (float)(maxval + minval) / 510;
		}

		public float GetSaturation ()
		{
			GetRgbValues (out int r, out int g, out int b);

			int minval = Math.Min (r, Math.Min (g, b));
			int maxval = Math.Max (r, Math.Max (g, b));
			
			if (maxval == minval)
					return 0.0f;

			int sum = maxval + minval;
			if (sum > 255)
				sum = 510 - sum;

			return (float)(maxval - minval) / sum;
		}

		public float GetHue ()
		{
			GetRgbValues (out int r, out int g, out int b);

			byte minval = (byte) Math.Min (r, Math.Min (g, b));
			byte maxval = (byte) Math.Max (r, Math.Max (g, b));
			
			if (maxval == minval)
					return 0.0f;
			
			float diff = (float)(maxval - minval);
			float rnorm = (maxval - r) / diff;
			float gnorm = (maxval - g) / diff;
			float bnorm = (maxval - b) / diff;
	
			float hue = 0.0f;
			if (r == maxval) 
				hue = 60.0f * (6.0f + bnorm - gnorm);
			if (g == maxval) 
				hue = 60.0f * (2.0f + rnorm - bnorm);
			if (b  == maxval) 
				hue = 60.0f * (4.0f + gnorm - rnorm);
			if (hue > 360.0f) 
				hue = hue - 360.0f;

			return hue;
		}
		
		public KnownColor ToKnownColor ()
		{
			return (KnownColor)knownColor;
		}

		public bool IsEmpty 
		{
			get {
				return state == 0;
			}
		}

		public byte A {
			get { return (byte) (Value >> 24); }
		}

		public byte R {
			get { return (byte) (Value >> 16); }
		}

		public byte G {
			get { return (byte) (Value >> 8); }
		}

		public byte B {
			get { return (byte) Value; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is Color))
				return false;
			Color c = (Color) obj;
			return this == c;
		}

		private static int CombineHashCode(int h1, int h2)
		{
			// RyuJIT optimizes this to use the ROL instruction
			// Related GitHub pull request: dotnet/coreclr#1830
			uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
			return ((int)rol5 + h1) ^ h2;
		}

		public override int GetHashCode ()
		{
			if (name != null)
				return name.GetHashCode ();
			return CombineHashCode (CombineHashCode (value.GetHashCode (), knownColor.GetHashCode ()), state.GetHashCode ());
		}

		public override string ToString ()
		{
			if (IsNamedColor) {
				return nameof(Color) + " [" + Name + "]";
			} else if ((state & StateValueMask) != 0) {
				return nameof(Color) + " [A=" + A.ToString() + ", R=" + R.ToString() + ", G=" + G.ToString() + ", B=" + B.ToString() + "]";
			} else {
				return nameof(Color) + " [Empty]";
			}
		}

		static ArgumentException CreateColorArgumentException (int value, string color)
		{
			return new ArgumentException (string.Format ("'{0}' is not a valid"
				+ " value for '{1}'. '{1}' should be greater or equal to 0 and"
				+ " less than or equal to 255.", value, color));
		}

		public static Color Transparent => new Color (KnownColor.Transparent);

		public static Color AliceBlue => new Color (KnownColor.AliceBlue);

		public static Color AntiqueWhite => new Color (KnownColor.AntiqueWhite);

		public static Color Aqua => new Color (KnownColor.Aqua);

		public static Color Aquamarine => new Color (KnownColor.Aquamarine);

		public static Color Azure => new Color (KnownColor.Azure);

		public static Color Beige => new Color (KnownColor.Beige);

		public static Color Bisque => new Color (KnownColor.Bisque);

		public static Color Black => new Color (KnownColor.Black);

		public static Color BlanchedAlmond => new Color (KnownColor.BlanchedAlmond);

		public static Color Blue => new Color (KnownColor.Blue);

		public static Color BlueViolet => new Color (KnownColor.BlueViolet);

		public static Color Brown => new Color (KnownColor.Brown);

		public static Color BurlyWood => new Color (KnownColor.BurlyWood);

		public static Color CadetBlue => new Color (KnownColor.CadetBlue);

		public static Color Chartreuse => new Color (KnownColor.Chartreuse);

		public static Color Chocolate => new Color (KnownColor.Chocolate);

		public static Color Coral => new Color (KnownColor.Coral);

		public static Color CornflowerBlue => new Color (KnownColor.CornflowerBlue);

		public static Color Cornsilk => new Color (KnownColor.Cornsilk);

		public static Color Crimson => new Color (KnownColor.Crimson);

		public static Color Cyan => new Color (KnownColor.Cyan);

		public static Color DarkBlue => new Color (KnownColor.DarkBlue);

		public static Color DarkCyan => new Color (KnownColor.DarkCyan);

		public static Color DarkGoldenrod => new Color (KnownColor.DarkGoldenrod);

		public static Color DarkGray => new Color (KnownColor.DarkGray);

		public static Color DarkGreen => new Color (KnownColor.DarkGreen);

		public static Color DarkKhaki => new Color (KnownColor.DarkKhaki);

		public static Color DarkMagenta => new Color (KnownColor.DarkMagenta);

		public static Color DarkOliveGreen => new Color (KnownColor.DarkOliveGreen);

		public static Color DarkOrange => new Color (KnownColor.DarkOrange);

		public static Color DarkOrchid => new Color (KnownColor.DarkOrchid);

		public static Color DarkRed => new Color (KnownColor.DarkRed);

		public static Color DarkSalmon => new Color (KnownColor.DarkSalmon);

		public static Color DarkSeaGreen => new Color (KnownColor.DarkSeaGreen);

		public static Color DarkSlateBlue => new Color (KnownColor.DarkSlateBlue);

		public static Color DarkSlateGray => new Color (KnownColor.DarkSlateGray);

		public static Color DarkTurquoise => new Color (KnownColor.DarkTurquoise);

		public static Color DarkViolet => new Color (KnownColor.DarkViolet);

		public static Color DeepPink => new Color (KnownColor.DeepPink);

		public static Color DeepSkyBlue => new Color (KnownColor.DeepSkyBlue);

		public static Color DimGray => new Color (KnownColor.DimGray);

		public static Color DodgerBlue => new Color (KnownColor.DodgerBlue);

		public static Color Firebrick => new Color (KnownColor.Firebrick);

		public static Color FloralWhite => new Color (KnownColor.FloralWhite);

		public static Color ForestGreen => new Color (KnownColor.ForestGreen);

		public static Color Fuchsia => new Color (KnownColor.Fuchsia);

		public static Color Gainsboro => new Color (KnownColor.Gainsboro);

		public static Color GhostWhite => new Color (KnownColor.GhostWhite);

		public static Color Gold => new Color (KnownColor.Gold);

		public static Color Goldenrod => new Color (KnownColor.Goldenrod);

		public static Color Gray => new Color (KnownColor.Gray);

		public static Color Green => new Color (KnownColor.Green);

		public static Color GreenYellow => new Color (KnownColor.GreenYellow);

		public static Color Honeydew => new Color (KnownColor.Honeydew);

		public static Color HotPink => new Color (KnownColor.HotPink);

		public static Color IndianRed => new Color (KnownColor.IndianRed);

		public static Color Indigo => new Color (KnownColor.Indigo);

		public static Color Ivory => new Color (KnownColor.Ivory);

		public static Color Khaki => new Color (KnownColor.Khaki);

		public static Color Lavender => new Color (KnownColor.Lavender);

		public static Color LavenderBlush => new Color (KnownColor.LavenderBlush);

		public static Color LawnGreen => new Color (KnownColor.LawnGreen);

		public static Color LemonChiffon => new Color (KnownColor.LemonChiffon);

		public static Color LightBlue => new Color (KnownColor.LightBlue);

		public static Color LightCoral => new Color (KnownColor.LightCoral);

		public static Color LightCyan => new Color (KnownColor.LightCyan);

		public static Color LightGoldenrodYellow => new Color (KnownColor.LightGoldenrodYellow);

		public static Color LightGreen => new Color (KnownColor.LightGreen);

		public static Color LightGray => new Color (KnownColor.LightGray);

		public static Color LightPink => new Color (KnownColor.LightPink);

		public static Color LightSalmon => new Color (KnownColor.LightSalmon);

		public static Color LightSeaGreen => new Color (KnownColor.LightSeaGreen);

		public static Color LightSkyBlue => new Color (KnownColor.LightSkyBlue);

		public static Color LightSlateGray => new Color (KnownColor.LightSlateGray);

		public static Color LightSteelBlue => new Color (KnownColor.LightSteelBlue);

		public static Color LightYellow => new Color (KnownColor.LightYellow);

		public static Color Lime => new Color (KnownColor.Lime);

		public static Color LimeGreen => new Color (KnownColor.LimeGreen);

		public static Color Linen => new Color (KnownColor.Linen);

		public static Color Magenta => new Color (KnownColor.Magenta);

		public static Color Maroon => new Color (KnownColor.Maroon);

		public static Color MediumAquamarine => new Color (KnownColor.MediumAquamarine);

		public static Color MediumBlue => new Color (KnownColor.MediumBlue);

		public static Color MediumOrchid => new Color (KnownColor.MediumOrchid);

		public static Color MediumPurple => new Color (KnownColor.MediumPurple);

		public static Color MediumSeaGreen => new Color (KnownColor.MediumSeaGreen);

		public static Color MediumSlateBlue => new Color (KnownColor.MediumSlateBlue);

		public static Color MediumSpringGreen => new Color (KnownColor.MediumSpringGreen);

		public static Color MediumTurquoise => new Color (KnownColor.MediumTurquoise);

		public static Color MediumVioletRed => new Color (KnownColor.MediumVioletRed);

		public static Color MidnightBlue => new Color (KnownColor.MidnightBlue);

		public static Color MintCream => new Color (KnownColor.MintCream);

		public static Color MistyRose => new Color (KnownColor.MistyRose);

		public static Color Moccasin => new Color (KnownColor.Moccasin);

		public static Color NavajoWhite => new Color (KnownColor.NavajoWhite);

		public static Color Navy => new Color (KnownColor.Navy);

		public static Color OldLace => new Color (KnownColor.OldLace);

		public static Color Olive => new Color (KnownColor.Olive);

		public static Color OliveDrab => new Color (KnownColor.OliveDrab);

		public static Color Orange => new Color (KnownColor.Orange);

		public static Color OrangeRed => new Color (KnownColor.OrangeRed);

		public static Color Orchid => new Color (KnownColor.Orchid);

		public static Color PaleGoldenrod => new Color (KnownColor.PaleGoldenrod);

		public static Color PaleGreen => new Color (KnownColor.PaleGreen);

		public static Color PaleTurquoise => new Color (KnownColor.PaleTurquoise);

		public static Color PaleVioletRed => new Color (KnownColor.PaleVioletRed);

		public static Color PapayaWhip => new Color (KnownColor.PapayaWhip);

		public static Color PeachPuff => new Color (KnownColor.PeachPuff);

		public static Color Peru => new Color (KnownColor.Peru);

		public static Color Pink => new Color (KnownColor.Pink);

		public static Color Plum => new Color (KnownColor.Plum);

		public static Color PowderBlue => new Color (KnownColor.PowderBlue);

		public static Color Purple => new Color (KnownColor.Purple);

		public static Color Red => new Color (KnownColor.Red);

		public static Color RosyBrown => new Color (KnownColor.RosyBrown);

		public static Color RoyalBlue => new Color (KnownColor.RoyalBlue);

		public static Color SaddleBrown => new Color (KnownColor.SaddleBrown);

		public static Color Salmon => new Color (KnownColor.Salmon);

		public static Color SandyBrown => new Color (KnownColor.SandyBrown);

		public static Color SeaGreen => new Color (KnownColor.SeaGreen);

		public static Color SeaShell => new Color (KnownColor.SeaShell);

		public static Color Sienna => new Color (KnownColor.Sienna);

		public static Color Silver => new Color (KnownColor.Silver);

		public static Color SkyBlue => new Color (KnownColor.SkyBlue);

		public static Color SlateBlue => new Color (KnownColor.SlateBlue);

		public static Color SlateGray => new Color (KnownColor.SlateGray);

		public static Color Snow => new Color (KnownColor.Snow);

		public static Color SpringGreen => new Color (KnownColor.SpringGreen);

		public static Color SteelBlue => new Color (KnownColor.SteelBlue);

		public static Color Tan => new Color (KnownColor.Tan);

		public static Color Teal => new Color (KnownColor.Teal);

		public static Color Thistle => new Color (KnownColor.Thistle);

		public static Color Tomato => new Color (KnownColor.Tomato);

		public static Color Turquoise => new Color (KnownColor.Turquoise);

		public static Color Violet => new Color (KnownColor.Violet);

		public static Color Wheat => new Color (KnownColor.Wheat);

		public static Color White => new Color (KnownColor.White);

		public static Color WhiteSmoke => new Color (KnownColor.WhiteSmoke);

		public static Color Yellow => new Color (KnownColor.Yellow);

		public static Color YellowGreen => new Color (KnownColor.YellowGreen);
	}
}
