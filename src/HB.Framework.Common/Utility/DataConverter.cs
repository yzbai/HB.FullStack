using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HB.Framework.Common
{
    public static class DataConverter
    {
        private static readonly Dictionary<Type, Func<object, object>> convertFunDict = new Dictionary<Type, Func<object, object>>();
        private static Dictionary<string, string> mediaType2FileTypeDict = new Dictionary<string, string>();

        static DataConverter()
        {
            #region type to type

            convertFunDict[typeof(byte)] = o => { return Convert.ToByte(o, GlobalSettings.Culture); };
            convertFunDict[typeof(sbyte)] = o => { return Convert.ToSByte(o, GlobalSettings.Culture); };
            convertFunDict[typeof(short)] = o => { return Convert.ToInt16(o, GlobalSettings.Culture); };
            convertFunDict[typeof(ushort)] = o => { return Convert.ToUInt16(o, GlobalSettings.Culture); };
            convertFunDict[typeof(int)] = o => { return Convert.ToInt32(o, GlobalSettings.Culture); };
            convertFunDict[typeof(uint)] = o => { return Convert.ToUInt32(o, GlobalSettings.Culture); };
            convertFunDict[typeof(long)] = o => { return Convert.ToInt64(o, GlobalSettings.Culture); };
            convertFunDict[typeof(ulong)] = o => { return Convert.ToUInt64(o, GlobalSettings.Culture); };
            convertFunDict[typeof(float)] = o => { return Convert.ToSingle(o, GlobalSettings.Culture); };
            convertFunDict[typeof(double)] = o => { return Convert.ToDouble(o, GlobalSettings.Culture); };
            convertFunDict[typeof(decimal)] = o => { return Convert.ToDecimal(o, GlobalSettings.Culture); };
            convertFunDict[typeof(bool)] = o => { return Convert.ToBoolean(o, GlobalSettings.Culture); };
            convertFunDict[typeof(string)] = o => { return Convert.ToString(o, GlobalSettings.Culture); };
            convertFunDict[typeof(char)] = o => { return Convert.ToChar(o, GlobalSettings.Culture); };
            convertFunDict[typeof(Guid)] = o => { return Guid.Parse(o.ToString()); };
            convertFunDict[typeof(DateTime)] = o => { return Convert.ToDateTime(o, GlobalSettings.Culture); };
            convertFunDict[typeof(DateTimeOffset)] = o => { return (DateTimeOffset)DateTime.SpecifyKind(Convert.ToDateTime(o, GlobalSettings.Culture), DateTimeKind.Utc); };
            convertFunDict[typeof(TimeSpan)] = o => { return Convert.ToDateTime(o, GlobalSettings.Culture); };
            convertFunDict[typeof(byte[])] = o => { return Serialize(o); };
            convertFunDict[typeof(byte?)] = o => { return o == null ? null : (object)Convert.ToByte(o, GlobalSettings.Culture); };
            convertFunDict[typeof(sbyte?)] = o => { return o == null ? null : (object)Convert.ToSByte(o, GlobalSettings.Culture); };
            convertFunDict[typeof(short?)] = o => { return o == null ? null : (object)Convert.ToInt16(o, GlobalSettings.Culture); };
            convertFunDict[typeof(ushort?)] = o => { return o == null ? null : (object)Convert.ToUInt16(o, GlobalSettings.Culture); };
            convertFunDict[typeof(int?)] = o => { return o == null ? null : (object)Convert.ToInt32(o, GlobalSettings.Culture); };
            convertFunDict[typeof(uint?)] = o => { return o == null ? null : (object)Convert.ToUInt32(o, GlobalSettings.Culture); };
            convertFunDict[typeof(long?)] = o => { return o == null ? null : (object)Convert.ToInt64(o, GlobalSettings.Culture); };
            convertFunDict[typeof(ulong?)] = o => { return o == null ? null : (object)Convert.ToUInt64(o, GlobalSettings.Culture); };
            convertFunDict[typeof(float?)] = o => { return o == null ? null : (object)Convert.ToSingle(o, GlobalSettings.Culture); };
            convertFunDict[typeof(double?)] = o => { return o == null ? null : (object)Convert.ToDouble(o, GlobalSettings.Culture); };
            convertFunDict[typeof(decimal?)] = o => { return o == null ? null : (object)Convert.ToDecimal(o, GlobalSettings.Culture); };
            convertFunDict[typeof(bool?)] = o => { return o == null ? null : (object)Convert.ToBoolean(o, GlobalSettings.Culture); };
            convertFunDict[typeof(char?)] = o => { return o == null ? null : (object)Convert.ToChar(o, GlobalSettings.Culture); };
            convertFunDict[typeof(Guid?)] = o => { return o == null ? null : (object)Guid.Parse(o.ToString()); };
            convertFunDict[typeof(DateTime?)] = o => { return o == null ? null : (object)Convert.ToDateTime(o, GlobalSettings.Culture); };
            convertFunDict[typeof(DateTimeOffset?)] = o => { return o == null ? null : (DateTimeOffset?)DateTime.SpecifyKind(Convert.ToDateTime(o, GlobalSettings.Culture), DateTimeKind.Utc); };
            convertFunDict[typeof(TimeSpan?)] = o => { return o == null ? null : (object)Convert.ToDateTime(o, GlobalSettings.Culture); };
            convertFunDict[typeof(Object)] = o => { return o ?? null; };
            convertFunDict[typeof(DBNull)] = o => { return o == null ? null : DBNull.Value; };

            #endregion

            #region media to file

            mediaType2FileTypeDict["audio/x-mei-aac"] = ".acp";
            mediaType2FileTypeDict["audio/aiff"] = ".aif";
            mediaType2FileTypeDict["audio/aiff"] = ".aiff";
            mediaType2FileTypeDict["text/asa"] = ".asa";
            mediaType2FileTypeDict["text/asp"] = ".asp";
            mediaType2FileTypeDict["audio/basic"] = ".au";
            mediaType2FileTypeDict["application/vnd.adobe.workflow"] = ".awf";
            mediaType2FileTypeDict["application/x-bmp"] = ".bmp";
            mediaType2FileTypeDict["application/x-c4t"] = ".c4t";
            mediaType2FileTypeDict["application/x-cals"] = ".cal";
            mediaType2FileTypeDict["application/x-netcdf"] = ".cdf";
            mediaType2FileTypeDict["application/x-cel"] = ".cel";
            mediaType2FileTypeDict["application/x-g4"] = ".cg4";
            mediaType2FileTypeDict["application/x-cit"] = ".cit";
            mediaType2FileTypeDict["text/xml"] = ".cml";
            mediaType2FileTypeDict["application/x-cmx"] = ".cmx";
            mediaType2FileTypeDict["application/pkix-crl"] = ".crl";
            mediaType2FileTypeDict["application/x-csi"] = ".csi";
            mediaType2FileTypeDict["application/x-cut"] = ".cut";
            mediaType2FileTypeDict["application/x-dbm"] = ".dbm";
            mediaType2FileTypeDict["text/xml"] = ".dcd";
            mediaType2FileTypeDict["application/x-x509-ca-cert"] = ".der";
            mediaType2FileTypeDict["application/x-dib"] = ".dib";
            mediaType2FileTypeDict["application/msword"] = ".doc";
            mediaType2FileTypeDict["application/x-drw"] = ".drw";
            mediaType2FileTypeDict["Model/vnd.dwf"] = ".dwf";
            mediaType2FileTypeDict["application/x-dwg"] = ".dwg";
            mediaType2FileTypeDict["application/x-dxf"] = ".dxf";
            mediaType2FileTypeDict["application/x-emf"] = ".emf";
            mediaType2FileTypeDict["text/xml"] = ".ent";
            mediaType2FileTypeDict["application/x-ps"] = ".eps";
            mediaType2FileTypeDict["application/x-ebx"] = ".etd";
            mediaType2FileTypeDict["image/fax"] = ".fax";
            mediaType2FileTypeDict["application/fractals"] = ".fif";
            mediaType2FileTypeDict["application/x-frm"] = ".frm";
            mediaType2FileTypeDict["application/x-gbr"] = ".gbr";
            mediaType2FileTypeDict["image/gif"] = ".gif";
            mediaType2FileTypeDict["application/x-gp4"] = ".gp4";
            mediaType2FileTypeDict["application/x-hmr"] = ".hmr";
            mediaType2FileTypeDict["application/x-hpl"] = ".hpl";
            mediaType2FileTypeDict["application/x-hrf"] = ".hrf";
            mediaType2FileTypeDict["text/x-component"] = ".htc";
            mediaType2FileTypeDict["text/html"] = ".html";
            mediaType2FileTypeDict["text/html"] = ".htx";
            mediaType2FileTypeDict["image/x-icon"] = ".ico";
            mediaType2FileTypeDict["application/x-iff"] = ".iff";
            mediaType2FileTypeDict["application/x-igs"] = ".igs";
            mediaType2FileTypeDict["application/x-img"] = ".img";
            mediaType2FileTypeDict["application/x-internet-signup"] = ".isp";
            mediaType2FileTypeDict["java/*"] = ".java";
            mediaType2FileTypeDict["image/jpeg"] = ".jpe";
            mediaType2FileTypeDict["image/jpeg"] = ".jpeg";
            mediaType2FileTypeDict["application/x-jpg"] = ".jpg";
            mediaType2FileTypeDict["text/html"] = ".jsp";
            mediaType2FileTypeDict["application/x-laplayer-reg"] = ".lar";
            mediaType2FileTypeDict["audio/x-liquid-secure"] = ".lavs";
            mediaType2FileTypeDict["audio/x-la-lms"] = ".lmsff";
            mediaType2FileTypeDict["application/x-ltr"] = ".ltr";
            mediaType2FileTypeDict["video/x-mpeg"] = ".m2v";
            mediaType2FileTypeDict["video/mpeg4"] = ".m4e";
            mediaType2FileTypeDict["application/x-troff-man"] = ".man";
            mediaType2FileTypeDict["application/msaccess"] = ".mdb";
            mediaType2FileTypeDict["application/x-shockwave-flash"] = ".mfp";
            mediaType2FileTypeDict["message/rfc822"] = ".mhtml";
            mediaType2FileTypeDict["audio/mid"] = ".mid";
            mediaType2FileTypeDict["application/x-mil"] = ".mil";
            mediaType2FileTypeDict["audio/x-musicnet-download"] = ".mnd";
            mediaType2FileTypeDict["application/x-javascript"] = ".mocha";
            mediaType2FileTypeDict["audio/mp1"] = ".mp1";
            mediaType2FileTypeDict["video/mpeg"] = ".mp2v";
            mediaType2FileTypeDict["video/mpeg4"] = ".mp4";
            mediaType2FileTypeDict["application/vnd.ms-project"] = ".mpd";
            mediaType2FileTypeDict["video/mpg"] = ".mpeg";
            mediaType2FileTypeDict["audio/rn-mpeg"] = ".mpga";
            mediaType2FileTypeDict["video/x-mpeg"] = ".mps";
            mediaType2FileTypeDict["video/mpg"] = ".mpv";
            mediaType2FileTypeDict["application/vnd.ms-project"] = ".mpw";
            mediaType2FileTypeDict["text/xml"] = ".mtx";
            mediaType2FileTypeDict["image/pnetvue"] = ".net";
            mediaType2FileTypeDict["message/rfc822"] = ".nws";
            mediaType2FileTypeDict["application/x-out"] = ".out";
            mediaType2FileTypeDict["application/x-pkcs12"] = ".p12";
            mediaType2FileTypeDict["application/pkcs7-mime"] = ".p7c";
            mediaType2FileTypeDict["application/x-pkcs7-certreqresp"] = ".p7r";
            mediaType2FileTypeDict["application/x-pc5"] = ".pc5";
            mediaType2FileTypeDict["application/x-pcl"] = ".pcl";
            mediaType2FileTypeDict["application/pdf"] = ".pdf";
            mediaType2FileTypeDict["application/vnd.adobe.pdx"] = ".pdx";
            mediaType2FileTypeDict["application/x-pgl"] = ".pgl";
            mediaType2FileTypeDict["application/vnd.ms-pki.pko"] = ".pko";
            mediaType2FileTypeDict["text/html"] = ".plg";
            mediaType2FileTypeDict["application/x-plt"] = ".plt";
            mediaType2FileTypeDict["application/x-png"] = ".png";
            mediaType2FileTypeDict["application/vnd.ms-powerpoint"] = ".ppa";
            mediaType2FileTypeDict["application/vnd.ms-powerpoint"] = ".pps";
            mediaType2FileTypeDict["application/x-ppt"] = ".ppt";
            mediaType2FileTypeDict["application/pics-rules"] = ".prf";
            mediaType2FileTypeDict["application/x-prt"] = ".prt";
            mediaType2FileTypeDict["application/postscript"] = ".ps";
            mediaType2FileTypeDict["application/vnd.ms-powerpoint"] = ".pwz";
            mediaType2FileTypeDict["audio/vnd.rn-realaudio"] = ".ra";
            mediaType2FileTypeDict["application/x-ras"] = ".ras";
            mediaType2FileTypeDict["text/xml"] = ".rdf";
            mediaType2FileTypeDict["application/x-red"] = ".red";
            mediaType2FileTypeDict["application/vnd.rn-realsystem-rjs"] = ".rjs";
            mediaType2FileTypeDict["application/x-rlc"] = ".rlc";
            mediaType2FileTypeDict["application/vnd.rn-realmedia"] = ".rm";
            mediaType2FileTypeDict["audio/mid"] = ".rmi";
            mediaType2FileTypeDict["audio/x-pn-realaudio"] = ".rmm";
            mediaType2FileTypeDict["application/vnd.rn-realmedia-secure"] = ".rms";
            mediaType2FileTypeDict["application/vnd.rn-realsystem-rmx"] = ".rmx";
            mediaType2FileTypeDict["image/vnd.rn-realpix"] = ".rp";
            mediaType2FileTypeDict["application/vnd.rn-rsml"] = ".rsml";
            mediaType2FileTypeDict["application/msword"] = ".rtf";
            mediaType2FileTypeDict["video/vnd.rn-realvideo"] = ".rv";
            mediaType2FileTypeDict["application/x-sat"] = ".sat";
            mediaType2FileTypeDict["application/x-sdw"] = ".sdw";
            mediaType2FileTypeDict["application/x-slb"] = ".slb";
            mediaType2FileTypeDict["drawing/x-slk"] = ".slk";
            mediaType2FileTypeDict["application/smil"] = ".smil";
            mediaType2FileTypeDict["audio/basic"] = ".snd";
            mediaType2FileTypeDict["text/plain"] = ".sor";
            mediaType2FileTypeDict["application/futuresplash"] = ".spl";
            mediaType2FileTypeDict["application/streamingmedia"] = ".ssm";
            mediaType2FileTypeDict["application/vnd.ms-pki.stl"] = ".stl";
            mediaType2FileTypeDict["application/x-sty"] = ".sty";
            mediaType2FileTypeDict["application/x-shockwave-flash"] = ".swf";
            mediaType2FileTypeDict["application/x-tg4"] = ".tg4";
            mediaType2FileTypeDict["image/tiff"] = ".tif";
            mediaType2FileTypeDict["image/tiff"] = ".tiff";
            mediaType2FileTypeDict["drawing/x-top"] = ".top";
            mediaType2FileTypeDict["text/xml"] = ".tsd";
            mediaType2FileTypeDict["application/x-icq"] = ".uin";
            mediaType2FileTypeDict["text/x-vcard"] = ".vcf";
            mediaType2FileTypeDict["application/vnd.visio"] = ".vdx";
            mediaType2FileTypeDict["application/x-vpeg005"] = ".vpg";
            mediaType2FileTypeDict["application/x-vsd"] = ".vsd";
            mediaType2FileTypeDict["application/vnd.visio"] = ".vst";
            mediaType2FileTypeDict["application/vnd.visio"] = ".vsw";
            mediaType2FileTypeDict["application/vnd.visio"] = ".vtx";
            mediaType2FileTypeDict["audio/wav"] = ".wav";
            mediaType2FileTypeDict["application/x-wb1"] = ".wb1";
            mediaType2FileTypeDict["application/x-wb3"] = ".wb3";
            mediaType2FileTypeDict["application/msword"] = ".wiz";
            mediaType2FileTypeDict["application/x-wk4"] = ".wk4";
            mediaType2FileTypeDict["application/x-wks"] = ".wks";
            mediaType2FileTypeDict["audio/x-ms-wma"] = ".wma";
            mediaType2FileTypeDict["application/x-wmf"] = ".wmf";
            mediaType2FileTypeDict["video/x-ms-wmv"] = ".wmv";
            mediaType2FileTypeDict["application/x-ms-wmz"] = ".wmz";
            mediaType2FileTypeDict["application/x-wpd"] = ".wpd";
            mediaType2FileTypeDict["application/vnd.ms-wpl"] = ".wpl";
            mediaType2FileTypeDict["application/x-wr1"] = ".wr1";
            mediaType2FileTypeDict["application/x-wrk"] = ".wrk";
            mediaType2FileTypeDict["application/x-ws"] = ".ws2";
            mediaType2FileTypeDict["text/xml"] = ".wsdl";
            mediaType2FileTypeDict["application/vnd.adobe.xdp"] = ".xdp";
            mediaType2FileTypeDict["application/vnd.adobe.xfd"] = ".xfd";
            mediaType2FileTypeDict["text/html"] = ".xhtml";
            mediaType2FileTypeDict["application/x-xls"] = ".xls";
            mediaType2FileTypeDict["text/xml"] = ".xml";
            mediaType2FileTypeDict["text/xml"] = ".xq";
            mediaType2FileTypeDict["text/xml"] = ".xquery";
            mediaType2FileTypeDict["text/xml"] = ".xsl";
            mediaType2FileTypeDict["application/x-xwd"] = ".xwd";
            mediaType2FileTypeDict["application/vnd.symbian.install"] = ".sis";
            mediaType2FileTypeDict["application/x-x_t"] = ".x_t";
            mediaType2FileTypeDict["application/vnd.android.package-archive"] = ".apk";
            mediaType2FileTypeDict["image/tiff"] = ".tif";
            mediaType2FileTypeDict["application/x-301"] = "..301";
            mediaType2FileTypeDict["application/x-906"] = "..906";
            mediaType2FileTypeDict["application/x-a11"] = ".a11";
            mediaType2FileTypeDict["application/postscript"] = ".ai";
            mediaType2FileTypeDict["audio/aiff"] = ".aifc";
            mediaType2FileTypeDict["application/x-anv"] = ".anv";
            mediaType2FileTypeDict["video/x-ms-asf"] = ".asf";
            mediaType2FileTypeDict["video/x-ms-asf"] = ".asx";
            mediaType2FileTypeDict["video/avi"] = ".avi";
            mediaType2FileTypeDict["text/xml"] = ".biz";
            mediaType2FileTypeDict["application/x-bot"] = ".bot";
            mediaType2FileTypeDict["application/x-c90"] = ".c90";
            mediaType2FileTypeDict["application/vnd.ms-pki.seccat"] = ".cat";
            mediaType2FileTypeDict["application/x-cdr"] = ".cdr";
            mediaType2FileTypeDict["application/x-x509-ca-cert"] = ".cer";
            mediaType2FileTypeDict["application/x-cgm"] = ".cgm";
            mediaType2FileTypeDict["java/*"] = ".class";
            mediaType2FileTypeDict["application/x-cmp"] = ".cmp";
            mediaType2FileTypeDict["application/x-cot"] = ".cot";
            mediaType2FileTypeDict["application/x-x509-ca-cert"] = ".crt";
            mediaType2FileTypeDict["text/css"] = ".css";
            mediaType2FileTypeDict["application/x-dbf"] = ".dbf";
            mediaType2FileTypeDict["application/x-dbx"] = ".dbx";
            mediaType2FileTypeDict["application/x-dcx"] = ".dcx";
            mediaType2FileTypeDict["application/x-dgn"] = ".dgn";
            mediaType2FileTypeDict["application/x-msdownload"] = ".dll";
            mediaType2FileTypeDict["application/msword"] = ".dot";
            mediaType2FileTypeDict["text/xml"] = ".dtd";
            mediaType2FileTypeDict["application/x-dwf"] = ".dwf";
            mediaType2FileTypeDict["application/x-dxb"] = ".dxb";
            mediaType2FileTypeDict["application/vnd.adobe.edn"] = ".edn";
            mediaType2FileTypeDict["message/rfc822"] = ".eml";
            mediaType2FileTypeDict["application/x-epi"] = ".epi";
            mediaType2FileTypeDict["application/postscript"] = ".eps";
            mediaType2FileTypeDict["application/x-msdownload"] = ".exe";
            mediaType2FileTypeDict["application/vnd.fdf"] = ".fdf";
            mediaType2FileTypeDict["text/xml"] = ".fo";
            mediaType2FileTypeDict["application/x-g4"] = ".g4";
            mediaType2FileTypeDict["application/x-"] = ".";
            mediaType2FileTypeDict["application/x-gl2"] = ".gl2";
            mediaType2FileTypeDict["application/x-hgl"] = ".hgl";
            mediaType2FileTypeDict["application/x-hpgl"] = ".hpg";
            mediaType2FileTypeDict["application/mac-binhex40"] = ".hqx";
            mediaType2FileTypeDict["application/hta"] = ".hta";
            mediaType2FileTypeDict["text/html"] = ".htm";
            mediaType2FileTypeDict["text/webviewhtml"] = ".htt";
            mediaType2FileTypeDict["application/x-icb"] = ".icb";
            mediaType2FileTypeDict["application/x-ico"] = ".ico";
            mediaType2FileTypeDict["application/x-g4"] = ".ig4";
            mediaType2FileTypeDict["application/x-iphone"] = ".iii";
            mediaType2FileTypeDict["application/x-internet-signup"] = ".ins";
            mediaType2FileTypeDict["video/x-ivf"] = ".IVF";
            mediaType2FileTypeDict["image/jpeg"] = ".jfif";
            mediaType2FileTypeDict["application/x-jpe"] = ".jpe";
            mediaType2FileTypeDict["image/jpeg"] = ".jpg";
            mediaType2FileTypeDict["application/x-javascript"] = ".js";
            mediaType2FileTypeDict["audio/x-liquid-file"] = ".la1";
            mediaType2FileTypeDict["application/x-latex"] = ".latex";
            mediaType2FileTypeDict["application/x-lbm"] = ".lbm";
            mediaType2FileTypeDict["application/x-javascript"] = ".ls";
            mediaType2FileTypeDict["video/x-mpeg"] = ".m1v";
            mediaType2FileTypeDict["audio/mpegurl"] = ".m3u";
            mediaType2FileTypeDict["application/x-mac"] = ".mac";
            mediaType2FileTypeDict["text/xml"] = ".math";
            mediaType2FileTypeDict["application/x-mdb"] = ".mdb";
            mediaType2FileTypeDict["message/rfc822"] = ".mht";
            mediaType2FileTypeDict["application/x-mi"] = ".mi";
            mediaType2FileTypeDict["audio/mid"] = ".midi";
            mediaType2FileTypeDict["text/xml"] = ".mml";
            mediaType2FileTypeDict["audio/x-musicnet-stream"] = ".mns";
            mediaType2FileTypeDict["video/x-sgi-movie"] = ".movie";
            mediaType2FileTypeDict["audio/mp2"] = ".mp2";
            mediaType2FileTypeDict["audio/mp3"] = ".mp3";
            mediaType2FileTypeDict["video/x-mpg"] = ".mpa";
            mediaType2FileTypeDict["video/x-mpeg"] = ".mpe";
            mediaType2FileTypeDict["video/mpg"] = ".mpg";
            mediaType2FileTypeDict["application/vnd.ms-project"] = ".mpp";
            mediaType2FileTypeDict["application/vnd.ms-project"] = ".mpt";
            mediaType2FileTypeDict["video/mpeg"] = ".mpv2";
            mediaType2FileTypeDict["application/vnd.ms-project"] = ".mpx";
            mediaType2FileTypeDict["application/x-mmxp"] = ".mxp";
            mediaType2FileTypeDict["application/x-nrf"] = ".nrf";
            mediaType2FileTypeDict["text/x-ms-odc"] = ".odc";
            mediaType2FileTypeDict["application/pkcs10"] = ".p10";
            mediaType2FileTypeDict["application/x-pkcs7-certificates"] = ".p7b";
            mediaType2FileTypeDict["application/pkcs7-mime"] = ".p7m";
            mediaType2FileTypeDict["application/pkcs7-signature"] = ".p7s";
            mediaType2FileTypeDict["application/x-pci"] = ".pci";
            mediaType2FileTypeDict["application/x-pcx"] = ".pcx";
            mediaType2FileTypeDict["application/pdf"] = ".pdf";
            mediaType2FileTypeDict["application/x-pkcs12"] = ".pfx";
            mediaType2FileTypeDict["application/x-pic"] = ".pic";
            mediaType2FileTypeDict["application/x-perl"] = ".pl";
            mediaType2FileTypeDict["audio/scpls"] = ".pls";
            mediaType2FileTypeDict["image/png"] = ".png";
            mediaType2FileTypeDict["application/vnd.ms-powerpoint"] = ".pot";
            mediaType2FileTypeDict["application/x-ppm"] = ".ppm";
            mediaType2FileTypeDict["application/vnd.ms-powerpoint"] = ".ppt";
            mediaType2FileTypeDict["application/x-pr"] = ".pr";
            mediaType2FileTypeDict["application/x-prn"] = ".prn";
            mediaType2FileTypeDict["application/x-ps"] = ".ps";
            mediaType2FileTypeDict["application/x-ptn"] = ".ptn";
            mediaType2FileTypeDict["text/vnd.rn-realtext3d"] = ".r3t";
            mediaType2FileTypeDict["audio/x-pn-realaudio"] = ".ram";
            mediaType2FileTypeDict["application/rat-file"] = ".rat";
            mediaType2FileTypeDict["application/vnd.rn-recording"] = ".rec";
            mediaType2FileTypeDict["application/x-rgb"] = ".rgb";
            mediaType2FileTypeDict["application/vnd.rn-realsystem-rjt"] = ".rjt";
            mediaType2FileTypeDict["application/x-rle"] = ".rle";
            mediaType2FileTypeDict["application/vnd.adobe.rmf"] = ".rmf";
            mediaType2FileTypeDict["application/vnd.rn-realsystem-rmj"] = ".rmj";
            mediaType2FileTypeDict["application/vnd.rn-rn_music_package"] = ".rmp";
            mediaType2FileTypeDict["application/vnd.rn-realmedia-vbr"] = ".rmvb";
            mediaType2FileTypeDict["application/vnd.rn-realplayer"] = ".rnx";
            mediaType2FileTypeDict["audio/x-pn-realaudio-plugin"] = ".rpm";
            mediaType2FileTypeDict["text/vnd.rn-realtext"] = ".rt";
            mediaType2FileTypeDict["application/x-rtf"] = ".rtf";
            mediaType2FileTypeDict["application/x-sam"] = ".sam";
            mediaType2FileTypeDict["application/sdp"] = ".sdp";
            mediaType2FileTypeDict["application/x-stuffit"] = ".sit";
            mediaType2FileTypeDict["application/x-sld"] = ".sld";
            mediaType2FileTypeDict["application/smil"] = ".smi";
            mediaType2FileTypeDict["application/x-smk"] = ".smk";
            mediaType2FileTypeDict["text/plain"] = ".sol";
            mediaType2FileTypeDict["application/x-pkcs7-certificates"] = ".spc";
            mediaType2FileTypeDict["text/xml"] = ".spp";
            mediaType2FileTypeDict["application/vnd.ms-pki.certstore"] = ".sst";
            mediaType2FileTypeDict["text/html"] = ".stm";
            mediaType2FileTypeDict["text/xml"] = ".svg";
            mediaType2FileTypeDict["application/x-tdf"] = ".tdf";
            mediaType2FileTypeDict["application/x-tga"] = ".tga";
            mediaType2FileTypeDict["application/x-tif"] = ".tif";
            mediaType2FileTypeDict["text/xml"] = ".tld";
            mediaType2FileTypeDict["application/x-bittorrent"] = ".torrent";
            mediaType2FileTypeDict["text/plain"] = ".txt";
            mediaType2FileTypeDict["text/iuls"] = ".uls";
            mediaType2FileTypeDict["application/x-vda"] = ".vda";
            mediaType2FileTypeDict["text/xml"] = ".vml";
            mediaType2FileTypeDict["application/vnd.visio"] = ".vsd";
            mediaType2FileTypeDict["application/vnd.visio"] = ".vss";
            mediaType2FileTypeDict["application/x-vst"] = ".vst";
            mediaType2FileTypeDict["application/vnd.visio"] = ".vsx";
            mediaType2FileTypeDict["text/xml"] = ".vxml";
            mediaType2FileTypeDict["audio/x-ms-wax"] = ".wax";
            mediaType2FileTypeDict["application/x-wb2"] = ".wb2";
            mediaType2FileTypeDict["image/vnd.wap.wbmp"] = ".wbmp";
            mediaType2FileTypeDict["application/x-wk3"] = ".wk3";
            mediaType2FileTypeDict["application/x-wkq"] = ".wkq";
            mediaType2FileTypeDict["video/x-ms-wm"] = ".wm";
            mediaType2FileTypeDict["application/x-ms-wmd"] = ".wmd";
            mediaType2FileTypeDict["text/vnd.wap.wml"] = ".wml";
            mediaType2FileTypeDict["video/x-ms-wmx"] = ".wmx";
            mediaType2FileTypeDict["application/x-wp6"] = ".wp6";
            mediaType2FileTypeDict["application/x-wpg"] = ".wpg";
            mediaType2FileTypeDict["application/x-wq1"] = ".wq1";
            mediaType2FileTypeDict["application/x-wri"] = ".wri";
            mediaType2FileTypeDict["application/x-ws"] = ".ws";
            mediaType2FileTypeDict["text/scriptlet"] = ".wsc";
            mediaType2FileTypeDict["video/x-ms-wvx"] = ".wvx";
            mediaType2FileTypeDict["text/xml"] = ".xdr";
            mediaType2FileTypeDict["application/vnd.adobe.xfdf"] = ".xfdf";
            mediaType2FileTypeDict["application/vnd.ms-excel"] = ".xls";
            mediaType2FileTypeDict["application/x-xlw"] = ".xlw";
            mediaType2FileTypeDict["audio/scpls"] = ".xpl";
            mediaType2FileTypeDict["text/xml"] = ".xql";
            mediaType2FileTypeDict["text/xml"] = ".xsd";
            mediaType2FileTypeDict["text/xml"] = ".xslt";
            mediaType2FileTypeDict["application/x-x_b"] = ".x_b";
            mediaType2FileTypeDict["application/vnd.symbian.install"] = ".sisx";
            mediaType2FileTypeDict["application/vnd.iphone"] = ".ipa";
            mediaType2FileTypeDict["application/x-silverlight-app"] = ".xap";

            #endregion
        }

        #region Media to File

        public static string GetFileTypeByMediaType(string mediaType)
        {
            string mType = mediaType.ToLower(GlobalSettings.Culture);

            if (mediaType2FileTypeDict.ContainsKey(mType))
            {
                return mediaType2FileTypeDict[mType];
            }

            return "";
        }

        #endregion

        #region Type to Type

        public static object To(Type type, object value)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            if (typeInfo.IsEnum)
            {
                return Convert.ToInt32(value, GlobalSettings.Culture);
            }

            if (value.GetType() == typeof(DBNull))
            {
                return DefaultForType(type);
            }

            Func<object, object> convertFn = convertFunDict[type];
            return convertFn(value);
        }

        public static object DefaultForType(Type targetType)
        {
            TypeInfo typeInfo = targetType.GetTypeInfo();
            return typeInfo.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        public static T To<T>(object value)
        {
            Type type = typeof(T);
            TypeInfo typeInfo = type.GetTypeInfo();

            if (typeInfo.IsEnum)
            {
                return (T)Enum.Parse(type, value.ToString());
            }

            return (T)To(type, value);
        }

        #endregion

        #region Type to String

        public static string GetObjectValueStringStatement(object value)
        {
            string valueStr = string.Empty;

            if (value != null)
            {
                Type type = value.GetType();
                TypeInfo typeInfo = type.GetTypeInfo();

                if (typeInfo.IsEnum)
                {
                    valueStr = ((Int32)value).ToString(GlobalSettings.Culture);
                }

                else if (type == typeof(string))
                {
                    valueStr = (string)value;
                }
                else if (type == typeof(DateTime))
                {
                    valueStr = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss", GlobalSettings.Culture);
                }
                else if (type == typeof(DateTimeOffset))
                {
                    valueStr = ((DateTimeOffset)value).ToString("yyyy-MM-dd HH:mm:ss", GlobalSettings.Culture);
                }
                else if (type == typeof(bool))
                {
                    valueStr = (bool)value ? "1" : "0";
                }
                else if (type == typeof(DBNull))
                {
                    valueStr = null;
                }
                else
                {
                    valueStr = value.ToString();
                }
            }
            else
            {
                valueStr = null;
            }

            return valueStr;
        }

        #endregion

        #region object to bytes

        public static byte[] SerializeInt(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static int DeserializeInt(byte[] value)
        {
            return BitConverter.ToInt32(value, 0);
        }

        public static T DeSerialize<T>(byte[] buffer)
        {
            if (buffer == null)
            {
                return default(T);
            }

            return MsgPack.Serialization.MessagePackSerializer.Get<T>().UnpackSingleObject(buffer);
        }

        public static object DeSerialize(Type type, byte[] buffer)
        {
            if (buffer == null)
            {
                return null;





            }

            return MsgPack.Serialization.MessagePackSerializer.Get(type).UnpackSingleObject(buffer);
        }


        public static byte[] Serialize<T>(T item)
        {
            if (item == null)
            {
                return null;
            }

            return MsgPack.Serialization.MessagePackSerializer.Get<T>().PackSingleObject(item);
        }

        public static byte[] Serialize(Type type, object item)
        {
            if (item == null)
            {
                return null;
            }

            return MsgPack.Serialization.MessagePackSerializer.Get(type).PackSingleObject(item);
        }

        #endregion

        #region String Encode to bytes

        public static byte[] GetUTF8Bytes(string item)
        {
            if (item == null)
            {
                return null;
            }
            return Encoding.UTF8.GetBytes(item);
        }

        public static string GetUTF8String(byte[] item)
        {
            if (item == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(item);
        }

        public static string ToHexString(byte[] bytes)
        {
            var hex = new StringBuilder();

            foreach (byte b in bytes)
            {
                hex.AppendFormat(GlobalSettings.Culture, "{0:x2}", b);
            }

            return hex.ToString();
        }

        #endregion

        #region Json
        //TODO: Use ServiceStack.Text Instead of.
        public static string ToJson(object domain)
        {
            return JsonConvert.SerializeObject(domain);
        }

        public static T FromJson<T>(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        #endregion

        #region Time

        public static long ToTimestamp(DateTimeOffset dt)
        {
            TimeSpan ts = dt - new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        public static DateTimeOffset ToDateTimeOffset(long timestamp)
        {
            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero).AddSeconds(timestamp);
        }

        #endregion
    }
}
