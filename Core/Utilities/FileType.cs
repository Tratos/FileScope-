// FileType.cs
// Copyright (C) 2002 Matt Zyzik (www.FileScope.com)
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;

namespace FileScope
{
	/// <summary>
	/// Class designated for determining what type of file a given extension represents.
	/// </summary>
	public class FileType
	{
		static Hashtable extensionsMap = new Hashtable();

		public static void FillExt()
		{
			extensionsMap.Add("323", 	"text/h323");
			extensionsMap.Add("acx", 	"application/internet-property-stream");
			extensionsMap.Add("ai", 	"application/postscript");
			extensionsMap.Add("aif", 	"audio/x-aiff");
			extensionsMap.Add("aifc", 	"audio/aiff");
			extensionsMap.Add("aiff", 	"audio/aiff");
			extensionsMap.Add("aip", 	"text/x-audiosoft-intra");
			extensionsMap.Add("arj", 	"application/x-arj");
			extensionsMap.Add("asf", 	"video/x-ms-asf");
			extensionsMap.Add("asr", 	"video/x-ms-asf");
			extensionsMap.Add("asx", 	"video/x-ms-asf");
			extensionsMap.Add("au", 	"audio/basic");
			extensionsMap.Add("avi", 	"video/x-msvideo");
			extensionsMap.Add("axs", 	"application/olescript");
			extensionsMap.Add("bas", 	"text/plain");
			extensionsMap.Add("bcpio",	"application/x-bcpio");
			extensionsMap.Add("bin", 	"application/octet-stream");
			extensionsMap.Add("bmp", 	"image/bmp");
			extensionsMap.Add("c",  	"text/plain");
			extensionsMap.Add("c++", 	"text/plain");
			extensionsMap.Add("cat", 	"application/vndms-pkiseccat");
			extensionsMap.Add("cc", 	"text/plain");
			extensionsMap.Add("cer", 	"application/x-x509-ca-cert");
			extensionsMap.Add("class",  "application/x-java-class");
			extensionsMap.Add("clp", 	"application/x-msclip");
			extensionsMap.Add("cmx", 	"image/x-cmx");
			extensionsMap.Add("cod", 	"image/cid-cod");
			extensionsMap.Add("cpio", 	"application/x-cpio");
			extensionsMap.Add("cpp", 	"text/plain");
			extensionsMap.Add("crd", 	"application/x-mscardfile");
			extensionsMap.Add("crl", 	"application/pkix-crl");
			extensionsMap.Add("crt", 	"application/x-x509-ca-cert");
			extensionsMap.Add("csh", 	"application/x-csh");
			extensionsMap.Add("css", 	"text/css");
			extensionsMap.Add("csv", 	"text/comma-seperated-values");
			extensionsMap.Add("dcr", 	"application/x-director");
			extensionsMap.Add("def", 	"text/plain");
			extensionsMap.Add("der", 	"application/x-x509-ca-cert");
			extensionsMap.Add("dib", 	"image/bmp");
			extensionsMap.Add("dir", 	"application/x-director");
			extensionsMap.Add("disco",	"text/xml");
			extensionsMap.Add("dll", 	"application/x-msdownload");
			extensionsMap.Add("dlm", 	"text/dlm");
			extensionsMap.Add("dms", 	"application/octet-stream");
			extensionsMap.Add("doc", 	"application/msword");
			extensionsMap.Add("dot", 	"application/msword");
			extensionsMap.Add("dvi", 	"application/x-dvi");
			extensionsMap.Add("dxr", 	"application/x-director");
			extensionsMap.Add("eml", 	"message/rfc822");
			extensionsMap.Add("eps", 	"application/postscript");
			extensionsMap.Add("etd", 	"application/x-ebx");
			extensionsMap.Add("etx", 	"text/x-setext");
			extensionsMap.Add("evy", 	"application/envoy");
			extensionsMap.Add("exe", 	"application/octet-stream");
			extensionsMap.Add("fdf", 	"application/vnd.fdf");
			extensionsMap.Add("fif", 	"application/fractals");
			extensionsMap.Add("flr", 	"x-world/x-vrml");
			extensionsMap.Add("gif", 	"image/gif");
			extensionsMap.Add("gtar", 	"application/x-gtar");
			extensionsMap.Add("gz", 	"application/x-gzip");
			extensionsMap.Add("h", 	    "text/plain");
			extensionsMap.Add("hdf", 	"application/x-hdf");
			extensionsMap.Add("hlp", 	"application/winhelp");
			extensionsMap.Add("hqx", 	"application/mac-binhex40");
			extensionsMap.Add("hta", 	"application/hta");
			extensionsMap.Add("htc", 	"text/x-component");
			extensionsMap.Add("htm", 	"text/html");
			extensionsMap.Add("html",	"text/html");
			extensionsMap.Add("htt",	"text/webviewhtml");
			extensionsMap.Add("ico",	"image/x-icon");
			extensionsMap.Add("ief",	"image/ief");
			extensionsMap.Add("iii",	"application/x-iphone");
			extensionsMap.Add("ini",	"text/plain");
			extensionsMap.Add("ins",	"application/x-internet-signup");
			extensionsMap.Add("isp",	"application/x-internet-signup");
			extensionsMap.Add("ivf",	"video/x-ivf");
			extensionsMap.Add("java",	"text/x-java-source");
			extensionsMap.Add("jfif",	"image/pipeg");
			extensionsMap.Add("jpe",	"image/jpeg");
			extensionsMap.Add("jpeg",	"image/jpeg");
			extensionsMap.Add("jpg",	"image/jpeg");
			extensionsMap.Add("js",	    "application/x-javascript");
			extensionsMap.Add("latex",	"application/x-latex");
			extensionsMap.Add("lha",	"application/octet-stream");
			extensionsMap.Add("lnk",	"application/x-ms-shortcut");
			extensionsMap.Add("ls",	    "application/x-javascript");
			extensionsMap.Add("lsf",	"video/x-la-asf");
			extensionsMap.Add("lsx",	"video/x-la-asf");
			extensionsMap.Add("lwp",	"application/vnd.lotus-wordpro");
			extensionsMap.Add("lzh",	"application/x-lzh");
			extensionsMap.Add("m13",	"application/x-msmediaview");
			extensionsMap.Add("m14",	"application/x-msmediaview");
			extensionsMap.Add("m3u",	"application/x-mpegurl");
			extensionsMap.Add("mal",	"application/x-mal");
			extensionsMap.Add("man",	"application/x-troff-man");
			extensionsMap.Add("mdb",	"application/x-msaccess");
			extensionsMap.Add("me",	    "application/x-troff-me");
			extensionsMap.Add("mht",	"message/rfc822");
			extensionsMap.Add("mhtml",	"message/rfc822");
			extensionsMap.Add("mid",	"audio/mid");
			extensionsMap.Add("mime",	"message/rfc822");
			extensionsMap.Add("mmz",	"application/x-mmjb-mmz");
			extensionsMap.Add("mny",	"application/x-msmoney");
			extensionsMap.Add("mocha",	"application/x-javascript");
			extensionsMap.Add("mov",	"video/quicktime");
			extensionsMap.Add("movie",	"video/x-sgi-movie");
			extensionsMap.Add("mp2",	"video/mpeg");
			extensionsMap.Add("mp3",	"audio/mpeg");
			extensionsMap.Add("mpa",	"video/mpeg");
			extensionsMap.Add("mpe",	"video/mpeg");
			extensionsMap.Add("mpeg",	"video/mpeg");
			extensionsMap.Add("mpg",	"video/mpeg");
			extensionsMap.Add("mpp",	"application/vnd.ms-project");
			extensionsMap.Add("mpv2",	"video/mpeg");
			extensionsMap.Add("ms",	    "application/x-troff-ms");
			extensionsMap.Add("mvb",	"application/x-msmdeiaview");
			extensionsMap.Add("nc",	    "application/x-netcdf");
			extensionsMap.Add("nfo",    "text/plain");
			extensionsMap.Add("nix",	"application/x-mix-transfer");
			extensionsMap.Add("nws",	"message/rfc822");
			extensionsMap.Add("oda",	"application/oda");
			extensionsMap.Add("p10",	"application/pkcs10");
			extensionsMap.Add("p12",	"application/x-pkcs12");
			extensionsMap.Add("p7b",	"application/x-pkcs7-certificates");
			extensionsMap.Add("p7c",	"application/x-pkcs7-mime");
			extensionsMap.Add("p7m",	"application/x-pkcs7-mime");
			extensionsMap.Add("p7r",	"application/x-pkcs7-certreqresp");
			extensionsMap.Add("p7s",	"application/x-pkcs7-signature");
			extensionsMap.Add("pbm",	"image/x-portable-bitmap");
			extensionsMap.Add("pdf",	"application/pdf");
			extensionsMap.Add("pfx",	"application/x-pkcs12");
			extensionsMap.Add("pgm",	"image/x-portable-graymap");
			extensionsMap.Add("pko",	"application/ynd.ms-pkipko");
			extensionsMap.Add("pl",	    "text/plain");
			extensionsMap.Add("pls",	"audio/x-scpls");
			extensionsMap.Add("pma",	"application/x-perfmon");
			extensionsMap.Add("pmc",	"application/x-perfmon");
			extensionsMap.Add("pml",	"application/x-perfmon");
			extensionsMap.Add("pmr",	"application/x-perfmon");
			extensionsMap.Add("pmw",	"application/x-perfmon");
			extensionsMap.Add("png",	"image/png");
			extensionsMap.Add("pnm",	"image/x-portable-anymap");
			extensionsMap.Add("pot",	"application/vnd.ms-powerpoint");
			extensionsMap.Add("ppt",	"application/vnd.ms-powerpoint");
			extensionsMap.Add("ppz",	"application/vnd.ms-powerpoint");
			extensionsMap.Add("prf",	"application/pics-rules");
			extensionsMap.Add("ps",		"application/postscript");
			extensionsMap.Add("pub",	"application/x-mspublisher");
			extensionsMap.Add("qt",		"video/quicktime");
			extensionsMap.Add("ra",		"audio/x-pn-realaudio");
			extensionsMap.Add("ram",	"audio/x-pn-realaudio");
			extensionsMap.Add("ras",	"image/x-cmu-raster");
			extensionsMap.Add("rc",		"text/plain");
			extensionsMap.Add("rf",		"image/vnd.rn-realflash");
			extensionsMap.Add("rgb",	"image/x-rgb");
			extensionsMap.Add("rjs",	"application/vnd.rn-realsystem-rjs");
			extensionsMap.Add("rm",		"application/vnd.rn-realmedia");
			extensionsMap.Add("rmi",	"audio/mid");
			extensionsMap.Add("rmp",	"application/vnd.rn-rn_music_package");
			extensionsMap.Add("rmx",	"application/vnd.rn-realsystem-rmx");
			extensionsMap.Add("rnx",	"application/vnd.rn-realplayer");
			extensionsMap.Add("roff",	"application/x-troff");
			extensionsMap.Add("rp",		"image/vnd.rn-realpix");
			extensionsMap.Add("rpm",	"audio/x-pn-realaudio");
			extensionsMap.Add("rsml",	"application/vnd.rn-rsml");
			extensionsMap.Add("rt",		"text/vnd.rn-realtext");
			extensionsMap.Add("rtf",	"application/rtf");
			extensionsMap.Add("rtx",	"text/richtext");
			extensionsMap.Add("rv",		"video/vnd.rn-realvideo");
			extensionsMap.Add("scd",	"application/x-msschedule");
			extensionsMap.Add("sct",	"text/scriptlet");
			extensionsMap.Add("sdp",	"application/sdp");
			extensionsMap.Add("sh",		"application/x-sh");
			extensionsMap.Add("shar",	"application/x-shar");
			extensionsMap.Add("sit",	"application/x-stuffit");
			extensionsMap.Add("smil",	"application/smil");
			extensionsMap.Add("snd",	"audio/basic");
			extensionsMap.Add("spc",	"application/x-pkcs7-certificates");
			extensionsMap.Add("spl",	"application/futuresplash");
			extensionsMap.Add("spt",	"application/x-spt");
			extensionsMap.Add("src",	"application/x-wais-source");
			extensionsMap.Add("ssm",	"application/streamingmedia");
			extensionsMap.Add("sst",	"application/vnd.ms-pkicertstore");
			extensionsMap.Add("stl",	"application/vnd.ms-pkistl");
			extensionsMap.Add("stm",	"text/html");
			extensionsMap.Add("sub",	"application/x-avantgo-subscription");
			extensionsMap.Add("subs",	"application/x-avantgo-channel");
			extensionsMap.Add("swf",	"application/x-shockwave-flash");
			extensionsMap.Add("t",		"application/x-troff");
			extensionsMap.Add("tar",	"application/x-tar");
			extensionsMap.Add("tbk",	"application/toolbook");
			extensionsMap.Add("tcl",	"application/x-tcl");
			extensionsMap.Add("tex",	"application/x-tex");
			extensionsMap.Add("texi",	"application/x-texinfo");
			extensionsMap.Add("texinfo","application/x-texinfo");
			extensionsMap.Add("text",	"text/plain");
			extensionsMap.Add("tgz",	"application/x-compressed");
			extensionsMap.Add("tif",	"image/tiff");
			extensionsMap.Add("tiff",	"image/tiff");
			extensionsMap.Add("tr",		"application/x-troff");
			extensionsMap.Add("trm",	"application/x-msterminal");
			extensionsMap.Add("tsv",	"text/tab-seperated-values");
			extensionsMap.Add("txt",	"text/plain");
			extensionsMap.Add("uls",	"text/iuls");
			extensionsMap.Add("ustar",	"application/x-ustar");
			extensionsMap.Add("uu",		"application/octet-stream");
			extensionsMap.Add("vcf",	"text/x-vcard");
			extensionsMap.Add("vox",	"audio/voxware");
			extensionsMap.Add("vrml",	"x-world/x-vrml");
			extensionsMap.Add("vsd",	"application/vnd.visio");
			extensionsMap.Add("wav",	"audio/x-wav");
			extensionsMap.Add("wax",	"audio/x-ms-wax");
			extensionsMap.Add("wcm",	"application/vnd.ms-works");
			extensionsMap.Add("wdb",	"application/vnd.ms-works");
			extensionsMap.Add("wks",	"application/vnd.ms-works");
			extensionsMap.Add("wm",		"video/x-ms-wm");
			extensionsMap.Add("wma",	"video/x-ms-wma");
			extensionsMap.Add("wmd",	"application/x-ms-wmd");
			extensionsMap.Add("wmf",	"application/x-msmetafile");
			extensionsMap.Add("wmp",	"video/x-ms-wmp");
			extensionsMap.Add("wms",	"application/x-ms-wms");
			extensionsMap.Add("wmv",	"video/x-ms-wmv");
			extensionsMap.Add("wmx",	"video/x-ms-wmx");
			extensionsMap.Add("wmz",	"application/x-ms-wmz");
			extensionsMap.Add("wps",	"application/vnd.ms-works");
			extensionsMap.Add("wri",	"application/x-mswrite");
			extensionsMap.Add("wrl",	"x-world/x-vrml");
			extensionsMap.Add("wrz",	"x-world/x-vrml");
			extensionsMap.Add("wsdl",	"text/xml");
			extensionsMap.Add("wsrc",	"application/x-wais-source");
			extensionsMap.Add("wvx",	"video/x-ms-wvx");
			extensionsMap.Add("xaf",	"x-world/x-vrml");
			extensionsMap.Add("xbm",	"image/x-bitmap");
			extensionsMap.Add("xla",	"application/vnd.ms-excel");
			extensionsMap.Add("xlc",	"application/vnd.ms-excel");
			extensionsMap.Add("xlm",	"application/vnd.ms-excel");
			extensionsMap.Add("xls",	"application/vnd.ms-excel");
			extensionsMap.Add("xlt",	"application/vnd.ms-excel");
			extensionsMap.Add("xlw",	"application/vnd.ms-excel");
			extensionsMap.Add("xml",	"text/xml");
			extensionsMap.Add("xof",	"x-world/x-vrml");
			extensionsMap.Add("xpm",	"image/x-xpixmap");
			extensionsMap.Add("xsd",	"text/xml");
			extensionsMap.Add("xsl",	"text/xml");
			extensionsMap.Add("xwd",	"image/x-xwindowdump");
			extensionsMap.Add("ymg",	"application/ymsgr");
			extensionsMap.Add("z",		"application/x-compress");
			extensionsMap.Add("zip", 	"application/zip");
		}

