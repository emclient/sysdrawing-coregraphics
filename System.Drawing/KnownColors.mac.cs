//
// System.Drawing.KnownColors
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Peter Dennis Bartok (pbartok@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//      Jiri Volejnik <aconcagua21@volny.cz>
//      Filip Navara <filip.navara@gmail.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using Foundation;
using System.Drawing.Mac;
using System.Collections.Generic;
using AppKit;

namespace System.Drawing
{

	internal static partial class KnownColors
	{
		static internal NSColor[] NativeColors = new NSColor[(int)KnownColor.WindowText + 1];

		static void Update ()
		{
			// note: Mono's SWF Theme class will call the static Update method to apply
			// correct system colors outside Windows

			NativeColors[(int)KnownColor.ActiveBorder] = NSColor.WindowFrame;
			// KnownColor.ActiveCaption
			// KnownColor.ActiveCaptionText
			// KnownColor.AppWorkspace
			NativeColors[(int)KnownColor.Control] = NSColor.Control;
			NativeColors[(int)KnownColor.ControlText] = NSColor.ControlText;
			NativeColors[(int)KnownColor.ControlDark] = NSColor.ControlShadow;
			NativeColors[(int)KnownColor.ControlDarkDark] = NSColor.ControlDarkShadow;
			NativeColors[(int)KnownColor.ControlLight] = NSColor.ControlHighlight;
			NativeColors[(int)KnownColor.ControlLightLight] = NSColor.ControlLightHighlight;
			// KnownColor.Desktop
			NativeColors[(int)KnownColor.GrayText] = NSColor.DisabledControlText;
			//ArgbValues[(int)KnownColor.Highlight] = NSColor.Highlight.ToUArgb();
			NativeColors[(int)KnownColor.Highlight] = NSColor.SelectedTextBackground;
			NativeColors[(int)KnownColor.HighlightText] = NSColor.SelectedText;
			// KnownColor.HighlightText
			// KnownColor.HotTrack
			// KnownColor.InactiveBorder
			// KnownColor.InactiveCaption
			// KnownColor.InactiveCaptionText
			//NSColors[(int)KnownColor.Info] = NSColor.WindowBackground;
			//NSColors[(int)KnownColor.InfoText] = NSColor.ControlText;
			// KnownColor.Menu
			// KnownColor.MenuText
			NativeColors[(int)KnownColor.ScrollBar] = NSColor.ScrollBar;
			NativeColors[(int)KnownColor.Window] = NSColor.WindowBackground;
			NativeColors[(int)KnownColor.WindowText] = NSColor.WindowFrameText;
			NativeColors[(int)KnownColor.WindowFrame] = NSColor.WindowFrame;
			NativeColors[(int)KnownColor.ButtonFace] = NSColor.Control;
			NativeColors[(int)KnownColor.ButtonHighlight] = NSColor.ControlHighlight;
			NativeColors[(int)KnownColor.ButtonShadow] = NSColor.ControlShadow;
			// KnownColor.GradientActiveCaption
			// KnownColor.GradientInactiveCaption
			// KnownColor.MenuBar
			// KnownColor.MenuHighlight
		}

		static long ConvertComponent (nfloat c)
		{
			return (byte)(c * 255);
		}

		static long FromSystemColor (KnownColor kc)
		{
			if (kc >= 0 && (int)kc <= NativeColors.Length && NativeColors[(int)kc] != null) {
				nfloat r = 0, g = 0, b = 0, a = 0;
				var rgba = NativeColors[(int)kc].UsingColorSpace (NSColorSpace.GenericRGBColorSpace);
				if (rgba != null) {
					rgba.GetRgba (out r, out g, out b, out a);
				} else {
					var cgc = NativeColors[(int)kc].CGColor; // 10.8+
					if (cgc != null)
					{
						// FIXME: CMYK, other color spaces?
						if (cgc.NumberOfComponents == 4) {
							a = (float)cgc.Components[3];
							r = (float)cgc.Components[0];
							g = (float)cgc.Components[1];
							b = (float)cgc.Components[2];
						} else if (cgc.NumberOfComponents == 2) {
							a = (float)cgc.Components[1];
							r = g = b = (float)cgc.Components[0];
						}
					}
				}
				return (ConvertComponent (a) << 24) + (ConvertComponent (r) << 16) + (ConvertComponent (g) << 8) + ConvertComponent (b);
			}

			return ArgbValues[(int)kc];
		}
	}
}
