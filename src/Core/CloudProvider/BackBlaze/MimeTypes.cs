﻿using System.Diagnostics;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {
    internal static class MimeType {

        private static readonly ActivitySource s_source = new( "MimeType" );

        internal static string GetMimeTypeByExtension( FileInfo path ) {
            using Activity? activity = s_source.StartActivity( "GetMimeTypeByExtension" );
            activity?.Start( );

            Dictionary<string, string>? mimetypes = GetMimeTypes( );

            string? mimeTypeFromFileExtension = mimetypes
                .Where( x => x.Key == path.Extension.ToLower( ) )
                .FirstOrDefault( )
                .Value;

            activity?.Stop( );
            return mimeTypeFromFileExtension ?? "application/octet-stream";
        }

        private static Dictionary<string, string> GetMimeTypes( ) {
            using Activity? activity = s_source.StartActivity( "GetMimeTypes" )?.Start( );

            Dictionary<string, string> mimeTypes = new( );
            // Ref: https://github.com/microsoft/referencesource/blob/5697c29004a34d80acdaf5742d7e699022c64ecd/System.Web/MimeMapping.cs#L96
            mimeTypes.Add( ".7z", "application/x-7z-compressed" );
            mimeTypes.Add( ".323", "text/h323" );
            mimeTypes.Add( ".aaf", "application/octet-stream" );
            mimeTypes.Add( ".aca", "application/octet-stream" );
            mimeTypes.Add( ".accdb", "application/msaccess" );
            mimeTypes.Add( ".accde", "application/msaccess" );
            mimeTypes.Add( ".accdt", "application/msaccess" );
            mimeTypes.Add( ".acx", "application/internet-property-stream" );
            mimeTypes.Add( ".afm", "application/octet-stream" );
            mimeTypes.Add( ".ai", "application/postscript" );
            mimeTypes.Add( ".aif", "audio/x-aiff" );
            mimeTypes.Add( ".aifc", "audio/aiff" );
            mimeTypes.Add( ".aiff", "audio/aiff" );
            mimeTypes.Add( ".application", "application/x-ms-application" );
            mimeTypes.Add( ".art", "image/x-jg" );
            mimeTypes.Add( ".asd", "application/octet-stream" );
            mimeTypes.Add( ".asf", "video/x-ms-asf" );
            mimeTypes.Add( ".asi", "application/octet-stream" );
            mimeTypes.Add( ".asm", "text/plain" );
            mimeTypes.Add( ".asr", "video/x-ms-asf" );
            mimeTypes.Add( ".asx", "video/x-ms-asf" );
            mimeTypes.Add( ".atom", "application/atom+xml" );
            mimeTypes.Add( ".au", "audio/basic" );
            mimeTypes.Add( ".avi", "video/x-msvideo" );
            mimeTypes.Add( ".axs", "application/olescript" );
            mimeTypes.Add( ".bas", "text/plain" );
            mimeTypes.Add( ".bcpio", "application/x-bcpio" );
            mimeTypes.Add( ".bin", "application/octet-stream" );
            mimeTypes.Add( ".bmp", "image/bmp" );
            mimeTypes.Add( ".c", "text/plain" );
            mimeTypes.Add( ".cab", "application/octet-stream" );
            mimeTypes.Add( ".calx", "application/vnd.ms-office.calx" );
            mimeTypes.Add( ".cat", "application/vnd.ms-pki.seccat" );
            mimeTypes.Add( ".cdf", "application/x-cdf" );
            mimeTypes.Add( ".chm", "application/octet-stream" );
            mimeTypes.Add( ".class", "application/x-java-applet" );
            mimeTypes.Add( ".clp", "application/x-msclip" );
            mimeTypes.Add( ".cmx", "image/x-cmx" );
            mimeTypes.Add( ".cnf", "text/plain" );
            mimeTypes.Add( ".cod", "image/cis-cod" );
            mimeTypes.Add( ".cpio", "application/x-cpio" );
            mimeTypes.Add( ".cpp", "text/plain" );
            mimeTypes.Add( ".crd", "application/x-mscardfile" );
            mimeTypes.Add( ".crl", "application/pkix-crl" );
            mimeTypes.Add( ".crt", "application/x-x509-ca-cert" );
            mimeTypes.Add( ".csh", "application/x-csh" );
            mimeTypes.Add( ".css", "text/css" );
            mimeTypes.Add( ".csv", "application/octet-stream" );
            mimeTypes.Add( ".cur", "application/octet-stream" );
            mimeTypes.Add( ".dcr", "application/x-director" );
            mimeTypes.Add( ".deploy", "application/octet-stream" );
            mimeTypes.Add( ".der", "application/x-x509-ca-cert" );
            mimeTypes.Add( ".dib", "image/bmp" );
            mimeTypes.Add( ".dir", "application/x-director" );
            mimeTypes.Add( ".disco", "text/xml" );
            mimeTypes.Add( ".dll", "application/x-msdownload" );
            mimeTypes.Add( ".dll.,          nfig", "text/xml" );
            mimeTypes.Add( ".dlm", "text/dlm" );
            mimeTypes.Add( ".doc", "application/msword" );
            mimeTypes.Add( ".docm", "application/vnd.ms-word.document.macroEnabled.12" );
            mimeTypes.Add( ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" );
            mimeTypes.Add( ".dot", "application/msword" );
            mimeTypes.Add( ".dotm", "application/vnd.ms-word.template.macroEnabled.12" );
            mimeTypes.Add( ".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" );
            mimeTypes.Add( ".dsp", "application/octet-stream" );
            mimeTypes.Add( ".dtd", "text/xml" );
            mimeTypes.Add( ".dvi", "application/x-dvi" );
            mimeTypes.Add( ".dwf", "drawing/x-dwf" );
            mimeTypes.Add( ".dwp", "application/octet-stream" );
            mimeTypes.Add( ".dxr", "application/x-director" );
            mimeTypes.Add( ".eml", "message/rfc822" );
            mimeTypes.Add( ".emz", "application/octet-stream" );
            mimeTypes.Add( ".eot", "application/octet-stream" );
            mimeTypes.Add( ".eps", "application/postscript" );
            mimeTypes.Add( ".etx", "text/x-setext" );
            mimeTypes.Add( ".evy", "application/envoy" );
            mimeTypes.Add( ".exe", "application/octet-stream" );
            mimeTypes.Add( ".exe.,          nfig", "text/xml" );
            mimeTypes.Add( ".fdf", "application/vnd.fdf" );
            mimeTypes.Add( ".fif", "application/fractals" );
            mimeTypes.Add( ".fla", "application/octet-stream" );
            mimeTypes.Add( ".flr", "x-world/x-vrml" );
            mimeTypes.Add( ".flv", "video/x-flv" );
            mimeTypes.Add( ".gif", "image/gif" );
            mimeTypes.Add( ".gtar", "application/x-gtar" );
            mimeTypes.Add( ".gz", "application/x-gzip" );
            mimeTypes.Add( ".h", "text/plain" );
            mimeTypes.Add( ".hdf", "application/x-hdf" );
            mimeTypes.Add( ".hdml", "text/x-hdml" );
            mimeTypes.Add( ".hhc", "application/x-oleobject" );
            mimeTypes.Add( ".hhk", "application/octet-stream" );
            mimeTypes.Add( ".hhp", "application/octet-stream" );
            mimeTypes.Add( ".hlp", "application/winhlp" );
            mimeTypes.Add( ".hqx", "application/mac-binhex40" );
            mimeTypes.Add( ".hta", "application/hta" );
            mimeTypes.Add( ".htc", "text/x-component" );
            mimeTypes.Add( ".htm", "text/html" );
            mimeTypes.Add( ".html", "text/html" );
            mimeTypes.Add( ".htt", "text/webviewhtml" );
            mimeTypes.Add( ".hxt", "text/html" );
            mimeTypes.Add( ".ico", "image/x-icon" );
            mimeTypes.Add( ".ics", "application/octet-stream" );
            mimeTypes.Add( ".ief", "image/ief" );
            mimeTypes.Add( ".iii", "application/x-iphone" );
            mimeTypes.Add( ".inf", "application/octet-stream" );
            mimeTypes.Add( ".ins", "application/x-internet-signup" );
            mimeTypes.Add( ".isp", "application/x-internet-signup" );
            mimeTypes.Add( ".IVF", "video/x-ivf" );
            mimeTypes.Add( ".jar", "application/java-archive" );
            mimeTypes.Add( ".java", "application/octet-stream" );
            mimeTypes.Add( ".jck", "application/liquidmotion" );
            mimeTypes.Add( ".jcz", "application/liquidmotion" );
            mimeTypes.Add( ".jfif", "image/pjpeg" );
            mimeTypes.Add( ".jpb", "application/octet-stream" );
            mimeTypes.Add( ".jpe", "image/jpeg" );
            mimeTypes.Add( ".jpeg", "image/jpeg" );
            mimeTypes.Add( ".jpg", "image/jpeg" );
            mimeTypes.Add( ".js", "application/x-javascript" );
            mimeTypes.Add( ".jsx", "text/jscript" );
            mimeTypes.Add( ".latex", "application/x-latex" );
            mimeTypes.Add( ".lit", "application/x-ms-reader" );
            mimeTypes.Add( ".lpk", "application/octet-stream" );
            mimeTypes.Add( ".lsf", "video/x-la-asf" );
            mimeTypes.Add( ".lsx", "video/x-la-asf" );
            mimeTypes.Add( ".lzh", "application/octet-stream" );
            mimeTypes.Add( ".m13", "application/x-msmediaview" );
            mimeTypes.Add( ".m14", "application/x-msmediaview" );
            mimeTypes.Add( ".m1v", "video/mpeg" );
            mimeTypes.Add( ".m3u", "audio/x-mpegurl" );
            mimeTypes.Add( ".man", "application/x-troff-man" );
            mimeTypes.Add( ".manifest", "application/x-ms-manifest" );
            mimeTypes.Add( ".map", "text/plain" );
            mimeTypes.Add( ".mdb", "application/x-msaccess" );
            mimeTypes.Add( ".mdp", "application/octet-stream" );
            mimeTypes.Add( ".me", "application/x-troff-me" );
            mimeTypes.Add( ".mht", "message/rfc822" );
            mimeTypes.Add( ".mhtml", "message/rfc822" );
            mimeTypes.Add( ".mid", "audio/mid" );
            mimeTypes.Add( ".midi", "audio/mid" );
            mimeTypes.Add( ".mix", "application/octet-stream" );
            mimeTypes.Add( ".mmf", "application/x-smaf" );
            mimeTypes.Add( ".mno", "text/xml" );
            mimeTypes.Add( ".mny", "application/x-msmoney" );
            mimeTypes.Add( ".mov", "video/quicktime" );
            mimeTypes.Add( ".movie", "video/x-sgi-movie" );
            mimeTypes.Add( ".mp2", "video/mpeg" );
            mimeTypes.Add( ".mp3", "audio/mpeg" );
            mimeTypes.Add( ".mpa", "video/mpeg" );
            mimeTypes.Add( ".mpe", "video/mpeg" );
            mimeTypes.Add( ".mpeg", "video/mpeg" );
            mimeTypes.Add( ".mpg", "video/mpeg" );
            mimeTypes.Add( ".mpp", "application/vnd.ms-project" );
            mimeTypes.Add( ".mpv2", "video/mpeg" );
            mimeTypes.Add( ".ms", "application/x-troff-ms" );
            mimeTypes.Add( ".msi", "application/octet-stream" );
            mimeTypes.Add( ".mso", "application/octet-stream" );
            mimeTypes.Add( ".mvb", "application/x-msmediaview" );
            mimeTypes.Add( ".mvc", "application/x-miva-compiled" );
            mimeTypes.Add( ".nc", "application/x-netcdf" );
            mimeTypes.Add( ".nsc", "video/x-ms-asf" );
            mimeTypes.Add( ".nws", "message/rfc822" );
            mimeTypes.Add( ".ocx", "application/octet-stream" );
            mimeTypes.Add( ".oda", "application/oda" );
            mimeTypes.Add( ".odc", "text/x-ms-odc" );
            mimeTypes.Add( ".ods", "application/oleobject" );
            mimeTypes.Add( ".one", "application/onenote" );
            mimeTypes.Add( ".onea", "application/onenote" );
            mimeTypes.Add( ".onetoc", "application/onenote" );
            mimeTypes.Add( ".onetoc2", "application/onenote" );
            mimeTypes.Add( ".onetmp", "application/onenote" );
            mimeTypes.Add( ".onepkg", "application/onenote" );
            mimeTypes.Add( ".osdx", "application/opensearchdescription+xml" );
            mimeTypes.Add( ".p10", "application/pkcs10" );
            mimeTypes.Add( ".p12", "application/x-pkcs12" );
            mimeTypes.Add( ".p7b", "application/x-pkcs7-certificates" );
            mimeTypes.Add( ".p7c", "application/pkcs7-mime" );
            mimeTypes.Add( ".p7m", "application/pkcs7-mime" );
            mimeTypes.Add( ".p7r", "application/x-pkcs7-certreqresp" );
            mimeTypes.Add( ".p7s", "application/pkcs7-signature" );
            mimeTypes.Add( ".pbm", "image/x-portable-bitmap" );
            mimeTypes.Add( ".pcx", "application/octet-stream" );
            mimeTypes.Add( ".pcz", "application/octet-stream" );
            mimeTypes.Add( ".pdf", "application/pdf" );
            mimeTypes.Add( ".pfb", "application/octet-stream" );
            mimeTypes.Add( ".pfm", "application/octet-stream" );
            mimeTypes.Add( ".pfx", "application/x-pkcs12" );
            mimeTypes.Add( ".pgm", "image/x-portable-graymap" );
            mimeTypes.Add( ".pko", "application/vnd.ms-pki.pko" );
            mimeTypes.Add( ".pma", "application/x-perfmon" );
            mimeTypes.Add( ".pmc", "application/x-perfmon" );
            mimeTypes.Add( ".pml", "application/x-perfmon" );
            mimeTypes.Add( ".pmr", "application/x-perfmon" );
            mimeTypes.Add( ".pmw", "application/x-perfmon" );
            mimeTypes.Add( ".png", "image/png" );
            mimeTypes.Add( ".pnm", "image/x-portable-anymap" );
            mimeTypes.Add( ".pnz", "image/png" );
            mimeTypes.Add( ".pot", "application/vnd.ms-powerpoint" );
            mimeTypes.Add( ".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12" );
            mimeTypes.Add( ".potx", "application/vnd.openxmlformats-officedocument.presentationml.template" );
            mimeTypes.Add( ".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12" );
            mimeTypes.Add( ".ppm", "image/x-portable-pixmap" );
            mimeTypes.Add( ".pps", "application/vnd.ms-powerpoint" );
            mimeTypes.Add( ".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12" );
            mimeTypes.Add( ".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" );
            mimeTypes.Add( ".ppt", "application/vnd.ms-powerpoint" );
            mimeTypes.Add( ".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" );
            mimeTypes.Add( ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" );
            mimeTypes.Add( ".prf", "application/pics-rules" );
            mimeTypes.Add( ".prm", "application/octet-stream" );
            mimeTypes.Add( ".prx", "application/octet-stream" );
            mimeTypes.Add( ".ps", "application/postscript" );
            mimeTypes.Add( ".psd", "application/octet-stream" );
            mimeTypes.Add( ".psm", "application/octet-stream" );
            mimeTypes.Add( ".psp", "application/octet-stream" );
            mimeTypes.Add( ".pub", "application/x-mspublisher" );
            mimeTypes.Add( ".qt", "video/quicktime" );
            mimeTypes.Add( ".qtl", "application/x-quicktimeplayer" );
            mimeTypes.Add( ".qxd", "application/octet-stream" );
            mimeTypes.Add( ".ra", "audio/x-pn-realaudio" );
            mimeTypes.Add( ".ram", "audio/x-pn-realaudio" );
            mimeTypes.Add( ".rar", "application/octet-stream" );
            mimeTypes.Add( ".ras", "image/x-cmu-raster" );
            mimeTypes.Add( ".rf", "image/vnd.rn-realflash" );
            mimeTypes.Add( ".rgb", "image/x-rgb" );
            mimeTypes.Add( ".rm", "application/vnd.rn-realmedia" );
            mimeTypes.Add( ".rmi", "audio/mid" );
            mimeTypes.Add( ".roff", "application/x-troff" );
            mimeTypes.Add( ".rpm", "audio/x-pn-realaudio-plugin" );
            mimeTypes.Add( ".rtf", "application/rtf" );
            mimeTypes.Add( ".rtx", "text/richtext" );
            mimeTypes.Add( ".scd", "application/x-msschedule" );
            mimeTypes.Add( ".sct", "text/scriptlet" );
            mimeTypes.Add( ".sea", "application/octet-stream" );
            mimeTypes.Add( ".setpay", "application/set-payment-initiation" );
            mimeTypes.Add( ".setreg", "application/set-registration-initiation" );
            mimeTypes.Add( ".sgml", "text/sgml" );
            mimeTypes.Add( ".sh", "application/x-sh" );
            mimeTypes.Add( ".shar", "application/x-shar" );
            mimeTypes.Add( ".sit", "application/x-stuffit" );
            mimeTypes.Add( ".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12" );
            mimeTypes.Add( ".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide" );
            mimeTypes.Add( ".smd", "audio/x-smd" );
            mimeTypes.Add( ".smi", "application/octet-stream" );
            mimeTypes.Add( ".smx", "audio/x-smd" );
            mimeTypes.Add( ".smz", "audio/x-smd" );
            mimeTypes.Add( ".snd", "audio/basic" );
            mimeTypes.Add( ".snp", "application/octet-stream" );
            mimeTypes.Add( ".spc", "application/x-pkcs7-certificates" );
            mimeTypes.Add( ".spl", "application/futuresplash" );
            mimeTypes.Add( ".src", "application/x-wais-source" );
            mimeTypes.Add( ".ssm", "application/streamingmedia" );
            mimeTypes.Add( ".sst", "application/vnd.ms-pki.certstore" );
            mimeTypes.Add( ".stl", "application/vnd.ms-pki.stl" );
            mimeTypes.Add( ".sv4cpio", "application/x-sv4cpio" );
            mimeTypes.Add( ".sv4crc", "application/x-sv4crc" );
            mimeTypes.Add( ".swf", "application/x-shockwave-flash" );
            mimeTypes.Add( ".t", "application/x-troff" );
            mimeTypes.Add( ".tar", "application/x-tar" );
            mimeTypes.Add( ".tcl", "application/x-tcl" );
            mimeTypes.Add( ".tex", "application/x-tex" );
            mimeTypes.Add( ".texi", "application/x-texinfo" );
            mimeTypes.Add( ".texinfo", "application/x-texinfo" );
            mimeTypes.Add( ".tgz", "application/x-compressed" );
            mimeTypes.Add( ".thmx", "application/vnd.ms-officetheme" );
            mimeTypes.Add( ".thn", "application/octet-stream" );
            mimeTypes.Add( ".tif", "image/tiff" );
            mimeTypes.Add( ".tiff", "image/tiff" );
            mimeTypes.Add( ".toc", "application/octet-stream" );
            mimeTypes.Add( ".tr", "application/x-troff" );
            mimeTypes.Add( ".trm", "application/x-msterminal" );
            mimeTypes.Add( ".tsv", "text/tab-separated-values" );
            mimeTypes.Add( ".ttf", "application/octet-stream" );
            mimeTypes.Add( ".txt", "text/plain" );
            mimeTypes.Add( ".u32", "application/octet-stream" );
            mimeTypes.Add( ".uls", "text/iuls" );
            mimeTypes.Add( ".ustar", "application/x-ustar" );
            mimeTypes.Add( ".vbs", "text/vbscript" );
            mimeTypes.Add( ".vcf", "text/x-vcard" );
            mimeTypes.Add( ".vcs", "text/plain" );
            mimeTypes.Add( ".vdx", "application/vnd.ms-visio.viewer" );
            mimeTypes.Add( ".vml", "text/xml" );
            mimeTypes.Add( ".vsd", "application/vnd.visio" );
            mimeTypes.Add( ".vss", "application/vnd.visio" );
            mimeTypes.Add( ".vst", "application/vnd.visio" );
            mimeTypes.Add( ".vsto", "application/x-ms-vsto" );
            mimeTypes.Add( ".vsw", "application/vnd.visio" );
            mimeTypes.Add( ".vsx", "application/vnd.visio" );
            mimeTypes.Add( ".vtx", "application/vnd.visio" );
            mimeTypes.Add( ".wav", "audio/wav" );
            mimeTypes.Add( ".wax", "audio/x-ms-wax" );
            mimeTypes.Add( ".wbmp", "image/vnd.wap.wbmp" );
            mimeTypes.Add( ".wcm", "application/vnd.ms-works" );
            mimeTypes.Add( ".wdb", "application/vnd.ms-works" );
            mimeTypes.Add( ".wks", "application/vnd.ms-works" );
            mimeTypes.Add( ".wm", "video/x-ms-wm" );
            mimeTypes.Add( ".wma", "audio/x-ms-wma" );
            mimeTypes.Add( ".wmd", "application/x-ms-wmd" );
            mimeTypes.Add( ".wmf", "application/x-msmetafile" );
            mimeTypes.Add( ".wml", "text/vnd.wap.wml" );
            mimeTypes.Add( ".wmlc", "application/vnd.wap.wmlc" );
            mimeTypes.Add( ".wmls", "text/vnd.wap.wmlscript" );
            mimeTypes.Add( ".wmlsc", "application/vnd.wap.wmlscriptc" );
            mimeTypes.Add( ".wmp", "video/x-ms-wmp" );
            mimeTypes.Add( ".wmv", "video/x-ms-wmv" );
            mimeTypes.Add( ".wmx", "video/x-ms-wmx" );
            mimeTypes.Add( ".wmz", "application/x-ms-wmz" );
            mimeTypes.Add( ".wps", "application/vnd.ms-works" );
            mimeTypes.Add( ".wri", "application/x-mswrite" );
            mimeTypes.Add( ".wrl", "x-world/x-vrml" );
            mimeTypes.Add( ".wrz", "x-world/x-vrml" );
            mimeTypes.Add( ".wsdl", "text/xml" );
            mimeTypes.Add( ".wvx", "video/x-ms-wvx" );
            mimeTypes.Add( ".x", "application/directx" );
            mimeTypes.Add( ".xaf", "x-world/x-vrml" );
            mimeTypes.Add( ".xaml", "application/xaml+xml" );
            mimeTypes.Add( ".xap", "application/x-silverlight-app" );
            mimeTypes.Add( ".xbap", "application/x-ms-xbap" );
            mimeTypes.Add( ".xbm", "image/x-xbitmap" );
            mimeTypes.Add( ".xdr", "text/plain" );
            mimeTypes.Add( ".xla", "application/vnd.ms-excel" );
            mimeTypes.Add( ".xlam", "application/vnd.ms-excel.addin.macroEnabled.12" );
            mimeTypes.Add( ".xlc", "application/vnd.ms-excel" );
            mimeTypes.Add( ".xlm", "application/vnd.ms-excel" );
            mimeTypes.Add( ".xls", "application/vnd.ms-excel" );
            mimeTypes.Add( ".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" );
            mimeTypes.Add( ".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" );
            mimeTypes.Add( ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" );
            mimeTypes.Add( ".xlt", "application/vnd.ms-excel" );
            mimeTypes.Add( ".xltm", "application/vnd.ms-excel.template.macroEnabled.12" );
            mimeTypes.Add( ".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" );
            mimeTypes.Add( ".xlw", "application/vnd.ms-excel" );
            mimeTypes.Add( ".xml", "text/xml" );
            mimeTypes.Add( ".xof", "x-world/x-vrml" );
            mimeTypes.Add( ".xpm", "image/x-xpixmap" );
            mimeTypes.Add( ".xps", "application/vnd.ms-xpsdocument" );
            mimeTypes.Add( ".xsd", "text/xml" );
            mimeTypes.Add( ".xsf", "text/xml" );
            mimeTypes.Add( ".xsl", "text/xml" );
            mimeTypes.Add( ".xslt", "text/xml" );
            mimeTypes.Add( ".xsn", "application/octet-stream" );
            mimeTypes.Add( ".xtp", "application/octet-stream" );
            mimeTypes.Add( ".xwd", "image/x-xwindowdump" );
            mimeTypes.Add( ".z", "application/x-compress" );
            mimeTypes.Add( ".zip", "application/x-zip-compressed" );

            activity?.Stop( );
            return mimeTypes;
        }
    }
}
