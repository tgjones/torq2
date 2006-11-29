using System;
using System.Runtime.InteropServices;

namespace Torq2.Video
{
	public class Win32Avi
	{
		public const int StreamtypeVIDEO = 1935960438; //mmioStringToFOURCC("vids", 0)

		//Initialize the AVI library
		[DllImport("avifil32.dll")]
		public static extern void AVIFileInit();

		//Open an AVI file
		[DllImport("avifil32.dll", PreserveSig = true)]
		public static extern int AVIFileOpen(
			ref int ppfile,
			String szFile,
			int uMode,
			int pclsidHandler);

		//Create a new stream in an open AVI file
		[DllImport("avifil32.dll")]
		public static extern int AVIFileCreateStream(
				int pfile,
				out IntPtr ppavi,
				ref AVISTREAMINFO ptr_streaminfo);

		//Set the format for a new stream
		[DllImport("avifil32.dll")]
		public static extern int AVIStreamSetFormat(
				IntPtr aviStream, Int32 lPos,
				ref BITMAPINFOHEADER lpFormat, Int32 cbFormat);

		//Write a sample to a stream
		[DllImport("avifil32.dll")]
		public static extern int AVIStreamWrite(
				IntPtr aviStream, Int32 lStart, Int32 lSamples,
				IntPtr lpBuffer, Int32 cbBuffer, Int32 dwFlags,
				Int32 dummy1, Int32 dummy2);

		//Release an open AVI stream
		[DllImport("avifil32.dll")]
		public static extern int AVIStreamRelease(IntPtr aviStream);

		//Release an open AVI file
		[DllImport("avifil32.dll")]
		public static extern int AVIFileRelease(int pfile);

		//Close the AVI library
		[DllImport("avifil32.dll")]
		public static extern void AVIFileExit();

		#region Structs

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct RECT
		{
			public UInt32 left;
			public UInt32 top;
			public UInt32 right;
			public UInt32 bottom;
		} 

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct AVISTREAMINFO
		{
			public UInt32 fccType;
			public UInt32 fccHandler;
			public UInt32 dwFlags;
			public UInt32 dwCaps;
			public UInt16 wPriority;
			public UInt16 wLanguage;
			public UInt32 dwScale;
			public UInt32 dwRate;
			public UInt32 dwStart;
			public UInt32 dwLength;
			public UInt32 dwInitialFrames;
			public UInt32 dwSuggestedBufferSize;
			public UInt32 dwQuality;
			public UInt32 dwSampleSize;
			public RECT rcFrame;
			public UInt32 dwEditCount;
			public UInt32 dwFormatChangeCount;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
			public UInt16[] szName;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BITMAPINFOHEADER
		{
			public UInt32 biSize;
			public Int32 biWidth;
			public Int32 biHeight;
			public Int16 biPlanes;
			public Int16 biBitCount;
			public UInt32 biCompression;
			public UInt32 biSizeImage;
			public Int32 biXPelsPerMeter;
			public Int32 biYPelsPerMeter;
			public UInt32 biClrUsed;
			public UInt32 biClrImportant;
		}

		#endregion
	}
}