		static string[] video = new string[]
		{
			"avi", "mpg", "mpeg", "mpe", "mng",
			"rm", "ram", "asf", "qt", "mov",
			"swf", "dcr", "jve"
		};

		static string[] audio = new string[]
		{
			"mp3", "mpa", "wma", "ogg", "ra",
			"rmj", "lqt", "wav", "au", "snd",
			"aif", "aiff", "aifc"
		};

		static string[] images = new string[]
		{
			"gif", "jpg", "jpeg", "jpe", "png",
			"tif", "tiff", "bmp", "ico", "pcx",
			"pic", "pbm", "pnm", "ppm", "xwd", "psd"
		};

		static string[] documents = new string[]
		{
			"html", "htm", "xml", "xhtml", "txt",
			"pdf", "ps", "eps", "rtf", "doc", "nfo",
			"tex", "texi", "latex", "man", "url"
		};

		static string[] archives = new string[]
		{
			"zip", "arj", "rar", "lha", "cab",
			"z", "gz", "gzip", "tar", "tgz",
			"taz", "shar"

		};

		static string[] programs = new string[]
		{
			"exe", "bin", "sh", "csh",
			"rpm", "deb", "msi", "msp",
			"hqx", "sit", "dmg", "vbs",
			"bat"
		};

		/// <summary>
		/// Return file type for a given extension.
		/// Types: unknown, video, audio, images, document, archive, program.
		/// </summary>
		public static string GetType(string ext)
		{
			ext = ext.ToLower();
			foreach(string str in audio)
				if(str == ext)
					return "Audio";
			foreach(string str in video)
				if(str == ext)
					return "Video";
			foreach(string str in images)
				if(str == ext)
					return "Image";
			foreach(string str in documents)
				if(str == ext)
					return "Document";
			foreach(string str in archives)
				if(str == ext)
					return "Archive";
			foreach(string str in programs)
				if(str == ext)
					return "Program";
			return "Unknown";
		}

		/// <summary>
		/// Return the content type for a given extension.
		/// i.e. exe = application/binary
		/// </summary>
		public static string GetContentType(string fileName)
		{
			string ext = System.IO.Path.GetExtension(fileName);
			if(ext.Length <= 1)
				return "unknown";
			ext = ext.Substring(1);

			if(extensionsMap.ContainsKey(ext))
				return (string)extensionsMap[ext];
			else
				return "unknown";
		}
	}
}
